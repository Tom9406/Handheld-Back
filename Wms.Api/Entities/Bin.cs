using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Wms.Api.Entities
{
    public class Bin
    {
        public Guid Id { get; set; }

        public string BinCode { get; set; } = null!;
        public string? Description { get; set; }

        public bool IsActive { get; set; }

        // Multi-company
        public Guid CompanyId { get; set; }

        // Warehouse
       // public Guid WarehouseId { get; set; }

        // Operación
        public string BinType { get; set; } = null!;
        public bool IsBlocked { get; set; }
        public bool AllowPicking { get; set; }
        public bool AllowPutaway { get; set; }

        // Auditoría
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // ===== Navegación =====
        public Company Company { get; set; }
        /*public Warehouse? Warehouse { get; set; }*/
    }
}
