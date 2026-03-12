using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using hodotaev_library.Models;
using hodotaev_library.Services;

namespace hodotaev_wpf;

public partial class PartnerEditWindow : Window
{
    private readonly IPartnerService _partnerService;
    private readonly int? _partnerId;
    private HodotaevPartner? _currentPartner;

    public PartnerEditWindow()
    {
        InitializeComponent();
        _partnerService = App.ServiceProvider.GetRequiredService<IPartnerService>();
        InitializeWindow();
    }

    public PartnerEditWindow(IPartnerService partnerService, int? partnerId)
    {
        InitializeComponent();
        _partnerService = partnerService;
        _partnerId = partnerId;
        InitializeWindow();
    }

    private async void InitializeWindow()
    {
        try
        {
            var partnerTypes = await _partnerService.GetPartnerTypesAsync();
            cmbPartnerType.ItemsSource = partnerTypes;
            if (partnerTypes.Any())
                cmbPartnerType.SelectedIndex = 0;

            if (_partnerId.HasValue)
            {
                _currentPartner = await _partnerService.GetPartnerByIdAsync(_partnerId.Value);
                if (_currentPartner == null)
                {
                    MessageBox.Show("Партнер не найден в базе данных.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }
                FillForm(_currentPartner);
            }
            else
            {
                _currentPartner = new HodotaevPartner { Rating = 0 };
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Не удалось загрузить данные: {ex.GetBaseException().Message}",
                "Ошибка загрузки",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Close();
        }
    }

    private HodotaevPartner? GetPartnerFromForm()
    {
        if (cmbPartnerType.SelectedItem == null)
            return null;

        return new HodotaevPartner
        {
            PartnerTypeId = (int)cmbPartnerType.SelectedValue,
            CompanyName = txtCompanyName.Text.Trim(),
            LegalAddress = txtLegalAddress.Text.Trim(),
            Inn = txtInn.Text.Trim(),
            DirectorFullName = txtDirectorFullName.Text.Trim(),
            Phone = txtPhone.Text.Trim(),
            Email = txtEmail.Text.Trim(),
            Rating = int.TryParse(txtRating.Text, out var rating) ? rating : 0
        };
    }

    private void FillForm(HodotaevPartner partner)
    {
        txtCompanyName.Text = partner.CompanyName;
        txtLegalAddress.Text = partner.LegalAddress;
        txtInn.Text = partner.Inn;
        txtDirectorFullName.Text = partner.DirectorFullName;
        txtPhone.Text = partner.Phone;
        txtEmail.Text = partner.Email;
        txtRating.Text = partner.Rating.ToString();

        if (partner.PartnerTypeId > 0)
        {
            cmbPartnerType.SelectedValue = partner.PartnerTypeId;
        }
    }

    private async void btnSave_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var partner = GetPartnerFromForm();
            if (partner == null)
            {
                MessageBox.Show("Данные партнера не заполнены.", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(partner.CompanyName))
            {
                MessageBox.Show("Наименование компании обязательно для заполнения.", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (partner.PartnerTypeId <= 0)
            {
                MessageBox.Show("Необходимо выбрать тип партнера.", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (partner.Rating < 0)
            {
                MessageBox.Show("Рейтинг должен быть неотрицательным числом.", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!string.IsNullOrWhiteSpace(partner.Email) && !IsValidEmail(partner.Email))
            {
                MessageBox.Show("Некорректный формат email адреса.", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_partnerId.HasValue)
            {
                partner.PartnerId = _partnerId.Value;
                await _partnerService.UpdatePartnerAsync(partner);
                MessageBox.Show(
                    $"Данные партнера \"{partner.CompanyName}\" успешно обновлены.",
                    "Редактирование",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                await _partnerService.AddPartnerAsync(partner);
                MessageBox.Show(
                    $"Партнер \"{partner.CompanyName}\" успешно добавлен.",
                    "Добавление",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Не удалось сохранить данные: {ex.GetBaseException().Message}",
                "Ошибка сохранения",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

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
