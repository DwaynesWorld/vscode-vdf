using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace VDFServer.Parser.Services
{
    public class VDFServerSerializer : IVDFServerSerializer
    {
        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public VDFServerSerializer()
        {
        }

        public string Serialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj, _serializerSettings);
        }

        public T Deserialize<T>(string obj)
        {
            return JsonConvert.DeserializeObject<T>(obj, _serializerSettings);
        }
    }
}