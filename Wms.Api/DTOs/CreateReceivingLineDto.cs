namespace Wms.Api.Dtos.ReceivingLine
{
    public class CreateReceivingLineDto
    {
        public Guid ItemId { get; set; }
        public Guid? BinId { get; set; }

        public decimal QuantityExpected { get; set; }
        public decimal QuantityReceived { get; set; }

        public string UOM { get; set; } = null!;
    }
}