namespace OpenEAP.Model
{
    public sealed class ImageReportQuery
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? LotNumberStart { get; set; }
        public string? LotNumberEnd { get; set; }
        public string? LotNumbers { get; set; }
        public string? Line { get; set; }
        public long? Processes { get; set; }
        public long? Equipent { get; set; }
        public string? TestResult { get; set; }
        public string? ImageType { get; set; }
        public string DefaultImageType { get; set; } = "10,11,12";
        public string? IsDefault { get; set; }
        public long? WorkOrderCode { get; set; }
        public string? PackageNo { get; set; }
        public int PageSize { get; set; } = 20;
        public int CurrentPage { get; set; } = 1;
        public string Lang { get; set; } = "zh-Hans";
        public ImageReportQuery Clone()
        {
            return new ImageReportQuery
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
                DefaultImageType = DefaultImageType,
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