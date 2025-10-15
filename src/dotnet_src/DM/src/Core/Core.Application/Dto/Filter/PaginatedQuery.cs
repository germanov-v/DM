namespace Core.Application.Dto.Filter;

public class PaginatedQuery<TFilter> where TFilter : IFilter
{
    public int CurrentPage { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public string? OrderByColumnStr { get; set; }


    /// <summary>
    /// true - new
    /// false - old
    /// </summary>
    public bool OrderDesc { get; set; }

    public required TFilter Filter { get; set; }
}