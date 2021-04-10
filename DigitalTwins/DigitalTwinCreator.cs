namespace ParkingDataSetup.DigitalTwins
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Web;
    using Azure.DigitalTwins.Core;
    using Azure.Identity;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json.Linq;
    using ParkingDataSetup.Model;

    public class DigitalTwinCreator
    {

        private readonly IConfiguration configuration;
        private readonly Parking parking;
        private readonly IList<ParkingBay> parkingBays;
        public DigitalTwinCreator(IConfiguration configuration, Parking parking, IList<ParkingBay> parkingBays)
        {           
            this.parking = parking;
            this.configuration = configuration;
            this.parkingBays = parkingBays;

        }

        public async void Create()
        {
            try
            {
                var parkingBay = this.parkingBays.SingleOrDefault(pb => pb.LocationId == this.parking.Id);
                if (parkingBay.AllowDigitalTwinCreation)
                {
                    Log.Ok("Authenticating...");
                    var credential = new DefaultAzureCredential();
                    var client = new DigitalTwinsClient(new Uri(this.configuration["DigitalTwinInstanceUrl"]), credential);

                    Log.Ok($"Service client created – ready to go");

                    var digitalTwinsHelper = new CommandLoop(client);

                    await digitalTwinsHelper.CommandDeleteAllTwins(new string[0]);
                    await digitalTwinsHelper.CommandDeleteAllModels(new string[0]);

                    string[] modelsToUpload = new string[3] { "CreateModels", "CarParkModel", "ParkingBayModel" };

                    await digitalTwinsHelper.CommandCreateModels(modelsToUpload);

                    Log.Out($"Creating CarPark and ParkingBay...");
                    await digitalTwinsHelper.CommandCreateDigitalTwin(new string[12]
                        {
                        "CreateTwin", "dtmi:com:model:CarPark;1", this.parking.Id,
                        "Name", "string", this.parking.Name,
                        "Address", "string", this.parking.Address,
                        "City", "string", this.parking.City
                        });

                    var parkingSlots = parkingBay.ParkingSlots;
                    foreach (string parkingSlot in parkingSlots)
                    {
                        var parkingBayTwinName = string.Join("_", this.parking.Id, parkingSlot);
                        var parkingBayRelation = $"rel_{parkingBayTwinName}";
                        await digitalTwinsHelper.CommandCreateDigitalTwin(new string[9]
                          {
                        "CreateTwin", "dtmi:com:model:ParkingBay;1", parkingBayTwinName,
                        "IsOccupied", "boolean", "false",
                        "IsAlertRaised", "boolean", "false"
                          });

                        Log.Out($"Creating relationship between CarPark and Parking Bay");
                        await digitalTwinsHelper.CommandCreateRelationship(new string[5]
                            {
                       "CreateRelation",  this.parking.Id, "Contains", parkingBayTwinName, parkingBayRelation
                            });
                    }


                    Log.Ok("Digital Twin creation successful for : " + this.parking.Id);
                }
            }
            catch (Exception ex)
            {               
                var errorMessage = string.Format("Digital Twin creation failed : {0}; {1}", this.parking.Id, ex);
                Log.Error(errorMessage);
            }
        }


    }
}