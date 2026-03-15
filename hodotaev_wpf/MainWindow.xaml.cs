using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using hodotaev_library.Models;
using hodotaev_library.Services;

namespace hodotaev_wpf;

public partial class MainWindow : Window
{
    private readonly IPartnerService _partnerService;

    public MainWindow()
    {
        InitializeComponent();
        _partnerService = App.ServiceProvider.GetRequiredService<IPartnerService>();
        this.Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        LoadPartners();
    }

    private async void LoadPartners()
    {
        try
        {
            var partners = await _partnerService.GetAllPartnersAsync();
            lstPartners.ItemsSource = partners;
            txtCount.Text = $"Партнеров: {partners.Count()}";
            txtStatus.Text = $"Данные загружены: {DateTime.Now:dd.MM.yyyy HH:mm}";

            ClearPartnerCard();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Не удалось загрузить список партнеров: {ex.GetBaseException().Message}\n\n" +
                $"Убедитесь, что база данных PostgreSQL запущена и доступны учетные данные.",
                "Ошибка загрузки данных",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private HodotaevPartner? GetSelectedPartner()
    {
        return lstPartners.SelectedItem as HodotaevPartner;
    }

    private void ClearPartnerCard()
    {
        txtCardTitle.Text = "Информация о партнере";
        txtCompanyName.Text = "";
        txtPartnerType.Text = "";
        txtDirector.Text = "";
        txtPhone.Text = "";
        txtEmail.Text = "";
        txtRating.Text = "";
        txtAddress.Text = "";
        txtInn.Text = "";
        txtTotalSales.Text = "";
        txtDiscount.Text = "";
    }

    private void FillPartnerCard(HodotaevPartner partner)
    {
        txtCardTitle.Text = "Информация о партнере";
        txtCompanyName.Text = partner.CompanyName;
        txtPartnerType.Text = partner.PartnerType?.TypeName ?? "Не указан";
        txtDirector.Text = partner.DirectorFullName ?? "Не указан";
        txtPhone.Text = partner.Phone ?? "Не указан";
        txtEmail.Text = partner.Email ?? "Не указан";
        txtRating.Text = partner.Rating.ToString();
        txtAddress.Text = partner.LegalAddress ?? "Не указан";
        txtInn.Text = partner.Inn ?? "Не указан";
        txtTotalSales.Text = $"Общий объем продаж: {partner.TotalSalesAmount:N2} руб.";
        txtDiscount.Text = $"Скидка: {partner.Discount}%";
    }

    #region Event Handlers для кнопок

    private void btnAdd_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var editWindow = new PartnerEditWindow(_partnerService, null);
            editWindow.Title = "Добавление партнера";
            editWindow.Owner = this;
            editWindow.ShowDialog();

            if (editWindow.DialogResult == true)
            {
                LoadPartners();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Не удалось добавить партнера: {ex.GetBaseException().Message}",
                "Ошибка добавления",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void btnEdit_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var selectedPartner = GetSelectedPartner();
            if (selectedPartner == null)
            {
                MessageBox.Show(
                    "Выберите партнера из списка для редактирования.",
                    "Нет выделения",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var editWindow = new PartnerEditWindow(_partnerService, selectedPartner.PartnerId);
            editWindow.Title = "Редактирование партнера";
            editWindow.Owner = this;
            editWindow.ShowDialog();

            if (editWindow.DialogResult == true)
            {
                LoadPartners();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Не удалось редактировать партнера: {ex.GetBaseException().Message}",
                "Ошибка редактирования",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async void btnDelete_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var selectedPartner = GetSelectedPartner();
            if (selectedPartner == null)
            {
                MessageBox.Show(
                    "Выберите партнера из списка для удаления.",
                    "Нет выделения",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var salesCount = selectedPartner.TotalSalesAmount > 0 ?
                $"У партнера есть история продаж, которая также будет удалена." :
                "У партнера нет истории продаж.";

            var result = MessageBox.Show(
                $"Вы действительно хотите удалить партнера \"{selectedPartner.CompanyName}\"?\n\n" +
                $"{salesCount}\n" +
                $"Это действие нельзя отменить.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            await _partnerService.DeletePartnerAsync(selectedPartner.PartnerId);

            MessageBox.Show(
                $"Партнер \"{selectedPartner.CompanyName}\" успешно удален.",
                "Удаление выполнено",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            LoadPartners();
        }
        catch (Exception ex)
        {
            var errorMsg = $"Не удалось удалить партнера: {ex.GetBaseException().Message}";
            
            if (ex.InnerException != null)
            {
                errorMsg += $"\n\nВнутренняя ошибка: {ex.InnerException.Message}";
            }
            
            if (ex is NpgsqlException npgsqlEx)
            {
                errorMsg += $"\n\nОшибка БД: {npgsqlEx.SqlState}";
            }
            
            MessageBox.Show(
                errorMsg,
                "Ошибка удаления",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void btnHistory_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var selectedPartner = GetSelectedPartner();
            if (selectedPartner == null)
            {
                MessageBox.Show(
                    "Выберите партнера из списка для просмотра истории продаж.",
                    "Нет выделения",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var salesHistoryWindow = new SalesHistoryWindow(_partnerService, selectedPartner);
            salesHistoryWindow.Title = $"История продаж - {selectedPartner.CompanyName}";
            salesHistoryWindow.Owner = this;
            salesHistoryWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Не удалось загрузить историю продаж: {ex.GetBaseException().Message}",
                "Ошибка загрузки истории",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void btnRefresh_Click(object sender, RoutedEventArgs e)
    {
        LoadPartners();
    }

    private void btnExit_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    #endregion

    #region ListBox Event Handlers

    private void lstPartners_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var partner = GetSelectedPartner();
        if (partner != null)
        {
            FillPartnerCard(partner);
        }
        else
        {
            ClearPartnerCard();
        }
    }

    private void lstPartners_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        var partner = GetSelectedPartner();
        if (partner != null)
        {
            btnEdit_Click(sender, e);
        }
    }

    #endregion
}
