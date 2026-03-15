namespace hodotaev_library.Models;

public class HodotaevPartnerType
{
    public int PartnerTypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string? Description { get; set; }

    public virtual ICollection<HodotaevPartner> Partners { get; set; } = new List<HodotaevPartner>();
}
