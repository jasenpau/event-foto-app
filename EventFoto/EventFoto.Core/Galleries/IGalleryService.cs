using EventFoto.Data.Models;

namespace EventFoto.Core.Galleries;

public interface IGalleryService
{
    public Task<ServiceResult<Gallery>> GetGalleryAsync(int id);
    public Task<ServiceResult<bool>> DeleteGalleryAsync(int id);
}
