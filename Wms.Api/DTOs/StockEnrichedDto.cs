namespace Wms.Api.DTOs
{
    public class StockEnrichedDto
    {
        // ===== Company =====
        public Guid CompanyId { get; set; }

        // ===== Item =====
        public Guid ItemId { get; set; }
        public string ItemNo { get; set; } = string.Empty;
        public string? ItemDescription { get; set; }
        public string ItemUOM { get; set; } = string.Empty;
        public string ItemType { get; set; } = string.Empty;

        public bool ItemIsActive { get; set; }

        public bool IsLotTracked { get; set; }
        public bool IsSerialTracked { get; set; }
        public bool IsExpirationTracked { get; set; }

        public decimal? UnitWeight { get; set; }
        public decimal? UnitVolume { get; set; }

        // ===== Bin =====
        public Guid BinId { get; set; }
        public string BinCode { get; set; } = string.Empty;
        public string? BinDescription { get; set; }
        public string BinType { get; set; } = string.Empty;

        public bool BinIsActive { get; set; }
        public bool IsBlocked { get; set; }
        public bool AllowPicking { get; set; }
        public bool AllowPutaway { get; set; }

        // ===== Stock =====
        public decimal StockQty { get; set; }
    }
}
