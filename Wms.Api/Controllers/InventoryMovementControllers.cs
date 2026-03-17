using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using Handheld.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wms.Api.Data;
using Wms.Api.Dtos.InventoryMovement;
using Wms.Api.DTOs;

namespace Wms.Api.Controllers;

[ApiController]
[Route("api/movements")]
public class InventoryMovementsController : ControllerBase
{
    private readonly WmsDbContext _db;

    public InventoryMovementsController(WmsDbContext db)
    {
        _db = db;
    }

    // ======================================================
    // GET: api/movements?pageNumber=1&pageSize=20
    //      &itemId=GUID
    //      &binId=GUID
    //      &movementType=IN
    //      &referenceNo=SO001
    //      &dateFrom=2025-01-01
    //      &dateTo=2025-01-31
    // ======================================================
    [HttpGet]
    public async Task<IActionResult> GetInventoryMovements(
    Guid companyId,
    int pageNumber = 1,
    int pageSize = 20,
    Guid? itemId = null,
    Guid? binId = null,
    string? movementType = null,
    string? referenceNo = null,
    DateTime? dateFrom = null,
    DateTime? dateTo = null,
    bool sortDesc = true,
    string? search = null)
    {
        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0) pageSize = 20;
        if (pageSize > 200) pageSize = 200;

        var query = _db.InventoryMovements
            .AsNoTracking()
            .Where(x => x.CompanyId == companyId);

