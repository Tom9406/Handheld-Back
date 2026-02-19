using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wms.Api.Common;
using Wms.Api.Data;
using Wms.Api.DTOs;

namespace Wms.Api.Controllers;

[ApiController]
[Route("api/stock")]
public class StockController : ControllerBase
{
    private readonly WmsDbContext _context;

    public StockController(WmsDbContext context)
    {
        _context = context;
    }

    // ====================================================
    // GET: api/stock?pageNumber=1&pageSize=20
    // ====================================================
    [HttpGet]
    public async Task<ActionResult<PagedResponse<StockEnrichedDto>>> Get(
        [FromQuery] PaginationParameters? pagination = null)
    {
        pagination ??= new PaginationParameters();

        var query = _context.CurrentStock
            .AsNoTracking()
            .AsQueryable();

        var totalRecords = await query.CountAsync();

        var stock = await query
            .OrderBy(s => s.ItemId)
            .ThenBy(s => s.BinId)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(s => new StockEnrichedDto
            {
                ItemId = s.ItemId,
                BinId = s.BinId,
                StockQty = s.StockQty
            })
            .ToListAsync();

        var response = new PagedResponse<StockEnrichedDto>
        {
            Data = stock,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pagination.PageSize)
        };

        return Ok(response);
    }

    // ====================================================
    // GET: api/stock/enriched?companyId=GUID&pageNumber=1&pageSize=20
    // ====================================================
    [HttpGet("enriched")]
    public async Task<ActionResult<PagedResponse<StockEnrichedDto>>> GetEnriched(
        [FromQuery] Guid? companyId,
        [FromQuery] PaginationParameters? pagination = null)
    {
        pagination ??= new PaginationParameters();

        var query =
            from s in _context.CurrentStock.AsNoTracking()
            join i in _context.Items.AsNoTracking() on s.ItemId equals i.Id
            join b in _context.Bins.AsNoTracking() on s.BinId equals b.Id
            where !companyId.HasValue || i.CompanyId == companyId
            select new StockEnrichedDto
            {
                CompanyId = i.CompanyId,

                ItemId = i.Id,
                ItemNo = i.ItemNo,
                ItemDescription = i.Description,
                ItemUOM = i.UOM,
                ItemType = i.ItemType,
                ItemIsActive = i.IsActive,

                IsLotTracked = i.IsLotTracked,
                IsSerialTracked = i.IsSerialTracked,
                IsExpirationTracked = i.IsExpirationTracked,

                UnitWeight = i.UnitWeight,
                UnitVolume = i.UnitVolume,

                BinId = b.Id,
                BinCode = b.BinCode,
                BinDescription = b.Description,
                BinType = b.BinType,

                BinIsActive = b.IsActive,
                IsBlocked = b.IsBlocked,
                AllowPicking = b.AllowPicking,
                AllowPutaway = b.AllowPutaway,

                StockQty = s.StockQty
            };

        var totalRecords = await query.CountAsync();

        var result = await query
            .OrderBy(x => x.ItemNo)
            .ThenBy(x => x.BinCode)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        var response = new PagedResponse<StockEnrichedDto>
        {
            Data = result,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pagination.PageSize)
        };

        return Ok(response);
    }
}
