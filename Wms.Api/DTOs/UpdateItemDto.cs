namespace Wms.Api.DTOs
{
    public class UpdateItemDto
    {
        public string Description { get; set; }
        public string UOM { get; set; }

        public string BaseUOM { get; set; }
        public string SalesUOM { get; set; }
        public string PurchaseUOM { get; set; }

        public string ItemType { get; set; }

        public string Barcode { get; set; }
        public string AltBarcode { get; set; }

        public bool IsLotTracked { get; set; }
        public bool IsSerialTracked { get; set; }
        public bool IsExpirationTracked { get; set; }

        public decimal? UnitWeight { get; set; }
        public decimal? UnitVolume { get; set; }

        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }

        public string CategoryCode { get; set; }
        public string Brand { get; set; }
        public string ABCClass { get; set; }

        public string Part_No { get; set; }
        public string Alternative_Code { get; set; }

        public bool IsActive { get; set; }
    }
}
