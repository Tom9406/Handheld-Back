using Wms.Api.Dtos.ReceivingLine;


namespace Wms.Api.Dtos.ReceivingHeader

{
    public class ReceivingHeaderDetailDto
    {
        public Guid Id { get; set; }

        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; } = null!;

        public string ReceiptNo { get; set; } = null!;
        public string? ExternalDocumentNo { get; set; }

        public string? VendorCode { get; set; }
        public string? VendorName { get; set; }

        public string Status { get; set; } = null!;
        public DateTime ReceiptDate { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? PostedAt { get; set; }

        public string CreatedBy { get; set; } = null!;
        public string? PostedBy { get; set; }

        public List<ReceivingLineDto> Lines { get; set; } = new();
    }
}
