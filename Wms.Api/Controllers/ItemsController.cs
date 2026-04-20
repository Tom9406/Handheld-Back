using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wms.Api.Common;
using Wms.Api.Data;
using Wms.Api.Dtos.Item;
using Wms.Api.DTOs;
using Wms.Api.Entities;

namespace Wms.Api.Controllers;

[Route("api/items")]
public class ItemsController : BaseController
{
    private readonly WmsDbContext _db;

    public ItemsController(WmsDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<ItemDto>>> GetItems(
        [FromQuery] Guid? companyId,
        [FromQuery] bool activeOnly = true,
        [FromQuery] string? search = null,
        [FromQuery] PaginationParameters? pagination = null)
    {
        pagination ??= new PaginationParameters();
        var activeCompanyId = ResolveCompanyId(companyId);

        var query = _db.Items
            .AsNoTracking()
            .Where(i => i.CompanyId == activeCompanyId);

        if (activeOnly)
            query = query.Where(i => i.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();

            query = query.Where(i =>
                i.ItemNo.ToLower().Contains(term) ||
                (i.Description != null && i.Description.ToLower().Contains(term)) ||
                i.UOM.ToLower().Contains(term) ||
                i.ItemType.ToLower().Contains(term));
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

        return Ok(new PagedResponse<ItemDto>
        {
            Data = items,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pagination.PageSize)
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ItemDetailDto>> GetItem(Guid id, [FromQuery] Guid? companyId = null)
    {
        var activeCompanyId = ResolveCompanyId(companyId);

        var item = await _db.Items
            .AsNoTracking()
            .Where(i => i.Id == id && i.CompanyId == activeCompanyId)
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

        var qtyOnHand = await _db.CurrentStock
            .Where(s => s.ItemId == id)
            .SumAsync(s => (decimal?)s.StockQty) ?? 0;

        var receiving = await _db.ReceivingLines
            .Where(r => r.ItemId == id && r.CompanyId == activeCompanyId)
            .GroupBy(r => r.ItemId)
            .Select(g => new
            {
                Expected = g.Sum(x => x.QuantityExpected),
                Received = g.Sum(x => x.QuantityReceived)
            })
            .FirstOrDefaultAsync();

        var shipment = await _db.ShipmentLines
            .Where(s => s.ItemId == id && s.CompanyId == activeCompanyId)
            .GroupBy(s => s.ItemId)
            .Select(g => new
            {
                Ordered = g.Sum(x => x.OrderedQty),
                Shipped = g.Sum(x => x.ShippedQty)
            })
            .FirstOrDefaultAsync();

        var qtyOnPurchOrder = receiving != null ? receiving.Expected - receiving.Received : 0;
        var qtyOnSalesOrder = shipment != null ? shipment.Ordered - shipment.Shipped : 0;

        item.QuantityOnHand = qtyOnHand;
        item.QtyOnPurchOrder = qtyOnPurchOrder;
        item.QtyOnSalesOrder = qtyOnSalesOrder;
        item.QtyAvailable = qtyOnHand - qtyOnSalesOrder;
        item.StockoutWarning = qtyOnHand <= 0;
        item.QtyOnComponentLines = 0;
        item.QtyOnProdOrder = 0;
        item.ProjectedKits = 0;

        return Ok(item);
    }

    [HttpGet("export")]
    public IActionResult ExportTemplate()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Items");

        var headers = new[]
        {
            "Description", "UOM", "BaseUOM", "SalesUOM", "PurchaseUOM",
            "ItemType", "IsActive", "Barcode", "AltBarcode", "IsLotTracked",
            "IsSerialTracked", "IsExpirationTracked", "UnitWeight", "UnitVolume",
            "Length", "Width", "Height", "CategoryCode", "Brand", "ABCClass",
            "Part_No", "Alternative_Code"
        };

        for (var i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
        }

        worksheet.Range(1, 1, 1, headers.Length).CreateTable().Theme = XLTableTheme.TableStyleMedium2;
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "items_template.xlsx");
    }

    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ImportItems([FromForm] ImportItemsRequestDto request, [FromQuery] Guid? companyId = null)
    {
        var activeCompanyId = ResolveCompanyId(companyId);
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

        var companyExists = await _db.Companies.AnyAsync(c => c.Id == activeCompanyId);
        if (!companyExists)
            return BadRequest("Company not found");

        var sequence = await _db.DocumentSequences
            .Where(x => x.CompanyId == activeCompanyId && x.DocumentType == "ITEM_CREATED")
            .FirstOrDefaultAsync();

        if (sequence == null)
        {
            sequence = new DocumentSequence
            {
                Id = Guid.NewGuid(),
                CompanyId = activeCompanyId,
                DocumentType = "ITEM_CREATED",
                LastNumber = 0
            };

            _db.DocumentSequences.Add(sequence);
        }

        var created = 0;
        var skipped = 0;

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

            var exists = await _db.Items.AnyAsync(i =>
                i.CompanyId == activeCompanyId &&
                ((!string.IsNullOrEmpty(barcode) && i.Barcode == barcode) ||
                 (!string.IsNullOrEmpty(partNo) && i.Part_No == partNo) ||
                 (!string.IsNullOrEmpty(alternativeCode) && i.Alternative_Code == alternativeCode)));

            if (exists)
            {
                skipped++;
                continue;
            }

            sequence.LastNumber++;
            var itemNo = $"ITM-{sequence.LastNumber:D6}";

            _db.Items.Add(new Item
            {
                Id = Guid.NewGuid(),
                ItemNo = itemNo,
                CompanyId = activeCompanyId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = UserEmail,
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
            });

            created++;
        }

        await _db.SaveChangesAsync();

        return Ok(new { created, skipped });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateItem(Guid id, [FromBody] UpdateItemDto dto, [FromQuery] Guid? companyId = null)
    {
        var activeCompanyId = ResolveCompanyId(companyId);

        var item = await _db.Items
            .FirstOrDefaultAsync(i => i.Id == id && i.CompanyId == activeCompanyId);

        if (item == null)
            return NotFound("Item not found");

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
        item.UpdatedBy = UserEmail;

        if (dto.Images != null)
        {
            var existingImages = _db.ItemImages.Where(x => x.ItemId == item.Id);
            _db.ItemImages.RemoveRange(existingImages);

            var newImages = dto.Images.Select(img => new ItemImage
            {
                Id = Guid.NewGuid(),
                ItemId = item.Id,
                Url = img.Url,
                IsPrimary = img.IsPrimary,
                CreatedAt = DateTime.UtcNow,
                FileName = string.Empty,
                FilePath = string.Empty
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
    public async Task<IActionResult> CreateItem([FromBody] CreateItemDto dto, [FromQuery] Guid? companyId = null)
    {
        var activeCompanyId = ResolveCompanyId(companyId);

        using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            var sequence = await _db.DocumentSequences
                .Where(x => x.CompanyId == activeCompanyId && x.DocumentType == "ITEM_CREATED")
                .OrderByDescending(x => x.LastNumber)
                .FirstOrDefaultAsync();

            if (sequence == null)
            {
                var lastItemNumber = await _db.Items
                    .Where(x => x.CompanyId == activeCompanyId)
                    .OrderByDescending(x => x.ItemNo)
                    .Select(x => x.ItemNo)
                    .FirstOrDefaultAsync();

                var lastNumber = 0;

                if (!string.IsNullOrEmpty(lastItemNumber))
                    lastNumber = int.Parse(lastItemNumber.Replace("ITM-", ""));

                sequence = new DocumentSequence
                {
                    Id = Guid.NewGuid(),
                    CompanyId = activeCompanyId,
                    DocumentType = "ITEM_CREATED",
                    LastNumber = lastNumber
                };

                _db.DocumentSequences.Add(sequence);
            }

            sequence.LastNumber++;
            var itemNo = $"ITM-{sequence.LastNumber:D6}";

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
                Part_No = dto.Part_No,
                Alternative_Code = dto.Alternative_Code,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = UserEmail,
                CompanyId = activeCompanyId,
                Images = new List<ItemImage>()
            };

            _db.Items.Add(item);

            if (dto.Images != null && dto.Images.Any())
            {
                var isFirst = true;

                foreach (var img in dto.Images)
                {
                    _db.ItemImages.Add(new ItemImage
                    {
                        Id = Guid.NewGuid(),
                        ItemId = item.Id,
                        Url = img.Url,
                        IsPrimary = isFirst,
                        CreatedAt = DateTime.UtcNow,
                        FileName = string.Empty,
                        FilePath = string.Empty
                    });

                    isFirst = false;
                }
            }

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
