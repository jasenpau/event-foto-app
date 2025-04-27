using EventFoto.Data.DTOs;
using EventFoto.Data.Models;

namespace EventFoto.Core.Watermarks;

public interface IWatermarkService
{
    public Task<ServiceResult<Watermark>> UploadWatermarkAsync(string name, MemoryStream imageData, CancellationToken cancellationToken);
    public Task<ServiceResult<bool>> DeleteWatermarkAsync(int id, CancellationToken cancellationToken);
    public ServiceResult<Watermark> GetWatermarkAsync(int id);
    public ServiceResult<PagedData<string, Watermark>> SearchWatermarksAsync(WatermarkSearchParams searchParams);
}

