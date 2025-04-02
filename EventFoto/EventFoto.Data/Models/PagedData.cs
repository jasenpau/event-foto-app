namespace EventFoto.Data.Models;

public record PagedData<TKey, TData>
{
    public List<TData> Data { get; set; }
    public TKey KeyOffset { get; set; }
    public int PageSize { get; set; }
    public bool HasNextPage { get; set; }

    public PagedData<TKey, TDto> ToDto<TDto>(Func<TData, TDto> mapper)
    {
        return new PagedData<TKey, TDto>
        {
            Data = Data.Select(mapper).ToList(),
            KeyOffset = KeyOffset,
            PageSize = PageSize,
            HasNextPage = HasNextPage,
        };
    }
}
