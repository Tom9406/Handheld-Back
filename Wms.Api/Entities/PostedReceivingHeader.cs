namespace Wms.Api.Entities
{
    public class PostedReceivingHeader
    {
        public Guid Id { get; set; }

        public string PostedReceivingNo { get; set; } = null!;
        public ReceivingHeader ReceivingHeader { get; set; } = null!;

        public Guid ReceivingHeaderId { get; set; }

        public Guid CompanyId { get; set; }

        public string CompanyCode { get; set; } = null!;

        public string ReceiptNo { get; set; } = null!;

        public string? ExternalDocumentNo { get; set; }

        public Guid? VendorId { get; set; }

        public string? VendorCode { get; set; }

        public string? VendorName { get; set; }

        public DateTime ReceiptDate { get; set; }

        public int? TotalLines { get; set; }

        public decimal TotalQty { get; set; }

        public decimal? TotalWeight { get; set; }

        public decimal? TotalVolume { get; set; }

        public string? SourceSystem { get; set; }

        public string? SourceEndpoint { get; set; }

        public Guid? IntegrationBatchId { get; set; }

        public string PostedBy { get; set; } = null!;

        public DateTime PostedAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? Status { get; set; }

        // Navegaciones
        public Company Company { get; set; } = null!;

        
    }
}