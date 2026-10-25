using ReactiveUI;

namespace OpenEAP.ViewModels
{
    /// <summary>
    /// 待判定串（队首为当前判定对象）
    /// </summary>
    public class PendingItemViewModel : ReactiveObject
    {
        private int _seq;
        private bool _isCurrent;

        public int Seq { get => _seq; set => this.RaiseAndSetIfChanged(ref _seq, value); }
        public string Serial { get; set; } = "";
        public string Line { get; set; } = "";

        /// <summary>
        /// 异常类型：外观 / EL
        /// </summary>
        public string DefectType { get; set; } = "";

        /// <summary>
        /// 列表展示的异常文案（外观NG / EL-NG）
        /// </summary>
        public string DefectLabel { get; set; } = "";

        public string Time { get; set; } = "";
        public string Judge { get; set; } = "";

        /// <summary>
        /// 是否为当前判定行
        /// </summary>
        public bool IsCurrent { get => _isCurrent; set => this.RaiseAndSetIfChanged(ref _isCurrent, value); }
    }
}
