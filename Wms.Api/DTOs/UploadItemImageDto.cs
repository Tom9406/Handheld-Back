namespace Wms.Api.DTOs
{
    public class UploadItemImageDto
    {
        public Guid ItemId { get; set; }

        public IFormFile? File { get; set; }
    }
}
