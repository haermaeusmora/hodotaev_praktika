using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using hodotaev_library.Models;
using hodotaev_library.Services;

namespace hodotaev_wpf;

public partial class SaleEditWindow : Window
{
    private readonly IPartnerService _partnerService;
    private readonly int _partnerId;
    private readonly int? _saleId;
    private HodotaevSale? _currentSale;
    private List<HodotaevProduct> _products = new();

    public SaleEditWindow()
    {
        InitializeComponent();
        _partnerService = App.ServiceProvider.GetRequiredService<IPartnerService>();
    }

    public SaleEditWindow(IPartnerService partnerService, int partnerId, int? saleId = null)
    {
        InitializeComponent();
        _partnerService = partnerService;
        _partnerId = partnerId;
        _saleId = saleId;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            // Загружаем список продуктов
            _products = await _partnerService.GetAllProductsAsync();
            cmbProduct.ItemsSource = _products;

            if (_saleId.HasValue)
            {
                // Режим редактирования
                Title = "Редактирование продажи";
                var sales = await _partnerService.GetPartnerSalesHistoryAsync(_partnerId);
                _currentSale = sales.FirstOrDefault(s => s.SaleId == _saleId.Value);

                if (_currentSale == null)
                {
                    MessageBox.Show("Продажа не найдена.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    DialogResult = false;
                    Close();
                    return;
                }

                // Заполняем форму
                cmbProduct.SelectedValue = _currentSale.ProductId;
                txtQuantity.Text = _currentSale.Quantity.ToString();
                txtSalePrice.Text = _currentSale.SalePrice.ToString("N2");
                dpSaleDate.SelectedDate = _currentSale.SaleDate;
            }
            else
            {
                // Режим добавления
                Title = "Добавление продажи";
                dpSaleDate.SelectedDate = DateTime.Today;
                
                // Если есть выбранный продукт, выбираем его
                if (_products.Any())
                    cmbProduct.SelectedIndex = 0;
            }

            UpdateTotalAmount();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Не удалось загрузить данные: {ex.GetBaseException().Message}",
                "Ошибка загрузки",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            DialogResult = false;
            Close();
        }
    }

    private void txtQuantity_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateTotalAmount();
    }

    private void txtSalePrice_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateTotalAmount();
    }

    private void UpdateTotalAmount()
    {
        if (int.TryParse(txtQuantity.Text, out int quantity) &&
            decimal.TryParse(txtSalePrice.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal price))
        {
            var total = quantity * price;
            txtTotalAmount.Text = $"{total:N2} руб.";
        }
        else
        {
            txtTotalAmount.Text = "0.00 руб.";
        }
    }

    private void txtQuantity_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        // Разрешаем только цифры
        e.Handled = !IsTextAllowed(e.Text, "^[0-9]+$");
    }

    private void txtSalePrice_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        // Разрешаем цифры и точку/запятую
        e.Handled = !IsTextAllowed(e.Text, "^[0-9.,]+$");
    }

    private static bool IsTextAllowed(string text, string pattern)
    {
        return Regex.IsMatch(text, pattern);
    }

    private async void btnSave_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Валидация
            if (cmbProduct.SelectedItem == null)
            {
                MessageBox.Show("Выберите продукцию.", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtQuantity.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Введите корректное количество (больше нуля).", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtSalePrice.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal price) || price < 0)
            {
                MessageBox.Show("Введите корректную цену.", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!dpSaleDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите дату продажи.", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var sale = new HodotaevSale
            {
                PartnerId = _partnerId,
                ProductId = (int)cmbProduct.SelectedValue,
                Quantity = quantity,
                SalePrice = price,
                SaleDate = dpSaleDate.SelectedDate.Value
            };

            if (_saleId.HasValue)
            {
                // При редактировании сначала удаляем старую продажу и создаем новую
                // (т.к. EF может не корректно обрабатывать обновление)
                sale.SaleId = _saleId.Value;
                await _partnerService.DeleteSaleAsync(_saleId.Value);
            }

            await _partnerService.AddSaleAsync(sale);

            MessageBox.Show(
                _saleId.HasValue 
                    ? "Продажа успешно обновлена." 
                    : "Продажа успешно добавлена.",
                "Сохранение",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Не удалось сохранить продажу: {ex.GetBaseException().Message}",
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
}
