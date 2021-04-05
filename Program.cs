namespace ParkingDataSetup
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Extensions.Configuration;
    using ParkingDataSetup.Model;

    public class Program
    {
        private static readonly IConfigurationRoot configurationSection;

        static Program()
        {
            configurationSection = InitConfig();
        }

        public static void Main(string[] args)
        {
            var filePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Data\\ParkingSampleData.json";
            var parkingData = File.ReadAllText(filePath);
            var parking = parkingData.DeserializeTo<Root>();

            ////TODO: Data reformating 

            var result = Task.Run(async () => await PushToDatabase(parking.Parkings))?.Result;

            Console.WriteLine(result);
            Console.ReadLine();
        }

        private static IConfigurationRoot InitConfig()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", true, true)
                .AddEnvironmentVariables();

            return builder.Build();
        }

        private static async Task<bool> PushToDatabase(ICollection<Parking> parkingList)
        {
            var cosmosDb = await InitializeCosmosClientInstanceAsync();
            foreach (var item in parkingList)
            {
                var isExist = await cosmosDb.GetItemAsync(item.Id, item.City);
                if (string.IsNullOrEmpty(isExist?.Id))
                {
                    await cosmosDb.AddItemAsync(item);
                }
                else
                {
                    await cosmosDb.UpdateItemAsync(item);
                }
            }

            return true;
        }


        /// <summary>
        /// Creates a Cosmos DB database and a container with the specified partition key. 
        /// </summary>
        /// <returns></returns>
        private static async Task<CosmosDbService> InitializeCosmosClientInstanceAsync()
        {
            string databaseName = configurationSection.GetSection("CosmosDb:DatabaseName").Value;
            string containerName = configurationSection.GetSection("CosmosDb:ContainerName").Value;
            string account = configurationSection.GetSection("CosmosDb:Account").Value;
            string key = configurationSection.GetSection("CosmosDb:Key").Value;

            var client = new CosmosClient(account, key);
            var cosmosDbService = new CosmosDbService(client, databaseName, containerName);
            var database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
            await database.Database.CreateContainerIfNotExistsAsync(containerName, "/city");

            return cosmosDbService;
        }
    }
}
