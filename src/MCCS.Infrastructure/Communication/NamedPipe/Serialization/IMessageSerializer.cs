using MCCS.Infrastructure.Communication.NamedPipe.Models;

namespace MCCS.Infrastructure.Communication.NamedPipe.Serialization;

/// <summary>
/// 消息序列化器接口
/// </summary>
public interface IMessageSerializer
{
    /// <summary>
    /// 序列化请求消息为字节数组
    /// </summary>
    byte[] SerializeRequest(PipeRequest request);

    /// <summary>
    /// 从字节数组反序列化请求消息
    /// </summary>
    PipeRequest DeserializeRequest(byte[] data);

    /// <summary>
    /// 序列化响应消息为字节数组
    /// </summary>
    byte[] SerializeResponse(PipeResponse response);

    /// <summary>
    /// 从字节数组反序列化响应消息
    /// </summary>
    PipeResponse DeserializeResponse(byte[] data);

    /// <summary>
    /// 序列化对象为字符串（用于Payload）
    /// </summary>
    string Serialize<T>(T obj);

    /// <summary>
    /// 从字符串反序列化对象（用于Payload）
    /// </summary>
    T? Deserialize<T>(string data);
}
