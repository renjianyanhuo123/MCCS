using System.Text;
using MCCS.Infrastructure.Communication.NamedPipe.Models;
using Newtonsoft.Json;

namespace MCCS.Infrastructure.Communication.NamedPipe.Serialization;

/// <summary>
/// JSON发布-订阅消息序列化器实现
/// </summary>
public sealed class JsonPubSubMessageSerializer : IPubSubMessageSerializer
{
    private static readonly JsonSerializerSettings DefaultSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        DateTimeZoneHandling = DateTimeZoneHandling.Utc
    };

    private readonly JsonSerializerSettings _settings;

    public JsonPubSubMessageSerializer() : this(DefaultSettings)
    {
    }

    public JsonPubSubMessageSerializer(JsonSerializerSettings settings)
    {
        _settings = settings;
    }

    public byte[] SerializeEnvelope(PipeMessageEnvelope envelope)
    {
        var json = JsonConvert.SerializeObject(envelope, _settings);
        return Encoding.UTF8.GetBytes(json);
    }

    public PipeMessageEnvelope DeserializeEnvelope(byte[] data)
    {
        var json = Encoding.UTF8.GetString(data);
        return JsonConvert.DeserializeObject<PipeMessageEnvelope>(json, _settings)
               ?? throw new InvalidOperationException("Failed to deserialize message envelope");
    }

    public string Serialize<T>(T obj) => JsonConvert.SerializeObject(obj, _settings);

    public string SerializeObject(object obj) => JsonConvert.SerializeObject(obj, _settings);

    public T? Deserialize<T>(string data) => JsonConvert.DeserializeObject<T>(data, _settings);
}
