namespace Wms.Api.Dtos
{
    public class PostedReceivingLineDto
    {
        public Guid Id { get; set; }

        public Guid PostedReceivingHeaderId { get; set; }

        public Guid ReceivingLineId { get; set; }

        public Guid CompanyId { get; set; }

        public Guid ItemId { get; set; }

        public Guid BinId { get; set; }

        public decimal QuantityExpected { get; set; }

        public decimal QuantityReceived { get; set; }

        public string UOM { get; set; } = null!;

        public DateTime PostedAt { get; set; }

        public string? Status { get; set; }

        public string PostedBy { get; set; } = null!;
    }
}