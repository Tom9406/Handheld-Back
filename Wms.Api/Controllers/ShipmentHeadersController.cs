using Handheld.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wms.Api.Data;
using Wms.Api.Dtos.ShipmentHeader;
using Wms.Api.Entities;

namespace Wms.Api.Controllers;

[Route("api/shipmentheaders")]
public class ShipmentHeadersController : BaseController
{
    private readonly WmsDbContext _db;

    public ShipmentHeadersController(WmsDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetShipmentHeaders(
        int pageNumber = 1,
        int pageSize = 20,
        string? status = null,
        string? shipmentNo = null,
        string? sortBy = "CreatedAt",
        bool sortDesc = true)
    {
        var companyId = CompanyId;

        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var query = _db.ShipmentHeaders
            .AsNoTracking()
            .Where(x => x.CompanyId == companyId);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(x => x.ShipmentStatus == status);

        if (!string.IsNullOrWhiteSpace(shipmentNo))
            query = query.Where(x => x.ShipmentNo.Contains(shipmentNo));

        query = sortBy?.ToLower() switch
        {
            "shipmentno" => sortDesc ? query.OrderByDescending(x => x.ShipmentNo) : query.OrderBy(x => x.ShipmentNo),
            _ => sortDesc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt)
        };

        var totalRecords = await query.CountAsync();

        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ShipmentHeaderDto
            {
                Id = x.Id,
                CompanyId = x.CompanyId,
                CompanyCode = x.CompanyCode,
                ShipmentNo = x.ShipmentNo,
                ExternalShipmentNo = x.ExternalShipmentNo,
                ShipmentType = x.ShipmentType,
                ShipmentStatus = x.ShipmentStatus,
                WarehouseCode = x.WarehouseCode,
                CustomerCode = x.CustomerCode,
                CustomerName = x.CustomerName,
                PlannedShipDate = x.PlannedShipDate,
                ActualShipDate = x.ActualShipDate,
                TotalLines = x.TotalLines,
                TotalQty = x.TotalQty,
                IsClosed = x.IsClosed,
                CreatedAt = x.CreatedAt
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

        var shipment = await _db.ShipmentHeaders
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId);

        if (shipment == null)
            return NotFound();

        return Ok(shipment);
    }

    [HttpPost]
    public async Task<IActionResult> CreateShipment(CreateShipmentHeaderDto dto)
    {
        var companyId = CompanyId;
        var userEmail = UserEmail;

        if (dto.Lines == null || dto.Lines.Count == 0)
            return BadRequest("Shipment must have at least one line.");

        using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            var sequence = await _db.DocumentSequences
                .FromSqlRaw(@"
                    SELECT TOP 1 * FROM DocumentSequences WITH (UPDLOCK, ROWLOCK, HOLDLOCK)
                    WHERE CompanyId = {0} AND DocumentType = {1}",
                    companyId, "SHIPMENT_CREATE")
                .AsTracking()
                .FirstOrDefaultAsync();

            if (sequence == null)
                return BadRequest("Sequence not configured.");

            sequence.LastNumber++;
            var shipmentNo = $"SHP-{sequence.LastNumber:D6}";

            var company = await _db.Companies.FirstOrDefaultAsync(x => x.Id == companyId);
            if (company == null)
                return BadRequest("Company not found.");

            var lines = dto.Lines
                .Where(l => l.OrderedQty > 0)
                .ToList();

            if (lines.Count == 0)
                return BadRequest("All shipment lines are invalid.");

            var header = new ShipmentHeaders
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                CompanyCode = company.Code,
                ShipmentNo = shipmentNo,
                ExternalShipmentNo = dto.ExternalShipmentNo,
                ReferenceNo = dto.ReferenceNo,
                ShipmentType = dto.ShipmentType,
                ShipmentStatus = "OPEN",
                WarehouseId = dto.WarehouseId,
                WarehouseCode = dto.WarehouseCode,
                CustomerId = dto.CustomerId,
                CustomerCode = dto.CustomerCode,
                CustomerName = dto.CustomerName,
                ShipToName = dto.ShipToName,
                ShipToAddress1 = dto.ShipToAddress1,
                ShipToAddress2 = dto.ShipToAddress2,
                ShipToCity = dto.ShipToCity,
                ShipToState = dto.ShipToState,
                ShipToPostalCode = dto.ShipToPostalCode,
                ShipToCountry = dto.ShipToCountry,
                OrderDate = dto.OrderDate,
                PlannedShipDate = dto.PlannedShipDate,
                CarrierCode = dto.CarrierCode,
                CarrierName = dto.CarrierName,
                ServiceLevel = dto.ServiceLevel,
                IsBackorderAllowed = dto.IsBackorderAllowed,
                IsPartialAllowed = dto.IsPartialAllowed,
                TotalLines = lines.Count,
                TotalQty = lines.Sum(l => l.OrderedQty),
                TotalWeight = lines.Sum(l => l.UnitWeight ?? 0),
                TotalVolume = lines.Sum(l => l.UnitVolume ?? 0),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userEmail
            };

