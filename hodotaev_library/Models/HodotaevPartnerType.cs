namespace hodotaev_library.Models;

/// <summary>
/// Тип партнера (справочник)
/// </summary>
public class HodotaevPartnerType
{
    public int PartnerTypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Навигационное свойство
    public virtual ICollection<HodotaevPartner> Partners { get; set; } = new List<HodotaevPartner>();
}
