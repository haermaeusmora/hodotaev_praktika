using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hodotaev_library.Models;

/// <summary>
/// Продукция компании (напольные покрытия)
/// </summary>
public class HodotaevProduct
{
    [Key]
    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("product_name")]
    [Required]
    [StringLength(255)]
    public string ProductName { get; set; } = string.Empty;

    [Column("article")]
    [StringLength(50)]
    public string? Article { get; set; }

    [Column("product_type")]
    [StringLength(100)]
    public string? ProductType { get; set; }

    [Column("description")]
    [StringLength(1000)]
    public string? Description { get; set; }

    [Column("min_price", TypeName = "decimal(10,2)")]
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Цена должна быть неотрицательной")]
    public decimal MinPrice { get; set; }

    [Column("unit")]
    [StringLength(50)]
    public string? Unit { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Навигационное свойство
    public virtual ICollection<HodotaevSale> Sales { get; set; } = new List<HodotaevSale>();
}
