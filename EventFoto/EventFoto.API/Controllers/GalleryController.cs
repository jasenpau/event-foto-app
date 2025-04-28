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

    [HttpGet("event/{eventId:int}")]
    public async Task<ActionResult<List<GalleryDto>>> GetGalleriesForEvent([FromRoute] int eventId)
    {
        var result = await _galleryService.GetGalleriesAsync(eventId);
        return result.Success ? Ok(result.Data.Select(GalleryDto.FromProjection)) : result.ToErrorResponse();
    }

    [HttpPost("event/{eventId:int}")]
    public async Task<ActionResult<GalleryDto>> CreateGallery([FromRoute] int eventId,
        [FromBody] CreateEditGalleryRequestDto request)
    {
        var result = await _galleryService.CreateGalleryAsync(eventId, request.Name, request.WatermarkId);
        return result.Success ? Ok(GalleryDto.FromModel(result.Data)) : result.ToErrorResponse();
    }

    [HttpGet("{galleryId:int}")]
    public async Task<ActionResult<GalleryDto>> GetGalleryAsync([FromRoute] int galleryId)
    {
        var result = await _galleryService.GetGalleryAsync(galleryId);
        return result.Success ? Ok(GalleryDto.FromModel(result.Data)) : result.ToErrorResponse();
    }

    [HttpPut("{galleryId:int}")]
    public async Task<ActionResult<GalleryDto>> UpdateGalleryAsync([FromRoute] int galleryId, [FromBody] CreateEditGalleryRequestDto request)
    {
        var result = await _galleryService.UpdateGalleryAsync(galleryId, request);
        return result.Success ? Ok(GalleryDto.FromModel(result.Data)) : result.ToErrorResponse();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteGallery([FromRoute] int id)
    {
        var result = await _galleryService.DeleteGalleryAsync(id);
        return result.Success ? Ok(result.Data) : result.ToErrorResponse();
    }

}
