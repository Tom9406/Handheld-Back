namespace Wms.Api.Entities
{
    public class DocumentSequence
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public string DocumentType { get; set; }
        public int LastNumber { get; set; }
    }
}
