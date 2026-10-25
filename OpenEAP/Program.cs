using AtomUI;
using Avalonia;
using ReactiveUI.Avalonia.Reactive;
using Serilog;
using Splat;
using System;
using System.IO;

namespace OpenEAP;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            try
            {
                var ex = e.ExceptionObject as Exception;
                File.AppendAllText(
                    "Logs/crash.txt",
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] FATAL: {ex}\r\n"
                );
            }
            catch { /* 忽略日志写入错误 */ }
        };
        var uiLogService = new LogService();
        Log.Logger = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Debug()
#else
            .MinimumLevel.Information()
#endif
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Async(c => c.File("Logs/logs.txt", rollingInterval: RollingInterval.Day, fileSizeLimitBytes: 10 * 1024 * 1024, rollOnFileSizeLimit: true, retainedFileCountLimit: 100, outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"))
            //.WriteTo.Async(c => c.File("Logs/logs.txt")) // 文件 sink
            .WriteTo.Console()
            .WriteTo.Sink(new UiLogSink(uiLogService)) // 把日志也推到 UI
            .CreateLogger();

        Locator.CurrentMutable.RegisterConstant<ILogService>(uiLogService);
        try
        {
            Log.Information("Starting Avalonia host.");
            BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
        }
        catch (StackOverflowException ex)
        {
            File.AppendAllText(
                    "Logs/Stack.txt",
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] StackOverflowException: {ex}\r\n"
                );
            Log.Fatal(ex, "Stack overflow detected during startup");
            // 记录堆栈跟踪信息
        }
        finally
        {
            // 确保所有 sink 刷新
            Log.CloseAndFlush();
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI(_ => { })
            .UseAtomUIPlatformDetect()
            .WithAtomUIDefaultOptions();
}
