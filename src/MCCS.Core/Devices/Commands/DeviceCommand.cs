using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Core.Devices.Commands;

/// <summary>
/// 设备指令结构
/// </summary>
public record DeviceCommand
{
    public string CommandId { get; } = Guid.NewGuid().ToString();
    public required string DeviceId { get; init; }
    public CommandTypeEnum Type { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTime.Now;
    public int TimeoutMs { get; set; } = 5000; // 默认5秒超时
}
