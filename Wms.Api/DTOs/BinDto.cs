namespace Wms.Api.Dtos.Bin
{
    public class BinDto
    {
        public Guid Id { get; set; }

        public string BinCode { get; set; } = null!;
        public string? Description { get; set; }

        public bool IsActive { get; set; }
        public bool IsBlocked { get; set; }

        public string BinType { get; set; } = null!;

        public bool AllowPicking { get; set; }
        public bool AllowPutaway { get; set; }

        public Guid CompanyId { get; set; }
    }
}
