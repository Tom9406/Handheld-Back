using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wms.Api.Common;
using Wms.Api.Data;
using Wms.Api.Dtos;

namespace Wms.Api.Controllers;

[Route("api/posted-receiving-headers")]
public class PostedReceivingHeadersController : BaseController
{
    private readonly WmsDbContext _db;

    public PostedReceivingHeadersController(WmsDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<PostedReceivingHeaderDto>>> GetAll(
        [FromQuery] Guid? companyId,
        [FromQuery] PaginationParameters? pagination = null)
    {
        pagination ??= new PaginationParameters();
        var activeCompanyId = ResolveCompanyId(companyId);

        var query = _db.PostedReceivingHeaders
            .AsNoTracking()
            .Where(x => x.CompanyId == activeCompanyId);

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

        return Ok(new PagedResponse<PostedReceivingHeaderDto>
        {
            Data = data,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pagination.PageSize)
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostedReceivingHeaderDto>> GetById(Guid id)
    {
        var allowedCompanies = AccessibleCompanyIds;

        var data = await _db.PostedReceivingHeaders
            .AsNoTracking()
            .Where(p => p.Id == id && allowedCompanies.Contains(p.CompanyId))
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

        if (data == null)
            return NotFound();

        return Ok(data);
    }

    [HttpGet("by-receiving/{receivingHeaderId:guid}")]
    public async Task<ActionResult<IEnumerable<PostedReceivingHeaderDto>>> GetByReceivingHeader(Guid receivingHeaderId, [FromQuery] Guid? companyId = null)
    {
        var activeCompanyId = ResolveCompanyId(companyId);

        var data = await _db.PostedReceivingHeaders
            .AsNoTracking()
            .Where(x => x.ReceivingHeaderId == receivingHeaderId && x.CompanyId == activeCompanyId)
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
