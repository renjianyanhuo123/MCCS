using Serilog.Events;

namespace MCCS.LoggerSettings
{
    internal class GlobalExceptionSetting
    {
        public static string ExceptionMessageTemplate(LogEventLevel level, string? title, Exception? e, string? customErrorInfo)
        {
            return $"""
                   Time:{DateTime.Now:yyyy-MM-dd hh:mm};
                   Title:{title ?? "无标题"}
                   Type:{level.ToString()};
                   Message:{e?.Message ?? customErrorInfo};
                   Position:{e?.Source ?? string.Empty};
                   """;
        }
    }
}
