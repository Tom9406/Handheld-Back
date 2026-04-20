using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wms.Api.Common;
using Wms.Api.Data;
using Wms.Api.Dtos.ReceivingLine;

namespace Wms.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ReceivingLinesController : BaseController
{
    private readonly WmsDbContext _context;

    public ReceivingLinesController(WmsDbContext context)
    {
        _context = context;
    }

    // ====================================================
    // GET ALL
    // ====================================================
    [HttpGet]
    public async Task<ActionResult<PagedResponse<ReceivingLineDto>>> GetAll(
        [FromQuery] PaginationParameters? pagination = null)
    {
        var companyId = CompanyId;
        pagination ??= new PaginationParameters();

        var query = _context.ReceivingLines
            .AsNoTracking()
            .Where(l => l.CompanyId == companyId);

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

                PostedQuantityReceived = _context.PostedReceivingLines
                    .Where(x =>
                        x.ReceivingLineId == l.Id &&
                        x.CompanyId == companyId)
                    .Sum(x => (decimal?)x.QuantityReceived) ?? 0,

                UOM = l.UOM,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();

        return Ok(new PagedResponse<ReceivingLineDto>
        {
            Data = lines,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pagination.PageSize)
        });
    }

    // ====================================================
    // GET DETAIL
    // ====================================================
    [HttpGet("{id:guid}/detail")]
    public async Task<ActionResult<ReceivingLineDetailDto>> GetDetail(Guid id)
    {
        var companyId = CompanyId;

        var line = await _context.ReceivingLines
            .AsNoTracking()
            .Where(l =>
                l.Id == id &&
                l.CompanyId == companyId)
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
    // GET BY HEADER
    // ====================================================
    [HttpGet("by-header/{headerId:guid}")]
    public async Task<ActionResult<PagedResponse<ReceivingLineDto>>> GetByHeader(
        Guid headerId,
        [FromQuery] PaginationParameters? pagination = null)
    {
        var companyId = CompanyId;
        pagination ??= new PaginationParameters();

        var query = _context.ReceivingLines
            .AsNoTracking()
            .Where(l =>
                l.ReceivingHeaderId == headerId &&
                l.CompanyId == companyId);

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

                PostedQuantityReceived = _context.PostedReceivingLines
                    .Where(x =>
                        x.ReceivingLineId == l.Id &&
                        x.CompanyId == companyId)
                    .Sum(x => (decimal?)x.QuantityReceived) ?? 0,

                UOM = l.UOM,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();

        return Ok(new PagedResponse<ReceivingLineDto>
        {
            Data = lines,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pagination.PageSize)
        });
    }

    // ====================================================
    // UPDATE LINE
    // ====================================================
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateLine(Guid id, UpdateReceivingLineDto dto)
    {
        var companyId = CompanyId;
        var userEmail = UserEmail;

        if (dto == null)
            return BadRequest("Invalid payload");

        if (dto.QuantityReceived < 0)
            return BadRequest("Invalid quantity");

        var line = await _context.ReceivingLines
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.CompanyId == companyId);

        if (line == null)
            return NotFound();

        var alreadyPostedQty = await _context.PostedReceivingLines
            .Where(x =>
                x.ReceivingLineId == id &&
                x.CompanyId == companyId)
            .SumAsync(x => (decimal?)x.QuantityReceived) ?? 0;

        if (alreadyPostedQty + dto.QuantityReceived > line.QuantityExpected)
            return BadRequest(
                $"Received cannot exceed expected. Already posted: {alreadyPostedQty}");

        line.QuantityReceived = dto.QuantityReceived;

        var totalProcessed = alreadyPostedQty + line.QuantityReceived;

        if (totalProcessed <= 0)
            line.Status = "OPEN";
        else if (totalProcessed < line.QuantityExpected)
            line.Status = "PARTIAL";
        else
            line.Status = "CLOSED";

        line.UpdatedAt = DateTime.UtcNow;
        line.UpdatedBy = userEmail;

        // ===============================
        // UPDATE HEADER
        // ===============================
        var header = await _context.ReceivingHeaders
            .Include(h => h.Lines)
            .FirstOrDefaultAsync(h =>
                h.Id == line.ReceivingHeaderId &&
                h.CompanyId == companyId);

        if (header != null)
        {
            var allClosed = header.Lines.All(l =>
            {
                var posted = _context.PostedReceivingLines
                    .Where(p =>
                        p.ReceivingLineId == l.Id &&
                        p.CompanyId == companyId)
                    .Sum(p => (decimal?)p.QuantityReceived) ?? 0;

                return (posted + l.QuantityReceived) >= l.QuantityExpected;
            });

            var anyProcessed = header.Lines.Any(l =>
            {
                var posted = _context.PostedReceivingLines
                    .Where(p =>
                        p.ReceivingLineId == l.Id &&
                        p.CompanyId == companyId)
                    .Sum(p => (decimal?)p.QuantityReceived) ?? 0;

                return (posted + l.QuantityReceived) > 0;
            });

            if (!anyProcessed)
                header.Status = "OPEN";
            else if (allClosed)
                header.Status = "CLOSED";
            else
                header.Status = "RECEIVING";

            header.UpdatedAt = DateTime.UtcNow;
            header.UpdatedBy = userEmail;
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }
}