using System.Collections.Generic;
using Newtonsoft.Json;

namespace ParkingDataSetup.Model
{
    public class Location
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("coordinates")]
        public List<double> Coordinates { get; set; }
    }
}
