namespace Wms.Api.Entities
{
    public class CurrentStock
    {
        public Guid ItemId { get; set; }
        public Guid BinId { get; set; }
        public decimal StockQty { get; set; }
    }

}
