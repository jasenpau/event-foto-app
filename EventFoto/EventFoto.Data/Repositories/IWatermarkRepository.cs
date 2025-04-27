using EventFoto.Data.DTOs;
using EventFoto.Data.Models;

namespace EventFoto.Data.Repositories;

public interface IWatermarkRepository
{
    public Task<Watermark> CreateWatermarkAsync(Watermark watermark);
    public Task<Watermark> GetWatermarkAsync(int id);
    public Task DeleteWatermarkAsync(int id);
    public Task<PagedData<string, Watermark>> SearchWatermarksAsync(WatermarkSearchParams searchParams);
}
