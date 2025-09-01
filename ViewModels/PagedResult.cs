namespace OnlineBookstore.ViewModels
{
    // Generic container for paged results (items + paging info)
    public class PagedResult<T>
    {
        public required IReadOnlyList<T> Items { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int Total { get; init; }
        public string? Query { get; init; }

        public int TotalPages => (int)Math.Ceiling((double)Total / PageSize);
        public bool HasPrev => Page > 1;
        public bool HasNext => Page < TotalPages;
    }
}
