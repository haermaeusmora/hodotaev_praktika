using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hodotaev_library.Models;

public class HodotaevSale
{
    [Key]
    [Column("sale_id")]
    public int SaleId { get; set; }

    [Column("partner_id")]
    [Required]
    public int PartnerId { get; set; }

    [Column("product_id")]
    [Required]
    public int ProductId { get; set; }

    [Column("quantity")]
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Количество должно быть больше нуля")]
    public int Quantity { get; set; }

    [Column("sale_price", TypeName = "decimal(10,2)")]
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Цена должна быть неотрицательной")]
    public decimal SalePrice { get; set; }

    [Column("sale_date")]
    [Required]
    public DateTime SaleDate { get; set; } = DateTime.UtcNow;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(PartnerId))]
    public virtual HodotaevPartner? Partner { get; set; }

    [ForeignKey(nameof(ProductId))]
    public virtual HodotaevProduct? Product { get; set; }
}
