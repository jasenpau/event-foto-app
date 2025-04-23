using EventFoto.Core.ControllerBase;
using EventFoto.Core.Extensions;
using EventFoto.Core.Galleries;
using EventFoto.Data.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventFoto.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GalleryController : AppControllerBase
{
    private readonly IGalleryService _galleryService;

    public GalleryController(IGalleryService galleryService)
    {
        _galleryService = galleryService;
    }

    [HttpGet("{galleryId:int}")]
    public async Task<ActionResult<GalleryDto>> GetGalleryAsync([FromRoute] int galleryId)
    {
        var result = await _galleryService.GetGalleryAsync(galleryId);
        return result.Success ? Ok(GalleryDto.FromModel(result.Data)) : result.ToErrorResponse();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteGallery([FromRoute] int id)
    {
        var result = await _galleryService.DeleteGalleryAsync(id);
        return result.Success ? Ok(result.Data) : result.ToErrorResponse();
    }

}
