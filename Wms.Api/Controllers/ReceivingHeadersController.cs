using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wms.Api.Common;
using Wms.Api.Data;
using Wms.Api.Dtos.ReceivingHeader;
using Wms.Api.Dtos.ReceivingLine;

namespace Wms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReceivingHeadersController : ControllerBase
{
    private readonly WmsDbContext _context;

    public ReceivingHeadersController(WmsDbContext context)
    {
        _context = context;
    }

    // ====================================================
    // GET: api/ReceivingHeaders?companyId={id}&pageNumber=1&pageSize=20
    // ====================================================
    [HttpGet]
    public async Task<ActionResult<PagedResponse<ReceivingHeaderDto>>> GetAll(
        [FromQuery] Guid? companyId,
        [FromQuery] PaginationParameters? pagination = null)
    {
        pagination ??= new PaginationParameters();

        var query = _context.ReceivingHeaders
            .AsNoTracking()
            .AsQueryable();

        if (companyId.HasValue)
            query = query.Where(h => h.CompanyId == companyId.Value);

        var totalRecords = await query.CountAsync();

        var headers = await query
            .OrderByDescending(h => h.ReceiptDate) // orden operativo lógico
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(h => new ReceivingHeaderDto
            {
                Id = h.Id,
                CompanyId = h.CompanyId,
                CompanyName = h.Company.Name,

                ReceiptNo = h.ReceiptNo,
                ExternalDocumentNo = h.ExternalDocumentNo,
                VendorCode = h.VendorCode,
                VendorName = h.VendorName,

                Status = h.Status,
                ReceiptDate = h.ReceiptDate,

                CreatedAt = h.CreatedAt,
                CreatedBy = h.CreatedBy
            })
            .ToListAsync();

        var response = new PagedResponse<ReceivingHeaderDto>
        {
            Data = headers,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pagination.PageSize)
        };

        return Ok(response);
    }

    // ====================================================
    // GET: api/ReceivingHeaders/{id}
    // ====================================================
    [HttpGet("{id}")]
    public async Task<ActionResult<ReceivingHeaderDetailDto>> GetById(Guid id)
    {
        var header = await _context.ReceivingHeaders
            .AsNoTracking()
            .Where(h => h.Id == id)
            .Select(h => new ReceivingHeaderDetailDto
            {
                Id = h.Id,
                CompanyId = h.CompanyId,
                CompanyName = h.Company.Name,

                ReceiptNo = h.ReceiptNo,
                ExternalDocumentNo = h.ExternalDocumentNo,
                VendorCode = h.VendorCode,
                VendorName = h.VendorName,

                Status = h.Status,
                ReceiptDate = h.ReceiptDate,

                CreatedAt = h.CreatedAt,
                PostedAt = h.PostedAt,
                CreatedBy = h.CreatedBy,
                PostedBy = h.PostedBy,

                Lines = h.Lines.Select(l => new ReceivingLineDto
                {
                    Id = l.Id,
                    ReceivingHeaderId = l.ReceivingHeaderId,
                    ItemId = l.ItemId,
                    BinId = l.BinId,
                    QuantityExpected = l.QuantityExpected,
                    QuantityReceived = l.QuantityReceived
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (header == null)
            return NotFound();

        return Ok(header);
    }
}
