namespace OpenEAP.Model
{
    public class SearchCondition
    {
        public string FieldName { get; set; }
        public object? Value { get; set; }
        public string? Operator { get; set; }
        public FiltersDetail Meta { get; set; }
        public SearchCondition(FiltersDetail filtersDetail)
        {
            Meta = filtersDetail;
            FieldName = string.Empty;
        }
    }
}

