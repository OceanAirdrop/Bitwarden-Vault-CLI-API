using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitwardenVaultCLI_API.Model
{
    [DebuggerDisplay("Name = {name}")]
    public class Collection
    {
        public string @object { get; set; }
        public string id { get; set; }
        public string organizationId { get; set; }
        public string name { get; set; }
        public string externalId { get; set; }
    }
}
