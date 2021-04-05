using System;

namespace ParkingDataSetup.Model
{
    public class Parking
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string ProfileImage { get; set; }

        public int AvailableSlots { get; set; }

        public Location Location { get; set; }

        public Guid DatasetID { get; set; }

        public Guid TilesetID { get; set; }

        public string Blueprint { get; set; }
    }
}
