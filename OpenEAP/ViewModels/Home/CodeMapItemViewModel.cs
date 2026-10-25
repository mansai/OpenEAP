using ReactiveUI;

namespace OpenEAP.ViewModels
{
    /// <summary>
    /// 异常原因编码设置弹窗中的一行
    /// </summary>
    public class CodeMapItemViewModel : ReactiveObject
    {
        private string _code;
        private string _reason;

        public string Code
        {
            get => _code;
            set => this.RaiseAndSetIfChanged(ref _code, value);
        }

        public string Reason
        {
            get => _reason;
            set => this.RaiseAndSetIfChanged(ref _reason, value);
        }

        public CodeMapItemViewModel(string code, string reason)
        {
            _code = code;
            _reason = reason;
        }
    }
}
