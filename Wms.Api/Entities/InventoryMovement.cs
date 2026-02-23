using Wms.Api.Entities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Handheld.Api.Entities;

public class InventoryMovements
{
    public Guid Id { get; set; }

    public Guid ItemId { get; set; }

    public Guid BinId { get; set; }

    public decimal Quantity { get; set; }

    public string MovementType { get; set; } = null!;
    // Ej: IN, OUT, TRANSFER, ADJUSTMENT

    public string ReferenceNo { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public Guid CompanyId { get; set; }

    public Guid? WarehouseId { get; set; }

    public string? EntityType { get; set; }
    // Ej: Shipment, PurchaseOrder, TransferOrder

    public Guid? EntityId { get; set; }

    public string? OldStatus { get; set; }

    public string? NewStatus { get; set; }

    public string? SourceSystem { get; set; }
    // Ej: BC, Shopify, Manual

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // 🔹 Navigation Properties (opcional pero recomendado)
    public Item Item { get; set; } = null!;
    public Bin Bin { get; set; } = null!;
    public Company Company { get; set; } = null!;
   // public Warehouse? Warehouse { get; set; }
}
