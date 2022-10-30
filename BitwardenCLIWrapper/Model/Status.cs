using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitwardenVaultCLI_API.Model
{
    public class Status
    {
        public object serverUrl { get; set; }
        public DateTime lastSync { get; set; }
        public string userEmail { get; set; }
        public string userId { get; set; }
        public string status { get; set; }
    }
}
