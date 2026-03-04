namespace Wms.Api.DTOs
{
    public class PostReceivingResponseDto
    {
        public Guid PostedReceivingId { get; set; }

        public string PostedReceivingNo { get; set; } = null!;

        public string OriginalReceiptNo { get; set; } = null!;

        public DateTime PostedAt { get; set; }

        public int TotalLines { get; set; }

        public decimal TotalQty { get; set; }

        public bool IsReceivingFullyPosted { get; set; }
    }
}