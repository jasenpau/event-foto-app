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
    private readonly IEventPhotoService _eventPhotoService;
    private readonly string _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

    public ImageController(IEventPhotoService eventPhotoService)
    {
        _eventPhotoService = eventPhotoService;
    }

    [HttpGet("photos")]
    public async Task<ActionResult> GetImages()
    {
        if (!Directory.Exists(_uploadPath))
            return Ok(new List<string>());

        var files = Directory.GetFiles(_uploadPath)
            .Select(Path.GetFileName)
            .Select(fileName => $"{Request.Scheme}://{Request.Host}/uploads/{fileName}")
            .ToList();

        return Ok(files);
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
}
