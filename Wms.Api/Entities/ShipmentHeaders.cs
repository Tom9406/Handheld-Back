using System;

namespace Wms.Api.Entities
{
    public class ShipmentHeaders
    {
        public Guid Id { get; set; }

        public Guid CompanyId { get; set; }
        public string CompanyCode { get; set; } = null!;

        public string ShipmentNo { get; set; } = null!;
        public string? ExternalShipmentNo { get; set; }
        public string? ReferenceNo { get; set; }

        public string ShipmentType { get; set; } = null!;
        public string ShipmentStatus { get; set; } = null!;

        public Guid WarehouseId { get; set; }
        public string WarehouseCode { get; set; } = null!;

        public Guid? CustomerId { get; set; }
        public string? CustomerCode { get; set; }
        public string? CustomerName { get; set; }

        public string? ShipToName { get; set; }
        public string? ShipToAddress1 { get; set; }
        public string? ShipToAddress2 { get; set; }
        public string? ShipToCity { get; set; }
        public string? ShipToState { get; set; }
        public string? ShipToPostalCode { get; set; }
        public string? ShipToCountry { get; set; }

        public DateTime? OrderDate { get; set; }
        public DateTime? PlannedShipDate { get; set; }
        public DateTime? ActualShipDate { get; set; }
        public DateTime? DeliveryDate { get; set; }

        public string? CarrierCode { get; set; }
        public string? CarrierName { get; set; }
        public string? ServiceLevel { get; set; }
        public string? TrackingNumber { get; set; }

        public int TotalLines { get; set; }
        public decimal TotalQty { get; set; }
        public decimal? TotalWeight { get; set; }
        public decimal? TotalVolume { get; set; }

        public string? SourceSystem { get; set; }
        public string? SourceEndpoint { get; set; }

        public Guid? IntegrationBatchId { get; set; }

        public bool IsBackorderAllowed { get; set; }
        public bool IsPartialAllowed { get; set; }
        public bool IsClosed { get; set; }

        public string CreatedBy { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<ShipmentLines> Lines { get; set; } = new List<ShipmentLines>();
    }
}
