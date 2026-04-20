using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wms.Api.Common;
using Wms.Api.Data;
using Wms.Api.DTOs;

namespace Wms.Api.Controllers;

[Route("api/stock")]
public class StockController : BaseController
{
    private readonly WmsDbContext _context;

    public StockController(WmsDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<StockEnrichedDto>>> Get(
        [FromQuery] Guid? companyId = null,
        [FromQuery] PaginationParameters? pagination = null)
    {
        pagination ??= new PaginationParameters();
        var activeCompanyId = ResolveCompanyId(companyId);

        var query =
            from s in _context.CurrentStock.AsNoTracking()
            join i in _context.Items.AsNoTracking() on s.ItemId equals i.Id
            where i.CompanyId == activeCompanyId
            select new StockEnrichedDto
            {
                CompanyId = i.CompanyId,
                ItemId = s.ItemId,
                BinId = s.BinId,
                StockQty = s.StockQty
            };

        var totalRecords = await query.CountAsync();

        var stock = await query
            .OrderBy(s => s.ItemId)
            .ThenBy(s => s.BinId)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return Ok(new PagedResponse<StockEnrichedDto>
        {
            Data = stock,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pagination.PageSize)
        });
    }

    [HttpGet("enriched")]
    public async Task<ActionResult<PagedResponse<StockEnrichedDto>>> GetEnriched(
        [FromQuery] Guid? companyId,
        [FromQuery] string? search,
        [FromQuery] PaginationParameters? pagination = null)
    {
        pagination ??= new PaginationParameters();
        var activeCompanyId = ResolveCompanyId(companyId);

        var query =
            from s in _context.CurrentStock.AsNoTracking()
            join i in _context.Items.AsNoTracking() on s.ItemId equals i.Id
            join b in _context.Bins.AsNoTracking() on s.BinId equals b.Id
            where i.CompanyId == activeCompanyId
               && (string.IsNullOrEmpty(search)
                   || i.ItemNo.Contains(search)
                   || (i.Description != null && i.Description.Contains(search))
                   || b.BinCode.Contains(search))
            select new StockEnrichedDto
            {
                CompanyId = i.CompanyId,
                ItemId = i.Id,
                ItemNo = i.ItemNo,
                ItemDescription = i.Description,
                ItemUOM = i.UOM,
                ItemType = i.ItemType,
                ItemIsActive = i.IsActive,
                ItemCategoryCode = i.CategoryCode,
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
                StockQty = s.StockQty,
                ImageUrl = _context.ItemImages
                    .Where(img => img.ItemId == i.Id)
                    .Select(img => img.Url)
                    .FirstOrDefault()
            };

        var totalRecords = await query.CountAsync();

        var result = await query
            .OrderBy(x => x.ItemNo)
            .ThenBy(x => x.BinCode)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return Ok(new PagedResponse<StockEnrichedDto>
        {
            Data = result,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pagination.PageSize)
        });
    }
}
