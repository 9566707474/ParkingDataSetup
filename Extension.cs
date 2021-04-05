namespace ParkingDataSetup
{
    using Newtonsoft.Json;
    
    public static class Extension
    {
        public static T DeserializeTo<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static string ToJson<T>(this T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}
