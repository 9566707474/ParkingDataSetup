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
    using ParkingDataSetup.DigitalTwins;
    using ParkingDataSetup.Model;

    public class Program
    {
        private static readonly IConfiguration configuration;

        static Program()
        {
            configuration = InitConfig();
        }

        public static void Main(string[] args)
        {
            var filePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Data\\ParkingSampleData.json";

            Log.Ok($"Input file path : " + filePath);

            var parkingData = File.ReadAllText(filePath);
            var parking = parkingData.DeserializeTo<Root>();

            filePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Data\\ParkingBaySampleData.json";

            Log.Ok($"Input file path : " + filePath);

            var parkingBayData = File.ReadAllText(filePath);
            var parkingBayRoot = parkingBayData.DeserializeTo<ParkingBayRoot>();

            Log.Ok($"File reading completed");

            //TODO: Data reformating 

            var indoorMapCreator = new List<IndoorMapCreator>();
            var cosmosDb = Task.Run(async () => await InitializeCosmosClientInstanceAsync())?.Result;

            var twinInitialise = Task.Run(async () => await new DigitalTwinCreator(configuration).InitialiseTwins());
            twinInitialise.Wait();

              foreach (var item in parking.Parkings)
                {
                    var parkingResponse = Task.Run(async () => await cosmosDb.GetItemAsync(item.Id, item.City))?.Result;

                    var isMapAlreadyGenerated = false;

                    if (parkingResponse != null && !string.IsNullOrEmpty(parkingResponse.StateSetID) && !string.IsNullOrEmpty(parkingResponse.TilesetID))
                    {
                        isMapAlreadyGenerated = true;
                    }

                    new DigitalTwinCreator(configuration, item, parkingBayRoot.ParkingBays).CreateTwins();

                    indoorMapCreator.Add(new IndoorMapCreator(configuration)
                    {
                        Parking = item,
                        IsMapAlreadyGenerated = isMapAlreadyGenerated
                    }); ;

                }

#if (DEBUG)
                var parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = 1 };
#else
                        var parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount };
#endif

                Parallel.ForEach(indoorMapCreator.Where(imc => !imc.IsMapAlreadyGenerated), parallelOptions, p =>
                   {
                       p.Run();
                   });

                var filteredRecords = indoorMapCreator.Where(e => !e.IsError).Select(p => p.Parking).ToList();

                var result = Task.Run(async () => await PushToDatabase(filteredRecords))?.Result;

                Log.Ok($"Data transfered to cosmos db");

                Console.ReadLine();
            
        }

        private static IConfiguration InitConfig()
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
            string databaseName = configuration["CosmosDb:DatabaseName"];
            string parkingSlotContainerName= configuration["CosmosDb:ParkingSlotsContainerName"];
            string bookingContainerName = configuration["CosmosDb:BookingContainerName"];
            string account = configuration["CosmosDb:Account"];
            string key = configuration["CosmosDb:Key"];

            var client = new CosmosClient(account, key);
            var cosmosDbService = new CosmosDbService(client, databaseName, parkingSlotContainerName);
            var database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
            await database.Database.CreateContainerIfNotExistsAsync(parkingSlotContainerName, "/city");
            await database.Database.CreateContainerIfNotExistsAsync(bookingContainerName, "/locationId");
            return cosmosDbService;
        }
    }
}
