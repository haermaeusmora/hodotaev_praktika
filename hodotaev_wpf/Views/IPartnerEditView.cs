using hodotaev_library.Models;

namespace hodotaev_wpf.Views;

public interface IPartnerEditView : IView
{
    HodotaevPartner? Partner { get; set; }
    IEnumerable<HodotaevPartnerType> PartnerTypes { get; set; }
    string WindowTitle { get; set; }
    event Action SaveRequested;
    event Action CancelRequested;
    bool DialogResult { get; set; }
    void ShowDialog();
}
