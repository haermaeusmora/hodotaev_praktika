using hodotaev_library.Services;
using hodotaev_library.Models;
using hodotaev_wpf.Views;

namespace hodotaev_wpf.Presenters;

/// <summary>
/// Презентер для окна добавления/редактирования партнера
/// </summary>
public class PartnerEditPresenter
{
    private readonly IPartnerEditView _view;
    private readonly IPartnerService _partnerService;
    private readonly int? _partnerId;
    private HodotaevPartner? _currentPartner;

    public PartnerEditPresenter(
        IPartnerEditView view,
        IPartnerService partnerService,
        int? partnerId)
    {
        _view = view;
        _partnerService = partnerService;
        _partnerId = partnerId;

        // Подписка на события
        _view.SaveRequested += OnSaveRequested;
        _view.CancelRequested += OnCancelRequested;
    }

    /// <summary>
    /// Инициализация презентера
    /// </summary>
    public void Initialize()
    {
        try
        {
            // Загрузка типов партнеров
            var partnerTypes = _partnerService.GetPartnerTypesAsync().Result;
            _view.PartnerTypes = partnerTypes;

            // Если редактируем - загружаем данные партнера
            if (_partnerId.HasValue)
            {
                _currentPartner = _partnerService.GetPartnerByIdAsync(_partnerId.Value).Result;
                if (_currentPartner == null)
                {
                    _view.ShowError("Партнер не найден в базе данных.", "Ошибка");
                    _view.Close();
                    return;
                }
                _view.Partner = ClonePartner(_currentPartner);
            }
            else
            {
                // Создаем нового партнера
                _view.Partner = new HodotaevPartner
                {
                    Rating = 0
                };
            }
        }
        catch (Exception ex)
        {
            _view.ShowError(
                $"Не удалось загрузить данные: {ex.GetBaseException().Message}",
                "Ошибка загрузки");
            _view.Close();
        }
    }

    /// <summary>
    /// Обработчик события сохранения
    /// </summary>
    private void OnSaveRequested()
    {
        try
        {
            if (_view.Partner == null)
            {
                _view.ShowError("Данные партнера не заполнены.", "Ошибка валидации");
                return;
            }

            // Валидация данных
            if (string.IsNullOrWhiteSpace(_view.Partner.CompanyName))
            {
                _view.ShowError("Наименование компании обязательно для заполнения.", "Ошибка валидации");
                return;
            }

            if (_view.Partner.PartnerTypeId <= 0)
            {
                _view.ShowError("Необходимо выбрать тип партнера.", "Ошибка валидации");
                return;
            }

            if (_view.Partner.Rating < 0)
            {
                _view.ShowError("Рейтинг должен быть неотрицательным числом.", "Ошибка валидации");
                return;
            }

            if (!string.IsNullOrWhiteSpace(_view.Partner.Email) && !IsValidEmail(_view.Partner.Email))
            {
                _view.ShowError("Некорректный формат email адреса.", "Ошибка валидации");
                return;
            }

            // Сохранение
            if (_partnerId.HasValue)
            {
                // Редактирование
                _view.Partner.PartnerId = _partnerId.Value;
                _partnerService.UpdatePartnerAsync(_view.Partner).Wait();
                _view.ShowInfo($"Данные партнера \"{_view.Partner.CompanyName}\" успешно обновлены.", "Редактирование");
            }
            else
            {
                // Добавление
                _partnerService.AddPartnerAsync(_view.Partner).Wait();
                _view.ShowInfo($"Партнер \"{_view.Partner.CompanyName}\" успешно добавлен.", "Добавление");
            }

            _view.DialogResult = true;
            _view.Close();
        }
        catch (AggregateException ex) when (ex.InnerException is ValidationException)
        {
            _view.ShowError(ex.InnerException.Message, "Ошибка валидации");
        }
        catch (Exception ex)
        {
            _view.ShowError(
                $"Не удалось сохранить данные: {ex.GetBaseException().Message}",
                "Ошибка сохранения");
        }
    }

    /// <summary>
    /// Обработчик события отмены
    /// </summary>
    private void OnCancelRequested()
    {
        _view.DialogResult = false;
        _view.Close();
    }

    /// <summary>
    /// Клонирование партнера
    /// </summary>
    private HodotaevPartner ClonePartner(HodotaevPartner partner)
    {
        return new HodotaevPartner
        {
            PartnerId = partner.PartnerId,
            PartnerTypeId = partner.PartnerTypeId,
            CompanyName = partner.CompanyName,
            LegalAddress = partner.LegalAddress,
            Inn = partner.Inn,
            DirectorFullName = partner.DirectorFullName,
            Phone = partner.Phone,
            Email = partner.Email,
            Rating = partner.Rating
        };
    }

    /// <summary>
    /// Проверка корректности email
    /// </summary>
    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
