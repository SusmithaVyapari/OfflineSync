using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfflineSync.DomainModel.Models
{
    public class Devices
    {
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public bool IsConnected { get; set; }
    }
}
