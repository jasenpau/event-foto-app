using EventFoto.Core.ControllerBase;
using EventFoto.Core.EventPhotos;
using EventFoto.Core.Extensions;
using EventFoto.Data.DTOs;
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
    public async Task<IActionResult> GetUploadSasUrl([FromRoute] int eventId)
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

        var thumbnailsDir = Path.Combine(_env.ContentRootPath, "Thumbnails", eventId.ToString());

        if (!Directory.Exists(thumbnailsDir))
            Directory.CreateDirectory(thumbnailsDir);

        var filePath = Path.Combine(thumbnailsDir, thumbnail.FileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await thumbnail.CopyToAsync(stream);

        return Ok("OK");
    }
}
