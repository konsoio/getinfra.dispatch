using System.Text;
using System.Text.Json;

namespace Getinfra.Dispatch.Abstractions.Serializers
{
    public class DefaultJsonSerializer : IJsonSerializer
    {
        private JsonSerializerOptions _serializationSettings;

        public DefaultJsonSerializer()
        {
            _serializationSettings = new JsonSerializerOptions() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        }

        public string Serialize(object value)
        {
            return JsonSerializer.Serialize(value, _serializationSettings);
        }

        public T Deserialize<T>(string value)
        {
            return JsonSerializer.Deserialize<T>(value, _serializationSettings);
        }

        public T Deserialize<T>(byte[] data)
        {
            var str = Encoding.UTF8.GetString(data);
            return JsonSerializer.Deserialize<T>(str, _serializationSettings);
        }
    }
}
