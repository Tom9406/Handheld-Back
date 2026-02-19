using System;

namespace Wms.Api.Dtos.Company
{
    public class CompanyDto
    {
        public Guid Id { get; set; }

        public string Code { get; set; } = null!;

        public string Name { get; set; } = null!;

        public bool IsActive { get; set; }

        public string? CompanyType { get; set; }

        public string CurrencyCode { get; set; } = null!;

        public string TimeZone { get; set; } = null!;

        public bool IsWmsEnabled { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
