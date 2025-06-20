using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Core.Devices.Commands
{
    public record CommandResponse
    {
        public required string CommandId { get; set; }
        public required string DeviceId { get; set; }
        public bool Success { get; set; }
        public object? Result { get; set; }
        public CommandExecuteStatusEnum CommandExecuteStatus { get; set; }
        public double Progress { get; set; } = 0.0;
        public string? ErrorMessage { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public DateTimeOffset Timestamp { get; set; } = DateTime.Now;
    }
}
