using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfflineSync.DomainModel.Models
{
    public class FailedRecords
    {
        string SyncID { get; set; }
        string exceptionmsg { get; set; }
        bool IsConflictedID { get; set; }
    }
}
