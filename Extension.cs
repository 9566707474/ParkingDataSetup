namespace ParkingDataSetup
{
    using System.Text.Json;

    public static class Extension
    {
        public static T ToDeserialize<T>(this string json)
        {
            return JsonSerializer.Deserialize<T>(json);
        }

        public static string ToJson<T>(this T obj)
        {
            return JsonSerializer.Serialize(obj);
        }
    }
}
