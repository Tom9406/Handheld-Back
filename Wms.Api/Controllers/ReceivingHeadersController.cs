using Handheld.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wms.Api.Common;
using Wms.Api.Data;
using Wms.Api.Dtos.ReceivingHeader;
using Wms.Api.Dtos.ReceivingLine;
using Wms.Api.Entities;

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

    // ======================================================
    // POST: api/shipments/{id}/post
    // ======================================================
    [HttpPost("{id:guid}/post")]
    public async Task<IActionResult> PostShipment(Guid id, Guid companyId)
    {
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
                return NotFound("Shipment not found.");

            if (receive.Lines == null || !receive.Lines.Any())
                return BadRequest("Shipment has no lines.");

            int lineNo = 1;
            int processedLines = 0;
            decimal totalQtyPostedNow = 0;

            // =========================================
            // Generar consecutivo PostedShipmentNo
            // =========================================
            var sequence = await _context.DocumentSequences
                .FirstOrDefaultAsync(x =>
                    x.CompanyId == companyId &&
                    x.DocumentType == "POSTED_RECEIVE");

            if (sequence == null)
            {
                sequence = new DocumentSequence
                {
                    Id = Guid.NewGuid(),
                    CompanyId = companyId,
                    DocumentType = "POSTED_RECEIVE",
                    LastNumber = 0
                };

                _context.DocumentSequences.Add(sequence);
            }

            sequence.LastNumber++;
            var postedReceiveNo = $"PS-{sequence.LastNumber:D6}";

            // =========================================
            // Crear header SOLO si hay algo que postear
            // =========================================
            var postedReceipt = new PostedReceivingHeader
            {
                Id = Guid.NewGuid(),
                PostedReceivingNo = postedReceiveNo,
                ReceivingHeaderId = receive.Id,
                CompanyId = receive.CompanyId,   
                CompanyCode = receive.Company.Code,
                ReceiptNo = receive.ReceiptNo,           
                ExternalDocumentNo = receive.ExternalDocumentNo,
                VendorCode = receive.VendorCode,
                VendorName = receive.VendorName,
                Status = "POSTED",
                ReceiptDate = receive.ReceiptDate,

                PostedAt = DateTime.UtcNow,
                PostedBy = "SYSTEM",
                CreatedAt = DateTime.UtcNow
            };

            _context.PostedReceivingHeaders.Add(postedReceipt);

            foreach (var line in receive.Lines)
            {
                if (line.QuantityReceived <= 0)
                    continue;

                //  Calcular cuánto ya fue posteado antes
                var alreadyPostedQty = await _context.PostedReceivingLines
                    .Where(x => x.ReceivingLineId == line.Id)
                    .SumAsync(x => (decimal?)x.QuantityReceived) ?? 0;

                var remainingQty = line.QuantityExpected - alreadyPostedQty;

                if (remainingQty <= 0)
                    continue;

                if (line.QuantityReceived > remainingQty)
                    return BadRequest($"Cannot ship more than remaining quantity for item {line.Item.ItemNo}.");

                if (line.BinId == null)
                    return BadRequest($"Shipment line for item {line.Item.ItemNo} has no Bin assigned.");

                var stockQty = await _context.InventoryMovements
                    .Where(x =>
                        x.CompanyId == companyId &&
                        x.ItemId == line.ItemId &&
                        x.BinId == line.BinId)
                    .SumAsync(x => (decimal?)x.Quantity) ?? 0;

                if (stockQty < line.QuantityReceived)
                    return BadRequest(
                        $"Insufficient stock for item {line.Item.ItemNo} in bin {line.Bin.BinCode}.");

                // ===============================
                // Inventory Movement
                // ===============================
                var movement = new InventoryMovements
                {
                    Id = Guid.NewGuid(),
                    CompanyId = companyId,
                    ItemId = line.ItemId,
                    BinId = line.BinId.Value,
                    Quantity = line.QuantityReceived,
                    MovementType = "IN",
                    ReferenceNo = postedReceiveNo,
                    CreatedAt = DateTime.UtcNow
                };

                _context.InventoryMovements.Add(movement);

                // ===============================
                // Posted Shipment Line
                // ===============================
                var postedLine = new PostedReceivingLine
                {
                    Id = Guid.NewGuid(),
                    PostedReceivingHeaderId = postedReceipt.Id,
                    ReceivingLineId = line.Id,                    
                    ItemId = line.ItemId,                    
                    CompanyId = companyId,
                    BinId = line.BinId.Value,
                    QuantityExpected = line.QuantityExpected,
                    QuantityReceived = line.QuantityReceived,
                    UOM = line.UOM,
                    Status = "POSTED",
                    PostedAt = DateTime.UtcNow,
                    PostedBy = "SYSTEM"
                };

                _context.PostedReceivingLines.Add(postedLine);

                totalQtyPostedNow += line.QuantityReceived;
                processedLines++;


                // ===============================
                // Actualizar estado línea original
                // ===============================
                var newPostedTotal = alreadyPostedQty + line.QuantityReceived;

                if (newPostedTotal >= line.QuantityExpected)
                    line.Status = "POSTED";
                else
                    line.Status = "PARTIALLY POSTED";

                line.QuantityReceived = 0;
                line.UpdatedAt = DateTime.UtcNow;
            }

            if (processedLines == 0)
                return BadRequest("Nothing to post.");

            // ===============================
            // Actualizar header original
            // ===============================
            var allLinesFullyPosted = receive.Lines.All(l =>
                (_context.PostedReceivingLines
                    .Where(x => x.ReceivingLineId == l.Id)
                    .Sum(x => (decimal?)x.QuantityReceived) ?? 0) >= l.QuantityExpected);

            receive.Status = allLinesFullyPosted
                ? "POSTED"
                : "PARTIALLY POSTED";

            receive.UpdatedAt = DateTime.UtcNow;

            postedReceipt.TotalLines = processedLines;
            postedReceipt.TotalQty = totalQtyPostedNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                message = "Receive posted successfully.",
                receivetId = receive.Id,
                receivetNo = receive.ReceiptNo,
                postedReceiveNo = postedReceiveNo,
                status = receive.Status
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, ex.Message);
        }
    }


}
