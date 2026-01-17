using MCCS.Infrastructure.Communication.NamedPipe.Models;

namespace MCCS.Infrastructure.Communication.NamedPipe.Serialization;

/// <summary>
/// 发布-订阅消息序列化器接口
/// </summary>
public interface IPubSubMessageSerializer
{
    /// <summary>
    /// 序列化消息包装器为字节数组
    /// </summary>
    byte[] SerializeEnvelope(PipeMessageEnvelope envelope);

    /// <summary>
    /// 从字节数组反序列化消息包装器
    /// </summary>
    PipeMessageEnvelope DeserializeEnvelope(byte[] data);

    /// <summary>
    /// 序列化对象为字符串
    /// </summary>
    string Serialize<T>(T obj);

    /// <summary>
    /// 序列化对象为字符串（非泛型）
    /// </summary>
    string SerializeObject(object obj);

    /// <summary>
    /// 从字符串反序列化对象
    /// </summary>
    T? Deserialize<T>(string data);
}
