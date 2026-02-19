namespace Wms.Api.Entities
{
    public class Company
    {
        public Guid Id { get; set; }

        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }   // ✅ Nullable

        public string? CompanyType { get; set; }

        public Guid? DefaultWarehouseId { get; set; }

        public string TimeZone { get; set; } = null!;
        public string CurrencyCode { get; set; } = null!;

        public string? LegalName { get; set; }
        public string? TaxId { get; set; }
        public string? Address1 { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }

        public string? ExternalSystem { get; set; }
        public string? ExternalCompanyId { get; set; }

        public string? ApiKey { get; set; }
        public string? CallbackUrl { get; set; }

        public bool IsWmsEnabled { get; set; }
        public bool AllowCrossWarehouse { get; set; }
        public bool AllowNegativeStock { get; set; }

        public ICollection<Item> Items { get; set; } = new List<Item>();
        public ICollection<Bin> Bins { get; set; } = new List<Bin>();
    }
}
