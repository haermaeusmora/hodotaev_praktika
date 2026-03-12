using hodotaev_library.Models;

namespace hodotaev_wpf.Views;

/// <summary>
/// Интерфейс окна истории продаж партнера
/// </summary>
public interface ISalesHistoryView : IView
{
    /// <summary>
    /// Партнер, для которого показываем историю
    /// </summary>
    HodotaevPartner? Partner { get; set; }

    /// <summary>
    /// Список продаж
    /// </summary>
    void SetSalesHistory(IEnumerable<HodotaevSale> sales);

    /// <summary>
    /// Заголовок окна
    /// </summary>
    string WindowTitle { get; set; }

    /// <summary>
    /// Событие закрытия
    /// </summary>
    event Action CloseRequested;
}
