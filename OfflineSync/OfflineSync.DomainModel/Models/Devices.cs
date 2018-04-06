using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfflineSync.DomainModel.Models
{
    public class Devices
    {
        string DeviceId { get; set; }
        string DeviceName { get; set; }
        bool IsConnected { get; set; }
    }
}
