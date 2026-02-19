using System;

namespace Wms.Api.Dtos.PickHeader
{
    public class PickHeaderDetailDto
    {
        public Guid Id { get; set; }

        public string PickNo { get; set; } = null!;

        public string? Status { get; set; }

        public string? AssignedUserName { get; set; }

        public string? SalesOrderNo { get; set; }

        public string? WarehouseShipmentNo { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? CompletedAt { get; set; }
    }
}
