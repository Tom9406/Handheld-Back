using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wms.Api.Common;
using Wms.Api.Data;
using Wms.Api.Dtos;

namespace Wms.Api.Controllers;

[ApiController]
[Route("api/posted-receiving-lines")]
public class PostedReceivingLinesController : ControllerBase
{
    private readonly WmsDbContext _db;

    public PostedReceivingLinesController(WmsDbContext db)
    {
        _db = db;
    }

    // ====================================================
    // GET: api/posted-receiving-lines?companyId={companyId}&pageNumber=1&pageSize=20
    // ====================================================
    [HttpGet]
    public async Task<ActionResult<PagedResponse<PostedReceivingLineDto>>> GetAll(
        [FromQuery] Guid? companyId,
        [FromQuery] PaginationParameters? pagination = null)
    {
        pagination ??= new PaginationParameters();

        var query = _db.PostedReceivingLines
            .AsNoTracking()
            .AsQueryable();

        if (companyId.HasValue)
            query = query.Where(x => x.CompanyId == companyId.Value);

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

        var response = new PagedResponse<PostedReceivingLineDto>
        {
            Data = data,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pagination.PageSize)
        };

        return Ok(response);
    }

    // ====================================================
    // GET: api/posted-receiving-lines/{id}
    // ====================================================
    [HttpGet("{id}")]
    public async Task<ActionResult<PostedReceivingLineDto>> GetById(Guid id)
    {
        var x = await _db.PostedReceivingLines
            .AsNoTracking()
            .Where(p => p.Id == id)
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

        if (x == null)
            return NotFound();

        return Ok(x);
    }

    // ====================================================
    // GET: api/posted-receiving-lines/by-header/{postedHeaderId}?companyId={companyId}
    // ====================================================
    [HttpGet("by-header/{postedHeaderId}")]
    public async Task<ActionResult<IEnumerable<PostedReceivingLineDto>>> GetByPostedHeader(
        Guid postedHeaderId,
        [FromQuery] Guid companyId)
    {
        var data = await _db.PostedReceivingLines
            .AsNoTracking()
            .Where(x => x.PostedReceivingHeaderId == postedHeaderId
                     && x.CompanyId == companyId)
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