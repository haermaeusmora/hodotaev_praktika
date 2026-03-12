using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hodotaev_library.Models;

/// <summary>
/// История изменений рейтинга партнера
/// </summary>
public class HodotaevPartnerRatingHistory
{
    [Key]
    [Column("history_id")]
    public int HistoryId { get; set; }

    [Column("partner_id")]
    [Required]
    public int PartnerId { get; set; }

    [Column("old_rating")]
    public int? OldRating { get; set; }

    [Column("new_rating")]
    [Required]
    public int NewRating { get; set; }

    [Column("change_reason")]
    [StringLength(500)]
    public string? ChangeReason { get; set; }

    [Column("changed_at")]
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    [Column("changed_by")]
    [StringLength(100)]
    public string? ChangedBy { get; set; }

    // Навигационное свойство
    [ForeignKey(nameof(PartnerId))]
    public virtual HodotaevPartner? Partner { get; set; }
}
