using EventFoto.Core.ControllerBase;
using EventFoto.Core.Extensions;
using EventFoto.Core.Watermarks;
using EventFoto.Data.DTOs;
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
    public async Task<ActionResult<WatermarkDto>> UploadWatermark([FromForm] string name, [FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        var result = await _watermarkService.UploadWatermarkAsync(name, memoryStream, cancellationToken);
        return result.Success 
            ? Ok(WatermarkDto.FromModel(result.Data)) 
            : result.ToErrorResponse();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<bool>> DeleteWatermark(int id, CancellationToken cancellationToken)
    {
        var result = await _watermarkService.DeleteWatermarkAsync(id, cancellationToken);
        return result.Success 
            ? Ok(result.Data) 
            : result.ToErrorResponse();
    }

    [HttpGet("{id:int}")]
    public ActionResult<WatermarkDto> GetWatermark(int id)
    {
        var result = _watermarkService.GetWatermarkAsync(id);
        return result.Success 
            ? Ok(WatermarkDto.FromModel(result.Data)) 
            : result.ToErrorResponse();
    }

    [HttpGet("search")]
    public ActionResult<PagedData<string, WatermarkDto>> SearchWatermarks([FromQuery] WatermarkSearchParams searchParams)
    {
        var result = _watermarkService.SearchWatermarksAsync(searchParams);
        return result.Success ? Ok(result.Data.ToDto(WatermarkDto.FromModel)) : result.ToErrorResponse();
    }
}

