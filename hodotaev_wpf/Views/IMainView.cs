using hodotaev_library.Models;

namespace hodotaev_wpf.Views;

/// <summary>
/// Интерфейс главного окна (список партнеров)
/// </summary>
public interface IMainView : IView
{
    /// <summary>
    /// Отобразить список партнеров
    /// </summary>
    void SetPartners(IEnumerable<HodotaevPartner> partners);

    /// <summary>
    /// Выбранный партнер
    /// </summary>
    HodotaevPartner? SelectedPartner { get; set; }

    /// <summary>
    /// Событие загрузки данных
    /// </summary>
    event Action LoadDataRequested;

    /// <summary>
    /// Событие добавления партнера
    /// </summary>
    event Action AddPartnerRequested;

    /// <summary>
    /// Событие редактирования партнера
    /// </summary>
    event Action EditPartnerRequested;

    /// <summary>
    /// Событие удаления партнера
    /// </summary>
    event Action DeletePartnerRequested;

    /// <summary>
    /// Событие просмотра истории продаж
    /// </summary>
    event Action ViewSalesHistoryRequested;

    /// <summary>
    /// Событие выхода из приложения
    /// </summary>
    event Action ExitRequested;
}
