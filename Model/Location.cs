using System.Collections.Generic;

namespace ParkingDataSetup.Model
{
    public class Location
    {
        public string Type { get; set; }

        public List<double> Coordinates { get; set; }
    }
}
