namespace MCCS.Infrastructure.Communication.NamedPipe;

/// <summary>
/// 标记命名管道处理器类
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ApiNamedPipeAttribute : Attribute
{
    public ApiNamedPipeAttribute(string name)
    {
        Name = name;
    }

    /// <summary>
    /// 管道名称
    /// </summary>
    public string Name { get; }
}

/// <summary>
/// 标记处理器方法路由
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class RouteAttribute(string template) : Attribute
{
    /// <summary>
    /// 路由模板
    /// </summary>
    public string Template { get; } = template;
}
