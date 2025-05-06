using EventFoto.API.Extensions;
using EventFoto.API.Filters;
using EventFoto.Core.Watermarks;
using EventFoto.Data.DTOs;
using EventFoto.Data.Enums;
using EventFoto.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventFoto.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class WatermarkController : AppControllerBase
{
    private readonly IWatermarkService _watermarkService;

    public WatermarkController(IWatermarkService watermarkService)
    {
        _watermarkService = watermarkService;
    }

    [HttpPost]
    [AccessGroupFilter(UserGroup.EventAdmin)]
    public async Task<ActionResult<WatermarkDto>> UploadWatermark([FromForm] string name, [FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new ProblemDetails
            {
                Detail = "No file provided",
                Status = StatusCodes.Status400BadRequest,
            });

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        var result = await _watermarkService.UploadWatermarkAsync(name, memoryStream, cancellationToken);
        return result.Success 
            ? Ok(WatermarkDto.FromModel(result.Data)) 
            : result.ToErrorResponse();
    }

    [HttpDelete("{id:int}")]
    [AccessGroupFilter(UserGroup.EventAdmin)]
    public async Task<ActionResult<bool>> DeleteWatermark(int id, CancellationToken cancellationToken)
    {
        var result = await _watermarkService.DeleteWatermarkAsync(id, cancellationToken);
        return result.Success 
            ? Ok(result.Data) 
            : result.ToErrorResponse();
    }

    [HttpGet("{id:int}")]
    [AccessGroupFilter(UserGroup.EventAdmin)]
    public ActionResult<WatermarkDto> GetWatermark(int id)
    {
        var result = _watermarkService.GetWatermarkAsync(id);
        return result.Success 
            ? Ok(WatermarkDto.FromModel(result.Data)) 
            : result.ToErrorResponse();
    }

    [HttpGet("search")]
    [AccessGroupFilter(UserGroup.EventAdmin)]
    public ActionResult<PagedData<string, WatermarkDto>> SearchWatermarks([FromQuery] WatermarkSearchParams searchParams)
    {
        var result = _watermarkService.SearchWatermarksAsync(searchParams);
        return result.Success ? Ok(result.Data.ToDto(WatermarkDto.FromModel)) : result.ToErrorResponse();
    }
}

