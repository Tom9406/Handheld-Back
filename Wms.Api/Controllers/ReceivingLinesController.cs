using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wms.Api.Common;
using Wms.Api.Data;
using Wms.Api.Dtos.ReceivingLine;

namespace Wms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReceivingLinesController : ControllerBase
{
    private readonly WmsDbContext _context;

    public ReceivingLinesController(WmsDbContext context)
    {
        _context = context;
    }

    // ====================================================
    // GET: api/ReceivingLines?companyId={id}&pageNumber=1&pageSize=20
    // ====================================================
    [HttpGet]
    public async Task<ActionResult<PagedResponse<ReceivingLineDto>>> GetAll(
        [FromQuery] Guid? companyId,
        [FromQuery] PaginationParameters? pagination = null)
    {
        pagination ??= new PaginationParameters();

        var query = _context.ReceivingLines
            .AsNoTracking()
            .AsQueryable();

        if (companyId.HasValue)
            query = query.Where(l => l.CompanyId == companyId.Value);

        var totalRecords = await query.CountAsync();

        var lines = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(l => new ReceivingLineDto
            {
                Id = l.Id,
                ReceivingHeaderId = l.ReceivingHeaderId,
                CompanyId = l.CompanyId,

                ItemId = l.ItemId,
                ItemCode = l.Item.ItemNo,

                BinId = l.BinId,
                BinCode = l.Bin != null ? l.Bin.BinCode : null,

                QuantityExpected = l.QuantityExpected,
                QuantityReceived = l.QuantityReceived,

                UOM = l.UOM,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();

        var response = new PagedResponse<ReceivingLineDto>
        {
            Data = lines,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pagination.PageSize)
        };

        return Ok(response);
    }

    // ====================================================
    // GET: api/ReceivingLines/{id}/detail
    // ====================================================
    [HttpGet("{id}/detail")]
    public async Task<ActionResult<ReceivingLineDetailDto>> GetDetail(Guid id)
    {
        var line = await _context.ReceivingLines
            .AsNoTracking()
            .Where(l => l.Id == id)
            .Select(l => new ReceivingLineDetailDto
            {
                Id = l.Id,

                ReceivingHeaderId = l.ReceivingHeaderId,
                ReceiptNo = l.ReceivingHeader.ReceiptNo,

                CompanyId = l.CompanyId,
                CompanyName = l.Company.Name,

                ItemId = l.ItemId,
                ItemCode = l.Item.ItemNo,
                ItemDescription = l.Item.Description,

                BinId = l.BinId,
                BinCode = l.Bin != null ? l.Bin.BinCode : null,

                QuantityExpected = l.QuantityExpected,
                QuantityReceived = l.QuantityReceived,

                UOM = l.UOM,
                CreatedAt = l.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (line == null)
            return NotFound();

        return Ok(line);
    }

    // ====================================================
    // GET: api/ReceivingLines/by-header/{headerId}?pageNumber=1&pageSize=20
    // ====================================================
    [HttpGet("by-header/{headerId}")]
    public async Task<ActionResult<PagedResponse<ReceivingLineDto>>> GetByHeader(
        Guid headerId,
        [FromQuery] PaginationParameters? pagination = null)
    {
        pagination ??= new PaginationParameters();

        var query = _context.ReceivingLines
            .Where(l => l.ReceivingHeaderId == headerId)
            .AsNoTracking();

        var totalRecords = await query.CountAsync();

        var lines = await query
            .OrderBy(l => l.Id)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(l => new ReceivingLineDto
            {
                Id = l.Id,
                ReceivingHeaderId = l.ReceivingHeaderId,
                CompanyId = l.CompanyId,

                ItemId = l.ItemId,
                ItemCode = l.Item.ItemNo,

                BinId = l.BinId,
                BinCode = l.Bin != null ? l.Bin.BinCode : null,

                QuantityExpected = l.QuantityExpected,
                QuantityReceived = l.QuantityReceived,

                UOM = l.UOM,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();

        var response = new PagedResponse<ReceivingLineDto>
        {
            Data = lines,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pagination.PageSize)
        };

        return Ok(response);
    }
}
