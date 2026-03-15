using hodotaev_library.Models;

namespace hodotaev_wpf.Views;

public interface ISalesHistoryView : IView
{
    HodotaevPartner? Partner { get; set; }
    void SetSalesHistory(IEnumerable<HodotaevSale> sales);
    string WindowTitle { get; set; }
    event Action CloseRequested;
}
