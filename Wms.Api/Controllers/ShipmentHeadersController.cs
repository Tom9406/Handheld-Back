using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wms.Api.Data;
using Wms.Api.Dtos.ShipmentHeader;

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
}