            _db.ShipmentHeaders.Add(header);

            var lineNumber = 1;
            foreach (var l in lines)
            {
                _db.ShipmentLines.Add(new ShipmentLines
                {
                    Id = Guid.NewGuid(),
                    ShipmentId = header.Id,
                    CompanyId = companyId,
                    LineNo = l.LineNo > 0 ? l.LineNo : lineNumber++,
                    ItemId = l.ItemId,
                    ItemNo = l.ItemNo,
                    ItemDescription = l.ItemDescription,
                    WarehouseId = l.WarehouseId,
                    BinId = l.BinId,
                    BinCode = l.BinCode,
                    OrderedQty = l.OrderedQty,
                    PickedQty = l.PickedQty,
                    ShippedQty = l.ShippedQty,
                    UnitOfMeasure = l.UnitOfMeasure,
                    BaseUomQty = l.BaseUomQty,
                    LotNo = l.LotNo,
                    SerialNo = l.SerialNo,
                    ExpirationDate = l.ExpirationDate,
                    UnitWeight = l.UnitWeight,
                    UnitVolume = l.UnitVolume,
                    LineStatus = l.LineStatus,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userEmail
                });
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                shipmentId = header.Id,
                shipmentNo
            });
        }
        catch
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{id:guid}/post")]
    public async Task<IActionResult> PostShipment(Guid id)
    {
        var companyId = CompanyId;
        var userEmail = UserEmail;

        using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            var shipment = await _db.ShipmentHeaders
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId);

            if (shipment == null)
                return NotFound("Shipment not found.");

            if (shipment.Lines == null || shipment.Lines.Count == 0)
                return BadRequest("Shipment has no lines.");

            var sequence = await _db.DocumentSequences
                .FromSqlRaw(@"
                    SELECT TOP 1 * FROM DocumentSequences WITH (UPDLOCK, ROWLOCK, HOLDLOCK)
                    WHERE CompanyId = {0} AND DocumentType = {1}",
                    companyId, "POSTED_SHIPMENT")
                .AsTracking()
                .FirstOrDefaultAsync();

            if (sequence == null)
                return BadRequest("Sequence not configured.");

            sequence.LastNumber++;
            var postedShipmentNo = $"PS-{sequence.LastNumber:D6}";

            var postedShipment = new PostedShipment
            {
                Id = Guid.NewGuid(),
                PostedShipmentNo = postedShipmentNo,
                ShipmentId = shipment.Id,
                CompanyId = companyId,
                CompanyCode = shipment.CompanyCode,
                ShipmentNo = shipment.ShipmentNo,
                ShipmentType = shipment.ShipmentType,
                ShipmentStatus = "POSTED",
                WarehouseId = shipment.WarehouseId,
                WarehouseCode = shipment.WarehouseCode,
                CustomerId = shipment.CustomerId,
                CustomerCode = shipment.CustomerCode,
                CustomerName = shipment.CustomerName,
                OrderDate = shipment.OrderDate,
                PlannedShipDate = shipment.PlannedShipDate,
                ActualShipDate = DateTime.UtcNow,
                IsBackorderAllowed = shipment.IsBackorderAllowed,
                IsPartialAllowed = shipment.IsPartialAllowed,
                PostedAt = DateTime.UtcNow,
                PostedBy = userEmail,
                CreatedAt = DateTime.UtcNow
            };

            _db.PostedShipments.Add(postedShipment);

            var processedLines = 0;
            var totalQtyPostedNow = 0m;
            var totalWeightPostedNow = 0m;
            var totalVolumePostedNow = 0m;
            var lineNo = 1;

            foreach (var line in shipment.Lines)
            {
                if (line.ShippedQty <= 0)
                    continue;

                var alreadyPostedQty = await _db.PostedShipmentLines
                    .Where(x => x.ShipmentLineId == line.Id && x.CompanyId == companyId)
                    .SumAsync(x => (decimal?)x.ShippedQty) ?? 0;

                var remainingQty = line.OrderedQty - alreadyPostedQty;
                if (remainingQty <= 0)
                    continue;

                if (line.ShippedQty > remainingQty)
                    return BadRequest($"Cannot ship more than remaining for item {line.ItemNo}.");

                if (line.BinId == null)
                    return BadRequest($"Item {line.ItemNo} has no Bin.");

                var stockQty = await _db.InventoryMovements
                    .Where(x => x.CompanyId == companyId && x.ItemId == line.ItemId && x.BinId == line.BinId)
                    .SumAsync(x => (decimal?)x.Quantity) ?? 0;

                if (stockQty < line.ShippedQty)
                    return BadRequest($"Insufficient stock for item {line.ItemNo}.");

                _db.InventoryMovements.Add(new InventoryMovements
                {
                    Id = Guid.NewGuid(),
                    CompanyId = companyId,
                    ItemId = line.ItemId,
                    BinId = line.BinId.Value,
                    Quantity = -line.ShippedQty,
                    MovementType = "OUT",
                    ReferenceNo = postedShipmentNo,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userEmail
                });

                _db.PostedShipmentLines.Add(new PostedShipmentLine
                {
                    Id = Guid.NewGuid(),
                    PostedShipmentId = postedShipment.Id,
                    ShipmentLineId = line.Id,
                    LineNo = lineNo++,
                    ItemId = line.ItemId,
                    ItemNo = line.ItemNo,
                    ItemDescription = line.ItemDescription,
                    WarehouseId = line.WarehouseId,
                    BinId = line.BinId,
                    BinCode = line.BinCode ?? string.Empty,
                    OrderedQty = line.OrderedQty,
                    PickedQty = line.PickedQty,
                    ShippedQty = line.ShippedQty,
                    UnitOfMeasure = line.UnitOfMeasure,
                    BaseUomQty = line.BaseUomQty,
                    LotNo = line.LotNo,
                    SerialNo = line.SerialNo,
                    UnitWeight = line.UnitWeight,
                    UnitVolume = line.UnitVolume,
                    CompanyId = companyId,
                    LineStatus = "POSTED",
                    PostedAt = DateTime.UtcNow,
                    PostedBy = userEmail
                });

                var newPostedTotal = alreadyPostedQty + line.ShippedQty;
                line.LineStatus = newPostedTotal >= line.OrderedQty ? "POSTED" : "PARTIALLY POSTED";
                line.ShippedQty = 0;
                line.UpdatedAt = DateTime.UtcNow;
                line.UpdatedBy = userEmail;

                totalQtyPostedNow += line.OrderedQty < 0 ? 0 : newPostedTotal - alreadyPostedQty;
                totalWeightPostedNow += (line.UnitWeight ?? 0) * (newPostedTotal - alreadyPostedQty);
                totalVolumePostedNow += (line.UnitVolume ?? 0) * (newPostedTotal - alreadyPostedQty);
                processedLines++;
            }

            if (processedLines == 0)
                return BadRequest("Nothing to post.");

            var allLinesPosted = shipment.Lines.All(l =>
                (_db.PostedShipmentLines
                    .Where(x => x.ShipmentLineId == l.Id && x.CompanyId == companyId)
                    .Sum(x => (decimal?)x.ShippedQty) ?? 0) >= l.OrderedQty);

            shipment.ShipmentStatus = allLinesPosted ? "POSTED" : "PARTIALLY POSTED";
            shipment.ActualShipDate = DateTime.UtcNow;
            shipment.UpdatedAt = DateTime.UtcNow;
            shipment.UpdatedBy = userEmail;
            shipment.IsClosed = allLinesPosted;

            postedShipment.TotalLines = processedLines;
            postedShipment.TotalQty = totalQtyPostedNow;
            postedShipment.TotalWeight = totalWeightPostedNow;
            postedShipment.TotalVolume = totalVolumePostedNow;

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                message = "Shipment posted successfully",
                shipmentId = shipment.Id,
                shipmentNo = shipment.ShipmentNo,
                postedShipmentNo,
                status = shipment.ShipmentStatus
            });
        }
        catch
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "Internal server error");
        }
    }
}
