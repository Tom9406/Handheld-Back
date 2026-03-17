namespace Wms.Api.DTOs
{
    public class Document_sequencesDto
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public string Document_type { get; set; } = null!;
        public int LastNumber { get; set; } = 0;
    }
}
