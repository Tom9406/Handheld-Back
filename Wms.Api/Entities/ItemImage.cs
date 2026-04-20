namespace Wms.Api.Entities
{
    public class ItemImage
    {
        public Guid Id { get; set; }

        public Guid ItemId { get; set; }

        public Item Item { get; set; } = null!;

        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public bool IsPrimary { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
