using AtomUI;
using AtomUI.Desktop.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenEAP.Common
{
    public class UIMessage
    {
        //出错了
        public static string errorTitle = "错误提示";
        //保存出错
        public static string SaveError = Localizer.Localizer.Instance["SaveError"];
        //已使用，不允许删除
        public static string DeleteError = Localizer.Localizer.Instance["DeleteError"];
        public static string SelectRowError = "请先选择行，再操作";
        //操作成功
        public static string OK = Localizer.Localizer.Instance["OK"];
        //提示
        public static string Tips = Localizer.Localizer.Instance["Tips"];
        //保存成功
        public static string SaveOK = Localizer.Localizer.Instance["SaveOK"];
        public static string CopyOK = Localizer.Localizer.Instance["CopyOK"];
        //导出成功
        public static string ExportOK = Localizer.Localizer.Instance["ExportOK"];
        //删除成功
        public static string DeleteOK = Localizer.Localizer.Instance["DeleteOK"];
        //删除确认
        public static string DeleteConfirm = Localizer.Localizer.Instance["DeleteConfirm"];
        public static string Required = "不能为空";
        public static Avalonia.Controls.Window? FindViableOwner()
        {
            if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var activeWindows = desktop.Windows
                    .Where(w => w.IsVisible && w.IsActive)
                    .ToList();
                return activeWindows.LastOrDefault(w => !ReferenceEquals(w, desktop.MainWindow))
                       ?? activeWindows.LastOrDefault()
                       ?? desktop.MainWindow;
            }
            return null;
        }
        public static void Show(string msg = null)
        {
            var owner = FindViableOwner();
            var _messageManager = new WindowMessageManager(owner);
            _messageManager.Show(new Message(msg ?? OK));
        }
        public static async Task<bool> ShowConfirm(string msg = null, string title = null, DialogStandardButton defaultButton = DialogStandardButton.Yes)
        {
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
            var textBlock = new AtomUI.Desktop.Controls.TextBlock();
            textBlock.Text = msg ?? DeleteConfirm;
            var options = new DialogOptions
            {
                Title = title,
                IsResizable = false,
                IsDragMovable = true,
                IsMaximizable = false,
                StandardButtons = DialogStandardButtons.Parse("No,Yes"),
                DefaultStandardButton = defaultButton,
                HorizontalStartupLocation = DialogHorizontalAnchor.Center,
                VerticalOffset = new Dimension(30, DimensionUnitType.Percentage)
            };
            var owner = FindViableOwner();
            var result = await Dialog.ShowDialogAsync(textBlock, options: options, topLevel: owner);
            if (result is AtomUI.Desktop.Controls.DialogCode code && code == AtomUI.Desktop.Controls.DialogCode.Accepted)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static void ShowAlert(string msg, string title = null)
        {
            var textBlock = new AtomUI.Desktop.Controls.TextBlock();
            textBlock.Text = msg;
            textBlock.TextWrapping = Avalonia.Media.TextWrapping.Wrap;
            textBlock.MinWidth = 240;
            var options = new DialogOptions
            {
                Title = title ?? Tips,
                IsResizable = false,
                IsDragMovable = true,
                IsMaximizable = false,
                StandardButtons = DialogStandardButtons.Parse("Yes"),
                DefaultStandardButton = DialogStandardButton.Yes,
                HorizontalStartupLocation = DialogHorizontalAnchor.Center,
            };
            _ = Dialog.ShowDialogAsync(textBlock, options: options, topLevel: FindViableOwner());
        }
        public static void ShowError(string errorMsg)
        {
            var owner = FindViableOwner();
            var _messageManager = new WindowMessageManager(owner);
            _messageManager.Show(new Message(errorMsg, MessageType.Error));
        }
        public static void Notification(string msg = null, NotificationType notificationType = NotificationType.Success, int maxItems = 3)
        {
            WindowNotificationManager? _manager = new WindowNotificationManager(FindViableOwner()) { MaxItems = maxItems };
            _manager?.Show(new Notification(Tips, msg ?? OK, notificationType));
        }
        public static void NotificationError(string errorMsg = null, int maxItems = 3)
        {
            WindowNotificationManager? _manager = new WindowNotificationManager(FindViableOwner()) { MaxItems = maxItems };
            _manager?.Show(new Notification(errorTitle, errorMsg, NotificationType.Error));
        }
        static WindowMessageManager? _managerLoading;
        public static void ShowLoading(string errorMsg = null, int maxItems = 3)
        {
            if (_managerLoading == null)
                _managerLoading = new WindowMessageManager(FindViableOwner()) { MaxItems = maxItems };
            _managerLoading?.Show(new Message(type: MessageType.Loading, content: "正在处理，请稍候..."));
        }
    }
}
