using System.Collections.Generic;
using System.Linq;

namespace OpenEAP.Models.FormDesigner
{
    /// <summary>低代码表格/下拉表格使用的内置示例数据。</summary>
    public static class FormSampleData
    {
        public static string GetDisplayName(FormDataSourceKind kind)
        {
            var key = kind switch
            {
                FormDataSourceKind.Devices => "FormDataSourceDevices",
                FormDataSourceKind.Products => "FormDataSourceProducts",
                FormDataSourceKind.Customers => "FormDataSourceCustomers",
                FormDataSourceKind.QualityRecords => "FormDataSourceQuality",
                _ => "FormDataSourceNone"
            };
            return Localizer.Localizer.Instance[key];
        }

        public static IReadOnlyList<FormColumnDefinition> GetColumns(FormDataSourceKind kind)
        {
            return kind switch
            {
                FormDataSourceKind.Devices => new[]
                {
                    new FormColumnDefinition("Code", L("FormDataCode"), 110),
                    new FormColumnDefinition("Name", L("FormDataDeviceName"), 190),
                    new FormColumnDefinition("Line", L("FormDataProductionLine"), 110),
                    new FormColumnDefinition("Status", L("FormDataStatus"), 90),
                },
                FormDataSourceKind.Products => new[]
                {
                    new FormColumnDefinition("Code", L("FormDataCode"), 110),
                    new FormColumnDefinition("Name", L("FormDataProductName"), 190),
                    new FormColumnDefinition("Category", L("FormDataType"), 120),
                    new FormColumnDefinition("Price", L("FormDataPrice"), 90),
                },
                FormDataSourceKind.Customers => new[]
                {
                    new FormColumnDefinition("Code", L("FormDataCode"), 110),
                    new FormColumnDefinition("Name", L("FormDataCustomerName"), 170),
                    new FormColumnDefinition("Region", L("FormDataRegion"), 130),
                    new FormColumnDefinition("Contact", L("FormDataContact"), 140),
                },
                FormDataSourceKind.QualityRecords => new[]
                {
                    new FormColumnDefinition("Serial", L("FormDataSerial"), 150),
                    new FormColumnDefinition("Station", L("FormDataStation"), 120),
                    new FormColumnDefinition("Result", L("FormDataResult"), 90),
                    new FormColumnDefinition("Time", L("FormDataTime"), 160),
                },
                _ => new[]
                {
                    new FormColumnDefinition("Code", L("FormDataCode"), 130),
                    new FormColumnDefinition("Name", L("FormDataName"), 220),
                }
            };
        }

        public static List<FormDataRow> GetRows(FormDataSourceKind kind)
        {
            return kind switch
            {
                FormDataSourceKind.Devices => new List<FormDataRow>
                {
                    new(new[] { "EQ-001", "Stringer A01", "Line A", "Run" }),
                    new(new[] { "EQ-002", "Stringer A02", "Line A", "Run" }),
                    new(new[] { "EQ-003", "EL Tester B01", "Line B", "Idle" }),
                    new(new[] { "EQ-004", "Laser Cutter C01", "Line C", "Run" }),
                    new(new[] { "EQ-005", "Laminator C02", "Line C", "Alarm" }),
                    new(new[] { "EQ-006", "IV Tester D01", "Line D", "Run" }),
                    new(new[] { "EQ-007", "Framing Machine D02", "Line D", "Idle" }),
                    new(new[] { "EQ-008", "Stringer E01", "Line E", "Run" }),
                },
                FormDataSourceKind.Products => new List<FormDataRow>
                {
                    new(new[] { "P-1001", "Mono PERC 182", "Solar Cell", "0.38" }),
                    new(new[] { "P-1002", "TOPCon 210", "Solar Cell", "0.52" }),
                    new(new[] { "P-1003", "HJT 182", "Solar Cell", "0.61" }),
                    new(new[] { "P-1004", "Bifacial 210", "Module", "1.25" }),
                    new(new[] { "P-1005", "Full-black 182", "Module", "1.08" }),
                },
                FormDataSourceKind.Customers => new List<FormDataRow>
                {
                    new(new[] { "C-001", "SolarTech GmbH", "Europe", "M. Weber" }),
                    new(new[] { "C-002", "GreenVolt Inc.", "North America", "A. Miller" }),
                    new(new[] { "C-003", "Sunrise Energy", "Asia", "Li Wei" }),
                    new(new[] { "C-004", "Nova Power", "Asia", "Chen Yu" }),
                    new(new[] { "C-005", "Helios Trading", "Middle East", "S. Ahmed" }),
                },
                FormDataSourceKind.QualityRecords => new List<FormDataRow>
                {
                    new(new[] { "SC-20261023-0001", "EL", "OK", "2026-10-23 08:12:04" }),
                    new(new[] { "SC-20261023-0002", "Stringer", "OK", "2026-10-23 08:14:21" }),
                    new(new[] { "SC-20261023-0003", "Appearance", "NG", "2026-10-23 08:16:35" }),
                    new(new[] { "SC-20261023-0004", "IV", "OK", "2026-10-23 08:18:02" }),
                    new(new[] { "SC-20261023-0005", "EL", "OK", "2026-10-23 08:20:44" }),
                    new(new[] { "SC-20261023-0006", "Appearance", "OK", "2026-10-23 08:22:16" }),
                },
                _ => new List<FormDataRow>
                {
                    new(new[] { "001", "Sample row 1" }),
                    new(new[] { "002", "Sample row 2" }),
                }
            };
        }

        public static IReadOnlyList<string> GetSearchableColumns(FormDataSourceKind kind)
            => GetColumns(kind).Where(c => c.Searchable).Select(c => c.Key).ToList();

        private static string L(string key) => Localizer.Localizer.Instance[key];
    }
}
