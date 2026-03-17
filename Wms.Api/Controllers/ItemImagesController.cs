
using Microsoft.AspNetCore.Mvc;

using Wms.Api.Data;
using Wms.Api.DTOs;
using Wms.Api.Entities;

namespace Wms.Api.Controllers;

[ApiController]
[Route("api/item-images")]
public class ItemImagesController : ControllerBase
{
    private readonly WmsDbContext _db;
    private readonly IWebHostEnvironment _env;

    public ItemImagesController(
        WmsDbContext db,
        IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload([FromForm] UploadItemImageDto dto)
    {
        if (dto.File == null || dto.File.Length == 0)
            return BadRequest("File required");

        var item = await _db.Items.FindAsync(dto.ItemId);

        if (item == null)
            return NotFound();

        var folder = Path.Combine(
            _env.WebRootPath,
            "images",
            "items",
            dto.ItemId.ToString());

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        var fileName = Guid.NewGuid() + Path.GetExtension(dto.File.FileName);

        var filePath = Path.Combine(folder, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await dto.File.CopyToAsync(stream);

        var image = new ItemImage
        {
            Id = Guid.NewGuid(),
            ItemId = dto.ItemId,
            FileName = fileName,
            Url = "/images/items/" + dto.ItemId + "/" + fileName,
            FilePath = filePath,
            CreatedAt = DateTime.UtcNow
        };

        _db.ItemImages.Add(image);
        await _db.SaveChangesAsync();

        var result = new ItemImageDto
        {
            Id = image.Id,
            Url = image.Url,
            IsPrimary = image.IsPrimary
        };

        return Ok(result);
    }
}