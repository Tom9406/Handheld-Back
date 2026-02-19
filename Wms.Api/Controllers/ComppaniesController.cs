using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wms.Api.Data;
using Wms.Api.Dtos.Company;

namespace Wms.Api.Controllers;

[ApiController]
[Route("api/companies")]
public class CompaniesController : ControllerBase
{
    private readonly WmsDbContext _db;

    public CompaniesController(WmsDbContext db)
    {
        _db = db;
    }

    // ======================================================
    // GET: api/companies?pageNumber=1&pageSize=20
    //      &name=ABC
    //      &code=COMP01
    //      &isActive=true
    //      &sortBy=Name
    // ======================================================
    [HttpGet]
    public async Task<IActionResult> GetCompanies(
        int pageNumber = 1,
        int pageSize = 20,
        string? name = null,
        string? code = null,
        bool? isActive = null,
        string? sortBy = "CreatedAt",
        bool sortDesc = true)
    {
        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var query = _db.Companies
            .AsNoTracking()
            .AsQueryable();

        // ========================
        // Filters
        // ========================
        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(x => x.Name.Contains(name));

        if (!string.IsNullOrWhiteSpace(code))
            query = query.Where(x => x.Code.Contains(code));

        if (isActive.HasValue)
            query = query.Where(x => x.IsActive == isActive);

        // ========================
        // Sorting
        // ========================
        query = sortBy?.ToLower() switch
        {
            "name" => sortDesc
                ? query.OrderByDescending(x => x.Name)
                : query.OrderBy(x => x.Name),

            "code" => sortDesc
                ? query.OrderByDescending(x => x.Code)
                : query.OrderBy(x => x.Code),

            _ => sortDesc
                ? query.OrderByDescending(x => x.CreatedAt)
                : query.OrderBy(x => x.CreatedAt)
        };

        var totalRecords = await query.CountAsync();

        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new CompanyDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                IsActive = x.IsActive,
                CompanyType = x.CompanyType,
                CurrencyCode = x.CurrencyCode,
                TimeZone = x.TimeZone,
                IsWmsEnabled = x.IsWmsEnabled,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();

        var response = new
        {
            pageNumber,
            pageSize,
            totalRecords,
            totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize),
            data
        };

        return Ok(response);
    }

    // ======================================================
    // GET: api/companies/{id}
    // ======================================================
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var company = await _db.Companies
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new CompanyDetailDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                LegalName = x.LegalName,
                TaxId = x.TaxId,
                Address1 = x.Address1,
                City = x.City,
                Country = x.Country,
                CompanyType = x.CompanyType,
                DefaultWarehouseId = x.DefaultWarehouseId,
                TimeZone = x.TimeZone,
                CurrencyCode = x.CurrencyCode,
                ExternalSystem = x.ExternalSystem,
                ExternalCompanyId = x.ExternalCompanyId,
                IsWmsEnabled = x.IsWmsEnabled,
                AllowCrossWarehouse = x.AllowCrossWarehouse,
                AllowNegativeStock = x.AllowNegativeStock,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (company == null)
            return NotFound();

        return Ok(company);
    }



    // ======================================================
    // POST: api/companies
    // ======================================================
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCompanyDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Validar duplicado por Code
        var exists = await _db.Companies
            .AnyAsync(x => x.Code == dto.Code);

        if (exists)
        {
            return Conflict(new
            {
                message = "Company code already exists."
            });
        }

        var entity = new Entities.Company
        {
            Id = Guid.NewGuid(),
            Code = dto.Code,
            Name = dto.Name,
            CurrencyCode = dto.CurrencyCode,
            TimeZone = dto.TimeZone,
            CompanyType = dto.CompanyType,
            LegalName = dto.LegalName,
            TaxId = dto.TaxId,
            Address1 = dto.Address1,
            City = dto.City,
            Country = dto.Country,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            IsWmsEnabled = true,
            AllowCrossWarehouse = false,
            AllowNegativeStock = false,
            ApiKey = Guid.NewGuid().ToString("N")
        };

        _db.Companies.Add(entity);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Protección adicional si existe índice único en BD
            return Conflict(new
            {
                message = "Company code already exists."
            });
        }

        var result = new CompanyDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            IsActive = entity.IsActive,
            CompanyType = entity.CompanyType,
            CurrencyCode = entity.CurrencyCode,
            TimeZone = entity.TimeZone,
            IsWmsEnabled = entity.IsWmsEnabled,
            CreatedAt = entity.CreatedAt
        };

        return CreatedAtAction(nameof(GetById),
            new { id = entity.Id },
            result);
    }

}
