namespace Wms.Api.Dtos.Item
{
    public class ItemDetailDto
    {
        public Guid Id { get; set; }

        public string ItemNo { get; set; } = null!;
        public string? Description { get; set; }

        public string UOM { get; set; }
        public string BaseUOM { get; set; } = null!;
        public string? SalesUOM { get; set; }
        public string? PurchaseUOM { get; set; }

        public decimal? ConversionFactor { get; set; }

        public bool IsActive { get; set; }
        public string ItemType { get; set; } = null!;

        public string? Barcode { get; set; }
        public string? AltBarcode { get; set; }

        public bool IsLotTracked { get; set; }
        public bool IsSerialTracked { get; set; }
        public bool IsExpirationTracked { get; set; }

        public decimal? UnitWeight { get; set; }
        public decimal? UnitVolume { get; set; }

        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }

        public string? CategoryCode { get; set; }
        public string? Brand { get; set; }
        public string? ABCClass { get; set; }

        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; } = null!;

        public string CreatedBy { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
