using EventFoto.Core.ControllerBase;
using EventFoto.Core.EventPhotos;
using EventFoto.Core.Extensions;
using EventFoto.Data.DTOs;
using EventFoto.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventFoto.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GalleryController : AppControllerBase
{
    private readonly IEventPhotoService _eventPhotoService;

    public GalleryController(IEventPhotoService eventPhotoService)
    {
        _eventPhotoService = eventPhotoService;
    }

    [HttpGet("search")]
    public async Task<ActionResult<PagedData<string, EventPhotoListDto>>> SearchEventPhotos(
        [FromQuery] EventPhotoSearchParams searchParams)
    {
        var searchResult = await _eventPhotoService.SearchEventPhotosAsync(searchParams);
        if (!searchResult.Success)
        {
            return searchResult.ToErrorResponse();
        }

        var result = searchResult.Data.ToDto(EventPhotoListDto.FromEventPhoto);
        return Ok(result);
    }
}
