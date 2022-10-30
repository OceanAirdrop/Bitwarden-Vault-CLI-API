using BitwardenVaultCLI_API.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitwardenVaultCLI_API.Controller
{
    // This class hosts the Create a Worker Service that  hosts bit warden CLI
    // https://bitwarden.com/help/cli/

    public class BitwardenCLI : IDisposable
    {
        private string m_session = "";

        public BitwardenCLI(string userName, string password)
        {
            m_session = LogIn(userName, password);
        }

        public string LogIn(string userName, string password)
        {
            // sanity logout!
            LogOut();

            var result = IssueBitWardenCommand($"login {userName} {password} --raw");

            if (!result.StartsWith("You are already"))
            {
                // remove the \r\n
                m_session = result.Replace("\r\n", string.Empty);
            }

            return result;
        }

        public string LogOut()
        {
            var result = IssueBitWardenCommand($"logout");
            return result;
        }

        public Status Status()
        {
            var json = IssueBitWardenCommand($"status");

            var status = JsonConvert.DeserializeObject<Status>(json);

            return status;
        }

        public List<Item> ListItems()
        {
            var cmd = $"list items --session \"{m_session}\"";

            var json = IssueBitWardenCommand(cmd);

            var itemList = JsonConvert.DeserializeObject<List<Item>>(json);

            return itemList;

        }

        public List<Item> ListItems(string searchPattern, string folderId = "", string collectionId = "")
        {
            var cmd = new StringBuilder();

            cmd.Append("list items");

            // search in a particular folder
            if (!string.IsNullOrEmpty(folderId))
                cmd.Append($" --folderid {folderId}");

            // search in a particular collection
            if (!string.IsNullOrEmpty(collectionId))
                cmd.Append($" --collectionid  {collectionId}");

            cmd.Append($" --session \"{m_session}\"");

            var json = IssueBitWardenCommand(cmd.ToString());

            var itemList = JsonConvert.DeserializeObject<List<Item>>(json);

            return itemList;

        }

        public List<Item> ListItems(string searchPattern)
        {
            var cmd = $"list items --session \"{m_session}\"";

            if (!string.IsNullOrEmpty(searchPattern))
                cmd = $"list items --search \"{searchPattern}\" --session \"{m_session}\"";

            var json = IssueBitWardenCommand(cmd);

            var itemList = JsonConvert.DeserializeObject<List<Item>>(json);

            return itemList;

        }

        public List<Organisation> ListOrganisations()
        {
            var json = IssueBitWardenCommand($"list organizations --session \"{m_session}\"");

            var itemList = JsonConvert.DeserializeObject<List<Organisation>>(json);

            return itemList;

        }

        public List<Collection> ListCollections()
        {
            var json = IssueBitWardenCommand($"list collections --session \"{m_session}\"");

            var itemList = JsonConvert.DeserializeObject<List<Collection>>(json);

            return itemList;

        }

        public void DeleteItem(string itemGuid, string orgId = "")
        {
            var cmd = $"delete item {itemGuid} --session \"{m_session}\"";

            if (!string.IsNullOrEmpty(orgId))
                cmd = $"delete item {itemGuid} --organizationid {orgId} --session \"{m_session}\"";

            var result = IssueBitWardenCommand(cmd);
        }


        public Item CreateLogin(string orgId, string collectionId, string itemname, string username, string password, string uri, string notes = "some notes here")
        {
            // JSON template from here: https://bitwarden.com/help/vault-management-api/

            var item = new NewItem();
            item.type = (int)ItemType.Login;
            item.organizationId = orgId;
            item.collectionId = collectionId;

            item.collectionIds = new List<string>();
            item.collectionIds.Add(collectionId);

            item.name = itemname;

            item.login = new Login();
            item.login.username = username;
            item.login.password = password;
            item.login.uris = new List<Model.Uri>();
            item.login.uris.Add(new Model.Uri { uri = uri });

            item.fields = new List<Field>();
            item.fields.Add(new Field { name = "Security Question", value = "Bitwarden Rules", type = (int)FieldType.Text });

            item.notes = notes;

            var json = JsonConvert.SerializeObject(item);

            var encodedJson = Base64Encode(json);

            var cmd2 = $"echo {encodedJson} | \"{GetAppLocation()}\\bw.exe\" create item  --session \"{m_session}\"";

            var newItemJson = IssueCmdCommand(cmd2);

            var newItem = JsonConvert.DeserializeObject<Item>(newItemJson);

            return newItem;

        }

        public SecureNote CreateSecureNote(string orgId, string collectionId, string itemname, string noteText)
        {
            // JSON template from here: https://bitwarden.com/help/vault-management-api/

            var item = new SecureNote();
            item.type = (int)ItemType.SecureNote;
            item.organizationId = orgId;
            item.collectionId = collectionId;
            item.collectionIds = new List<string>();
            item.collectionIds.Add(collectionId);
            item.name = itemname;
            item.notes = noteText;

            var json = JsonConvert.SerializeObject(item);
            var encodedJson = Base64Encode(json);

            var cmd2 = $"echo {encodedJson} | \"{GetAppLocation()}\\bw.exe\" create item  --session \"{m_session}\"";

            var newItemJson = IssueCmdCommand(cmd2);

            var newItem = JsonConvert.DeserializeObject<SecureNote>(newItemJson);
            return newItem;

        }

        public string IssueBitWardenCommand(string cmd)
        {

            var output = new StringBuilder();
            var error = new StringBuilder();

            var p = new Process();
            p.StartInfo.FileName = $"{GetAppLocation()}\\bw.exe";
            p.StartInfo.Arguments = $"{cmd}";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.ErrorDialog = false;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardInput = false;

            p.ErrorDataReceived += (sender, eargs) =>
            {
                if (eargs.Data != null)
                    error.AppendLine(eargs.Data);
            };
            p.OutputDataReceived += (sender, eargs) =>
            {
                if (eargs.Data != null)
                    output.AppendLine(eargs.Data);
            };

            p.Start();
            p.BeginErrorReadLine();
            p.BeginOutputReadLine();
            p.WaitForExit();

            var errorResponse = error.ToString();
            if (!string.IsNullOrWhiteSpace(errorResponse))
            {
                return errorResponse;
            }

            return output.ToString();
        }

        public string IssueCmdCommand(string cmd)
        {

            var output = new StringBuilder();
            var error = new StringBuilder();

            var p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = $"/c {cmd}";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.ErrorDialog = false;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardInput = false;

            p.ErrorDataReceived += (sender, eargs) =>
            {
                if (eargs.Data != null)
                    error.AppendLine(eargs.Data);
            };
            p.OutputDataReceived += (sender, eargs) =>
            {
                if (eargs.Data != null)
                    output.AppendLine(eargs.Data);
            };

            p.Start();
            p.BeginErrorReadLine();
            p.BeginOutputReadLine();
            p.WaitForExit();

            var errorResponse = error.ToString();
            if (!string.IsNullOrWhiteSpace(errorResponse))
            {
                return errorResponse;
            }

            return output.ToString();
        }

        private string GetAppLocation()
        {
            string runningFrom = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            return runningFrom;
        }

        public string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public void Dispose()
        {
            LogOut();
        }
    }
}
