namespace Wms.Api.Dtos.Company
{
    public class CreateCompanyDto
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string CurrencyCode { get; set; } = null!;
        public string TimeZone { get; set; } = null!;

        public string? CompanyType { get; set; }
        public string? LegalName { get; set; }
        public string? TaxId { get; set; }
        public string? Address1 { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
    }
}
