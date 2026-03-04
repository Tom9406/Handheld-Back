public class PostedShipmentLine
{
    public Guid Id { get; set; }

    public Guid PostedShipmentId { get; set; }
    public PostedShipment PostedShipment { get; set; } = null!;

    public Guid ShipmentLineId { get; set; }

    public Guid CompanyId { get; set; }

    public int LineNo { get; set; }

    public Guid? ItemId { get; set; }
    public string? ItemNo { get; set; }
    public string? ItemDescription { get; set; }

    public Guid WarehouseId { get; set; }
    public Guid? BinId { get; set; }
    public string BinCode { get; set; }

    public decimal OrderedQty { get; set; }
    public decimal PickedQty { get; set; }
    public decimal ShippedQty { get; set; }

    public string UnitOfMeasure { get; set; } = null!;
    public decimal? BaseUomQty { get; set; }

    public string? LotNo { get; set; }
    public string? SerialNo { get; set; }

    public decimal? UnitWeight { get; set; }
    public decimal? UnitVolume { get; set; }

    public string LineStatus { get; set; } = null!;

    public string PostedBy { get; set; } = null!;
    public DateTime PostedAt { get; set; }
}