namespace ParkingDataSetup
{
    using Newtonsoft.Json;
    using System;

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

        public static Guid ToGuid(this string id)
        {
            if (Guid.TryParse(id, out Guid result))
            {
                return result;
            }

            return result;
        }
    }
}
