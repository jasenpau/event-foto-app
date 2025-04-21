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
    private readonly IEventPhotoService _eventPhotoService;

    public ImageController(IEventPhotoService eventPhotoService)
    {
        _eventPhotoService = eventPhotoService;
    }

    [HttpGet("details/{id:int}")]
    public async Task<ActionResult<EventPhotoDto>> GetPhotoById(int id)
    {
        var result = await _eventPhotoService.GetByIdAsync(id);
        return result.Success ? Ok(EventPhotoDto.FromEventPhoto(result.Data)) : result.ToErrorResponse();
    }

    [HttpPost("bulk-action")]
    public async Task<ActionResult<int>> BulkAction([FromBody] BulkPhotoModifyParams bulkPhotoModifyParams,
        CancellationToken cancellationToken)
    {
        switch (bulkPhotoModifyParams.Action)
        {
            case "delete":
                var result = await _eventPhotoService.DeletePhotosAsync(bulkPhotoModifyParams.PhotoIds, cancellationToken);
                return result.Success ? Ok(result.Data) : result.ToErrorResponse();
            default:
                return BadRequest();
        }
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

    [HttpGet("sas")]
    public ActionResult<string> GetReadOnlySasToken()
    {
        var result = _eventPhotoService.GetReadOnlySasUri();
        return result.Success ? Ok(result.Data) : result.ToErrorResponse();
    }

    [HttpGet("sas/{eventId:int}")]
    public async Task<ActionResult<SasUriResponseDto>> GetUploadSasUri([FromRoute] int eventId)
    {
        var result = await _eventPhotoService.GetUploadSasUri(eventId);
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
