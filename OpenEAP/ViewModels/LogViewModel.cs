using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using AtomUI.Icons.AntDesign;
using Avalonia.Controls;
using Avalonia.Threading;
using Dock.Model.ReactiveUI.Controls;
using OpenEAP.Model;
using ReactiveUI;
using Serilog.Events;
using Splat;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OpenEAP.ViewModels
{
    public class LogViewModel : Tool, IDisposable
    {
        public ObservableCollection<LogEntry> Items { get; } = new ObservableCollection<LogEntry>();
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private int _maxItems = 100; // 可调整保留条数
        public LogViewModel(ILogService logService)
        {
            if (logService == null)
            {
                logService = Locator.Current.GetService<ILogService>();
            }
            // 订阅日志流
            var sub = logService.LogStream.Subscribe(entry =>
            {
                if (entry == null) return;
                // 更新 UI 必须在 UI 线程
                if (Dispatcher.UIThread.CheckAccess())
                {
                    AddEntry(entry);
                }
                else
                {
                    Dispatcher.UIThread.Post(() => AddEntry(entry));
                }
            });

            _disposables.Add(sub);
        }

        private void AddEntry(LogEntry entry)
        {
            Items.Add(entry);
            if (Items.Count > _maxItems)
            {
                // 移除最早的条目（简单策略）
                Items.RemoveAt(0);
            }
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }

}
