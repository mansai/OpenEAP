using Avalonia.Media;
using ReactiveUI;

namespace OpenEAP.ViewModels
{
    /// <summary>
    /// 机台行（左侧机台选择列表项）
    /// </summary>
    public class MachineItemViewModel : ReactiveObject
    {
        private bool _isSelected;

        public int Id { get; }
        public string Name { get; }

        /// <summary>
        /// 运行状态：run / idle / down
        /// </summary>
        public string Status { get; }

        /// <summary>
        /// 状态文案（运行 / 待机 / 停机）
        /// </summary>
        public string StatusText { get; }

        /// <summary>
        /// 状态灯颜色
        /// </summary>
        public IBrush StatusBrush { get; }

        public bool IsSelected
        {
            get => _isSelected;
            set => this.RaiseAndSetIfChanged(ref _isSelected, value);
        }

        public MachineItemViewModel(int id, string status, string statusText, IBrush statusBrush)
        {
            Id = id;
            Name = $"{id}#机";
            Status = status;
            StatusText = statusText;
            StatusBrush = statusBrush;
        }
    }
}
