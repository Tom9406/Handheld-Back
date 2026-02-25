using Handheld.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Wms.Api.Entities;

namespace Wms.Api.Data
{
    public class WmsDbContext : DbContext
    {
        public WmsDbContext(DbContextOptions<WmsDbContext> options)
            : base(options)
        {
        }

        // Tablas
        public DbSet<Item> Items => Set<Item>();
        public DbSet<Bin> Bins => Set<Bin>();
        public DbSet<PickHeaders> PickHeaders => Set<PickHeaders>();
        public DbSet<InventoryMovements> InventoryMovements => Set<InventoryMovements>();
        public DbSet<ShipmentHeaders> ShipmentHeaders => Set<ShipmentHeaders>();
        public DbSet<Company> Companies => Set<Company>();
        public DbSet<ReceivingHeader> ReceivingHeaders => Set<ReceivingHeader>();
        public DbSet<ReceivingLine> ReceivingLines => Set<ReceivingLine>();
        public DbSet<ShipmentLines> ShipmentLines => Set<ShipmentLines>();





        // Vistas
        public DbSet<CurrentStock> CurrentStock => Set<CurrentStock>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== Items =====
            modelBuilder.Entity<Item>(entity =>
            {
                entity.ToTable("Items", "core");

                entity.HasKey(e => e.Id);

                // ===== Campos básicos =====
                entity.Property(e => e.ItemNo)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.HasIndex(e => new { e.CompanyId, e.ItemNo })
                      .IsUnique(); // Un item no debe repetirse dentro de la misma compañía

                entity.Property(e => e.Description)
                      .HasMaxLength(200);

                entity.Property(e => e.UOM)
                      .HasMaxLength(20)
                      .IsRequired();

                entity.Property(e => e.IsActive)
                      .IsRequired();

                entity.Property(e => e.ItemType)
                      .HasMaxLength(30)
                      .IsRequired();

                entity.Property(e => e.Barcode)
                      .HasMaxLength(50);

                entity.Property(e => e.AltBarcode)
                      .HasMaxLength(50);

                entity.Property(e => e.BaseUOM)
                      .HasMaxLength(20)
                      .IsRequired();

                entity.Property(e => e.SalesUOM)
                      .HasMaxLength(20);

                entity.Property(e => e.PurchaseUOM)
                      .HasMaxLength(20);

                entity.Property(e => e.CategoryCode)
                      .HasMaxLength(50);

                entity.Property(e => e.Brand)
                      .HasMaxLength(100);

                entity.Property(e => e.ABCClass)
                      .HasMaxLength(10);

                entity.Property(e => e.CreatedBy)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(e => e.UpdatedBy)
                      .HasMaxLength(50);

                entity.Property(e => e.CreatedAt)
                      .IsRequired();

                entity.Property(e => e.UpdatedAt);

                // ===== Decimales (18,6) =====
                entity.Property(e => e.UnitWeight)
                      .HasPrecision(18, 6);

                entity.Property(e => e.UnitVolume)
                      .HasPrecision(18, 6);

                entity.Property(e => e.Length)
                      .HasPrecision(18, 6);

                entity.Property(e => e.Width)
                      .HasPrecision(18, 6);

                entity.Property(e => e.Height)
                      .HasPrecision(18, 6);

                entity.Property(e => e.ConversionFactor)
                      .HasPrecision(18, 6);

                // ===== Multi-company =====
                entity.Property(e => e.CompanyId)
                      .IsRequired();

                entity.HasIndex(e => e.CompanyId);

                /*entity.HasOne(e => e.Company)
                      .WithMany(c => c.Items)
                      .HasForeignKey(e => e.CompanyId)
                      .OnDelete(DeleteBehavior.Restrict);*/
            });


