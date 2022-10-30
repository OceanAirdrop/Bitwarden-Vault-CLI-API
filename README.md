# Bitwarden-Vault-CLI-API

A simple C# wrapper that integrates with the Bitwarden CLI (Command Line Interface) and makes it available as an API (App Programming Interface) accessible via C#.

The Documentation on the Bitwarden CLI is available here: https://bitwarden.com/help/cli/

The Bitwarden CLI meeds to be in the same diretcory as the .exe

This proof of concept API has only been tested on Windows

Example C# Code:

    static void TestBitwardenCLI()
    {
        using (var bitwarden = new BitwardenCLI("username@gmail.com", "password"))
        {
            // Get the vault status
            var status  = bitwarden.Status();
            
            // Get a list of the oragnisations
            var orgs    = bitwarden.ListOrganisations();
            
            // Get a list of all the available collections
            var colls   = bitwarden.ListCollections();
        
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

            // Delete all test items from the vault
            foreach (var item in testItemsInColl)
            {
                bitwarden.DeleteItem(item.id);
            }

        } // LogOut()  is called automatically when you exit this using statement.
    }
