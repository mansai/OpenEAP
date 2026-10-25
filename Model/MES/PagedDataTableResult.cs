using System.Data;

namespace OpenEAP.Model
{
    public class PagedDataTableResult
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public long TotalRecords { get; set; }
        public DataTable Table { get; set; } = new DataTable();
    }
}