            // ===== Bins =====
            modelBuilder.Entity<Bin>(entity =>
            {
                entity.ToTable("Bins", "core");

                entity.HasKey(e => e.Id);

                // ===== Campos básicos =====
                entity.Property(e => e.BinCode)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(e => e.Description)
                      .HasMaxLength(100);

                entity.Property(e => e.IsActive)
                      .IsRequired();

                entity.Property(e => e.BinType)
                      .HasMaxLength(30)
                      .IsRequired();

                entity.Property(e => e.IsBlocked)
                      .IsRequired();

                entity.Property(e => e.AllowPicking)
                      .IsRequired();

                entity.Property(e => e.AllowPutaway)
                      .IsRequired();

                entity.Property(e => e.CreatedAt)
                      .IsRequired();

                entity.Property(e => e.UpdatedAt);

                // ===== Multi-company =====
                entity.Property(e => e.CompanyId)
                      .IsRequired();

                entity.HasIndex(e => e.CompanyId);

                /*// ===== Índices recomendados =====
                entity.HasIndex(e => new { e.CompanyId, e.WarehouseId, e.BinCode })
                      .IsUnique(); // Un bin no se repite dentro del mismo warehouse y compañía*/

                // ===== Relaciones =====
                entity.HasOne(e => e.Company)
                      .WithMany(c => c.Bins)
                      .HasForeignKey(e => e.CompanyId)
                      .OnDelete(DeleteBehavior.Restrict);

                /*entity.HasOne(e => e.Warehouse)
                      .WithMany(w => w.Bins)
                      .HasForeignKey(e => e.WarehouseId)
                      .OnDelete(DeleteBehavior.Restrict);*/
            });


            // ===== View: Current Stock =====
            modelBuilder.Entity<CurrentStock>(entity =>
            {
                entity.ToView("v_CurrentStock", "core");
                entity.HasNoKey();

                entity.Property(e => e.StockQty)
                      .HasColumnType("decimal(38,2)");
            });

            // ===== Picks =====
            modelBuilder.Entity<PickHeaders>(entity => 
            {
                entity.ToTable("PickHeaders", "core");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.PickNo)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(e => e.Status)
                      .HasMaxLength(30);


                entity.Property(e => e.SalesOrderNo)
                      .HasMaxLength(20);


                entity.Property(e => e.WarehouseShipmentNo)
                      .HasMaxLength(20);


                entity.Property(e => e.AssignedUserName)
                      .HasMaxLength(20)
                      .IsRequired(false);


                entity.Property(e => e.CompletedAt)
                      .IsRequired(false);

                entity.Property(e => e.CreatedAt)
                      .IsRequired();
            });

