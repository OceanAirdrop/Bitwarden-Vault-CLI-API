using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitwardenVaultCLI_API.Model
{
    [DebuggerDisplay("Username = {username}")]
    public class Login
    {
        public List<Uri> uris { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public object totp { get; set; }
        public object passwordRevisionDate { get; set; }
    }

    [DebuggerDisplay("Name = {name}")]
    public class Item
    {
        public string @object { get; set; }
        public string id { get; set; }
        public string organizationId { get; set; }
        public object folderId { get; set; }
        public int type { get; set; }
        public int reprompt { get; set; }
        public string name { get; set; }
        public string notes { get; set; }
        public bool favorite { get; set; }
        public Login login { get; set; }
        public List<string> collectionIds { get; set; }
        public DateTime revisionDate { get; set; }
        public object deletedDate { get; set; }
    }

    [DebuggerDisplay("Uri = {uri}")]
    public class Uri
    {
        public object match { get; set; }
        public string uri { get; set; }
    }

    [DebuggerDisplay("Name = {name}")]
    public class Field
    {
        public string name { get; set; }
        public string value { get; set; }
        public int type { get; set; }
    }
}
