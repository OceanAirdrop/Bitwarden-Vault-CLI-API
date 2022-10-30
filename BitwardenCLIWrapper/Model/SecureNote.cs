using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitwardenVaultCLI_API.Model
{
    public class SecureNote
    {
        public List<string> collectionIds { get; set; }
        public object organizationId { get; set; }
        public object collectionId { get; set; }
        public string folderId { get; set; }
        public int type { get; set; }
        public string name { get; set; }
        public string notes { get; set; }
        public bool favorite { get; set; }
        public List<object> fields { get; set; }
        public SecureNoteType secureNote { get; set; }
        public int reprompt { get; set; }

        public SecureNote()
        {
            secureNote = new SecureNoteType();
            secureNote.type = 0;
        }
    }

    public class SecureNoteType
    {
        public int type { get; set; }
    }
}
