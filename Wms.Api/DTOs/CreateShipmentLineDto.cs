namespace Wms.Api.Dtos.ShipmentLine
{
    public class CreateShipmentLineDto
    {
        public int LineNo { get; set; }
        public string? ExternalLineNo { get; set; }

        public Guid ItemId { get; set; }
        public string ItemNo { get; set; } = null!;
        public string? ItemDescription { get; set; }

        public Guid WarehouseId { get; set; }

        public Guid? BinId { get; set; }
        public string? BinCode { get; set; }

        public decimal OrderedQty { get; set; }
        public decimal PickedQty { get; set; }
        public decimal ShippedQty { get; set; }

        public string UnitOfMeasure { get; set; } = null!;
        public decimal? BaseUomQty { get; set; }

        public string? LotNo { get; set; }
        public string? SerialNo { get; set; }
        public DateTime? ExpirationDate { get; set; }

        public decimal? UnitWeight { get; set; }
        public decimal? UnitVolume { get; set; }

        public string LineStatus { get; set; } = "OPEN";
        public string? SourceSystem { get; set; }
        public string? SourceLineId { get; set; }
    }
}