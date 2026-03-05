using Handheld.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Xml;
using Wms.Api.Common;
using Wms.Api.Data;
using Wms.Api.Dtos.Item;
using Wms.Api.DTOs;
using Wms.Api.Entities;

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
                CompanyId = i.CompanyId,
                CreateAt = i.CreatedAt
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

    [HttpPost("create_item")]
    public async Task<IActionResult> CreateItem(
    [FromBody] CreateItemDto dto,
    [FromQuery] Guid companyId)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            // ==============================
            // Obtener o crear secuencia
            // ==============================
            var sequence = await _db.DocumentSequences
                .FirstOrDefaultAsync(x =>
                    x.CompanyId == companyId &&
                    x.DocumentType == "ITEM_CREATED");

            if (sequence == null)
            {
                sequence = new DocumentSequence
                {
                    Id = Guid.NewGuid(),
                    CompanyId = companyId,
                    DocumentType = "ITEM_CREATED",
                    LastNumber = 0
                };

                _db.DocumentSequences.Add(sequence);
            }

            sequence.LastNumber++;

            var itemNo = $"ITM-{sequence.LastNumber:D6}";

            // ==============================
            // Crear entidad Item
            // ==============================
            var item = new Item
            {
                Id = Guid.NewGuid(),
                ItemNo = itemNo,
                Description = dto.Description,
                UOM = dto.UOM,
                ItemType = dto.ItemType,
                Barcode = dto.Barcode,
                AltBarcode = dto.AltBarcode,

                IsLotTracked = dto.IsLotTracked,
                IsSerialTracked = dto.IsSerialTracked,
                IsExpirationTracked = dto.IsExpirationTracked,

                UnitWeight = dto.UnitWeight,
                UnitVolume = dto.UnitVolume,

                BaseUOM = dto.BaseUOM,
                SalesUOM = dto.SalesUOM,
                PurchaseUOM = dto.PurchaseUOM,

                CategoryCode = dto.CategoryCode,
                Brand = dto.Brand,

                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "SYSTEM",
                CompanyId = companyId,

                Part_No = dto.Part_No,
                Alternative_Code = dto.Alternative_Code
            };


            _db.Items.Add(item);

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                message = "Item created successfully",
                item.Id,
                item.ItemNo
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, ex.Message);
        }
    }


}
