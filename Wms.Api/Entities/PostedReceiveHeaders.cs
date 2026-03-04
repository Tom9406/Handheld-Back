using System.ComponentModel.DataAnnotations;

public class PostedReceivingHeader
{
    public Guid Id { get; set; }

    // Referencia al documento original
    public Guid ReceivingHeaderId { get; set; }

    // Número consecutivo generado al postear
    public string PostedReceivingNo { get; set; } = null!;

    // Company
    public Guid CompanyId { get; set; }
    public string CompanyCode { get; set; } = null!;

    // Documento
    public string ReceiptNo { get; set; } = null!;
    public string? ExternalDocumentNo { get; set; }

    // Vendor (snapshot)
    public Guid? VendorId { get; set; }
    public string? VendorCode { get; set; }
    public string? VendorName { get; set; }

    // Fecha del documento
    public DateTime ReceiptDate { get; set; }

    // Totales
    public int TotalLines { get; set; }
    public decimal TotalQty { get; set; }
    public decimal? TotalWeight { get; set; }
    public decimal? TotalVolume { get; set; }

    // Integración
    public string? SourceSystem { get; set; }
    public string? SourceEndpoint { get; set; }
    public Guid? IntegrationBatchId { get; set; }

    // Auditoría
    public string PostedBy { get; set; } = null!;
    public DateTime PostedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    [MaxLength(30)]
    public string Status { get; set; } = null!;
    // OPEN | POSTED | CANCELLED

    // Navegación
    public ICollection<PostedReceivingLine> Lines { get; set; }
        = new List<PostedReceivingLine>();
}