using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitwardenVaultCLI_API.Model
{
    public class NewItem
    {
        public List<string> collectionIds { get; set; }

        public string organizationId { get; set; }
        public string collectionId { get; set; }
        public object folderId { get; set; }
        public int type { get; set; }
        public string name { get; set; }
        public object notes { get; set; }
        public bool favorite { get; set; }
        public List<Field> fields { get; set; }
        public Login login { get; set; }
        public int reprompt { get; set; }
    }
}
