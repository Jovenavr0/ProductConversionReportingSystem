using System.Text;
using Confluent.Kafka;
using Newtonsoft.Json;

namespace Infrastructure.Messaging.Serializers;

public class JsonDeserializer<T> : IDeserializer<T>
{
    public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(data))!;
    }
}