        // ========================
        // Global Search
        // ========================
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search}%";

            query = query.Where(x =>
                EF.Functions.Like(x.Item.ItemNo, pattern) ||
                EF.Functions.Like(x.Item.Description, pattern) ||
                EF.Functions.Like(x.Bin.BinCode, pattern) ||
                (x.ReferenceNo != null && EF.Functions.Like(x.ReferenceNo, pattern)) ||
                EF.Functions.Like(x.MovementType, pattern));
        }

        // ========================
        // Filters
        // ========================
        if (itemId.HasValue)
            query = query.Where(x => x.ItemId == itemId);

        if (binId.HasValue)
            query = query.Where(x => x.BinId == binId);

        if (!string.IsNullOrWhiteSpace(movementType))
            query = query.Where(x => x.MovementType == movementType);

        if (!string.IsNullOrWhiteSpace(referenceNo))
            query = query.Where(x =>
                x.ReferenceNo != null &&
                x.ReferenceNo.Contains(referenceNo));

        if (dateFrom.HasValue)
            query = query.Where(x => x.CreatedAt >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(x => x.CreatedAt <= dateTo.Value);

        // ========================
        // Sorting
        // ========================
        query = sortDesc
            ? query.OrderByDescending(x => x.CreatedAt)
            : query.OrderBy(x => x.CreatedAt);

        // ========================
        // Projection
        // ========================
        var baseQuery = query.Select(x => new InventoryMovementDto
        {
            Id = x.Id,
            ItemId = x.ItemId,
            ItemNo = x.Item.ItemNo,
            ItemDescription = x.Item.Description,
            BinId = x.BinId,
            BinCode = x.Bin.BinCode,
            Quantity = x.Quantity,
            MovementType = x.MovementType,
            ReferenceNo = x.ReferenceNo,
            CreatedAt = x.CreatedAt
        });

        var totalRecords = await baseQuery.CountAsync();

        var data = await baseQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var response = new
        {
            pageNumber,
            pageSize,
            totalRecords,
            totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize),
            data
        };

        return Ok(response);
    }

    // ======================================================
    // GET: api/movements/{id}
    // ======================================================
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, Guid companyId)
    {
        var movement = await _db.InventoryMovements
            .AsNoTracking()
            .Where(x => x.Id == id && x.CompanyId == companyId)
            .Select(x => new InventoryMovementDetailDto
            {
                Id = x.Id,
                ItemId = x.ItemId,
                ItemNo = x.Item.ItemNo,
                ItemDescription = x.Item.Description,
                BinId = x.BinId,
                BinCode = x.Bin.BinCode,
                Quantity = x.Quantity,
                MovementType = x.MovementType,
                ReferenceNo = x.ReferenceNo,
                CreatedAt = x.CreatedAt

            })
            .FirstOrDefaultAsync();

        if (movement == null)
            return NotFound();

        return Ok(movement);
    }

    [HttpGet("item/{itemId}")]
    public async Task<IActionResult> GetItemMovements(Guid itemId, Guid companyId)
    {
        var movements = await _db.InventoryMovements
            .Where(x => x.CompanyId == companyId && x.ItemId == itemId)
            .Select(m => new InventoryMovementDto
            {
                Id = m.Id,
                ItemId = m.ItemId,
                ItemNo = m.Item.ItemNo,
                ItemDescription = m.Item.Description,
                BinId = m.BinId,
                BinCode = m.Bin.BinCode,
                Quantity = m.Quantity,
                MovementType = m.MovementType,
                ReferenceNo = m.ReferenceNo,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync();

        return Ok(movements);
    }



    [HttpGet("export")]
    public IActionResult ExportTemplate()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("InventoryMovements");

        var headers = new[]
        {
        "ItemNo",
        "BinCode",
        "WarehouseCode",
        "Quantity",
        "MovementType",
        "ReferenceNo",
        "EntityType",
        "EntityReference",
        "OldStatus",
        "NewStatus",
        "SourceSystem",
        "CreatedBy"
    };

        // Definir rango de la tabla
        var range = worksheet.Range(1, 1, 1, headers.Length);

        // Crear tabla
        var table = range.CreateTable();

        // Nombre opcional de la tabla
        table.Name = "InventoryMovementsTable";

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
            "inventory_movements_template.xlsx"
        );
    }


    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ImportInventoryMovements(
    [FromForm] ImportItemsRequestDto request,
    [FromQuery] Guid companyId)
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

        int created = 0;
        int skipped = 0;

        foreach (var row in rows)
        {
            var itemNo = row.Cell(1).GetString();
            var binCode = row.Cell(2).GetString();
            var warehouseCode = row.Cell(3).GetString();
            var quantity = row.Cell(4).GetValue<decimal>();
            var movementType = row.Cell(5).GetString();
            var referenceNo = row.Cell(6).GetString();
            var entityType = row.Cell(7).GetString();
            var entityReference = row.Cell(8).GetString();
            var oldStatus = row.Cell(9).GetString();
            var newStatus = row.Cell(10).GetString();
            var sourceSystem = row.Cell(11).GetString();
            var createdBy = row.Cell(12).GetString();

            if (string.IsNullOrWhiteSpace(itemNo))
            {
                skipped++;
                continue;
            }

            // =========================
            // Resolver Item
            // =========================
            var item = await _db.Items
                .FirstOrDefaultAsync(i =>
                    i.CompanyId == companyId &&
                    i.ItemNo == itemNo);

            if (item == null)
            {
                skipped++;
                continue;
            }

            // =========================
            // Resolver Bin
            // =========================
            Guid? binId = null;

            if (!string.IsNullOrWhiteSpace(binCode))
            {
                var bin = await _db.Bins
                    .FirstOrDefaultAsync(b =>
                        b.CompanyId == companyId &&
                        b.BinCode == binCode);

                if (bin != null)
                    binId = bin.Id;
            }

            // =========================
            // Resolver Warehouse
            // =========================
            /*Guid? warehouseId = null;

            if (!string.IsNullOrWhiteSpace(warehouseCode))
            {
                var warehouse = await _db.Warehouses
                    .FirstOrDefaultAsync(w =>
                        w.CompanyId == companyId &&
                        w.Code == warehouseCode);

                if (warehouse != null)
                    warehouseId = warehouse.Id;
            }*/

            // =========================
            // Validar MovementType
            // =========================
            var validTypes = new[] { "IN", "OUT", "TRANSFER", "ADJUSTMENT" };

            if (!validTypes.Contains(movementType))
            {
                skipped++;
                continue;
            }

            var movement = new InventoryMovements
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                CreatedAt = DateTime.UtcNow,

                ItemId = item.Id,
                BinId = binId,
                //WarehouseId = warehouseId,

                Quantity = quantity,
                MovementType = movementType,
                ReferenceNo = referenceNo,

                EntityType = entityType,
                OldStatus = oldStatus,
                NewStatus = newStatus,

                SourceSystem = string.IsNullOrWhiteSpace(sourceSystem)
                    ? "EXCEL_IMPORT"
                    : sourceSystem,

                CreatedBy = string.IsNullOrWhiteSpace(createdBy)
                    ? "EXCEL_IMPORT"
                    : createdBy
            };

            _db.InventoryMovements.Add(movement);
            created++;
        }

        await _db.SaveChangesAsync();

        return Ok(new
        {
            created,
            skipped
        });
    }


}
