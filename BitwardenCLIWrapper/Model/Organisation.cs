using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitwardenVaultCLI_API.Model
{
    public class Organisation
    {
        public string @object { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public int status { get; set; }
        public int type { get; set; }
        public bool enabled { get; set; }
    }
}
