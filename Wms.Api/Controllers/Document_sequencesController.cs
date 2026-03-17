using Handheld.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Xml;
using Wms.Api.Common;
using Wms.Api.Data;
using Wms.Api.Dtos.Item;
using Wms.Api.DTOs;
using Wms.Api.Entities;

namespace Wms.Api.Controllers;

[ApiController]
[Route("api/document_sequence")]

    public class Document_sequencesController : ControllerBase
    {
    private readonly WmsDbContext _db;

    public Document_sequencesController(WmsDbContext db)
    {
        _db = db;
    }

    // ====================================================
    // GET: api/document_sequence?companyId={companyId}&pageNumber=1&pageSize=20
    // ====================================================
    [HttpGet]
    public async Task<ActionResult<PagedResponse<ItemDto>>> GetDocument(
        [FromQuery] Guid? companyId,
        
        [FromQuery] PaginationParameters? pagination = null)
    {
        pagination ??= new PaginationParameters();

        var query = _db.DocumentSequences
            .AsNoTracking()
            .AsQueryable();

        if (companyId.HasValue)
            query = query.Where(i => i.CompanyId == companyId.Value);

        

        var totalRecords = await query.CountAsync();

        var document_sequence = await query
            .OrderBy(ds => ds.DocumentType) // orden estable obligatorio para paginación
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

        var response = new PagedResponse<Document_sequencesDto>
        {
            Data = document_sequence,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pagination.PageSize)
        };

        return Ok(response);
    }

}

