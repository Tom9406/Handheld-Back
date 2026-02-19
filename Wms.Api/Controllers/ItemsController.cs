using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wms.Api.Common;
using Wms.Api.Data;
using Wms.Api.Dtos.Item;

namespace Wms.Api.Controllers;

[ApiController]
[Route("api/items")]
public class ItemsController : ControllerBase
{
    private readonly WmsDbContext _db;

    public ItemsController(WmsDbContext db)
    {
        _db = db;
    }

    // ====================================================
    // GET: api/items?companyId={companyId}&activeOnly=true&pageNumber=1&pageSize=20
    // ====================================================
    [HttpGet]
    public async Task<ActionResult<PagedResponse<ItemDto>>> GetItems(
        [FromQuery] Guid? companyId,
        [FromQuery] bool activeOnly = true,
        [FromQuery] PaginationParameters? pagination = null)
    {
        pagination ??= new PaginationParameters();

        var query = _db.Items
            .AsNoTracking()
            .AsQueryable();

        if (companyId.HasValue)
            query = query.Where(i => i.CompanyId == companyId.Value);

        if (activeOnly)
            query = query.Where(i => i.IsActive);

        var totalRecords = await query.CountAsync();

        var items = await query
            .OrderBy(i => i.ItemNo) // orden estable obligatorio para paginación
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(i => new ItemDto
            {
                Id = i.Id,
                ItemNo = i.ItemNo,
                Description = i.Description,
                UOM = i.UOM,
                IsActive = i.IsActive,
                ItemType = i.ItemType,
                Barcode = i.Barcode,
                BaseUOM = i.BaseUOM,
                CategoryCode = i.CategoryCode,
                Brand = i.Brand,
                ABCClass = i.ABCClass,
                CompanyId = i.CompanyId
            })
            .ToListAsync();

        var response = new PagedResponse<ItemDto>
        {
            Data = items,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pagination.PageSize)
        };

        return Ok(response);
    }

    // ====================================================
    // GET: api/items/{id}
    // ====================================================
    [HttpGet("{id}")]
    public async Task<ActionResult<ItemDetailDto>> GetItem(Guid id)
    {
        var item = await _db.Items
            .AsNoTracking()
            .Where(i => i.Id == id)
            .Select(i => new ItemDetailDto
            {
                Id = i.Id,
                ItemNo = i.ItemNo,
                Description = i.Description,

                UOM = i.UOM,
                BaseUOM = i.BaseUOM,
                SalesUOM = i.SalesUOM,
                PurchaseUOM = i.PurchaseUOM,
                ConversionFactor = i.ConversionFactor,

                IsActive = i.IsActive,
                ItemType = i.ItemType,

                Barcode = i.Barcode,
                AltBarcode = i.AltBarcode,

                IsLotTracked = i.IsLotTracked,
                IsSerialTracked = i.IsSerialTracked,
                IsExpirationTracked = i.IsExpirationTracked,

                UnitWeight = i.UnitWeight,
                UnitVolume = i.UnitVolume,
                Length = i.Length,
                Width = i.Width,
                Height = i.Height,

                CategoryCode = i.CategoryCode,
                Brand = i.Brand,
                ABCClass = i.ABCClass,

                CompanyId = i.CompanyId,
                CompanyName = i.Company.Name,

                CreatedBy = i.CreatedBy,
                CreatedAt = i.CreatedAt,
                UpdatedBy = i.UpdatedBy,
                UpdatedAt = i.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (item == null)
            return NotFound();

        return Ok(item);
    }
}
