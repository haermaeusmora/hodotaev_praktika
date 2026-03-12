using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hodotaev_library.Models;

/// <summary>
/// Партнер компании
/// </summary>
public class HodotaevPartner
{
    [Key]
    [Column("partner_id")]
    public int PartnerId { get; set; }

    [Column("partner_type_id")]
    [Required]
    public int PartnerTypeId { get; set; }

    [Column("company_name")]
    [Required]
    [StringLength(255)]
    public string CompanyName { get; set; } = string.Empty;

    [Column("legal_address")]
    [StringLength(500)]
    public string? LegalAddress { get; set; }

    [Column("inn")]
    [StringLength(20)]
    public string? Inn { get; set; }

    [Column("director_full_name")]
    [StringLength(255)]
    public string? DirectorFullName { get; set; }

    [Column("phone")]
    [StringLength(50)]
    public string? Phone { get; set; }

    [Column("email")]
    [StringLength(100)]
    [EmailAddress]
    public string? Email { get; set; }

    [Column("rating")]
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Рейтинг должен быть неотрицательным числом")]
    public int Rating { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Навигационные свойства
    [ForeignKey(nameof(PartnerTypeId))]
    public virtual HodotaevPartnerType? PartnerType { get; set; }
    public virtual ICollection<HodotaevSale> Sales { get; set; } = new List<HodotaevSale>();
    public virtual ICollection<HodotaevPartnerRatingHistory> RatingHistory { get; set; } = new List<HodotaevPartnerRatingHistory>();

    // Вычисляемое свойство для скидки (не сохраняется в БД)
    [NotMapped]
    public decimal Discount { get; set; }

    // Вычисляемое свойство для общей суммы продаж (не сохраняется в БД)
    [NotMapped]
    public decimal TotalSalesAmount { get; set; }
}
