using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using hodotaev_library.Models;
using hodotaev_library.Services;

namespace hodotaev_wpf;

public partial class SalesHistoryWindow : Window
{
    private readonly IPartnerService _partnerService;
    private readonly HodotaevPartner _partner;

    public SalesHistoryWindow()
    {
        InitializeComponent();
        _partnerService = App.ServiceProvider.GetRequiredService<IPartnerService>();
    }

    public SalesHistoryWindow(IPartnerService partnerService, HodotaevPartner partner)
    {
        InitializeComponent();
        _partnerService = partnerService;
        _partner = partner;
        InitializeWindow();
    }

    private async void InitializeWindow()
    {
        try
        {
            txtCompanyName.Text = $"Компания: {_partner.CompanyName}";
            txtPartnerType.Text = _partner.PartnerType?.TypeName ?? "Тип не указан";
            txtDirector.Text = $"Директор: {_partner.DirectorFullName ?? "Не указан"}";
            txtPhone.Text = $"Телефон: {_partner.Phone ?? "Не указан"}";
            txtEmail.Text = $"Email: {_partner.Email ?? "Не указан"}";
            txtRating.Text = $"Рейтинг: {_partner.Rating}";

            var totalSales = _partner.TotalSalesAmount;
            var discount = _partner.Discount;
            txtTotalSales.Text = $"Общий объем продаж: {totalSales:N2} руб. (скидка: {discount}%)";

            await LoadSalesHistoryAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Не удалось загрузить историю продаж: {ex.GetBaseException().Message}",
                "Ошибка загрузки",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async Task LoadSalesHistoryAsync()
    {
        var sales = await _partnerService.GetPartnerSalesHistoryAsync(_partner.PartnerId);

        var salesWithTotal = sales.Select(s => new
        {
            s.SaleId,
            s.PartnerId,
            s.ProductId,
            s.Quantity,
            s.SalePrice,
            s.SaleDate,
            s.CreatedAt,
            s.Product,
            TotalAmount = s.Quantity * s.SalePrice
        });

        dgSales.ItemsSource = salesWithTotal;
        
        // Обновляем заголовок окна с количеством записей
        Title = $"История продаж - {_partner.CompanyName} ({sales.Count} записей)";
    }

    private void btnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void btnAddSale_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var editWindow = new SaleEditWindow(_partnerService, _partner.PartnerId, null);
            editWindow.Owner = this;
            editWindow.ShowDialog();

            if (editWindow.DialogResult == true)
            {
                await LoadSalesHistoryAsync();
                // Перезагружаем данные партнера для обновления общей суммы
                var partners = await _partnerService.GetAllPartnersAsync();
                var updatedPartner = partners.FirstOrDefault(p => p.PartnerId == _partner.PartnerId);
                if (updatedPartner != null)
                {
                    _partner.TotalSalesAmount = updatedPartner.TotalSalesAmount;
                    _partner.Discount = updatedPartner.Discount;
                    txtTotalSales.Text = $"Общий объем продаж: {_partner.TotalSalesAmount:N2} руб. (скидка: {_partner.Discount}%)";
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Не удалось добавить продажу: {ex.GetBaseException().Message}",
                "Ошибка добавления",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async void btnDeleteSale_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var selectedSale = dgSales.SelectedItem as dynamic;
            if (selectedSale == null)
            {
                MessageBox.Show(
                    "Выберите продажу из списка для удаления.",
                    "Нет выделения",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var saleId = selectedSale.SaleId;
            var result = MessageBox.Show(
                $"Вы действительно хотите удалить эту продажу?\n\n" +
                $"Это действие нельзя отменить.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            await _partnerService.DeleteSaleAsync(saleId);

            MessageBox.Show(
                "Продажа успешно удалена.",
                "Удаление выполнено",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            await LoadSalesHistoryAsync();
            // Перезагружаем данные партнера для обновления общей суммы
            var partners = await _partnerService.GetAllPartnersAsync();
            var updatedPartner = partners.FirstOrDefault(p => p.PartnerId == _partner.PartnerId);
            if (updatedPartner != null)
            {
                _partner.TotalSalesAmount = updatedPartner.TotalSalesAmount;
                _partner.Discount = updatedPartner.Discount;
                txtTotalSales.Text = $"Общий объем продаж: {_partner.TotalSalesAmount:N2} руб. (скидка: {_partner.Discount}%)";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Не удалось удалить продажу: {ex.GetBaseException().Message}",
                "Ошибка удаления",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void btnEditSale_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var selectedSale = dgSales.SelectedItem as dynamic;
            if (selectedSale == null)
            {
                MessageBox.Show(
                    "Выберите продажу из списка для редактирования.",
                    "Нет выделения",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var saleId = selectedSale.SaleId;
            var editWindow = new SaleEditWindow(_partnerService, _partner.PartnerId, saleId);
            editWindow.Owner = this;
            editWindow.ShowDialog();

            if (editWindow.DialogResult == true)
            {
                InitializeWindow();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Не удалось редактировать продажу: {ex.GetBaseException().Message}",
                "Ошибка редактирования",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void dgSales_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Можно добавить логику для активации/деактивации кнопок
    }
}
