namespace ParkingDataSetup
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
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

            Console.WriteLine($"Input file path : " + filePath);

            var parkingData = File.ReadAllText(filePath);
            var parking = parkingData.DeserializeTo<Root>();

            Console.WriteLine($"File reading completed");

            ////TODO: Data reformating 

            var dwgZipUploaders = new List<DWGZipUploader>();

            foreach (var item in parking.Parkings)
            {
                dwgZipUploaders.Add(new DWGZipUploader()
                {
                    Parking = item
                });
            }

#if (DEBUG)
            var parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = 1 };
#else
            var parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount };
#endif

            Parallel.ForEach(dwgZipUploaders, parallelOptions, p =>
               {
                   p.Run();
               });

            var filteredRecords = dwgZipUploaders.Where(e => e.IsError == true).Select(p => p.Parking).ToList();

            var result = Task.Run(async () => await PushToDatabase(filteredRecords))?.Result;

            Console.WriteLine($"Data transfered to cosmos db");

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
