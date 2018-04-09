using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfflineSync.DomainModel.Models
{
    public class FailedRecords
    {
        public string SyncID { get; set; }
        public string exceptionmsg { get; set; }
        public bool IsConflictedID { get; set; }
    }
}
