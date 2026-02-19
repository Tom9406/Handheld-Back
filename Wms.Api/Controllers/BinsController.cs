using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wms.Api.Common;
using Wms.Api.Data;
using Wms.Api.Dtos.Bin;

namespace Wms.Api.Controllers;

[ApiController]
[Route("api/bins")]
public class BinsController : ControllerBase
{
    private readonly WmsDbContext _db;

    public BinsController(WmsDbContext db)
    {
        _db = db;
    }

    // ====================================================
    // GET: api/bins?companyId={companyId}&activeOnly=true&pageNumber=1&pageSize=20
    // ====================================================
    [HttpGet]
    public async Task<ActionResult<PagedResponse<BinDto>>> GetBins(
        [FromQuery] Guid? companyId,
        [FromQuery] bool activeOnly = true,
        [FromQuery] PaginationParameters? pagination = null)
    {
        var query = _db.Bins
            .AsNoTracking()
            .AsQueryable();

        if (companyId.HasValue)
            query = query.Where(b => b.CompanyId == companyId.Value);

        if (activeOnly)
            query = query.Where(b => b.IsActive);

        var totalRecords = await query.CountAsync();

        var bins = await query
            .OrderBy(b => b.BinCode)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(b => new BinDto
            {
                Id = b.Id,
                BinCode = b.BinCode,
                Description = b.Description,
                IsActive = b.IsActive,
                IsBlocked = b.IsBlocked,
                BinType = b.BinType,
                AllowPicking = b.AllowPicking,
                AllowPutaway = b.AllowPutaway,
                CompanyId = b.CompanyId
            })
            .ToListAsync();

        var response = new PagedResponse<BinDto>
        {
            Data = bins,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pagination.PageSize)
        };

        return Ok(response);
    }

    // ====================================================
    // GET: api/bins/{id}
    // ====================================================
    [HttpGet("{id}")]
    public async Task<ActionResult<BinDetailDto>> GetBin(Guid id)
    {
        var bin = await _db.Bins
            .AsNoTracking()
            .Where(b => b.Id == id)
            .Select(b => new BinDetailDto
            {
                Id = b.Id,
                BinCode = b.BinCode,
                Description = b.Description,
                IsActive = b.IsActive,
                IsBlocked = b.IsBlocked,
                BinType = b.BinType,
                AllowPicking = b.AllowPicking,
                AllowPutaway = b.AllowPutaway,

                CompanyId = b.CompanyId,
                CompanyName = b.Company.Name,

                // Preparado para futuro Warehouse
                // WarehouseId = b.WarehouseId,
                // WarehouseCode = b.Warehouse.Code,

                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (bin == null)
            return NotFound();

        return Ok(bin);
    }
}
