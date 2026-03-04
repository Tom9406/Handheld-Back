namespace Wms.Api.DTOs
{
    public class PostShipmentResponseDto
    {
        public Guid PostedShipmentId { get; set; }

        public string PostedShipmentNo { get; set; } = null!;

        public string OriginalShipmentNo { get; set; } = null!;

        public DateTime PostedAt { get; set; }

        public int TotalLines { get; set; }

        public decimal TotalQty { get; set; }

        public bool IsShipmentFullyPosted { get; set; }
    }
}
