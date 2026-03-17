namespace Wms.Api.Entities
{
    public class ItemImage
    {
        public Guid Id { get; set; }

        public Guid ItemId { get; set; }

        public Item Item { get; set; }

        public string FileName { get; set; }
        public string FilePath { get; set; }

        public string Url { get; set; }

        public bool IsPrimary { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
