using SQLite;
using System;
using System.Threading;
using OfflineSyncClient;
//using OfflineSyncClient;
using OfflineSync.DomainModel.Models;
using OfflineSyncClient.Enums;
using OfflineSyncClient.Models;
using System.Collections.Generic;
using System.Linq;

namespace ClientApp
{
    public class TestTable : ISyncBaseModel
    {
        [PrimaryKey]
        public string VersionID { get; set; }
        public string TransactionID { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public bool IsDeleted { get; set; }
        public string Name { get; set; }
        public bool IsSynced { get; set; }
        public bool IsInserted { get; set; }
    }

    class Program
    {
        public List<TestTable> ShowRecords(string DBPath)
        {
            using (SQLiteConnection context = new SQLiteConnection(DBPath))
            {

                //return conn.Table<SyncSettings>().Where(m => m.TableName == tableName).FirstOrDefault()
                var list = context.Table<TestTable>().Where(m => m.VersionID != null && m.IsDeleted == false).ToList();
                int i = 1;
                foreach (var item in list)
                {
                    Console.Write(i + "" +
                        item.VersionID + "    " + item.CreatedAt + "  " + item.ModifiedAt + "   " + item.Name + "  " + item.IsDeleted);
                    Console.WriteLine("");
                    Console.WriteLine("");
                    i++;
                }
                return list;

            }
        }

        static void Main(string[] args)
        {
           Program program = new Program();
            string url = "http://localhost:52058/api/";
           string DBPath = @"newofflinesync.db";
           var a= typeof(TestTable).Assembly.FullName;

            SQLiteConnection conn = new SQLiteConnection(DBPath);
            Console.WriteLine("enter what operation to be performed ");
            Console.WriteLine("1. create table 2. insert 3.update 4. delete 5.Show Records");
            int choice = Int32.Parse(Console.ReadLine());
            conn.CreateTable<SyncSettingsModel>();
            switch (choice)
            {
                case 1:
                    conn.CreateTable<TestTable>();

                    Console.WriteLine("enter the priority");
                    Console.WriteLine(" 1. last modified 2 . client 3. server 4.  user ");
                    int priority_choice = Int32.Parse(Console.ReadLine());
                    OveridePriority x = OveridePriority.LastUpdated;
                    if (priority_choice == 0)
                    {
                        x = OveridePriority.LastUpdated;
                    }
                    else if (priority_choice == 1)
                    {
                        x = OveridePriority.Server;
                    }
                    else if (priority_choice == 2)
                    {
                        x = OveridePriority.Client;
                    }
                    else if (priority_choice == 3)
                    {
                        x = OveridePriority.User;
                    }
                    conn.Insert(new SyncSettingsModel()
                    {
                        ClientTableName = "TestTable",

                        ServerAssemblyName = "ServerApp.API",
                        ControllerName = "Test",
                        AutoSync = true,
                        ControllerData = "TestTable",
                        CreatedAt = DateTime.UtcNow,
                        ModifiedAt = DateTime.UtcNow,
                        LastSyncedAt = null,
                        ServerTableName = "Testtbl",

                        Priority = x,
                    });


                    break;
                case 2:
                    Console.WriteLine("enter the name");
                    string name = Console.ReadLine();
                    conn.Insert(new TestTable()
                    {
                        VersionID = Guid.NewGuid().ToString(),
                        CreatedAt = DateTime.Now,
                        IsDeleted = false,
                        ModifiedAt = DateTime.Now,
                        Name = name
                    });

                    break;

                case 3:
                    List<TestTable> data = program.ShowRecords(DBPath);
                    Console.WriteLine("Enter Which record needs to be updated");
                    int key = Int32.Parse(Console.ReadLine());
                    TestTable testtable = new TestTable();
                    testtable.VersionID = data[key - 1].VersionID;
                    testtable.CreatedAt = data[key - 1].CreatedAt;
                    testtable.ModifiedAt = DateTime.UtcNow;
                    Console.WriteLine("Enter Name");
                    testtable.Name = Console.ReadLine();
                    testtable.IsDeleted = data[key - 1].IsDeleted;
                    using (SQLiteConnection context = new SQLiteConnection(DBPath))
                    {
                        context.Update(testtable);
                    }
                    program.ShowRecords(DBPath);

                    break;

                case 4:
                    List<TestTable> data1 = program.ShowRecords(DBPath);
                    Console.WriteLine("Enter Which record needs to be updated");
                    int key1 = Int32.Parse(Console.ReadLine());
                    TestTable testtable1 = new TestTable();
                    testtable1.VersionID = data1[key1 - 1].VersionID;
                    testtable1.CreatedAt = data1[key1 - 1].CreatedAt;
                    testtable1.ModifiedAt = DateTime.UtcNow;
                    testtable1.Name = data1[key1 - 1].Name;
                    testtable1.IsDeleted = true;

                    using (SQLiteConnection context = new SQLiteConnection(DBPath))
                    {
                        context.Update(testtable1);
                    }
                    program.ShowRecords(DBPath);

                    break;

                case 5:
                    program.ShowRecords(DBPath);
                    break;

                default:
                    Console.WriteLine("Pls enter a valid integer");
                    break;
            }


            Sync<TestTable> sync = new Sync<TestTable>(DBPath, url, null, DBType.SQLite);
            // call a method which can get all foreign key relationships 

            sync.StartSyncAsync();

            while (true)
            {
                Thread.Sleep(1000);
            }
        }
    }
}