using System;
using System.Collections.Generic;
//using System.Data.SqlClient;  // installed through NuGet..
using System.Text;
using System.Security.Cryptography;
using Azure.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.AlwaysEncrypted.AzureKeyVaultProvider;

namespace ConsoleUseAzureDatabase
{
    public class Program
    {
        static readonly string s_algorithm = "RSA_OAEP";

        //static readonly string s_akvUrl = "https://{KeyVaultName}.vault.azure.net/keys/{Key}/{KeyIdentifier}";
        static readonly string s_akvUrl = "https://samkeyvault1.vault.azure.net/keys/samkey";

        //static readonly string s_connectionString = "Server={Server}; Database={database}; Integrated Security=true; Column Encryption Setting=Enabled;";
        static readonly string s_connectionString = "Server=samklok.database.windows.net; Database=Phones; Integrated Security=true; Column Encryption Setting=Enabled;";


        static void Main(string[] args)
        {
            VaultConnect();

            // DirectConnect()

            Console.WriteLine("\nDone. Press enter.");
            Console.ReadLine();
        }

        // connect to Azure SQL database by using KeyVault passwords
        // described: 
        // https://docs.microsoft.com/en-us/sql/connect/ado-net/sql/azure-key-vault-enclave-example?view=sql-server-ver15
        //
        static void VaultConnect()
        {
            // Initialize Token Credential instance using InteractiveBrowserCredential. For other authentication options,
            // see classes derived from TokenCredential: https://docs.microsoft.com/dotnet/api/azure.core.tokencredential
            var interactiveBrowserCredential = new InteractiveBrowserCredential();

            // Initialize AKV provider
            var akvProvider = new SqlColumnEncryptionAzureKeyVaultProvider(interactiveBrowserCredential);

            // Register AKV provider
            SqlConnection.RegisterColumnEncryptionKeyStoreProviders(
                customProviders: new Dictionary<string, SqlColumnEncryptionKeyStoreProvider>(capacity: 1, comparer: StringComparer.OrdinalIgnoreCase)
                {
                    { SqlColumnEncryptionAzureKeyVaultProvider.ProviderName, akvProvider}
                });

            Console.WriteLine("AKV provider Registered");

            // Create connection to database
            using (var sqlConnection = new SqlConnection(s_connectionString))
            {
                string cmkName = "CMK_WITH_AKV";
                string cekName = "CEK_WITH_AKV";
                string tblName = "AKV_TEST_TABLE";

                //CustomerRecord customer = new CustomerRecord(1, @"Microsoft", @"Corporation");

                try
                {
                    sqlConnection.Open();

                    // Drop Objects if exists
                    //dropObjects(sqlConnection, cmkName, cekName, tblName);

                    //// Create Column Master Key with AKV Url
                    //createCMK(sqlConnection, cmkName, akvProvider);
                    //Console.WriteLine("Column Master Key created.");

                    //// Create Column Encryption Key
                    //createCEK(sqlConnection, cmkName, cekName, akvProvider);
                    //Console.WriteLine("Column Encryption Key created.");

                    //// Create Table with Encrypted Columns
                    //createTbl(sqlConnection, cekName, tblName);
                    //Console.WriteLine("Table created with Encrypted columns.");

                    //// Insert Customer Record in table
                    //insertData(sqlConnection, tblName, customer);
                    //Console.WriteLine("Encryted data inserted.");

                    //// Read data from table
                    //verifyData(sqlConnection, tblName, customer);
                    //Console.WriteLine("Data validated successfully.");
                }
                finally
                {
                    // Drop table and keys
                    //dropObjects(sqlConnection, cmkName, cekName, tblName);
                    //Console.WriteLine("Dropped Table, CEK and CMK");
                }

                Console.WriteLine("Completed AKV provider Sample.");
            }
        }

        // simple use of password 
        static void DirectConnect()
        {
            try
            {
                var builder = new SqlConnectionStringBuilder();

                builder.DataSource = "samklok.database.windows.net";
                builder.UserID = "samklok";
                builder.Password = "******";  // real password
                builder.InitialCatalog = "Phones";

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    Console.WriteLine("\nQuery data example:");
                    Console.WriteLine("=========================================\n");

                    connection.Open();

                    String sql = "SELECT name, collation_name FROM sys.databases";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Console.WriteLine("{0} {1}", reader.GetString(0), reader.GetString(1));
                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
