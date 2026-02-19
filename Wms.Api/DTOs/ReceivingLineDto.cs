namespace Wms.Api.Dtos.ReceivingLine
{
    public class ReceivingLineDto
    {
        public Guid Id { get; set; }

        public Guid ReceivingHeaderId { get; set; }
        public Guid CompanyId { get; set; }

        public Guid ItemId { get; set; }
        public string ItemCode { get; set; } = null!;

        public Guid? BinId { get; set; }
        public string? BinCode { get; set; }

        public decimal QuantityExpected { get; set; }
        public decimal QuantityReceived { get; set; }

        public string UOM { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
    }
}
