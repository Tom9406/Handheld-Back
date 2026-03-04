public class PostedShipment
{
    public Guid Id { get; set; }

    public Guid ShipmentId { get; set; }

    public string PostedShipmentNo { get; set; } = null!;

    public Guid CompanyId { get; set; }
    public string CompanyCode { get; set; } = null!;

    public string ShipmentNo { get; set; } = null!;
    public string ShipmentType { get; set; } = null!;
    public string ShipmentStatus { get; set; } = null!;

    public Guid WarehouseId { get; set; }
    public string WarehouseCode { get; set; } = null!;

    public Guid? CustomerId { get; set; }
    public string? CustomerCode { get; set; }
    public string? CustomerName { get; set; }

    public DateTime? OrderDate { get; set; }
    public DateTime? PlannedShipDate { get; set; }
    public DateTime? ActualShipDate { get; set; }

    public int TotalLines { get; set; }
    public decimal TotalQty { get; set; }
    public decimal? TotalWeight { get; set; }
    public decimal? TotalVolume { get; set; }

    public bool IsBackorderAllowed { get; set; }
    public bool? IsPartialAllowed { get; set; }

    public string PostedBy { get; set; } = null!;
    public DateTime PostedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<PostedShipmentLine> Lines { get; set; } = new List<PostedShipmentLine>();
}