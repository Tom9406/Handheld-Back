using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wms.Api.Common;
using Wms.Api.Data;
using Wms.Api.Dtos.Bin;

namespace Wms.Api.Controllers;

[Route("api/bins")]
public class BinsController : BaseController
{
    private readonly WmsDbContext _db;

    public BinsController(WmsDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<BinDto>>> GetBins(
        [FromQuery] Guid? companyId,
        [FromQuery] bool activeOnly = true,
        [FromQuery] PaginationParameters? pagination = null)
    {
        pagination ??= new PaginationParameters();
        var activeCompanyId = ResolveCompanyId(companyId);

        var query = _db.Bins
            .AsNoTracking()
            .Where(b => b.CompanyId == activeCompanyId);

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

        return Ok(new PagedResponse<BinDto>
        {
            Data = bins,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pagination.PageSize)
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BinDetailDto>> GetBin(Guid id)
    {
        var allowedCompanies = AccessibleCompanyIds;

        var bin = await _db.Bins
            .AsNoTracking()
            .Where(b => b.Id == id && allowedCompanies.Contains(b.CompanyId))
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
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (bin == null)
            return NotFound();

        return Ok(bin);
    }
}
