using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wms.Api.Common;
using Wms.Api.Data;
using Wms.Api.Dtos;

namespace Wms.Api.Controllers;

[ApiController]
[Route("api/posted-receiving-headers")]
public class PostedReceivingHeadersController : ControllerBase
{
    private readonly WmsDbContext _db;

    public PostedReceivingHeadersController(WmsDbContext db)
    {
        _db = db;
    }

    // ====================================================
    // GET: api/posted-receiving-headers?companyId={companyId}&pageNumber=1&pageSize=20
    // ====================================================
    [HttpGet]
    public async Task<ActionResult<PagedResponse<PostedReceivingHeaderDto>>> GetAll(
        [FromQuery] Guid? companyId,
        [FromQuery] PaginationParameters? pagination = null)
    {
        pagination ??= new PaginationParameters();

        var query = _db.PostedReceivingHeaders
            .AsNoTracking()
            .AsQueryable();

        if (companyId.HasValue)
            query = query.Where(x => x.CompanyId == companyId.Value);

        var totalRecords = await query.CountAsync();

        var data = await query
            .OrderByDescending(x => x.PostedAt)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(x => new PostedReceivingHeaderDto
            {
                Id = x.Id,
                PostedReceivingNo = x.PostedReceivingNo,
                ReceivingHeaderId = x.ReceivingHeaderId,
                CompanyId = x.CompanyId,
                CompanyCode = x.CompanyCode,
                ReceiptNo = x.ReceiptNo,
                ExternalDocumentNo = x.ExternalDocumentNo,
                VendorId = x.VendorId,
                VendorCode = x.VendorCode,
                VendorName = x.VendorName,
                ReceiptDate = x.ReceiptDate,
                TotalLines = x.TotalLines,
                TotalQty = x.TotalQty,
                TotalWeight = x.TotalWeight,
                TotalVolume = x.TotalVolume,
                PostedBy = x.PostedBy,
                PostedAt = x.PostedAt,
                CreatedAt = x.CreatedAt,
                Status = x.Status
            })
            .ToListAsync();

        var response = new PagedResponse<PostedReceivingHeaderDto>
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
    // GET: api/posted-receiving-headers/{id}
    // ====================================================
    [HttpGet("{id}")]
    public async Task<ActionResult<PostedReceivingHeaderDto>> GetById(Guid id)
    {
        var x = await _db.PostedReceivingHeaders
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(x => new PostedReceivingHeaderDto
            {
                Id = x.Id,
                PostedReceivingNo = x.PostedReceivingNo,
                ReceivingHeaderId = x.ReceivingHeaderId,
                CompanyId = x.CompanyId,
                CompanyCode = x.CompanyCode,
                ReceiptNo = x.ReceiptNo,
                ExternalDocumentNo = x.ExternalDocumentNo,
                VendorId = x.VendorId,
                VendorCode = x.VendorCode,
                VendorName = x.VendorName,
                ReceiptDate = x.ReceiptDate,
                TotalLines = x.TotalLines,
                TotalQty = x.TotalQty,
                TotalWeight = x.TotalWeight,
                TotalVolume = x.TotalVolume,
                PostedBy = x.PostedBy,
                PostedAt = x.PostedAt,
                CreatedAt = x.CreatedAt,
                Status = x.Status
            })
            .FirstOrDefaultAsync();

        if (x == null)
            return NotFound();

        return Ok(x);
    }

    // ====================================================
    // GET: api/posted-receiving-headers/by-receiving/{receivingHeaderId}?companyId={companyId}
    // ====================================================
    [HttpGet("by-receiving/{receivingHeaderId}")]
    public async Task<ActionResult<IEnumerable<PostedReceivingHeaderDto>>> GetByReceivingHeader(
        Guid receivingHeaderId,
        [FromQuery] Guid companyId)
    {
        var data = await _db.PostedReceivingHeaders
            .AsNoTracking()
            .Where(x => x.ReceivingHeaderId == receivingHeaderId
                     && x.CompanyId == companyId)
            .OrderByDescending(x => x.PostedAt)
            .Select(x => new PostedReceivingHeaderDto
            {
                Id = x.Id,
                PostedReceivingNo = x.PostedReceivingNo,
                ReceivingHeaderId = x.ReceivingHeaderId,
                CompanyId = x.CompanyId,
                CompanyCode = x.CompanyCode,
                ReceiptNo = x.ReceiptNo,
                ExternalDocumentNo = x.ExternalDocumentNo,
                VendorId = x.VendorId,
                VendorCode = x.VendorCode,
                VendorName = x.VendorName,
                ReceiptDate = x.ReceiptDate,
                TotalLines = x.TotalLines,
                TotalQty = x.TotalQty,
                TotalWeight = x.TotalWeight,
                TotalVolume = x.TotalVolume,
                PostedBy = x.PostedBy,
                PostedAt = x.PostedAt,
                CreatedAt = x.CreatedAt,
                Status = x.Status
            })
            .ToListAsync();

        return Ok(data);
    }
}