using hodotaev_library.Services;
using hodotaev_library.Models;
using hodotaev_wpf.Views;

namespace hodotaev_wpf.Presenters;

/// <summary>
/// Презентер для окна истории продаж
/// </summary>
public class SalesHistoryPresenter
{
    private readonly ISalesHistoryView _view;
    private readonly IPartnerService _partnerService;
    private readonly HodotaevPartner _partner;

    public SalesHistoryPresenter(
        ISalesHistoryView view,
        IPartnerService partnerService,
        HodotaevPartner partner)
    {
        _view = view;
        _partnerService = partnerService;
        _partner = partner;

        _view.CloseRequested += OnCloseRequested;
    }

    /// <summary>
    /// Инициализация презентера
    /// </summary>
    public void Initialize()
    {
        try
        {
            _view.Partner = _partner;
            LoadSalesHistory();
        }
        catch (Exception ex)
        {
            _view.ShowError(
                $"Не удалось загрузить историю продаж: {ex.GetBaseException().Message}",
                "Ошибка загрузки");
        }
    }

    /// <summary>
    /// Загрузка истории продаж
    /// </summary>
    private void LoadSalesHistory()
    {
        var sales = _partnerService.GetPartnerSalesHistoryAsync(_partner.PartnerId).Result;
        _view.SetSalesHistory(sales);
    }

    /// <summary>
    /// Обработчик события закрытия
    /// </summary>
    private void OnCloseRequested()
    {
        _view.Close();
    }
}
