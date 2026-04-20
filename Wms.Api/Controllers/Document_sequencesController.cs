using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wms.Api.Common;
using Wms.Api.Data;
using Wms.Api.DTOs;

namespace Wms.Api.Controllers;

[Route("api/document-sequence")]
public class DocumentSequencesController : BaseController
{
    private readonly WmsDbContext _db;

    public DocumentSequencesController(WmsDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<Document_sequencesDto>>> GetDocument(
        [FromQuery] Guid? companyId,
        [FromQuery] PaginationParameters? pagination = null)
    {
        pagination ??= new PaginationParameters();
        var activeCompanyId = ResolveCompanyId(companyId);

        var query = _db.DocumentSequences
            .AsNoTracking()
            .Where(i => i.CompanyId == activeCompanyId);

        var totalRecords = await query.CountAsync();

        var documentSequence = await query
            .OrderBy(ds => ds.DocumentType)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(ds => new Document_sequencesDto
            {
                Id = ds.Id,
                CompanyId = ds.CompanyId,
                Document_type = ds.DocumentType,
                LastNumber = ds.LastNumber
            })
            .ToListAsync();

        return Ok(new PagedResponse<Document_sequencesDto>
        {
            Data = documentSequence,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pagination.PageSize)
        });
    }
}
