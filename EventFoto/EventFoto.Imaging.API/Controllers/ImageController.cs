using EventFoto.Core.ControllerBase;
using EventFoto.Core.EventPhotoService;
using EventFoto.Core.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventFoto.Imaging.API.Controllers;

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
    public async Task<IActionResult> UploadImage([FromForm] IFormFile? image, [FromForm] int eventId, [FromForm] DateTime captureDate)
    {
        if (image is null || image.Length == 0)
            return BadRequest("Invalid file.");

        if (eventId <= 0)
        {
            return BadRequest("Invalid event ID.");
        }

        var uploadPhotoData = new EventPhotoUploadData
        {
            ImageFile = image,
            CaptureDate = captureDate,
            EventId = eventId,
            UserId = RequestUserId()
        };

        var result = await _eventPhotoService.UploadPhoto(uploadPhotoData);
        return result.Success ? Ok(uploadPhotoData) : result.ToErrorResponse();
    }

    [HttpGet("sas/{eventId:int}")]
    public async Task<IActionResult> GetUploadSasUrl([FromRoute] int eventId)
    {
        var result = await _eventPhotoService.GetUploadSasUri(eventId);
        return result.Success ? Ok(result.Data) : result.ToErrorResponse();
    }
}
