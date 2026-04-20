using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wms.Api.Data;
using Wms.Api.Dtos.Shipments;
using Wms.Api.DTOs;

namespace Wms.Api.Controllers;

[Route("api/shipmentlines")]
public class ShipmentLinesController : BaseController
{
    private readonly WmsDbContext _db;

    public ShipmentLinesController(WmsDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetShipmentLines(
        Guid shipmentId,
        string? status = null,
        int pageNumber = 1,
        int pageSize = 50)
    {
        var companyId = CompanyId;

        if (shipmentId == Guid.Empty)
            return BadRequest("ShipmentId is required.");

        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0) pageSize = 50;
        if (pageSize > 200) pageSize = 200;

        var query = _db.ShipmentLines
            .AsNoTracking()
            .Where(x => x.ShipmentId == shipmentId && x.CompanyId == companyId);

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
                AlreadyPostedQty = _db.PostedShipmentLines
                    .Where(p => p.ShipmentLineId == x.Id && p.CompanyId == companyId)
                    .Sum(p => (decimal?)p.ShippedQty) ?? 0,
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

        return Ok(new
        {
            pageNumber,
            pageSize,
            totalRecords,
            totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize),
            data
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var companyId = CompanyId;

        var line = await _db.ShipmentLines
            .AsNoTracking()
            .Where(x => x.Id == id && x.CompanyId == companyId)
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
                AlreadyPostedQty = _db.PostedShipmentLines
                    .Where(p => p.ShipmentLineId == x.Id && p.CompanyId == companyId)
                    .Sum(p => (decimal?)p.ShippedQty) ?? 0,
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
        var companyId = CompanyId;
        var userEmail = UserEmail;

        if (dto.ShippedQty < 0)
            return BadRequest("Invalid quantity.");

        var line = await _db.ShipmentLines
            .FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId);

        if (line == null)
            return NotFound();

        var alreadyPostedQty = await _db.PostedShipmentLines
            .Where(x => x.ShipmentLineId == id && x.CompanyId == companyId)
            .SumAsync(x => (decimal?)x.ShippedQty) ?? 0;

        if (alreadyPostedQty + dto.ShippedQty > line.OrderedQty)
            return BadRequest($"Shipped cannot exceed ordered quantity. Posted qty is: {alreadyPostedQty}");

        line.ShippedQty = dto.ShippedQty;

        var totalProcessed = alreadyPostedQty + line.ShippedQty;
        if (totalProcessed <= 0)
            line.LineStatus = "OPEN";
        else if (totalProcessed < line.OrderedQty)
            line.LineStatus = "PARTIAL";
        else
            line.LineStatus = "READY_TO_POST";

        line.UpdatedAt = DateTime.UtcNow;
        line.UpdatedBy = userEmail;

        var header = await _db.ShipmentHeaders
            .Include(h => h.Lines)
            .FirstOrDefaultAsync(h => h.Id == line.ShipmentId && h.CompanyId == companyId);

        if (header != null)
        {
            var anyPrepared = header.Lines.Any(l => l.ShippedQty > 0 || l.PickedQty > 0);
            var allReady = header.Lines.All(l => l.ShippedQty + (_db.PostedShipmentLines
                .Where(x => x.ShipmentLineId == l.Id && x.CompanyId == companyId)
                .Sum(x => (decimal?)x.ShippedQty) ?? 0) >= l.OrderedQty);

            var anyPosted = header.Lines.Any(l => (_db.PostedShipmentLines
                .Where(x => x.ShipmentLineId == l.Id && x.CompanyId == companyId)
                .Sum(x => (decimal?)x.ShippedQty) ?? 0) > 0);

            if (!anyPrepared && !anyPosted)
                header.ShipmentStatus = "OPEN";
            else if (allReady)
                header.ShipmentStatus = "READY_TO_POST";
            else
                header.ShipmentStatus = "SHIPPING";

            header.UpdatedAt = DateTime.UtcNow;
            header.UpdatedBy = userEmail;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }
}
