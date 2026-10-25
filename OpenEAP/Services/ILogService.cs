using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenEAP.Model;

namespace OpenEAP;

public interface ILogService
{
    void Log(LogEntry entry);
    void Log(string message, string level = "Information", string source = "", string exception = "");
    IObservable<LogEntry> LogStream { get; }
}
