namespace Core.Application.Dto.Filter;

public class PaginatedResult<TResult>
{
    public long TotalCount { get; set; }

    public int CurrentPage { get; set; }

    public int PageSize { get; set; }

    public required IReadOnlyList<TResult> Records { get; set; }
}