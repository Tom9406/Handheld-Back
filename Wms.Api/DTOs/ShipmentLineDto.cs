using System;

namespace Wms.Api.Dtos.Shipments
{
    public class ShipmentLineDto
    {
        public Guid Id { get; set; }

        public Guid ShipmentId { get; set; }

        public int LineNo { get; set; }

        public Guid ItemId { get; set; }
        public string ItemNo { get; set; } = null!;
        public string? ItemDescription { get; set; }

        public Guid WarehouseId { get; set; }

        public Guid? BinId { get; set; }
        public string? BinCode { get; set; }

        public decimal OrderedQty { get; set; }
        public decimal PickedQty { get; set; }
        public decimal ShippedQty { get; set; }

        public string UnitOfMeasure { get; set; } = null!;
        public decimal? BaseUomQty { get; set; }

        public string? LotNo { get; set; }
        public string? SerialNo { get; set; }
        public DateTime? ExpirationDate { get; set; }

        public decimal? UnitWeight { get; set; }
        public decimal? UnitVolume { get; set; }

        public string LineStatus { get; set; } = null!;

        public Guid CompanyId { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public decimal RemainingQty => OrderedQty - PickedQty - ShippedQty;
        public bool IsCompleted => PickedQty >= OrderedQty;

    }
}
