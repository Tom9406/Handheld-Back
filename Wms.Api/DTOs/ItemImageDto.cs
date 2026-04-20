namespace Wms.Api.DTOs
{
    public class ItemImageDto
    {
        public Guid Id { get; set; }

        public string Url { get; set; } = string.Empty;

        public bool IsPrimary { get; set; }
    }
}
