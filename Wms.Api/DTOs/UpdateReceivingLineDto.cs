using System.ComponentModel.DataAnnotations;

namespace Wms.Api.Dtos.ReceivingLine
{
    public class UpdateReceivingLineDto
    {
        [Range(0, double.MaxValue)]
        public decimal QuantityReceived { get; set; }
    }
}