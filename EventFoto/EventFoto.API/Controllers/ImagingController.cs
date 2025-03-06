using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventFoto.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class ImagingController : ControllerBase
{
    private readonly string _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
    
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
    
    [HttpPost("camera-upload")]
    public async Task<IActionResult> UploadImage([FromForm] IFormFile image)
    {
        if (image is null || image.Length == 0)
            return BadRequest("Invalid file.");
        
        
        
        var filePath = Path.Combine(_uploadPath, $"{Guid.NewGuid()}.jpg");
        
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await image.CopyToAsync(stream);
        }

        return Ok(new { Message = "File uploaded successfully" });
    }
}
