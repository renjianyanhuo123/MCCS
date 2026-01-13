using System.Text;
using MCCS.Infrastructure.Communication.NamedPipe.Models;
using Newtonsoft.Json;

namespace MCCS.Infrastructure.Communication.NamedPipe.Serialization;

/// <summary>
/// JSON消息序列化器实现
/// </summary>
public sealed class JsonMessageSerializer : IMessageSerializer
{
    private static readonly JsonSerializerSettings DefaultSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        DateTimeZoneHandling = DateTimeZoneHandling.Utc
    };

    private readonly JsonSerializerSettings _settings;

    public JsonMessageSerializer() : this(DefaultSettings)
    {
    }

    public JsonMessageSerializer(JsonSerializerSettings settings)
    {
        _settings = settings;
    }

    public byte[] SerializeRequest(PipeRequest request)
    {
        var json = JsonConvert.SerializeObject(request, _settings);
        return Encoding.UTF8.GetBytes(json);
    }

    public PipeRequest DeserializeRequest(byte[] data)
    {
        var json = Encoding.UTF8.GetString(data);
        return JsonConvert.DeserializeObject<PipeRequest>(json, _settings)
               ?? throw new InvalidOperationException("Failed to deserialize request");
    }

    public byte[] SerializeResponse(PipeResponse response)
    {
        var json = JsonConvert.SerializeObject(response, _settings);
        return Encoding.UTF8.GetBytes(json);
    }

    public PipeResponse DeserializeResponse(byte[] data)
    {
        var json = Encoding.UTF8.GetString(data);
        return JsonConvert.DeserializeObject<PipeResponse>(json, _settings)
               ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    public string Serialize<T>(T obj)
    {
        return JsonConvert.SerializeObject(obj, _settings);
    }

    public T? Deserialize<T>(string data)
    {
        return JsonConvert.DeserializeObject<T>(data, _settings);
    }
}
