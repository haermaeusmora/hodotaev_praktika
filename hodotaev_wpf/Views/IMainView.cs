using hodotaev_library.Models;

namespace hodotaev_wpf.Views;

public interface IMainView : IView
{
    void SetPartners(IEnumerable<HodotaevPartner> partners);
    HodotaevPartner? SelectedPartner { get; set; }
    event Action LoadDataRequested;
    event Action AddPartnerRequested;
    event Action EditPartnerRequested;
    event Action DeletePartnerRequested;
    event Action ViewSalesHistoryRequested;
    event Action ExitRequested;
}
