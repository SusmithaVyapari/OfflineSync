using SQLite;
using System.Collections.Generic;
using System.Linq;
using OfflineSync.DomainModel.Models;
using OfflineSyncClient.Models;
using System;

namespace OfflineSyncClient.DB
{
    public class SQLiteDBOperations : IDBOperations
    {
        public string _DBPath;

        public SQLiteDBOperations(string DBpath)
        {
            _DBPath = DBpath;
        }

        public List<SyncSettingsModel> GetSyncSettingByTable(string clientTableName)
        {
            using (SQLiteConnection conn = new SQLiteConnection(_DBPath))
            {
                return conn.Table<SyncSettingsModel>().Where(m => m.ClientTableName == clientTableName).ToList();
            }
        }

        public List<T> GetData<T>(DateTime? lastsync) where T : ISyncBaseModel, new()
        {
            DateTime syncTime = lastsync.Value;

            using (SQLiteConnection conn = new SQLiteConnection(_DBPath))
            {
                List<T> data = conn.Table<T>().ToList().Where(m => DateTime.Compare(syncTime, m.ModifiedAt) < 0).ToList();

                return data;
            }

        }
        public List<T> GetFailedTransactionData<T>() where T : ISyncBaseModel, new()
        {
            using (SQLiteConnection conn = new SQLiteConnection(_DBPath))
            {
                List<T> data = conn.Table<T>().ToList().Where(m => m.IsSynced == false).ToList();

                return data;
            }

        }
    }
}