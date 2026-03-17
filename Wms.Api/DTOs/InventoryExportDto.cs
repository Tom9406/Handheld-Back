namespace Handheld.Api.Dtos;

public class InventoryExportDto
{
    public Guid Id { get; set; }

    public Guid? ItemId { get; set; }

    public Guid? BinId { get; set; }

    public decimal Quantity { get; set; }

    public string MovementType { get; set; } = null!;

    public string ReferenceNo { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public Guid CompanyId { get; set; }

    public Guid? WarehouseId { get; set; }

    public string? EntityType { get; set; }

    public Guid? EntityId { get; set; }

    public string? OldStatus { get; set; }

    public string? NewStatus { get; set; }

    public string? SourceSystem { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }
}