            // ===== InventoryMovements =====
            modelBuilder.Entity<InventoryMovements>(entity =>
            {
                entity.ToTable("InventoryMovements", "core");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.ItemId).IsRequired();
                entity.Property(e => e.BinId).IsRequired();

                entity.Property(e => e.Quantity)
                      .HasColumnType("decimal(18,2)")
                      .IsRequired();

                entity.Property(e => e.MovementType)
                      .HasMaxLength(30)
                      .IsRequired();

                entity.Property(e => e.ReferenceNo)
                      .HasMaxLength(50);

                entity.Property(e => e.CreatedAt).IsRequired();

                entity.HasOne(e => e.Item)
                      .WithMany()
                      .HasForeignKey(e => e.ItemId);

                entity.HasOne(e => e.Bin)
                      .WithMany()
                      .HasForeignKey(e => e.BinId);
            });

            // ===== Shipment Headers =====
            modelBuilder.Entity<ShipmentHeaders>(entity =>
            {
                entity.ToTable("Shipments", "core");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.CompanyCode)
                      .HasMaxLength(20)
                      .IsRequired();

                entity.Property(e => e.ShipmentNo)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(e => e.ExternalShipmentNo)
                      .HasMaxLength(50);

                entity.Property(e => e.ReferenceNo)
                      .HasMaxLength(100);

                entity.Property(e => e.ShipmentType)
                      .HasMaxLength(30)
                      .IsRequired();

                entity.Property(e => e.ShipmentStatus)
                      .HasMaxLength(30)
                      .IsRequired();

                entity.Property(e => e.WarehouseCode)
                      .HasMaxLength(30)
                      .IsRequired();

                entity.Property(e => e.CustomerCode)
                      .HasMaxLength(50);

                entity.Property(e => e.CustomerName)
                      .HasMaxLength(200);

                entity.Property(e => e.ShipToName)
                      .HasMaxLength(200);

                entity.Property(e => e.ShipToAddress1)
                      .HasMaxLength(200);

                entity.Property(e => e.ShipToAddress2)
                      .HasMaxLength(200);

                entity.Property(e => e.ShipToCity)
                      .HasMaxLength(100);

                entity.Property(e => e.ShipToState)
                      .HasMaxLength(100);

                entity.Property(e => e.ShipToPostalCode)
                      .HasMaxLength(20);

                entity.Property(e => e.ShipToCountry)
                      .HasMaxLength(50);

                entity.Property(e => e.CarrierCode)
                      .HasMaxLength(50);

                entity.Property(e => e.CarrierName)
                      .HasMaxLength(200);

                entity.Property(e => e.ServiceLevel)
                      .HasMaxLength(50);

                entity.Property(e => e.TrackingNumber)
                      .HasMaxLength(100);

                entity.Property(e => e.TotalQty)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.TotalWeight)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.TotalVolume)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.SourceSystem)
                      .HasMaxLength(50);

                entity.Property(e => e.SourceEndpoint)
                      .HasMaxLength(200);

                entity.Property(e => e.CreatedBy)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(e => e.CreatedAt)
                      .IsRequired();

                entity.Property(e => e.UpdatedBy)
                      .HasMaxLength(50);
            });

            //------------Company---------
            modelBuilder.Entity<Company>(entity =>
            {
                entity.ToTable("Companies", "dbo");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Code)
                      .HasMaxLength(50)                
                      .IsRequired();

                entity.HasIndex(e => e.Code)
                       .IsUnique();

                entity.Property(e => e.Name)
                      .HasMaxLength(200)
                      .IsRequired();

                entity.Property(e => e.IsActive)
                      .IsRequired();

                entity.Property(e => e.CreatedAt)
                      .IsRequired();

                entity.Property(e => e.UpdatedAt)
                      .IsRequired(false); // ✅ Nullable

                entity.Property(e => e.CompanyType)
                      .HasMaxLength(30)
                      .IsRequired(false); // ✅ Nullable

                entity.Property(e => e.DefaultWarehouseId)
                      .IsRequired(false); // ✅ Nullable

                entity.Property(e => e.TimeZone)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(e => e.CurrencyCode)
                      .HasMaxLength(10)
                      .IsRequired();

                entity.Property(e => e.LegalName)
                      .HasMaxLength(200);

                entity.Property(e => e.TaxId)
                      .HasMaxLength(50);

                entity.Property(e => e.Address1)
                      .HasMaxLength(200);

                entity.Property(e => e.City)
                      .HasMaxLength(100);

                entity.Property(e => e.Country)
                      .HasMaxLength(100);

                entity.Property(e => e.ExternalSystem)
                      .HasMaxLength(50);

                entity.Property(e => e.ExternalCompanyId)
                      .HasMaxLength(100);

                entity.Property(e => e.ApiKey)
                      .HasMaxLength(100);

                entity.Property(e => e.CallbackUrl)
                      .HasMaxLength(200);

                entity.Property(e => e.IsWmsEnabled)
                      .IsRequired();

                entity.Property(e => e.AllowCrossWarehouse)
                      .IsRequired();

                entity.Property(e => e.AllowNegativeStock)
                      .IsRequired();

                // ===== Relaciones =====
                entity.HasMany(e => e.Items)
                      .WithOne(i => i.Company)
                      .HasForeignKey(i => i.CompanyId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Bins)
                      .WithOne(b => b.Company)
                      .HasForeignKey(b => b.CompanyId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            //---------ReceivingHeader------------
            modelBuilder.Entity<ReceivingHeader>(entity =>
            {
                entity.ToTable("ReceivingHeaders", "core");

                entity.HasKey(e => e.Id);

                // ===== Multi-company =====
                entity.Property(e => e.CompanyId)
                      .IsRequired();

                entity.HasIndex(e => e.CompanyId);

                entity.HasOne(e => e.Company)
                      .WithMany()
                      .HasForeignKey(e => e.CompanyId)
                      .OnDelete(DeleteBehavior.Restrict);

                // ===== Documento =====
                entity.Property(e => e.ReceiptNo)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.HasIndex(e => new { e.CompanyId, e.ReceiptNo })
                      .IsUnique();

                entity.Property(e => e.ExternalDocumentNo)
                      .HasMaxLength(50);

                entity.Property(e => e.VendorCode)
                      .HasMaxLength(50);

                entity.Property(e => e.VendorName)
                      .HasMaxLength(200);

                entity.Property(e => e.Status)
                      .HasMaxLength(30)
                      .IsRequired();

                entity.Property(e => e.ReceiptDate)
                      .IsRequired();

                // ===== Auditoría =====
                entity.Property(e => e.CreatedAt)
                      .IsRequired();

                entity.Property(e => e.PostedAt);

                entity.Property(e => e.CreatedBy)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(e => e.PostedBy)
                      .HasMaxLength(50);

                // ===== Relación con líneas =====
                entity.HasMany(e => e.Lines)
                      .WithOne(l => l.ReceivingHeader)
                      .HasForeignKey(l => l.ReceivingHeaderId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            //----------ReceivingLine-----------
            modelBuilder.Entity<ReceivingLine>(entity =>
            {
                entity.ToTable("ReceivingLines", "core");

                // ===== PK =====
                entity.HasKey(e => e.Id);

                // ===== Foreign Keys =====
                entity.Property(e => e.ReceivingHeaderId)
                      .IsRequired();

                entity.Property(e => e.CompanyId)
                      .IsRequired();

                entity.Property(e => e.ItemId)
                      .IsRequired();

                entity.Property(e => e.BinId)
                      .IsRequired(false);

                entity.HasIndex(e => e.ReceivingHeaderId);
                entity.HasIndex(e => e.CompanyId);
                entity.HasIndex(e => e.ItemId);
                entity.HasIndex(e => e.BinId);

                // ===== Relaciones =====
                entity.HasOne(e => e.ReceivingHeader)
                      .WithMany(h => h.Lines)
                      .HasForeignKey(e => e.ReceivingHeaderId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Company)
                      .WithMany()
                      .HasForeignKey(e => e.CompanyId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Item)
                      .WithMany()
                      .HasForeignKey(e => e.ItemId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Bin)
                      .WithMany()
                      .HasForeignKey(e => e.BinId)
                      .OnDelete(DeleteBehavior.Restrict);

                // ===== Cantidades =====
                entity.Property(e => e.QuantityExpected)
                      .HasColumnType("decimal(18,2)")
                      .IsRequired();

                entity.Property(e => e.QuantityReceived)
                      .HasColumnType("decimal(18,2)")
                      .IsRequired();

                // ===== UOM =====
                entity.Property(e => e.UOM)
                      .HasMaxLength(20)
                      .IsRequired();

                // ===== Auditoría =====
                entity.Property(e => e.CreatedAt)
                      .HasColumnType("datetime2(7)")
                      .IsRequired();
            });

            // ===== Shipment Lines =====
            modelBuilder.Entity<ShipmentLines>(entity =>
            {
                entity.ToTable("ShipmentLines", "core");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.LineNo).IsRequired();

                entity.Property(e => e.LineStatus)
                      .HasMaxLength(30);

                entity.Property(e => e.OrderedQty)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.PickedQty)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.ShippedQty)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.UnitWeight)
                      .HasPrecision(18, 6);

                entity.Property(e => e.UnitVolume)
                      .HasPrecision(18, 6);

                entity.Property(e => e.BaseUomQty)
                      .HasPrecision(18, 6);

                entity.Property(e => e.CompanyId).IsRequired();
                entity.HasIndex(e => e.CompanyId);

                entity.Property(e => e.CreatedAt).IsRequired();
            });



        }
    }
}
