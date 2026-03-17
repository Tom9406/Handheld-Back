using Handheld.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Xml;
using Wms.Api.Common;
using Wms.Api.Data;
using Wms.Api.Dtos.Item;
using Wms.Api.DTOs;
using Wms.Api.Entities;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;

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
        [FromQuery] string? search = null,
        [FromQuery] PaginationParameters? pagination = null)
    {
        pagination ??= new PaginationParameters();

        var query = _db.Items
            .AsNoTracking()
            .AsQueryable();

        // filtro por compañía
        if (companyId.HasValue)
            query = query.Where(i => i.CompanyId == companyId.Value);

        // solo activos
        if (activeOnly)
            query = query.Where(i => i.IsActive);

        // filtro de búsqueda
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();

            query = query.Where(i =>
                i.ItemNo.ToLower().Contains(term) ||
                i.Description.ToLower().Contains(term) ||
                i.UOM.ToLower().Contains(term) ||
                i.ItemType.ToLower().Contains(term)
            );
        }

        var totalRecords = await query.CountAsync();

        var items = await query
            .OrderBy(i => i.ItemNo)
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
                CreateAt = i.CreatedAt,
                Images = i.Images.Select(img => new ItemImageDto
                {
                    Id = img.Id,
                    Url = img.Url,
                    IsPrimary = img.IsPrimary
                }).ToList()
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
                UpdatedAt = i.UpdatedAt,

                Part_No = i.Part_No,
                Alternative_Code = i.Alternative_Code,
                Images = i.Images.Select(img => new ItemImageDto
                {
                    Id = img.Id,
                    Url = img.Url,
                    IsPrimary = img.IsPrimary
                }).ToList()

            })
            .FirstOrDefaultAsync();

        if (item == null)
            return NotFound();

        return Ok(item);
    }



    [HttpGet("export")]
    public IActionResult ExportTemplate()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Items");

        var headers = new[]
        {
        "Description",
        "UOM",
        "BaseUOM",
        "SalesUOM",
        "PurchaseUOM",
        "ItemType",
        "IsActive",
        "Barcode",
        "AltBarcode",
        "IsLotTracked",
        "IsSerialTracked",
        "IsExpirationTracked",
        "UnitWeight",
        "UnitVolume",
        "Length",
        "Width",
        "Height",
        "CategoryCode",
        "Brand",
        "ABCClass",
        "Part_No",
        "Alternative_Code"
    };

        // Definir rango de la tabla
        var range = worksheet.Range(1, 1, 1, headers.Length);

        // Crear tabla
        var table = range.CreateTable();

        // Nombre opcional de la tabla
        table.Name = "Items";

        // Estilo visual 
        table.Theme = XLTableTheme.TableStyleMedium2;

        // Activar filtros 
        table.ShowAutoFilter = true;

        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "items_template.xlsx"
        );
    }


    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ImportItems([FromForm] ImportItemsRequestDto request, [FromQuery] Guid companyId)
    {
        var file = request.File;

        if (file == null || file.Length == 0)
            return BadRequest("File is required");

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Position = 0;

        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet(1);
        var rows = worksheet.RangeUsed()?.RowsUsed().Skip(1);

        if (rows == null)
            return BadRequest("Excel empty");

        var companyExists = await _db.Companies
            .AnyAsync(c => c.Id == companyId);

        if (!companyExists)
            return BadRequest("Company not found");

        // =========================
        // Obtener o crear secuencia
        // =========================
        var sequence = await _db.DocumentSequences
            .Where(x => x.CompanyId == companyId &&
                        x.DocumentType == "ITEM_CREATED")
            .FirstOrDefaultAsync();

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

        int created = 0;
        int skipped = 0;

        foreach (var row in rows)
        {
            var description = row.Cell(1).GetString();
            var barcode = row.Cell(8).GetString();
            var partNo = row.Cell(21).GetString();
            var alternativeCode = row.Cell(22).GetString();

            if (string.IsNullOrWhiteSpace(description))
            {
                skipped++;
                continue;
            }

            // =========================
            // Evitar duplicados
            // =========================
            var exists = await _db.Items.AnyAsync(i =>
                i.CompanyId == companyId &&
                (
                    (!string.IsNullOrEmpty(barcode) && i.Barcode == barcode) ||
                    (!string.IsNullOrEmpty(partNo) && i.Part_No == partNo) ||
                    (!string.IsNullOrEmpty(alternativeCode) && i.Alternative_Code == alternativeCode)
                ));

            if (exists)
            {
                skipped++;
                continue;
            }

            sequence.LastNumber++;

            var itemNo = $"ITM-{sequence.LastNumber:D6}";

            var item = new Item
            {
                Id = Guid.NewGuid(),
                ItemNo = itemNo,
                CompanyId = companyId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "EXCEL_IMPORT",

                Description = description,
                UOM = row.Cell(2).GetString(),
                BaseUOM = row.Cell(3).GetString(),
                SalesUOM = row.Cell(4).GetString(),
                PurchaseUOM = row.Cell(5).GetString(),

                ItemType = row.Cell(6).GetString(),
                IsActive = row.Cell(7).GetBoolean(),

                Barcode = barcode,
                AltBarcode = row.Cell(9).GetString(),

                IsLotTracked = row.Cell(10).GetBoolean(),
                IsSerialTracked = row.Cell(11).GetBoolean(),
                IsExpirationTracked = row.Cell(12).GetBoolean(),

                UnitWeight = row.Cell(13).GetValue<decimal?>(),
                UnitVolume = row.Cell(14).GetValue<decimal?>(),

                Length = row.Cell(15).GetValue<decimal?>(),
                Width = row.Cell(16).GetValue<decimal?>(),
                Height = row.Cell(17).GetValue<decimal?>(),

                CategoryCode = row.Cell(18).GetString(),
                Brand = row.Cell(19).GetString(),
                ABCClass = row.Cell(20).GetString(),

                Part_No = partNo,
                Alternative_Code = alternativeCode
            };

            _db.Items.Add(item);
            created++;
        }

        await _db.SaveChangesAsync();

        return Ok(new
        {
            created,
            skipped
        });
    }


    // ====================================================
    // PUT: api/items/{id}
    // ====================================================
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateItem(Guid id, [FromBody] UpdateItemDto dto, [FromQuery] Guid companyId)
    {
        var item = await _db.Items
    .FirstOrDefaultAsync(i =>
        i.Id == id &&
        i.CompanyId == companyId);

        if (item == null)
            return NotFound("Item not found");

        // ==========================
        // Actualizar campos
        // ==========================

        item.Description = dto.Description;
        item.UOM = dto.UOM;
        item.BaseUOM = dto.BaseUOM;
        item.SalesUOM = dto.SalesUOM;
        item.PurchaseUOM = dto.PurchaseUOM;

        item.ItemType = dto.ItemType;

        item.Barcode = dto.Barcode;
        item.AltBarcode = dto.AltBarcode;

        item.IsLotTracked = dto.IsLotTracked;
        item.IsSerialTracked = dto.IsSerialTracked;
        item.IsExpirationTracked = dto.IsExpirationTracked;

        item.UnitWeight = dto.UnitWeight;
        item.UnitVolume = dto.UnitVolume;

        item.Length = dto.Length;
        item.Width = dto.Width;
        item.Height = dto.Height;

        item.CategoryCode = dto.CategoryCode;
        item.Brand = dto.Brand;
        item.ABCClass = dto.ABCClass;

        item.Part_No = dto.Part_No;
        item.Alternative_Code = dto.Alternative_Code;

        item.IsActive = dto.IsActive;

        item.UpdatedAt = DateTime.UtcNow;
        item.UpdatedBy = "SYSTEM";

        // ==========================
        // IMAGES
        // ==========================

        if (dto.Images != null)
        {
            // Eliminar imágenes actuales
            var existingImages = _db.ItemImages.Where(x => x.ItemId == item.Id);
            _db.ItemImages.RemoveRange(existingImages);

            // Agregar nuevas
            var newImages = dto.Images.Select(img => new ItemImage
            {
                Id = Guid.NewGuid(),
                ItemId = item.Id,
                Url = img.Url,
                IsPrimary = img.IsPrimary
            }).ToList();

            await _db.ItemImages.AddRangeAsync(newImages);
        }

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Item updated successfully",
            item.Id,
            item.ItemNo
        });
    }


    [HttpPost("create_item")]
    public async Task<IActionResult> CreateItem([FromBody] CreateItemDto dto,[FromQuery] Guid companyId)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            // ==============================
            // Obtener o crear secuencia
            // ==============================
            var sequence = await _db.DocumentSequences
                .Where(x => x.CompanyId == companyId &&
                            x.DocumentType == "ITEM_CREATED")
                .OrderByDescending(x => x.LastNumber)
                .FirstOrDefaultAsync();

            if (sequence == null)
            {
                var lastItemNumber = await _db.Items
                    .Where(x => x.CompanyId == companyId)
                    .OrderByDescending(x => x.ItemNo)
                    .Select(x => x.ItemNo)
                    .FirstOrDefaultAsync();

                int lastNumber = 0;

                if (!string.IsNullOrEmpty(lastItemNumber))
                {
                    lastNumber = int.Parse(lastItemNumber.Replace("ITM-", ""));
                }

                sequence = new DocumentSequence
                {
                    Id = Guid.NewGuid(),
                    CompanyId = companyId,
                    DocumentType = "ITEM_CREATED",
                    LastNumber = lastNumber
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
            return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
        }
    }


}
