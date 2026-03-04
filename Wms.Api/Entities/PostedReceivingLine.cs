public class PostedReceivingLine
{
    public Guid Id { get; set; }

    // FK hacia header posted
    public Guid PostedReceivingHeaderId { get; set; }
    public PostedReceivingHeader PostedReceivingHeader { get; set; } = null!;

    // Referencia a línea original
    public Guid ReceivingLineId { get; set; }

    // Company
    public Guid CompanyId { get; set; }

    // Item
    public Guid ItemId { get; set; }

    // Bin
    public Guid BinId { get; set; }

    // Cantidades finales al momento del post
    public decimal QuantityExpected { get; set; }
    public decimal QuantityReceived { get; set; }

    // Unidad de medida
    public string UOM { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string PostedBy { get; set; } = null!;

    // Auditoría
    public DateTime PostedAt { get; set; }
}