using System.Windows;
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

    private void btnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
