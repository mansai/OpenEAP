using OpenEAP.Model;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace OpenEAP;

public class UiLogSink : ILogEventSink
{
    private readonly ILogService _logService;
    private readonly IFormatProvider _formatProvider;

    public UiLogSink(ILogService logService, IFormatProvider formatProvider = null)
    {
        _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        _formatProvider = formatProvider;
    }

    public void Emit(LogEvent logEvent)
    {
        if (logEvent == null || logEvent.Properties.Count > 0) return;

        // 格式化消息（包含模板参数）
        var msg = logEvent.RenderMessage(_formatProvider);

        var level = logEvent.Level.ToString();
        var ex = logEvent.Exception?.ToString() ?? "";

        var entry = new LogEntry
        {
            Time = logEvent.Timestamp.UtcDateTime.ToLocalTime(),
            Level = level,
            Message = msg,
            Exception = ex,
            Source = logEvent.Properties.ContainsKey("SourceContext") ? logEvent.Properties["SourceContext"].ToString().Trim('"') : ""
        };
        // 直接推到 LogService（LogService 使用 Subject，不应阻塞）
        _logService.Log(entry);
    }
}
