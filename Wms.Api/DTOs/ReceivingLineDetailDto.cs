namespace Wms.Api.Dtos.ReceivingLine
{
    public class ReceivingLineDetailDto
    {
        public Guid Id { get; set; }

        public Guid ReceivingHeaderId { get; set; }
        public string ReceiptNo { get; set; } = null!;

        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; } = null!;

        public Guid ItemId { get; set; }
        public string ItemCode { get; set; } = null!;
        public string ItemDescription { get; set; } = null!;

        public Guid? BinId { get; set; }
        public string? BinCode { get; set; }

        public decimal QuantityExpected { get; set; }
        public decimal QuantityReceived { get; set; }

        public string UOM { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
    }
}
