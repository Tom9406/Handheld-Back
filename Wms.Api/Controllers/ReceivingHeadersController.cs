using Handheld.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wms.Api.Common;
using Wms.Api.Data;
using Wms.Api.Dtos.ReceivingHeader;
using Wms.Api.Dtos.ReceivingLine;
using Wms.Api.Entities;

namespace Wms.Api.Controllers;

[Route("api/[controller]")]
public class ReceivingHeadersController : BaseController
{
    private readonly WmsDbContext _context;

    public ReceivingHeadersController(WmsDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<ReceivingHeaderDto>>> GetAll([FromQuery] PaginationParameters? pagination = null)
    {
        pagination ??= new PaginationParameters();
        var companyId = CompanyId;

        var query = _context.ReceivingHeaders
            .AsNoTracking()
            .Where(h => h.CompanyId == companyId);

        var totalRecords = await query.CountAsync();

        var headers = await query
            .OrderByDescending(h => h.ReceiptDate)
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

        return Ok(new PagedResponse<ReceivingHeaderDto>
        {
            Data = headers,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pagination.PageSize)
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReceivingHeaderDetailDto>> GetById(Guid id)
    {
        var companyId = CompanyId;

        var header = await _context.ReceivingHeaders
            .AsNoTracking()
            .Where(h => h.Id == id && h.CompanyId == companyId)
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
                    QuantityReceived = l.QuantityReceived,
                    PostedQuantityReceived = _context.PostedReceivingLines
                        .Where(x =>
                            x.ReceivingLineId == l.Id &&
                            x.CompanyId == companyId)
                        .Sum(x => (decimal?)x.QuantityReceived) ?? 0
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (header == null)
            return NotFound();

        return Ok(header);
    }

    [HttpPost("{id:guid}/post")]
    public async Task<IActionResult> PostReceipt(Guid id)
    {
        var companyId = CompanyId;
        var email = UserEmail;

        if (Role != "ADMIN" && Role != "SUPERVISOR" && Role != "OPERATOR")
            return Forbid();

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var receive = await _context.ReceivingHeaders
                .Include(x => x.Lines)
                .Include(x => x.Company)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.CompanyId == companyId);

            if (receive == null)
                return NotFound("Receipt not found.");

            if (receive.Status == "POSTED")
                return BadRequest("Already posted.");

            if (receive.Lines == null || !receive.Lines.Any())
                return BadRequest("Receipt has no lines.");

            var sequence = await _context.DocumentSequences
                .FromSqlRaw(@"
                    SELECT TOP 1 * FROM DocumentSequences WITH (UPDLOCK, ROWLOCK, HOLDLOCK)
                    WHERE CompanyId = {0} AND DocumentType = {1}",
                    companyId, "POSTED_RECEIVE")
                .AsTracking()
                .FirstOrDefaultAsync();

            if (sequence == null)
                return BadRequest("Sequence not configured.");

            sequence.LastNumber++;
            var postedReceiveNo = $"PR-{sequence.LastNumber:D6}";

            var postedReceipt = new PostedReceivingHeader
            {
                Id = Guid.NewGuid(),
                PostedReceivingNo = postedReceiveNo,
                ReceivingHeaderId = receive.Id,
                CompanyId = companyId,
                CompanyCode = receive.Company.Code,
                ReceiptNo = receive.ReceiptNo,
                ExternalDocumentNo = receive.ExternalDocumentNo,
                VendorCode = receive.VendorCode,
                VendorName = receive.VendorName,
                ReceiptDate = receive.ReceiptDate,
                Status = "POSTED",
                PostedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                PostedBy = email
            };

            _context.PostedReceivingHeaders.Add(postedReceipt);

            var totalQty = 0m;
            var processedLines = 0;

            foreach (var line in receive.Lines)
            {
                if (line.QuantityReceived <= 0)
                    continue;

                if (line.BinId == null)
                    return BadRequest("BinId is required.");

                if (line.QuantityReceived > line.QuantityExpected)
                    return BadRequest("Over-receiving is not allowed.");

                _context.InventoryMovements.Add(new InventoryMovements
                {
                    Id = Guid.NewGuid(),
                    CompanyId = companyId,
                    ItemId = line.ItemId,
                    BinId = line.BinId.Value,
                    Quantity = line.QuantityReceived,
                    MovementType = "IN",
                    ReferenceNo = postedReceiveNo,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = email
                });

                _context.PostedReceivingLines.Add(new PostedReceivingLine
                {
                    Id = Guid.NewGuid(),
                    PostedReceivingHeaderId = postedReceipt.Id,
                    ReceivingLineId = line.Id,
                    CompanyId = companyId,
                    ItemId = line.ItemId,
                    BinId = line.BinId.Value,
                    QuantityExpected = line.QuantityExpected,
                    QuantityReceived = line.QuantityReceived,
                    UOM = line.UOM,
                    Status = "POSTED",
                    PostedBy = email,
                    PostedAt = DateTime.UtcNow
                });

                line.Status = "POSTED";
                line.UpdatedAt = DateTime.UtcNow;
                line.UpdatedBy = email;

                totalQty += line.QuantityReceived;
                processedLines++;
            }

            if (processedLines == 0)
                return BadRequest("Nothing to post.");

            postedReceipt.TotalLines = processedLines;
            postedReceipt.TotalQty = totalQty;

            receive.Status = "POSTED";
            receive.PostedAt = DateTime.UtcNow;
            receive.PostedBy = email;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                message = "Receipt posted successfully.",
                postedReceiveNo
            });
        }
        catch
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateReceipt(CreateReceivingHeaderDto dto)
    {
        var companyId = CompanyId;
        var email = UserEmail;

        if (Role != "ADMIN" && Role != "SUPERVISOR")
            return Forbid();

        if (dto.Lines == null || !dto.Lines.Any())
            return BadRequest("Receipt must have lines.");

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var sequence = await _context.DocumentSequences
                .FromSqlRaw(@"
                    SELECT TOP 1 * FROM DocumentSequences WITH (UPDLOCK, ROWLOCK, HOLDLOCK)
                    WHERE CompanyId = {0} AND DocumentType = {1}",
                    companyId, "RECEIPT_CREATED")
                .AsTracking()
                .FirstOrDefaultAsync();

            if (sequence == null)
                return BadRequest("Sequence not configured.");

            sequence.LastNumber++;
            var receiptNo = $"RCP-{sequence.LastNumber:D6}";

            var header = new ReceivingHeader
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                ReceiptNo = receiptNo,
                ExternalDocumentNo = dto.ExternalDocumentNo,
                VendorCode = dto.VendorCode,
                VendorName = dto.VendorName,
                ReceiptDate = dto.ReceiptDate ?? DateTime.UtcNow,
                Status = "OPEN",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = email
            };

            _context.ReceivingHeaders.Add(header);

            foreach (var l in dto.Lines)
            {
                if (l.QuantityExpected <= 0)
                    return BadRequest("Invalid quantity.");

                if (string.IsNullOrWhiteSpace(l.UOM))
                    return BadRequest("UOM is required.");

                _context.ReceivingLines.Add(new ReceivingLine
                {
                    Id = Guid.NewGuid(),
                    ReceivingHeaderId = header.Id,
                    CompanyId = companyId,
                    ItemId = l.ItemId,
                    BinId = l.BinId,
                    QuantityExpected = l.QuantityExpected,
                    QuantityReceived = 0,
                    UOM = l.UOM,
                    Status = "OPEN",
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                headerId = header.Id,
                receiptNo
            });
        }
        catch
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "Internal server error");
        }
    }
}
