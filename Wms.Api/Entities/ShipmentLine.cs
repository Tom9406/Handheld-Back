using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wms.Api.Entities;

public class ShipmentLines
{
    [Key]
    public Guid Id { get; set; }

    // ======================================================
    // Multi-company
    // ======================================================
    public Guid CompanyId { get; set; }

    // ======================================================
    // Header relation
    // ======================================================
    public Guid ShipmentId { get; set; }
    public ShipmentHeaders Shipment { get; set; } = null!;

    public int LineNo { get; set; }

    // ======================================================
    // Item info (denormalized snapshot)
    // ======================================================
    public Guid ItemId { get; set; }

    [MaxLength(50)]
    public string ItemNo { get; set; } = null!;

    [MaxLength(250)]
    public string? ItemDescription { get; set; }

    // ======================================================
    // Location
    // ======================================================
    public Guid WarehouseId { get; set; }

    public Guid? BinId { get; set; }

    [MaxLength(50)]
    public string? BinCode { get; set; }

    // ======================================================
    // Quantities
    // ======================================================
    [Column(TypeName = "decimal(18,6)")]
    public decimal OrderedQty { get; set; }

    [Column(TypeName = "decimal(18,6)")]
    public decimal PickedQty { get; set; }

    [Column(TypeName = "decimal(18,6)")]
    public decimal ShippedQty { get; set; }

    [MaxLength(20)]
    public string UnitOfMeasure { get; set; } = null!;

    [Column(TypeName = "decimal(18,6)")]
    public decimal? BaseUomQty { get; set; }

    // ======================================================
    // Traceability
    // ======================================================
    [MaxLength(100)]
    public string? LotNo { get; set; }

    [MaxLength(100)]
    public string? SerialNo { get; set; }

    public DateTime? ExpirationDate { get; set; }

    // ======================================================
    // Logistics metrics
    // ======================================================
    [Column(TypeName = "decimal(18,6)")]
    public decimal? UnitWeight { get; set; }

    [Column(TypeName = "decimal(18,6)")]
    public decimal? UnitVolume { get; set; }

    // ======================================================
    // Status
    // ======================================================
    [MaxLength(30)]
    public string LineStatus { get; set; } = null!;

    // ======================================================
    // Auditing
    // ======================================================
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
