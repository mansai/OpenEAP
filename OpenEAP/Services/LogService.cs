using OpenEAP.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace OpenEAP;

public class LogService : ILogService, IDisposable
{
    //private readonly Subject<LogEntry> _subject = new Subject<LogEntry>();
    private readonly ReplaySubject<LogEntry> _subject;
    public IObservable<LogEntry> LogStream => _subject;
    public LogService(int bufferSize = 0)
    {
        // ReplaySubject 会把最近 bufferSize 条重放给后来的订阅者
        _subject = new ReplaySubject<LogEntry>(bufferSize);
    }
    public void Log(LogEntry entry)
    {
        if (entry == null) return;
        // 非阻塞地发布
        try { _subject.OnNext(entry); }
        catch { /* swallow: 保证不抛给调用者 */ }
    }

    public void Log(string message, string level = "Information", string source = "", string exception = "")
    {
        Log(new LogEntry { Message = message, Level = level, Source = source, Exception = exception, Time = DateTime.Now });
    }

    public void Dispose()
    {
        try { _subject.OnCompleted(); _subject.Dispose(); }
        catch { }
    }
}
