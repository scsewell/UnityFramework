using Newtonsoft.Json;

namespace Framework.IO
{
    public static class JsonConverter
    {
        public static T SerializedCopy<T>(T obj)
        {
            return FromJson<T>(ToJson(obj));
        }

        public static string ToJson<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented, GetSerializerSettings());
        }

        public static T FromJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, GetSerializerSettings());
        }

        private static JsonSerializerSettings GetSerializerSettings()
        {
            JsonSerializerSettings jss = new JsonSerializerSettings();
            jss.ContractResolver = new PrivateContractResolver();
            jss.TypeNameHandling = TypeNameHandling.Auto;
            return jss;
        }
    }
}
