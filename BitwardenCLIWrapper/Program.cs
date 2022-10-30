using BitwardenVaultCLI_API.Controller;

Console.WriteLine("Testing BitwardenCLI API");

Console.WriteLine("This integrates the Bitwarden CLI (Command Line Interface) and makes it available as an API (App Programming Interface) accessible via C#");

TestBitwardenCLI();


static void TestBitwardenCLI()
{
    using (var bitwarden = new BitwardenCLI("username@gmail.com", "password"))
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

        foreach (var item in items33)
        {
            bitwarden.DeleteItem(item.id);
        }

    } // LogOut()  is called automatically when you exit this using statement.
}