# C# Bitwarden-Vault-CLI-API

The offical Bitwarden API is only for organisational management.  If you want to manage your Bitwarden Vault there is the BitWarden CLI.

Via the CLI, you do have the ability to call Resful API's from HTTP using the `serve` command but I wanted to experiment with wrapping the CLI interface with a C# class.

This project is a simple C# wrapper that integrates with the Bitwarden CLI (Command Line Interface) and makes it available as an API (App Programming Interface) accessible via C# code.

The Documentation on the Bitwarden CLI is available here: https://bitwarden.com/help/cli/

The Bitwarden `bw.exe` CLI needs to be in the same diretcory as the .exe.  You can download the BitWarden CLI from the Bitwarden website

This proof of concept API has only been tested on Windows

Example C# Code:

    static void TestBitwardenCLI()
    {
        using (var bitwarden = new BitwardenCLI("username@gmail.com", "password"))
        {
            // Get the vault status
            var status = bitwarden.Status();
            
            // Get a list of the oragnisations
            var orgs = bitwarden.ListOrganisations();
            
            // Get a list of all the available collections
            var colls = bitwarden.ListCollections();
        
            // Get a list of all the items in the vault
            var vaultItems = bitwarden.ListItems();

            // Create a new secure note
            var newNote = bitwarden.CreateSecureNote("org-guid", "collection-guid", "my test secure note", "some text here");

            // Create a new login for a website
            var newLogin = bitwarden.CreateLogin("org-guid", "collection-guid", "My test Login", "user", "pass", "https://127.0.0.1");

            // Get a list of items that contain the word "test"
            var testItems = bitwarden.ListItems(searchPattern: "test");
        
            // Get a list of items that contain the word "test" in a specific collection
            var testItemsInColl = bitwarden.ListItems(searchPattern: "test", collectionId: "collection-guid");
            
            // Get Item by Name
            var vaultItem1 = bitwarden.GetItem("My test Login");

            // Get Item by Id
            var vaultItem2 = bitwarden.GetItem("f6184129-6cf5-4a61-8904-318e821a7615");

            // Edit existing item in the vault
            vaultItem2.item.notes = "some extra notes";
            bitwarden.EditItem(vaultItem2.item);
            
            // Create an attachement
            bitwarden.CreateAttachment("f6184129-6cf5-4a61-8904-318e821a7615", @"C:\Files\SomeFile.txt");
            
            // Download an attachement
            bitwarden.DownloadAttachment("f6184129-6cf5-4a61-8904-318e821a7615", "SomeFile.txt" );
            
            // Delete all test items from the vault
            foreach (var item in testItemsInColl)
            {
                bitwarden.DeleteItem(item.id);
            }

        } // LogOut()  is called automatically when you exit this using statement.
    }
