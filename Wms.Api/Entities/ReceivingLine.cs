using System;

namespace Wms.Api.Entities
{
    public class ReceivingLine
    {
        public Guid Id { get; set; }

        // ===== Foreign Keys =====
        public Guid ReceivingHeaderId { get; set; }
        public Guid CompanyId { get; set; }
        public Guid ItemId { get; set; }
        public Guid? BinId { get; set; }

        // ===== Cantidades =====
        public decimal QuantityExpected { get; set; }
        public decimal QuantityReceived { get; set; }

        // ===== Datos operativos =====
        public string UOM { get; set; } = null!;

        // ===== Auditoría =====
        public DateTime CreatedAt { get; set; }

        // ===== Navegaciones =====
        public ReceivingHeader ReceivingHeader { get; set; } = null!;
        public Company Company { get; set; } = null!;
        public Item Item { get; set; } = null!;
        public Bin? Bin { get; set; }
    }
}
