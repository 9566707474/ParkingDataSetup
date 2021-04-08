namespace ParkingDataSetup.Model
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public class ParkingBay
    {
        [JsonProperty("locationId")]
        public string LocationId { get; set; }

        [JsonProperty("parkingSlots")]
        public List<string> ParkingSlots { get; set; }

        [JsonProperty("allowDigitalTwinCreation")]
        public bool AllowDigitalTwinCreation { get; set; }

    }

    public class ParkingBayRoot
    {
        [JsonProperty("parkingBays")]
        public List<ParkingBay> ParkingBays { get; set; }
    }
}
