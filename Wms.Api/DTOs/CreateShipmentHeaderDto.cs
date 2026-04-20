using Wms.Api.Dtos.ShipmentLine;

namespace Wms.Api.Dtos.ShipmentHeader
{
    public class CreateShipmentHeaderDto
    {
        public Guid CompanyId { get; set; }
       

        public string ShipmentType { get; set; } = null!;
        public string? ExternalShipmentNo { get; set; }
        public string? ReferenceNo { get; set; }

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

        public string? CarrierCode { get; set; }
        public string? CarrierName { get; set; }
        public string? ServiceLevel { get; set; }

        public bool IsBackorderAllowed { get; set; }
        public bool IsPartialAllowed { get; set; }

        public List<CreateShipmentLineDto> Lines { get; set; } = new();
    }
}