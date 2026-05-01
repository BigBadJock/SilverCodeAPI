namespace Core.Common.DataModels
{
    public record Pagination
    {
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int PageCount { get; init; }
        public int TotalCount { get; init; }
    }
}
