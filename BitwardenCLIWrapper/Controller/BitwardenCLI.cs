using BitwardenVaultCLI_API.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitwardenVaultCLI_API.Controller
{
    // https://bitwarden.com/help/cli/

    public class BitwardenCLI : IDisposable
    {
        private string m_session = "";

        public BitwardenCLI(string userName, string password)
        {
            m_session = LogIn(userName, password);
        }
        
        public BitwardenCLI(string clientId, string clientSecret, string password)
        {
            m_session = LogInUsingApi(clientId, clientSecret, password);
        }

        public string LogIn(string userName, string password)
        {
            EnsureBwExeExists();
            LogOut(); // sanity logout!

            var result = IssueBitWardenCommand($"login {userName} {password} --raw");

            if (!result.StartsWith("You are already"))
            {
                // remove the \r\n
                m_session = result.Replace("\r\n", string.Empty);
            }

            return result;
        }
        
        string LogInUsingApi(string clientId, string clientSecret, string password)
        {
            string result = "";
            
            EnsureBwExeExists();
            
            var appLocation = GetAppLocation();

            // Write a batch file to execute
            string batFileName = Path.Combine(GetAppLocation(), $"{Guid.NewGuid()}.bat");
            using (var batFile = new StreamWriter(batFileName))
            {
                batFile.WriteLine($"CD {appLocation}");
                batFile.WriteLine($"set BW_CLIENTID={clientId}");
                batFile.WriteLine($"set BW_CLIENTSECRET={clientSecret}");
                batFile.WriteLine("bw login --apikey");
                batFile.WriteLine($"bw unlock {password} --raw >{Path.Combine(appLocation, "temp.log")} 2>&1");
            }

            // The flag /c tells "cmd" to execute what follows and exit
            var procStartInfo = new ProcessStartInfo("cmd", "/c " + batFileName);
            procStartInfo.UseShellExecute = true;
            procStartInfo.CreateNoWindow = true;
            procStartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            var proc = new Process();
            proc.StartInfo = procStartInfo;
            proc.Start();
            proc.WaitForExit();

            // now read file back to get session key.
            string filePath = Path.Combine(GetAppLocation(), "temp.log");
            result = System.IO.File.ReadAllText(filePath);

            // now cleanup
            File.Delete(filePath);
            File.Delete(batFileName);

            return result;
        }

        void EnsureBwExeExists()
        {
            var filePath = Path.Combine(GetAppLocation(), "bw.exe");

            if (!File.Exists(filePath))
                throw new Exception("bw.exe not found in current directory");
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

        public (bool result, string resultMsg, Item item) GetItem(string id_or_name)
        {
            // If the item cant be found in the vault the msg field explains what happened.

            Item item = null; // return null if cant find 

            // by default assume successful retrieval of item from vault
            var vaultItem = (result: true, msg: "Success", item: item);

            var cmd = $"get item {id_or_name} --session \"{m_session}\"";

            var json = IssueBitWardenCommand(cmd);

            if (ValidateJSON(json))
                vaultItem.item = JsonConvert.DeserializeObject<Item>(json);
            else
            {
                vaultItem.msg = json;
                vaultItem.result = false; 
            }

            return vaultItem;
        }

        public bool EditItem(Item editedItem)
        {
            // This will edit an item in your vault.
            // To use this function you Will first need to GetItem() or ListItems() to get an Item object..
            // Then edit any fields you need and call EditItem

            bool editSuccess = false;

            var json = JsonConvert.SerializeObject(editedItem);

            var encodedJson = Base64Encode(json);

            var cmd = $"echo {encodedJson} | \"{GetAppLocation()}\\bw.exe\" edit item {editedItem.id} --session \"{m_session}\"";

            var result = IssueCmdCommand(cmd);

            // if we get JSON back the edit was successful
            editSuccess = ValidateJSON(json);

            return editSuccess;
        }

        public void CreateAttachment(string itemGuid, string filePath )
        {
            var cmd = $"create attachment --file \"{filePath}\" --itemid {itemGuid} --session \"{m_session}\"";

            var json = IssueBitWardenCommand(cmd);
        }

        public string DownloadAttachment(string itemGuid, string attachementName, string outputPath = "")
        {
            var cmd = $"get attachment \"{attachementName}\" --itemid {itemGuid} --session \"{m_session}\"";

            if (!string.IsNullOrEmpty(outputPath))
                cmd = $"get attachment \"{attachementName}\" --itemid {itemGuid} --output \"{outputPath}\" --session \"{m_session}\"";

            var result = IssueBitWardenCommand(cmd);

            return result;
        }
        
        public void DeleteAttachment(string itemGuid, string attachmentId, string orgId = "")
        {
            var cmd = $"delete attachment {attachmentId} --itemid {itemGuid} --session \"{m_session}\"";

            if (!string.IsNullOrEmpty(orgId))
                cmd = $"delete attachment {attachmentId} --itemid {itemGuid} --organizationid {orgId} --session \"{m_session}\"";

            var result = IssueBitWardenCommand(cmd);
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

        private string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        private bool ValidateJSON(string s)
        {
            try
            {
                JToken.Parse(s);
                return true;
            }
            catch (JsonReaderException ex)
            {
                Trace.WriteLine(ex);
                return false;
            }
        }

        public void Dispose()
        {
            LogOut();
        }
    }
}
