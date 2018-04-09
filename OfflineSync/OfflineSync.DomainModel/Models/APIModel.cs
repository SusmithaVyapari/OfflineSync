using System;
using OfflineSync.DomainModel.Models;
using System.Collections.Generic;

namespace OfflineSync.DomainModel
{
    public class APIModel
    {
        public object FailedTrasationData { get; set; }
        public string[] FailedTransactionID { get; set; }
        public object Data { get; set; }
        public DateTime LastSyncDate { get; set; }
        public string TableName { get; set; }
        public string DeviceID { get; set; }
        public List<FailedRecords> FailedSyncRecords { get; set; }

    }
}