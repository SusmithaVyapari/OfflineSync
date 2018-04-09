using System.Threading.Tasks;
using System;
using OfflineSyncClient.DB;
using OfflineSync.DomainModel.Models;
using System.Collections.Generic;
using OfflineSyncClient.Enums;
using OfflineSyncClient.Models;
//using TwoWaySync.DomainModel;
using OfflineSync.DomainModel;
using SQLite;

namespace OfflineSyncClient
{
    public class Sync<T> where T : ISyncBaseModel, new()
    {
        private string _DBPath { get; set; }
        private string _token;
        private string _baseURL;

        IDBOperations _dBOperations;

        public Sync(string databasePath, string baseURL, string token, DBType dbType = DBType.SQLite)
        {
            _DBPath = databasePath;
            _baseURL = baseURL;
            _token = token;

            switch (dbType)
            {
                case DBType.SQLite:
                    _dBOperations = new SQLiteDBOperations(databasePath);

                    break;
            }
        }

        public async Task StartSyncAsync()
        {
            try
            {
                IDBOperations operations = new SQLiteDBOperations(_DBPath);
                List<SyncSettingsModel> settingslist = operations.GetSyncSettingByTable(typeof(T).Name);
                // Devices devices = operations.GetDeviceID();

                // Having dublicate entries
                if (settingslist.Count > 1)
                {
                    throw new Exception(StringUtility.DulplicateSettings);
                }

                if (settingslist != null)
                {
                    SyncSettingsModel settings = settingslist[0];

                    string data = string.Empty;
                    // add device Id to the route
                    if (settings.AutoSync)
                    {
                        data = string.Format(StringUtility.AutoSyncAPIGetCall
                                                   , settings.ServerTableName
                                                   , settings.ServerAssemblyName
                                                   , settings.LastSyncedAt
                                                   , settings.ControllerData);
                    }
                    else
                    {
                        data = string.Format(StringUtility.UserAPIGetCall
                                                 , settings.ControllerName
                                                 , settings.LastSyncedAt
                                                 , settings.ControllerData);
                    }

                    SyncAPI syncAPI = new SyncAPI(_baseURL, _token);
                    APIModel model = null;
                    // Initial Sync
                    if (settings.LastSyncedAt == null)
                    {
                        List<T> ServerList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<T>>(model.Data.ToString());
                        using (SQLiteConnection conn = new SQLiteConnection(_DBPath))
                        {
                            conn.InsertAll(ServerList);
                        }
                        DateTime CurrentTime = DateTime.UtcNow;
                        long minTicks = (CurrentTime - ServerList[0].ModifiedAt).Ticks;
                        foreach (T item in ServerList)
                        {
                            if ((CurrentTime - item.ModifiedAt).Ticks < minTicks)
                            {
                                minTicks = (CurrentTime - item.ModifiedAt).Ticks;
                                settings.LastSyncedAt = item.ModifiedAt;
                            }
                        }
                        using (SQLiteConnection context = new SQLiteConnection(_DBPath))
                        {
                            context.Update(settings);
                        }
                    }
                    else
                    {
                        List<T> FailedTransactionData = _dBOperations.GetFailedTransactionData<T>();
                        if (FailedTransactionData.Count != 0)
                        {
                            model.FailedTrasationData = FailedTransactionData;
                            model.DeviceID = "23456";
                            model.LastSyncDate = settings.LastSyncedAt.Value;
                            model.TableName = settings.ServerTableName;
                        }
                        if (FailedTransactionData.Count == 0)
                        {
                            model.DeviceID = "23456";
                            model.LastSyncDate = settings.LastSyncedAt.Value;
                            model.TableName = settings.ServerTableName;
                        }
                        // await syncAPI.Post<T>(model,data);
                        APIModel modelReturned = await syncAPI.Get<APIModel>(data);
                        List<T> FailedTransactionIds = Newtonsoft.Json.JsonConvert.DeserializeObject<List<T>>(model.FailedTransactionID.ToString());
                        List<FailedRecords> FailedSyncRecords = Newtonsoft.Json.JsonConvert.DeserializeObject<List<FailedRecords>>(model.FailedSyncRecords.ToString());
                        if (FailedTransactionIds != null)
                        {
                            foreach (T id in FailedTransactionIds)
                            {
                                id.TransactionID = Guid.NewGuid().ToString();
                            }
                            using (SQLiteConnection conn = new SQLiteConnection(_DBPath))
                            {
                                conn.UpdateAll(FailedTransactionIds);
                            }
                        }

                        else if (FailedSyncRecords != null)
                        {
                            foreach (FailedRecords id in FailedSyncRecords)
                            {
                                if (id.IsConflictedID == true)
                                {
                                    id.SyncID = Guid.NewGuid().ToString();
                                    // update in db.
                                }
                                else
                                {
                                    string errormessage = id.exceptionmsg;
                                    //show message to user by any means.
                                }
                            }

                        }
                        List<T> InsertList = new List<T>();
                        List<T> ModifyList = new List<T>();
                        List<T> ServerList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<T>>(model.Data.ToString());
                        List<T> UpdatedClientList = _dBOperations.GetData<T>(settings.LastSyncedAt);
                        else
                        { // Normal Sync Operations

                            foreach (T item in ServerList)
                            {
                                // Insert logic
                                if (DateTime.Compare(settings.LastSyncedAt.Value, item.CreatedAt) < 0)
                                {
                                    InsertList.Add(item);
                                }
                                // update and delete logic
                                else
                                {
                                    int index = UpdatedClientList.FindIndex(m => m.VersionID == item.VersionID);
                                    // If record is modified both at the server and at the client
                                    if (index != -1)
                                    {
                                        // Based on Timestamps
                                        if (settings.Priority == OveridePriority.LastUpdated)
                                        {
                                            if (DateTime.Compare(item.ModifiedAt, UpdatedClientList[index].ModifiedAt) > 0)
                                            {
                                                ModifyList.Add(item);
                                                UpdatedClientList.Remove(UpdatedClientList[index]);
                                            }
                                        }
                                        // Based on server priority
                                        if (settings.Priority == OveridePriority.Server)
                                        {
                                            ModifyList.Add(item);
                                            UpdatedClientList.Remove(UpdatedClientList[index]);
                                        }
                                        // Based on Client priority
                                        // Ask user choice
                                        else
                                        {
                                            Console.WriteLine("Select 1. Server 2.Client");
                                            int choice = Int32.Parse(Console.ReadLine());
                                            if (choice == 1)
                                            {
                                                ModifyList.Add(item);
                                                UpdatedClientList.Remove(UpdatedClientList[index]);

                                            }
                                        }
                                    }
                                    // If the record is only modifies at the server.(No Conflict)
                                    if (index == -1) ModifyList.Add(item);
                                }
                            }

                        }
                        using (SQLiteConnection conn = new SQLiteConnection(_DBPath))
                        {
                            conn.InsertAll(InsertList);
                            conn.UpdateAll(ModifyList);
                        }
                        // Client list should be sent to server
                        // getting response from client whether the transaction is succeeded or not.
                        // Updating last sync to Latest Modified At.
                        DateTime CurrentTime = DateTime.UtcNow;
                        long minTicks = (CurrentTime - ServerList[0].ModifiedAt).Ticks;
                        foreach (T item in ServerList)
                        {
                            if ((CurrentTime - item.ModifiedAt).Ticks < minTicks)
                            {
                                minTicks = (CurrentTime - item.ModifiedAt).Ticks;
                                settings.LastSyncedAt = item.ModifiedAt;

                            }
                        }
                        using (SQLiteConnection context = new SQLiteConnection(_DBPath))
                        {
                            context.Update(settings);
                        }
                    }
                }


            }

            catch (Exception ex)
            {
                throw;
            }

        }
    }
}
