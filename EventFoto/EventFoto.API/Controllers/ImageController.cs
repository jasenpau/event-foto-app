using EventFoto.Core.ControllerBase;
using EventFoto.Core.EventPhotos;
using EventFoto.Core.Extensions;
using EventFoto.Data.DTOs;
using EventFoto.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventFoto.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ImageController : AppControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _configuration;
    private readonly IEventPhotoService _eventPhotoService;

    public ImageController(IWebHostEnvironment env,
        IConfiguration configuration,
        IEventPhotoService eventPhotoService)
    {
        _env = env;
        _configuration = configuration;
        _eventPhotoService = eventPhotoService;
    }

    [HttpGet("details/{id:int}")]
    public async Task<ActionResult<EventPhotoDto>> GetPhotoById(int id)
    {
        var result = await _eventPhotoService.GetByIdAsync(id);
        return result.Success ? Ok(EventPhotoDto.FromEventPhoto(result.Data)) : result.ToErrorResponse();
    }

    [HttpGet("raw/{eventId:int}/{filename}")]
    public async Task<IActionResult> GetRawPhoto(int eventId, string filename, CancellationToken cancellationToken)
    {
        var result = await _eventPhotoService.GetRawPhotoAsync(eventId, filename, cancellationToken);
        if (!result.Success)
            return result.ToErrorResponse();

        return File(result.Data, "image/jpeg", filename);
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadImage([FromBody] UploadMessageDto uploadMessage)
    {
        if (uploadMessage.EventId <= 0)
        {
            return BadRequest("Invalid event ID.");
        }

        var result = await _eventPhotoService.UploadPhoto(RequestUserId(), uploadMessage);
        return result.Success ? Ok(result.Data) : result.ToErrorResponse();
    }

    [HttpGet("sas/{eventId:int}")]
    public async Task<ActionResult<SasUriResponseDto>> GetUploadSasUrl([FromRoute] int eventId)
    {
        var result = await _eventPhotoService.GetUploadSasUri(eventId);
        return result.Success ? Ok(result.Data) : result.ToErrorResponse();
    }

    [HttpPost("thumbnail")]
    [AllowAnonymous]
    public async Task<IActionResult> UploadThumbnail([FromForm] IFormFile? thumbnail, [FromForm] int eventId)
    {
        if (Request.Headers.Authorization != _configuration["ImageProcessorOptions:AuthorizationKey"])
        {
            return Unauthorized("Invalid authorization key.");
        }

        if (thumbnail == null || thumbnail.Length == 0)
            return BadRequest("No thumbnail uploaded.");

        if (eventId <= 0)
        {
            return BadRequest("Invalid event ID.");
        }

        var ms = new MemoryStream();
        await thumbnail.CopyToAsync(ms);
        ms.Position = 0;

        var result = await _eventPhotoService.SaveThumbnail(eventId, _env.ContentRootPath, thumbnail.FileName, ms);
        return result.Success ? Ok(result.Data) : result.ToErrorResponse();
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
