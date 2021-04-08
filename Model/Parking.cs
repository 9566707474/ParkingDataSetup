namespace ParkingDataSetup.Model
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;

    public class Parking
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("profileImage")]
        public string ProfileImage { get; set; }

        [JsonProperty("availableSlots")]
        public int AvailableSlots { get; set; }

        [JsonProperty("location")]
        public Location Location { get; set; }

        [JsonProperty("statesetId")]
        public string StateSetID { get; set; }

        [JsonProperty("tilesetID")]
        public string TilesetID { get; set; }

      
        [JsonProperty("blueprint")]
        public string Blueprint { get; set; }
    }

    public class Root
    {
        [JsonProperty("parkings")]
        public List<Parking> Parkings { get; set; }
    }
}
