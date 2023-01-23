using BitwardenVaultCLI_API.Controller;

Console.WriteLine("Testing BitwardenCLI API");

Console.WriteLine("This integrates the Bitwarden CLI (Command Line Interface) and makes it available as an API (App Programming Interface) accessible via C#");

TestBitwardenCLI();


static void TestBitwardenCLI()
{
    string client_id = "user.YourClientID";       //see https://bitwarden.com/help/public-api/
    string client_secret = "YourClientSecret";    //see https://bitwarden.com/help/public-api/
    string email = "yourmail@yourdomain.com";
    string password = "Sup3rS3cr3tP@ssw0rd";

    //login with email
    //var bitwarden = new BitwardenCLI(email, password); 

    //login with client_id and client_secret
    var bitwarden = new BitwardenCLI(client_id, client_secret, password);


    {
        var status  = bitwarden.Status();
        var orgs    = bitwarden.ListOrganisations();
        var colls   = bitwarden.ListCollections();
        var items   = bitwarden.ListItems();

        // Create a 
        var newNote = bitwarden.CreateSecureNote("org-guid", "collection-guid", "my test secure note", "some text here");

        var newLogin = bitwarden.CreateLogin("org-guid", "collection-guid", "My test Login", "user", "pass", "https://127.0.0.1");

        var items3 = bitwarden.ListItems(searchPattern: "test");
        var items33 = bitwarden.ListItems(searchPattern: "test", collectionId: "collection-guid");

        var vaultItem1 = bitwarden.GetItem("f6184129-6cf5-4a61-8904-318e821a7615");
        var vaultItem2 = bitwarden.GetItem("My test Login");
        
        // Edit item
        vaultItem2.item.notes = "some extra notes";
        bitwarden.EditItem(vaultItem2.item);
        
        foreach (var item in items33)
        {
            bitwarden.DeleteItem(item.id);
        }

    } // LogOut()  is called automatically when you exit this using statement.
}
