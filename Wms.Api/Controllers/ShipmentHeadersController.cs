using Handheld.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wms.Api.Data;
using Wms.Api.Dtos.ShipmentHeader;
using Wms.Api.Entities;

namespace Wms.Api.Controllers;

[ApiController]
[Route("api/shipmentheaders")]
public class ShipmentHeadersController : ControllerBase
{
    private readonly WmsDbContext _db;

    public ShipmentHeadersController(WmsDbContext db)
    {
        _db = db;
    }

    // ======================================================
    // GET: api/shipmentheaders?pageNumber=1&pageSize=20
    //      &companyId=GUID
    //      &status=Open
    //      &shipmentNo=SH0001
    //      &sortBy=CreatedAt
    //      &sortDesc=true
    // ======================================================
    [HttpGet]
    public async Task<IActionResult> GetShipmentHeaders(
        int pageNumber = 1,
        int pageSize = 20,
        Guid? companyId = null,
        string? status = null,
        string? shipmentNo = null,
        string? sortBy = "CreatedAt",
        bool sortDesc = true)
    {
        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var query = _db.ShipmentHeaders
            .AsNoTracking()
            .AsQueryable();

        // ========================
        // Filters
        // ========================
        if (companyId.HasValue)
            query = query.Where(x => x.CompanyId == companyId);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(x => x.ShipmentStatus == status);

        if (!string.IsNullOrWhiteSpace(shipmentNo))
            query = query.Where(x => x.ShipmentNo.Contains(shipmentNo));

        // ========================
        // Sorting
        // ========================
        query = sortBy?.ToLower() switch
        {
            "shipmentno" => sortDesc
                ? query.OrderByDescending(x => x.ShipmentNo)
                : query.OrderBy(x => x.ShipmentNo),

            "plannedshipdate" => sortDesc
                ? query.OrderByDescending(x => x.PlannedShipDate)
                : query.OrderBy(x => x.PlannedShipDate),

            "status" => sortDesc
                ? query.OrderByDescending(x => x.ShipmentStatus)
                : query.OrderBy(x => x.ShipmentStatus),

            _ => sortDesc
                ? query.OrderByDescending(x => x.CreatedAt)
                : query.OrderBy(x => x.CreatedAt)
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
    // GET: api/shipmentheaders/{id}
    // ======================================================
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var shipment = await _db.ShipmentHeaders
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ShipmentHeaderDetailDto
            {
                Id = x.Id,
                CompanyId = x.CompanyId,
                CompanyCode = x.CompanyCode,
                ShipmentNo = x.ShipmentNo,
                ExternalShipmentNo = x.ExternalShipmentNo,
                ReferenceNo = x.ReferenceNo,
                ShipmentType = x.ShipmentType,
                ShipmentStatus = x.ShipmentStatus,
                WarehouseId = x.WarehouseId,
                WarehouseCode = x.WarehouseCode,
                CustomerId = x.CustomerId,
                CustomerCode = x.CustomerCode,
                CustomerName = x.CustomerName,
                ShipToName = x.ShipToName,
                ShipToAddress1 = x.ShipToAddress1,
                ShipToAddress2 = x.ShipToAddress2,
                ShipToCity = x.ShipToCity,
                ShipToState = x.ShipToState,
                ShipToPostalCode = x.ShipToPostalCode,
                ShipToCountry = x.ShipToCountry,
                OrderDate = x.OrderDate,
                PlannedShipDate = x.PlannedShipDate,
                ActualShipDate = x.ActualShipDate,
                DeliveryDate = x.DeliveryDate,
                CarrierCode = x.CarrierCode,
                CarrierName = x.CarrierName,
                ServiceLevel = x.ServiceLevel,
                TrackingNumber = x.TrackingNumber,
                TotalLines = x.TotalLines,
                TotalQty = x.TotalQty,
                TotalWeight = x.TotalWeight,
                TotalVolume = x.TotalVolume,
                SourceSystem = x.SourceSystem,
                SourceEndpoint = x.SourceEndpoint,
                IntegrationBatchId = x.IntegrationBatchId,
                IsBackorderAllowed = x.IsBackorderAllowed,
                IsPartialAllowed = x.IsPartialAllowed,
                IsClosed = x.IsClosed,
                CreatedBy = x.CreatedBy,
                CreatedAt = x.CreatedAt,
                UpdatedBy = x.UpdatedBy,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (shipment == null)
            return NotFound();

        return Ok(shipment);
    }

    // ======================================================
    // POST: api/shipments/{id}/post
    // ======================================================
    [HttpPost("{id:guid}/post")]
    public async Task<IActionResult> PostShipment(Guid id, Guid companyId)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            var shipment = await _db.ShipmentHeaders
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.CompanyId == companyId);

            if (shipment == null)
                return NotFound("Shipment not found.");

            if (shipment.Lines == null || !shipment.Lines.Any())
                return BadRequest("Shipment has no lines.");

            int lineNo = 1;
            int processedLines = 0;
            decimal totalQtyPostedNow = 0;

            // =========================================
            // Generar consecutivo PostedShipmentNo
            // =========================================
            var sequence = await _db.DocumentSequences
                .FirstOrDefaultAsync(x =>
                    x.CompanyId == companyId &&
                    x.DocumentType == "POSTED_SHIPMENT");

            if (sequence == null)
            {
                sequence = new DocumentSequence
                {
                    Id = Guid.NewGuid(),
                    CompanyId = companyId,
                    DocumentType = "POSTED_SHIPMENT",
                    LastNumber = 0
                };

                _db.DocumentSequences.Add(sequence);
            }

            sequence.LastNumber++;
            var postedShipmentNo = $"PS-{sequence.LastNumber:D6}";

            // =========================================
            // Crear header SOLO si hay algo que postear
            // =========================================
            var postedShipment = new PostedShipment
            {
                Id = Guid.NewGuid(),
                PostedShipmentNo = postedShipmentNo,
                ShipmentId = shipment.Id,
                CompanyId = shipment.CompanyId,
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
                PostedAt = DateTime.UtcNow,
                PostedBy = "SYSTEM",
                CreatedAt = DateTime.UtcNow
            };

            _db.PostedShipments.Add(postedShipment);

            foreach (var line in shipment.Lines)
            {
                if (line.ShippedQty <= 0)
                    continue;

                //  Calcular cuánto ya fue posteado antes
                var alreadyPostedQty = await _db.PostedShipmentLines
                    .Where(x => x.ShipmentLineId == line.Id)
                    .SumAsync(x => (decimal?)x.ShippedQty) ?? 0;

                var remainingQty = line.OrderedQty - alreadyPostedQty;

                if (remainingQty <= 0)
                    continue;

                if (line.ShippedQty > remainingQty)
                    return BadRequest($"Cannot ship more than remaining quantity for item {line.ItemNo}.");

                if (line.BinId == null)
                    return BadRequest($"Shipment line for item {line.ItemNo} has no Bin assigned.");

                var stockQty = await _db.InventoryMovements
                    .Where(x =>
                        x.CompanyId == companyId &&
                        x.ItemId == line.ItemId &&
                        x.BinId == line.BinId)
                    .SumAsync(x => (decimal?)x.Quantity) ?? 0;

                if (stockQty < line.ShippedQty)
                    return BadRequest(
                        $"Insufficient stock for item {line.ItemNo} in bin {line.BinCode}.");

                // ===============================
                // Inventory Movement
                // ===============================
                var movement = new InventoryMovements
                {
                    Id = Guid.NewGuid(),
                    CompanyId = companyId,
                    ItemId = line.ItemId,
                    BinId = line.BinId.Value,
                    Quantity = -line.ShippedQty,
                    MovementType = "OUT",
                    ReferenceNo = postedShipmentNo,
                    CreatedAt = DateTime.UtcNow
                };

                _db.InventoryMovements.Add(movement);

                // ===============================
                // Posted Shipment Line
                // ===============================
                var postedLine = new PostedShipmentLine
                {
                    Id = Guid.NewGuid(),
                    PostedShipmentId = postedShipment.Id,
                    ShipmentLineId = line.Id,
                    LineNo = lineNo++,
                    ItemId = line.ItemId,
                    ItemNo = line.ItemNo,
                    ItemDescription = line.ItemDescription,
                    WarehouseId = shipment.WarehouseId,
                    BinId = line.BinId,
                    BinCode = line.BinCode,
                    OrderedQty = line.OrderedQty,
                    PickedQty = line.PickedQty,
                    ShippedQty = line.ShippedQty,
                    UnitOfMeasure = line.UnitOfMeasure,
                    UnitWeight = line.UnitWeight,
                    UnitVolume = line.UnitVolume,
                    CompanyId = companyId,
                    LineStatus = "POSTED",
                    PostedAt = DateTime.UtcNow,
                    PostedBy = "SYSTEM"
                };

                _db.PostedShipmentLines.Add(postedLine);

                totalQtyPostedNow += line.ShippedQty;
                processedLines++;
                

                // ===============================
                // Actualizar estado línea original
                // ===============================
                var newPostedTotal = alreadyPostedQty + line.ShippedQty;

                if (newPostedTotal >= line.OrderedQty)
                    line.LineStatus = "POSTED";
                else
                    line.LineStatus = "PARTIALLY POSTED";

                line.ShippedQty = 0;
                line.UpdatedAt = DateTime.UtcNow;
            }

            if (processedLines == 0)
                return BadRequest("Nothing to post.");

            // ===============================
            // Actualizar header original
            // ===============================
            var allLinesFullyPosted = shipment.Lines.All(l =>
                (_db.PostedShipmentLines
                    .Where(x => x.ShipmentLineId == l.Id)
                    .Sum(x => (decimal?)x.ShippedQty) ?? 0) >= l.OrderedQty);

            shipment.ShipmentStatus = allLinesFullyPosted
                ? "POSTED"
                : "PARTIALLY POSTED";

            shipment.UpdatedAt = DateTime.UtcNow;

            postedShipment.TotalLines = processedLines;
            postedShipment.TotalQty = totalQtyPostedNow;

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                message = "Shipment posted successfully.",
                shipmentId = shipment.Id,
                shipmentNo = shipment.ShipmentNo,
                postedShipmentNo = postedShipmentNo,
                status = shipment.ShipmentStatus
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, ex.Message);
        }
    }


}
