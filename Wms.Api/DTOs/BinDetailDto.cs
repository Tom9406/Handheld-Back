namespace Wms.Api.Dtos.Bin
{
    public class BinDetailDto
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
        public string CompanyName { get; set; } = null!;

        // Preparado para futuro Warehouse
        // public Guid? WarehouseId { get; set; }
        // public string? WarehouseCode { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
