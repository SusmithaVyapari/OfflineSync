using System.Collections.Generic;
using OfflineSync.DomainModel.Models;
using OfflineSyncClient.Models;
using System;

namespace OfflineSyncClient.DB
{
    public interface IDBOperations
    {
        List<T> GetData<T>(DateTime? lastsync) where T : ISyncBaseModel, new();
        List<T> GetFailedTransactionData<T>() where T : ISyncBaseModel, new();
        List<SyncSettingsModel> GetSyncSettingByTable(string tableName);
    }
}