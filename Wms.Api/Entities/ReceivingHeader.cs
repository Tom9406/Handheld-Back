using System.ComponentModel.DataAnnotations;

namespace Wms.Api.Entities
{
    public class ReceivingHeader
    {
        public Guid Id { get; set; }

        // ===== Multi-company =====
        public Guid CompanyId { get; set; }
        public Company Company { get; set; } = null!;

        // ===== Documento =====
        [MaxLength(50)]
        public string ReceiptNo { get; set; } = null!;

        [MaxLength(50)]
        public string? ExternalDocumentNo { get; set; }

        [MaxLength(50)]
        public string? VendorCode { get; set; }

        [MaxLength(200)]
        public string? VendorName { get; set; }

        [MaxLength(30)]
        public string Status { get; set; } = null!;
        // OPEN | POSTED | CANCELLED

        public DateTime ReceiptDate { get; set; }

        // ===== Auditoría =====
        public DateTime CreatedAt { get; set; }
        public DateTime? PostedAt { get; set; }

        [MaxLength(50)]
        public string CreatedBy { get; set; } = null!;

        [MaxLength(50)]
        public string? PostedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string? UpdatedBy { get; set; }

        // ===== Relaciones =====
        public ICollection<ReceivingLine> Lines { get; set; } = new List<ReceivingLine>();
    }
}
