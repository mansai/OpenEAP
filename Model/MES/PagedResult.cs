namespace OpenEAP.Model
{
    public sealed class PagedResult<T>
    {
        public int TotalRecords { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public List<T> Items { get; set; } = new();
    }
}