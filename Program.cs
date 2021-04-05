namespace ParkingDataSetup
{
    using ParkingDataSetup.Model;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    public class Program
    {
        public static void Main(string[] args)
        {
            var filePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Data\\ParkingSampleData.json";
            var parkingData = File.ReadAllText(filePath);
            var parking = parkingData.DeserializeTo<Root>();

            ////TODO: Data reformating 




        }

    }
}
