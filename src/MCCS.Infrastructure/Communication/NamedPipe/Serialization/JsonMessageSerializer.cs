 
using System.Text;

using MCCS.Infrastructure.Communication.NamedPipe.Models;
using Newtonsoft.Json;

namespace MCCS.Infrastructure.Communication.NamedPipe.Serialization;

/// <summary>
/// JSON消息序列化器实现
/// </summary>
public sealed class JsonMessageSerializer(JsonSerializerSettings settings) : IMessageSerializer
{
    private static readonly JsonSerializerSettings _defaultSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        DateTimeZoneHandling = DateTimeZoneHandling.Utc
    };

    public JsonMessageSerializer() : this(_defaultSettings)
    {
    }

    public byte[] SerializeRequest(PipeRequest request)
    {
        var json = JsonConvert.SerializeObject(request, settings);
        return Encoding.UTF8.GetBytes(json);
    }

    public PipeRequest DeserializeRequest(byte[] data)
    {
        var json = Encoding.UTF8.GetString(data);
        return JsonConvert.DeserializeObject<PipeRequest>(json, settings)
               ?? throw new InvalidOperationException("Failed to deserialize request");
    }

    public byte[] SerializeResponse(PipeResponse response)
    {
        var json = JsonConvert.SerializeObject(response, settings);
        return Encoding.UTF8.GetBytes(json);
    }

    public PipeResponse DeserializeResponse(byte[] data)
    {
        var json = Encoding.UTF8.GetString(data);
        return JsonConvert.DeserializeObject<PipeResponse>(json, settings)
               ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    public string Serialize<T>(T obj) => JsonConvert.SerializeObject(obj, settings);

    public T? Deserialize<T>(string data) => JsonConvert.DeserializeObject<T>(data, settings);
}
