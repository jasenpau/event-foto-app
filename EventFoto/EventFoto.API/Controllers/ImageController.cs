using EventFoto.API.Extensions;
using EventFoto.API.Filters;
using EventFoto.Core.EventPhotos;
using EventFoto.Data.DTOs;
using EventFoto.Data.Enums;
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

    [HttpPost("bulk-delete")]
    [AccessGroupFilter(UserGroup.Photographer)]
    public async Task<ActionResult<int>> BulkDelete([FromBody] BulkPhotoModifyDto dto,
        CancellationToken cancellationToken)
    {
        var deleteResult = await _eventPhotoService.DeletePhotosAsync(dto.PhotoIds, cancellationToken);
        return deleteResult.Success ? Ok(deleteResult.Data) : deleteResult.ToErrorResponse();
    }

    [HttpPost("bulk-download")]
    public async Task<ActionResult<DownloadRequestDto>> BulkDownload([FromBody] BulkPhotoDownloadDto dto)
    {
        var userId = RequestUserId();
        var downloadResult = await _eventPhotoService.DownloadPhotosAsync(userId, dto.PhotoIds, dto.Processed, dto.Quality);
        return downloadResult.Success
            ? Ok(DownloadRequestDto.FromModel(downloadResult.Data))
            : downloadResult.ToErrorResponse();
    }

    [HttpPost("bulk-move")]
    [AccessGroupFilter(UserGroup.Photographer)]
    public async Task<ActionResult<int>> BulkMove([FromBody] BulkPhotoMoveDto dto)
    {
        var result = await _eventPhotoService.MovePhotos(dto.PhotoIds, dto.TargetGalleryId);
        return result.Success ? Ok(result.Data) : result.ToErrorResponse();
    }

    [HttpPost("upload")]
    [AccessGroupFilter(UserGroup.Photographer)]
    public async Task<ActionResult<UploadBatchDto>> UploadImages([FromBody] UploadMessageDto uploadMessage)
    {
        if (uploadMessage.EventId <= 0)
        {
            return BadRequest(new ProblemDetails
            {
                Detail = "Invalid event ID",
            });
        }

        var result = await _eventPhotoService.UploadPhotoBatch(RequestUserId(), uploadMessage);
        return result.Success ? Ok(new UploadBatchDto
        {
            Id = result.Data.Id,
            PhotoCount = uploadMessage.PhotoFilenames.Count,
            Ready = result.Data.IsReady,
        }) : result.ToErrorResponse();
    }

    [HttpGet("batch/{batchId:int}")]
    public async Task<ActionResult<UploadBatchDto>> GetBatch(int batchId)
    {
        var result = await _eventPhotoService.GetUploadBatchById(batchId);
        return result.Success ? Ok(new UploadBatchDto
        {
            Id = result.Data.Id,
            PhotoCount = result.Data.EventPhotos.Count,
            Ready = result.Data.IsReady,
        }) : result.ToErrorResponse();
    }

    [HttpGet("sas")]
    public ActionResult<ActionResult<SasUriResponseDto>> GetReadOnlySasToken()
    {
        var result = _eventPhotoService.GetReadOnlySasUri();
        return result.Success ? Ok(result.Data) : result.ToErrorResponse();
    }

    [HttpGet("sas/{eventId:int}")]
    [AccessGroupFilter(UserGroup.Photographer)]
    public async Task<ActionResult<SasUriResponseDto>> GetUploadSasUri([FromRoute] int eventId)
    {
        var result = await _eventPhotoService.GetUploadSasUri(eventId);
        return result.Success ? Ok(result.Data) : result.ToErrorResponse();
    }

    [HttpGet("search")]
    public async Task<ActionResult<PagedData<string, EventPhotoListDto>>> SearchEventPhotos(
        [FromQuery] EventPhotoSearchParams searchParams)
    {
        var searchResult = await _eventPhotoService.SearchPhotosAsync(searchParams);
        if (!searchResult.Success)
        {
            return searchResult.ToErrorResponse();
        }

        var result = searchResult.Data.ToDto(EventPhotoListDto.FromEventPhoto);
        return Ok(result);
    }

    [HttpGet("archive-download/{id:int}")]
    public async Task<ActionResult<EventPhotoDto>> GetArchiveDownloadById(int id)
    {
        var result = await _eventPhotoService.GetDownloadRequestAsync(RequestUserId(), id);
        return result.Success ? Ok(DownloadRequestDto.FromModel(result.Data)) : result.ToErrorResponse();
    }
}
