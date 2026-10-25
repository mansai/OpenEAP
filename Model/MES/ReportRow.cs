namespace OpenEAP.Model
{
    public sealed class ReportRow
    {
        public string? LotNumber { get; set; }
        public string? WorkOrder { get; set; }
        public DateTime? TestTime { get; set; }
        public string? EquipmentCode { get; set; }
        public string? TestResultName { get; set; }
        public string? ImageType { get; set; }
        public string? Image { get; set; }
        public string? ProductionLine { get; set; }
        public string? ProcessesName { get; set; }
        public string? ShifName { get; set; }
        public string? IsDefault { get; set; }
        public string? ItemNo { get; set; }
        public string? Creator { get; set; }
        public DateTime? CreationTime { get; set; }
    }
}