using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wms.Api.Common;
using Wms.Api.Data;
using Wms.Api.Dtos;

namespace Wms.Api.Controllers;

[Route("api/posted-receiving-lines")]
public class PostedReceivingLinesController : BaseController
{
    private readonly WmsDbContext _db;

    public PostedReceivingLinesController(WmsDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<PostedReceivingLineDto>>> GetAll(
        [FromQuery] Guid? companyId,
        [FromQuery] PaginationParameters? pagination = null)
    {
        pagination ??= new PaginationParameters();
        var activeCompanyId = ResolveCompanyId(companyId);

        var query = _db.PostedReceivingLines
            .AsNoTracking()
            .Where(x => x.CompanyId == activeCompanyId);

        var totalRecords = await query.CountAsync();

        var data = await query
            .OrderByDescending(x => x.PostedAt)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(x => new PostedReceivingLineDto
            {
                Id = x.Id,
                PostedReceivingHeaderId = x.PostedReceivingHeaderId,
                ReceivingLineId = x.ReceivingLineId,
                CompanyId = x.CompanyId,
                ItemId = x.ItemId,
                BinId = x.BinId,
                QuantityExpected = x.QuantityExpected,
                QuantityReceived = x.QuantityReceived,
                UOM = x.UOM,
                PostedAt = x.PostedAt,
                Status = x.Status,
                PostedBy = x.PostedBy
            })
            .ToListAsync();

        return Ok(new PagedResponse<PostedReceivingLineDto>
        {
            Data = data,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pagination.PageSize)
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostedReceivingLineDto>> GetById(Guid id)
    {
        var allowedCompanies = AccessibleCompanyIds;

        var data = await _db.PostedReceivingLines
            .AsNoTracking()
            .Where(p => p.Id == id && allowedCompanies.Contains(p.CompanyId))
            .Select(x => new PostedReceivingLineDto
            {
                Id = x.Id,
                PostedReceivingHeaderId = x.PostedReceivingHeaderId,
                ReceivingLineId = x.ReceivingLineId,
                CompanyId = x.CompanyId,
                ItemId = x.ItemId,
                BinId = x.BinId,
                QuantityExpected = x.QuantityExpected,
                QuantityReceived = x.QuantityReceived,
                UOM = x.UOM,
                PostedAt = x.PostedAt,
                Status = x.Status,
                PostedBy = x.PostedBy
            })
            .FirstOrDefaultAsync();

        if (data == null)
            return NotFound();

        return Ok(data);
    }

    [HttpGet("by-header/{postedHeaderId:guid}")]
    public async Task<ActionResult<IEnumerable<PostedReceivingLineDto>>> GetByPostedHeader(
        Guid postedHeaderId,
        [FromQuery] Guid? companyId = null)
    {
        var activeCompanyId = ResolveCompanyId(companyId);

        var data = await _db.PostedReceivingLines
            .AsNoTracking()
            .Where(x => x.PostedReceivingHeaderId == postedHeaderId && x.CompanyId == activeCompanyId)
            .OrderBy(x => x.Id)
            .Select(x => new PostedReceivingLineDto
            {
                Id = x.Id,
                PostedReceivingHeaderId = x.PostedReceivingHeaderId,
                ReceivingLineId = x.ReceivingLineId,
                CompanyId = x.CompanyId,
                ItemId = x.ItemId,
                BinId = x.BinId,
                QuantityExpected = x.QuantityExpected,
                QuantityReceived = x.QuantityReceived,
                UOM = x.UOM,
                PostedAt = x.PostedAt,
                Status = x.Status,
                PostedBy = x.PostedBy
            })
            .ToListAsync();

        return Ok(data);
    }
}
