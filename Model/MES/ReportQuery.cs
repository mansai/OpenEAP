namespace OpenEAP.Model
{
    public sealed class ReportQuery
    {
        // 必填
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        // 可选筛选
        public string? LotNumberStart { get; set; }
        public string? LotNumberEnd { get; set; }
        public string? LotNumbers { get; set; }     // 多个组件号，整理成字符串传给存储过程
        public string? Line { get; set; }           // 线别，多值字符串
        public long? Processes { get; set; }        // 工序 Id
        public long? Equipent { get; set; }         // 注意：存储过程参数名就是 Equipent
        public string? TestResult { get; set; }     // 0 / 1
        public string? ImageType { get; set; }      // 0 / 1 / 3
        public string? IsDefault { get; set; }      // 1 / 0
        public long? WorkOrderCode { get; set; }    // 大概率传工单 Id
        public string? PackageNo { get; set; }      // 多值字符串
        // 分页
        public int PageSize { get; set; } = 10;
        public int CurrentPage { get; set; } = 1;   // UI 从 1 开始
        // 语言
        public string Lang { get; set; } = "zh-Hans";

        public ReportQuery Clone()
        {
            return new ReportQuery
            {
                StartDate = StartDate,
                EndDate = EndDate,
                LotNumberStart = LotNumberStart,
                LotNumberEnd = LotNumberEnd,
                LotNumbers = LotNumbers,
                Line = Line,
                Processes = Processes,
                Equipent = Equipent,
                TestResult = TestResult,
                ImageType = ImageType,
                IsDefault = IsDefault,
                WorkOrderCode = WorkOrderCode,
                PackageNo = PackageNo,
                PageSize = PageSize,
                CurrentPage = CurrentPage,
                Lang = Lang
            };
        }
    }
}