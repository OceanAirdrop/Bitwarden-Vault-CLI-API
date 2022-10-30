using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitwardenVaultCLI_API.Model
{
    // https://bitwarden.com/help/cli/#global-options

    public enum ItemType
    {
        Login = 1,
        SecureNote = 2,
        Card = 3,
        Identity = 4
    }

    public enum FieldType
    {
        Text = 0,
        Hidden = 1,
        Boolean = 2
    }
}
