using System;

namespace Wms.Api.Dtos.ShipmentHeader
{
    public class ShipmentHeaderDto
    {
        public Guid Id { get; set; }

        public Guid CompanyId { get; set; }
        public string CompanyCode { get; set; } = null!;

        public string ShipmentNo { get; set; } = null!;
        public string? ExternalShipmentNo { get; set; }
        public string ShipmentType { get; set; } = null!;
        public string ShipmentStatus { get; set; } = null!;

        public string WarehouseCode { get; set; } = null!;

        public string? CustomerCode { get; set; }
        public string? CustomerName { get; set; }

        public DateTime? PlannedShipDate { get; set; }
        public DateTime? ActualShipDate { get; set; }

        public int TotalLines { get; set; }
        public decimal TotalQty { get; set; }

        public bool IsClosed { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
