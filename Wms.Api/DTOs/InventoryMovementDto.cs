using System;

namespace Wms.Api.Dtos.InventoryMovement
{
    public class InventoryMovementDto
    {
        public Guid Id { get; set; }

        public Guid ItemId { get; set; }
        public string ItemNo { get; set; } = null!;
        public string ItemDescription { get; set; } = null!;

        public Guid BinId { get; set; }
        public string BinCode { get; set; } = null!;

        public decimal Quantity { get; set; }

        public string MovementType { get; set; } = null!;

        public string? ReferenceNo { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
