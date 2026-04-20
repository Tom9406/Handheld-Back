using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wms.Api.Data;
using Wms.Api.Dtos.PickHeader;

namespace Wms.Api.Controllers;

[Route("api/pickheaders")]
public class PickHeadersController : BaseController
{
    private readonly WmsDbContext _db;

    public PickHeadersController(WmsDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetPickHeaders(
        int pageNumber = 1,
        int pageSize = 20,
        string? status = null,
        string? assignedUser = null,
        string? pickNo = null,
        string? sortBy = "CreatedAt",
        bool sortDesc = true)
    {
        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var query = _db.PickHeaders.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(x => x.Status == status);

        if (!string.IsNullOrWhiteSpace(assignedUser))
            query = query.Where(x => x.AssignedUserName != null && x.AssignedUserName.Contains(assignedUser));

        if (!string.IsNullOrWhiteSpace(pickNo))
            query = query.Where(x => x.PickNo.Contains(pickNo));

        query = sortBy?.ToLower() switch
        {
            "pickno" => sortDesc ? query.OrderByDescending(x => x.PickNo) : query.OrderBy(x => x.PickNo),
            "status" => sortDesc ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            "assigneduser" => sortDesc ? query.OrderByDescending(x => x.AssignedUserName) : query.OrderBy(x => x.AssignedUserName),
            "completedat" => sortDesc ? query.OrderByDescending(x => x.CompletedAt) : query.OrderBy(x => x.CompletedAt),
            _ => sortDesc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt)
        };

        var totalRecords = await query.CountAsync();

        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new PickHeaderDto
            {
                Id = x.Id,
                PickNo = x.PickNo,
                Status = x.Status,
                AssignedUserName = x.AssignedUserName,
                SalesOrderNo = x.SalesOrderNo,
                WarehouseShipmentNo = x.WarehouseShipmentNo,
                CreatedAt = x.CreatedAt,
                CompletedAt = x.CompletedAt
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
        var pick = await _db.PickHeaders
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new PickHeaderDetailDto
            {
                Id = x.Id,
                PickNo = x.PickNo,
                Status = x.Status,
                AssignedUserName = x.AssignedUserName,
                SalesOrderNo = x.SalesOrderNo,
                WarehouseShipmentNo = x.WarehouseShipmentNo,
                CreatedAt = x.CreatedAt,
                CompletedAt = x.CompletedAt
            })
            .FirstOrDefaultAsync();

        if (pick == null)
            return NotFound();

        return Ok(pick);
    }
}
