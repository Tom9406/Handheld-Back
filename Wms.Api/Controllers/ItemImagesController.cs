using Microsoft.AspNetCore.Mvc;
using Wms.Api.Data;
using Wms.Api.DTOs;
using Wms.Api.Entities;

namespace Wms.Api.Controllers;

[Route("api/item-images")]
public class ItemImagesController : BaseController
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

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

        if (dto.File.Length > MaxFileSizeBytes)
            return BadRequest("The file is too large.");

        var extension = Path.GetExtension(dto.File.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            return BadRequest("Unsupported image format.");

        var item = await _db.Items.FindAsync(dto.ItemId);
        if (item == null)
            return NotFound();

        if (!HasCompanyAccess(item.CompanyId))
            return Forbid();

        var webRootPath = string.IsNullOrWhiteSpace(_env.WebRootPath)
            ? Path.Combine(AppContext.BaseDirectory, "wwwroot")
            : _env.WebRootPath;

        var folder = Path.Combine(webRootPath, "images", "items", dto.ItemId.ToString());
        Directory.CreateDirectory(folder);

        var fileName = Guid.NewGuid() + extension;
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

        return Ok(new ItemImageDto
        {
            Id = image.Id,
            Url = image.Url,
            IsPrimary = image.IsPrimary
        });
    }
}
