using System.Text;

namespace MCCS.Infrastructure.Communication.NamedPipe.Models;

/// <summary>
/// 管道消息状态码
/// </summary>
public enum PipeStatusCode
{
    /// <summary>
    /// 成功
    /// </summary>
    Success = 0,

    /// <summary>
    /// 未知错误
    /// </summary>
    UnknownError = 1,

    /// <summary>
    /// 请求超时
    /// </summary>
    Timeout = 2,

    /// <summary>
    /// 无效请求
    /// </summary>
    InvalidRequest = 3,

    /// <summary>
    /// 处理器未找到
    /// </summary>
    HandlerNotFound = 4,

    /// <summary>
    /// 处理器执行异常
    /// </summary>
    HandlerException = 5,

    /// <summary>
    /// 序列化错误
    /// </summary>
    SerializationError = 6,

    /// <summary>
    /// 连接错误
    /// </summary>
    ConnectionError = 7,

    /// <summary>
    /// 服务端已关闭
    /// </summary>
    ServerClosed = 8
}

/// <summary>
/// 管道请求消息
/// </summary>
public sealed class PipeRequest
{
    /// <summary>
    /// 请求唯一标识符
    /// </summary>
    public string RequestId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 请求类型/路由标识
    /// </summary>
    public string Route { get; set; } = string.Empty;

    /// <summary>
    /// 请求负载数据（JSON格式）
    /// </summary>
    public string? Payload { get; set; }

    /// <summary>
    /// 请求时间戳（UTC毫秒）
    /// </summary>
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    /// <summary>
    /// 创建请求
    /// </summary>
    public static PipeRequest Create(string route, string? payload = null) =>
        new()
        {
            Route = route,
            Payload = payload
        };

    /// <summary>
    /// 创建带泛型数据的请求
    /// </summary>
    public static PipeRequest Create<T>(string route, T data, Func<T, string> serializer) =>
        new()
        {
            Route = route,
            Payload = serializer(data)
        };
}

/// <summary>
/// 管道响应消息
/// </summary>
public sealed class PipeResponse
{
    /// <summary>
    /// 对应的请求ID
    /// </summary>
    public string RequestId { get; set; } = string.Empty;

    /// <summary>
    /// 状态码
    /// </summary>
    public PipeStatusCode StatusCode { get; set; } = PipeStatusCode.Success;

    /// <summary>
    /// 错误消息（当状态码非Success时）
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 响应负载数据（JSON格式）
    /// </summary>
    public string? Payload { get; set; }

    /// <summary>
    /// 响应时间戳（UTC毫秒）
    /// </summary>
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    /// <summary>
    /// 处理耗时（毫秒）
    /// </summary>
    public long ProcessingTimeMs { get; set; }

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess => StatusCode == PipeStatusCode.Success;

    /// <summary>
    /// 创建成功响应
    /// </summary>
    public static PipeResponse Success(string requestId, string? payload = null)
    {
        return new PipeResponse
        {
            RequestId = requestId,
            StatusCode = PipeStatusCode.Success,
            Payload = payload
        };
    }

    /// <summary>
    /// 创建成功响应（泛型）
    /// </summary>
    public static PipeResponse Success<T>(string requestId, T data, Func<T, string> serializer)
    {
        return new PipeResponse
        {
            RequestId = requestId,
            StatusCode = PipeStatusCode.Success,
            Payload = serializer(data)
        };
    }

    /// <summary>
    /// 创建失败响应
    /// </summary>
    public static PipeResponse Failure(string requestId, PipeStatusCode statusCode, string? errorMessage = null)
    {
        return new PipeResponse
        {
            RequestId = requestId,
            StatusCode = statusCode,
            ErrorMessage = errorMessage
        };
    }

    /// <summary>
    /// 创建异常响应
    /// </summary>
    public static PipeResponse FromException(string requestId, Exception ex)
    {
        return new PipeResponse
        {
            RequestId = requestId,
            StatusCode = PipeStatusCode.HandlerException,
            ErrorMessage = ex.Message
        };
    }
}
