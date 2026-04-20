namespace Wms.Api.DTOs
{
    public class UpdateItemDto
    {
        public string Description { get; set; } = string.Empty;
        public string UOM { get; set; } = string.Empty;

        public string BaseUOM { get; set; } = string.Empty;
        public string SalesUOM { get; set; } = string.Empty;
        public string PurchaseUOM { get; set; } = string.Empty;

        public string ItemType { get; set; } = string.Empty;

        public string Barcode { get; set; } = string.Empty;
        public string AltBarcode { get; set; } = string.Empty;

        public bool IsLotTracked { get; set; }
        public bool IsSerialTracked { get; set; }
        public bool IsExpirationTracked { get; set; }

        public decimal? UnitWeight { get; set; }
        public decimal? UnitVolume { get; set; }

        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }

        public string CategoryCode { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string ABCClass { get; set; } = string.Empty;

        public string Part_No { get; set; } = string.Empty;
        public string Alternative_Code { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public List<ItemImageDto> Images { get; set; } = new();
    }
}
