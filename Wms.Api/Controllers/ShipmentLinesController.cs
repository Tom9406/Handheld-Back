using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wms.Api.Data;
using Wms.Api.Dtos.Shipments;
using Wms.Api.DTOs;

namespace Wms.Api.Controllers;

[ApiController]
[Route("api/shipmentlines")]
public class ShipmentLinesController : ControllerBase
{
    private readonly WmsDbContext _db;

    public ShipmentLinesController(WmsDbContext db)
    {
        _db = db;
    }

    // ======================================================
    // GET: api/shipmentlines
    //      ?shipmentId=GUID
    //      &companyId=GUID
    //      &status=Open
    //      &pageNumber=1&pageSize=50
    // ======================================================
    [HttpGet]
    public async Task<IActionResult> GetShipmentLines(
        Guid shipmentId,
        Guid? companyId = null,
        string? status = null,
        int pageNumber = 1,
        int pageSize = 50)
    {
        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0) pageSize = 50;
        if (pageSize > 200) pageSize = 200;

        var query = _db.ShipmentLines
            .AsNoTracking()
            .Where(x => x.ShipmentId == shipmentId);

        // ========================
        // Multi-company filter
        // ========================
        if (companyId.HasValue)
            query = query.Where(x => x.CompanyId == companyId);

        // ========================
        // Status filter
        // ========================
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(x => x.LineStatus == status);

        var totalRecords = await query.CountAsync();

        var data = await query
            .OrderBy(x => x.LineNo)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ShipmentLineDto
            {
                Id = x.Id,
                ShipmentId = x.ShipmentId,
                CompanyId = x.CompanyId,
                LineNo = x.LineNo,
                ItemId = x.ItemId,
                ItemNo = x.ItemNo,
                ItemDescription = x.ItemDescription,
                WarehouseId = x.WarehouseId,
                BinId = x.BinId,
                BinCode = x.BinCode,
                OrderedQty = x.OrderedQty,
                PickedQty = x.PickedQty,
                ShippedQty = x.ShippedQty,
                UnitOfMeasure = x.UnitOfMeasure,
                BaseUomQty = x.BaseUomQty,
                LotNo = x.LotNo,
                SerialNo = x.SerialNo,
                ExpirationDate = x.ExpirationDate,
                UnitWeight = x.UnitWeight,
                UnitVolume = x.UnitVolume,
                LineStatus = x.LineStatus,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
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
    // GET: api/shipmentlines/{id}
    // ======================================================
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var line = await _db.ShipmentLines
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ShipmentLineDto
            {
                Id = x.Id,
                ShipmentId = x.ShipmentId,
                CompanyId = x.CompanyId,
                LineNo = x.LineNo,
                ItemId = x.ItemId,
                ItemNo = x.ItemNo,
                ItemDescription = x.ItemDescription,
                WarehouseId = x.WarehouseId,
                BinId = x.BinId,
                BinCode = x.BinCode,
                OrderedQty = x.OrderedQty,
                PickedQty = x.PickedQty,
                ShippedQty = x.ShippedQty,
                UnitOfMeasure = x.UnitOfMeasure,
                BaseUomQty = x.BaseUomQty,
                LotNo = x.LotNo,
                SerialNo = x.SerialNo,
                ExpirationDate = x.ExpirationDate,
                UnitWeight = x.UnitWeight,
                UnitVolume = x.UnitVolume,
                LineStatus = x.LineStatus,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (line == null)
            return NotFound();

        return Ok(line);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateLine(Guid id, UpdateShipmentLineDto dto)
    {
        var line = await _db.ShipmentLines
            .FirstOrDefaultAsync(x => x.Id == id);

        if (line == null)
            return NotFound();

        if (dto.ShippedQty < 0)
            return BadRequest("Invalid quantity");

        // Validación: Picked + Shipped no puede superar Ordered
        if (line.PickedQty + dto.ShippedQty > line.OrderedQty)
            return BadRequest("Picked + Shipped cannot exceed ordered quantity");

        line.ShippedQty = dto.ShippedQty;

        // Recalcular estado
        var totalProcessed = line.PickedQty + line.ShippedQty;

        if (totalProcessed <= 0)
        {
            line.LineStatus = "Open";
        }
        else if (totalProcessed < line.OrderedQty)
        {
            line.LineStatus = "Partial";
        }
        else
        {
            line.LineStatus = "Closed";
        }

        line.UpdatedAt = DateTime.UtcNow;
        line.UpdatedBy = "SYSTEM";

        await _db.SaveChangesAsync();

        return NoContent();
    }
}
