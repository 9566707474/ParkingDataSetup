namespace ParkingDataSetup
{
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos;
    using ParkingDataSetup.Model;

    public class CosmosDbService
    {
        private Container _container;

        public CosmosDbService(
            CosmosClient dbClient,
            string databaseName,
            string containerName)
        {
            this._container = dbClient.GetContainer(databaseName, containerName);
        }

        public async Task AddItemAsync(Parking parking)
        {
            await this._container.CreateItemAsync(parking, new PartitionKey(parking.City));
        }

        public async Task DeleteItemAsync(string id, string partitionKey)
        {
            await this._container.DeleteItemAsync<Parking>(id, new PartitionKey(partitionKey));
        }

        public async Task<Parking> GetItemAsync(string id, string partitionKey)
        {
            try
            {
                ItemResponse<Parking> response = await this._container.ReadItemAsync<Parking>(id, new PartitionKey(partitionKey));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

        }

        public async Task UpdateItemAsync(Parking parking)
        {
            await this._container.UpsertItemAsync(parking, new PartitionKey(parking.City));
        }
    }
}

