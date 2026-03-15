using Microsoft.EntityFrameworkCore;
using hodotaev_library.Models;

namespace hodotaev_library.Data;

public class HodotaevPraktikaContext : DbContext
{
    public HodotaevPraktikaContext()
    {
    }

    public HodotaevPraktikaContext(DbContextOptions<HodotaevPraktikaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<HodotaevPartner> HodotaevPartners { get; set; }
    public virtual DbSet<HodotaevPartnerType> HodotaevPartnerTypes { get; set; }
    public virtual DbSet<HodotaevProduct> HodotaevProducts { get; set; }
    public virtual DbSet<HodotaevSale> HodotaevSales { get; set; }
    public virtual DbSet<HodotaevPartnerRatingHistory> HodotaevPartnerRatingHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("app");

        modelBuilder.Entity<HodotaevPartnerType>(entity =>
        {
            entity.ToTable("hodotaev_partner_types");
            entity.HasKey(e => e.PartnerTypeId);
            entity.Property(e => e.PartnerTypeId).HasColumnName("partner_type_id");
            entity.Property(e => e.TypeName).HasColumnName("type_name").IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);
            entity.HasIndex(e => e.TypeName).IsUnique();
        });

        modelBuilder.Entity<HodotaevPartner>(entity =>
        {
            entity.ToTable("hodotaev_partners");
            entity.HasKey(e => e.PartnerId);
            entity.Property(e => e.PartnerId).HasColumnName("partner_id");
            entity.Property(e => e.PartnerTypeId).HasColumnName("partner_type_id");
            entity.Property(e => e.CompanyName).HasColumnName("company_name").IsRequired().HasMaxLength(255);
            entity.Property(e => e.LegalAddress).HasColumnName("legal_address").HasMaxLength(500);
            entity.Property(e => e.Inn).HasColumnName("inn").HasMaxLength(20);
            entity.Property(e => e.DirectorFullName).HasColumnName("director_full_name").HasMaxLength(255);
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(50);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(100);
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.PartnerType)
                  .WithMany(e => e.Partners)
                  .HasForeignKey(e => e.PartnerTypeId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("fk_partners_partner_type");

            entity.HasCheckConstraint("CK_Partners_Rating", "rating >= 0");
        });

        modelBuilder.Entity<HodotaevProduct>(entity =>
        {
            entity.ToTable("hodotaev_products");
            entity.HasKey(e => e.ProductId);
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ProductName).HasColumnName("product_name").IsRequired().HasMaxLength(255);
            entity.Property(e => e.Article).HasColumnName("article").HasMaxLength(50);
            entity.Property(e => e.ProductType).HasColumnName("product_type").HasMaxLength(100);
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(1000);
            entity.Property(e => e.MinPrice).HasColumnName("min_price").HasColumnType("decimal(10,2)");
            entity.Property(e => e.Unit).HasColumnName("unit").HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(e => e.Article).IsUnique();

            entity.HasCheckConstraint("CK_Products_MinPrice", "min_price >= 0");
        });

        modelBuilder.Entity<HodotaevSale>(entity =>
        {
            entity.ToTable("hodotaev_sales");
            entity.HasKey(e => e.SaleId);
            entity.Property(e => e.SaleId).HasColumnName("sale_id");
            entity.Property(e => e.PartnerId).HasColumnName("partner_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.SalePrice).HasColumnName("sale_price").HasColumnType("decimal(10,2)");
            entity.Property(e => e.SaleDate).HasColumnName("sale_date");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Partner)
                  .WithMany(e => e.Sales)
                  .HasForeignKey(e => e.PartnerId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("fk_sales_partner");

            entity.HasOne(e => e.Product)
                  .WithMany(e => e.Sales)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("fk_sales_product");

            entity.HasCheckConstraint("CK_Sales_Quantity", "quantity > 0");
            entity.HasCheckConstraint("CK_Sales_SalePrice", "sale_price >= 0");

            entity.HasIndex(e => e.PartnerId);
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.SaleDate);
        });

        modelBuilder.Entity<HodotaevPartnerRatingHistory>(entity =>
        {
            entity.ToTable("hodotaev_partner_rating_history");
            entity.HasKey(e => e.HistoryId);
            entity.Property(e => e.HistoryId).HasColumnName("history_id");
            entity.Property(e => e.PartnerId).HasColumnName("partner_id");
            entity.Property(e => e.OldRating).HasColumnName("old_rating");
            entity.Property(e => e.NewRating).HasColumnName("new_rating");
            entity.Property(e => e.ChangeReason).HasColumnName("change_reason").HasMaxLength(500);
            entity.Property(e => e.ChangedAt).HasColumnName("changed_at");
            entity.Property(e => e.ChangedBy).HasColumnName("changed_by").HasMaxLength(100);

            entity.HasOne(e => e.Partner)
                  .WithMany(e => e.RatingHistory)
                  .HasForeignKey(e => e.PartnerId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("fk_rating_history_partner");

            entity.HasIndex(e => e.PartnerId);
        });

        modelBuilder.HasDbFunction(typeof(HodotaevPraktikaContext)
            .GetMethod(nameof(CalculateDiscount), new[] { typeof(int) })!);
    }

    [DbFunction("hodotaev_calculate_discount", "app")]
    public static decimal CalculateDiscount(int partnerId)
    {
        throw new NotSupportedException();
    }
}
