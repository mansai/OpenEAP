using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenEAP.Model;
/// <summary>
/// 记录日志
/// </summary>
public class LogEntry
{
    public DateTime Time { get; set; } = DateTime.Now;
    public string Level { get; set; } = "Information";
    public string Message { get; set; } = "";
    public string Source { get; set; } = "";
    public string Exception { get; set; } = "";

    public override string ToString() => $"[{Time:HH:mm:ss}] {Level}: {Message} {Exception}";
}
