using System.Text;
using MCCS.Infrastructure.Communication.NamedPipe.Models;
using Newtonsoft.Json;

namespace MCCS.Infrastructure.Communication.NamedPipe.Serialization;

/// <summary>
/// JSON发布-订阅消息序列化器实现
/// </summary>
public sealed class JsonPubSubMessageSerializer(JsonSerializerSettings settings) : IPubSubMessageSerializer
{
    private static readonly JsonSerializerSettings _defaultSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        DateTimeZoneHandling = DateTimeZoneHandling.Utc
    };

    public JsonPubSubMessageSerializer() : this(_defaultSettings)
    {
    }

    public byte[] SerializeEnvelope(PipeMessageEnvelope envelope)
    {
        var json = JsonConvert.SerializeObject(envelope, settings);
        return Encoding.UTF8.GetBytes(json);
    }

    public PipeMessageEnvelope DeserializeEnvelope(byte[] data)
    {
        var json = Encoding.UTF8.GetString(data);
        return JsonConvert.DeserializeObject<PipeMessageEnvelope>(json, settings)
               ?? throw new InvalidOperationException("Failed to deserialize message envelope");
    }

    public string Serialize<T>(T obj) => JsonConvert.SerializeObject(obj, settings);

    public string SerializeObject(object obj) => JsonConvert.SerializeObject(obj, settings);

    public T? Deserialize<T>(string data) => JsonConvert.DeserializeObject<T>(data, settings);
}
