using Avalonia.Media;
using ReactiveUI;

namespace OpenEAP.ViewModels
{
    /// <summary>
    /// 已判定结果（最新在上）
    /// </summary>
    public class ResultItemViewModel : ReactiveObject
    {
        private static readonly IBrush OkBrush = new SolidColorBrush(Color.Parse("#34D399"));
        private static readonly IBrush NgBrush = new SolidColorBrush(Color.Parse("#F87171"));

        private int _seq;

        public int Seq { get => _seq; set => this.RaiseAndSetIfChanged(ref _seq, value); }
        public string Serial { get; set; } = "";

        /// <summary>
        /// 评定结果：OK / NG
        /// </summary>
        public string Verdict { get; set; } = "";

        /// <summary>
        /// 评定结果颜色
        /// </summary>
        public IBrush VerdictBrush => Verdict == "OK" ? OkBrush : NgBrush;

        public string Reason { get; set; } = "";
        public string Judge { get; set; } = "";
        public string Time { get; set; } = "";
    }
}
