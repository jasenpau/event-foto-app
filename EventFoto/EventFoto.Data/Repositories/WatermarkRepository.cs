using EventFoto.Data.DTOs;
using EventFoto.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EventFoto.Data.Repositories;

public class WatermarkRepository : IWatermarkRepository
{
    private readonly EventFotoContext _context;

    public WatermarkRepository(EventFotoContext context)
    {
        _context = context;
    }

    public async Task<Watermark> CreateWatermarkAsync(Watermark watermark)
    {
        _context.Watermarks.Add(watermark);
        await _context.SaveChangesAsync();
        return watermark;
    }

    public async Task<Watermark> GetWatermarkAsync(int id)
    {
        return await _context.Watermarks.FindAsync(id);
    }

    public async Task DeleteWatermarkAsync(int id)
    {
        var watermark = await _context.Watermarks.FindAsync(id);
        if (watermark != null)
        {
            _context.Watermarks.Remove(watermark);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<PagedData<string, Watermark>> SearchWatermarksAsync(WatermarkSearchParams searchParams)
    {
        IQueryable<Watermark> query = _context.Watermarks.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchParams.KeyOffset))
        {
            var idOffset = int.Parse(searchParams.KeyOffset);
            query = query.Where(u => u.Id > idOffset);
        }

        if (!string.IsNullOrWhiteSpace(searchParams.Query))
        {
            query = query.Where(u =>
                EF.Functions.ILike(u.Name, $"%{searchParams.Query}%"));
        }

        query = query.OrderBy(u => u.Id);
        // Add +1 to the query to check, if we have the next page.
        query = query.Take(searchParams.PageSize + 1);

        var queryResult = await query.ToListAsync();
        var hasNextPage = queryResult.Count > searchParams.PageSize;
        // Remove last element to constrain to the queried page size.
        if (hasNextPage) queryResult.RemoveAt(queryResult.Count - 1);

        return new PagedData<string, Watermark>
        {
            Data = queryResult,
            PageSize = searchParams.PageSize,
            KeyOffset = searchParams.KeyOffset,
            HasNextPage = hasNextPage
        };
    }
}
