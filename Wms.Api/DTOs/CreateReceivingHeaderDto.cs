using Wms.Api.Dtos.ReceivingLine;

namespace Wms.Api.Dtos.ReceivingHeader
{
    public class CreateReceivingHeaderDto
    {
        public Guid CompanyId { get; set; }

        public string? ExternalDocumentNo { get; set; }
        public string? VendorCode { get; set; }
        public string? VendorName { get; set; }

        public DateTime? ReceiptDate { get; set; }

        public List<CreateReceivingLineDto> Lines { get; set; } = new();
    }
}