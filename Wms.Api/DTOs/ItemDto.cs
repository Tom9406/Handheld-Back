namespace Wms.Api.Dtos.Item
{
    public class ItemDto
    {
        public Guid Id { get; set; }

        public string ItemNo { get; set; } = null!;
        public string? Description { get; set; }

        public string UOM { get; set; }

        public bool IsActive { get; set; }

        public string ItemType { get; set; } = null!;

        public string? Barcode { get; set; }

        public string BaseUOM { get; set; } = null!;

        public string? CategoryCode { get; set; }
        public string? Brand { get; set; }

        public string? ABCClass { get; set; }

        public Guid CompanyId { get; set; }
    }
}
