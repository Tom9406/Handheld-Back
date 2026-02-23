using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wms.Api.Data;
using Wms.Api.Dtos.InventoryMovement;

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
        bool sortDesc = true)
    {
        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0) pageSize = 20;
        if (pageSize > 200) pageSize = 200;

        var query = _db.InventoryMovements
            .AsNoTracking()
            .Where(x => x.CompanyId == companyId)
            .AsQueryable();


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
            query = query.Where(x => x.ReferenceNo!.Contains(referenceNo));

        if (dateFrom.HasValue)
            query = query.Where(x => x.CreatedAt >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(x => x.CreatedAt <= dateTo.Value);

        // ========================
        // Sorting (Kardex style)
        // ========================
        query = sortDesc
            ? query.OrderByDescending(x => x.CreatedAt)
            : query.OrderBy(x => x.CreatedAt);

        var totalRecords = await query.CountAsync();

        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new InventoryMovementDto
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

}
