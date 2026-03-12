using Microsoft.EntityFrameworkCore;
using hodotaev_library.Data;
using hodotaev_library.Models;

namespace hodotaev_library.Services;

/// <summary>
/// Реализация сервиса для работы с партнерами
/// </summary>
public class PartnerService : IPartnerService
{
    private readonly HodotaevPraktikaContext? _context;

    public PartnerService(HodotaevPraktikaContext? context = null)
    {
        _context = context;
    }

    /// <summary>
    /// Получить всех партнеров с информацией о типе и сумме продаж
    /// </summary>
    public async Task<List<HodotaevPartner>> GetAllPartnersAsync()
    {
        var partners = await _context.HodotaevPartners
            .Include(p => p.PartnerType)
            .Select(p => new HodotaevPartner
            {
                PartnerId = p.PartnerId,
                PartnerTypeId = p.PartnerTypeId,
                CompanyName = p.CompanyName,
                LegalAddress = p.LegalAddress,
                Inn = p.Inn,
                DirectorFullName = p.DirectorFullName,
                Phone = p.Phone,
                Email = p.Email,
                Rating = p.Rating,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                PartnerType = p.PartnerType,
                TotalSalesAmount = _context.HodotaevSales
                    .Where(s => s.PartnerId == p.PartnerId)
                    .Sum(s => s.Quantity * s.SalePrice)
            })
            .ToListAsync();

        // Рассчитываем скидку для каждого партнера
        foreach (var partner in partners)
        {
            partner.Discount = CalculateDiscount(partner.TotalSalesAmount);
        }

        return partners;
    }

    /// <summary>
    /// Получить партнера по ID
    /// </summary>
    public async Task<HodotaevPartner?> GetPartnerByIdAsync(int id)
    {
        return await _context.HodotaevPartners
            .Include(p => p.PartnerType)
            .FirstOrDefaultAsync(p => p.PartnerId == id);
    }

    /// <summary>
    /// Добавить нового партнера
    /// </summary>
    public async Task<HodotaevPartner> AddPartnerAsync(HodotaevPartner partner)
    {
        // Валидация данных
        ValidatePartner(partner);

        partner.CreatedAt = DateTime.UtcNow;
        partner.UpdatedAt = DateTime.UtcNow;

        _context.HodotaevPartners.Add(partner);
        await _context.SaveChangesAsync();

        // Загружаем тип партнера для возвращаемого объекта
        if (partner.PartnerTypeId != 0)
        {
            partner.PartnerType = await _context.HodotaevPartnerTypes
                .FirstOrDefaultAsync(pt => pt.PartnerTypeId == partner.PartnerTypeId);
        }

        return partner;
    }

    /// <summary>
    /// Обновить данные партнера
    /// </summary>
    public async Task<HodotaevPartner> UpdatePartnerAsync(HodotaevPartner partner)
    {
        // Валидация данных
        ValidatePartner(partner);

        var existingPartner = await _context.HodotaevPartners
            .FirstOrDefaultAsync(p => p.PartnerId == partner.PartnerId);

        if (existingPartner == null)
        {
            throw new InvalidOperationException($"Партнер с ID {partner.PartnerId} не найден");
        }

        // Проверяем изменение рейтинга для логирования
        if (existingPartner.Rating != partner.Rating)
        {
            await LogRatingChangeAsync(
                partner.PartnerId,
                existingPartner.Rating,
                partner.Rating,
                "Изменение через интерфейс приложения",
                "System");
        }

        // Обновляем поля
        existingPartner.PartnerTypeId = partner.PartnerTypeId;
        existingPartner.CompanyName = partner.CompanyName;
        existingPartner.LegalAddress = partner.LegalAddress;
        existingPartner.Inn = partner.Inn;
        existingPartner.DirectorFullName = partner.DirectorFullName;
        existingPartner.Phone = partner.Phone;
        existingPartner.Email = partner.Email;
        existingPartner.Rating = partner.Rating;
        existingPartner.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Загружаем обновленные данные
        return await GetPartnerByIdAsync(partner.PartnerId) 
            ?? throw new InvalidOperationException("Не удалось загрузить обновленные данные партнера");
    }

    /// <summary>
    /// Удалить партнера
    /// </summary>
    public async Task DeletePartnerAsync(int id)
    {
        var partner = await _context.HodotaevPartners
            .Include(p => p.Sales)
            .FirstOrDefaultAsync(p => p.PartnerId == id);

        if (partner == null)
        {
            throw new InvalidOperationException($"Партнер с ID {id} не найден");
        }

        // Проверяем наличие продаж
        if (partner.Sales.Any())
        {
            throw new InvalidOperationException(
                "Невозможно удалить партнера: существуют записи о продажах. " +
                "Сначала удалите историю продаж или выберите другого партнера.");
        }

        _context.HodotaevPartners.Remove(partner);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Получить список типов партнеров
    /// </summary>
    public async Task<List<HodotaevPartnerType>> GetPartnerTypesAsync()
    {
        return await _context.HodotaevPartnerTypes
            .OrderBy(pt => pt.TypeName)
            .ToListAsync();
    }

    /// <summary>
    /// Рассчитать скидку для партнера на основе объема продаж
    /// Правила из методички:
    /// до 10000 – 0%, от 10000 до 50000 – 5%, от 50000 до 300000 – 10%, более 300000 – 15%
    /// </summary>
    public decimal CalculateDiscount(decimal totalSalesAmount)
    {
        if (totalSalesAmount < 10000)
            return 0;
        else if (totalSalesAmount >= 10000 && totalSalesAmount < 50000)
            return 5;
        else if (totalSalesAmount >= 50000 && totalSalesAmount < 300000)
            return 10;
        else
            return 15;
    }

    /// <summary>
    /// Получить скидку партнера по ID (асинхронная версия)
    /// </summary>
    public async Task<decimal> CalculateDiscountAsync(int partnerId)
    {
        var totalSales = await _context.HodotaevSales
            .Where(s => s.PartnerId == partnerId)
            .SumAsync(s => s.Quantity * s.SalePrice);

        return CalculateDiscount(totalSales);
    }

    /// <summary>
    /// Получить историю продаж партнера
    /// </summary>
    public async Task<List<HodotaevSale>> GetPartnerSalesHistoryAsync(int partnerId)
    {
        return await _context.HodotaevSales
            .Include(s => s.Product)
            .Where(s => s.PartnerId == partnerId)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();
    }

    /// <summary>
    /// Записать изменение рейтинга в историю
    /// </summary>
    public async Task LogRatingChangeAsync(int partnerId, int oldRating, int newRating, string reason, string changedBy)
    {
        var historyEntry = new HodotaevPartnerRatingHistory
        {
            PartnerId = partnerId,
            OldRating = oldRating,
            NewRating = newRating,
            ChangeReason = reason,
            ChangedAt = DateTime.UtcNow,
            ChangedBy = changedBy
        };

        _context.HodotaevPartnerRatingHistories.Add(historyEntry);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Валидация данных партнера
    /// </summary>
    private void ValidatePartner(HodotaevPartner partner)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(partner.CompanyName))
            errors.Add("Наименование компании обязательно для заполнения");

        if (partner.PartnerTypeId <= 0)
            errors.Add("Необходимо выбрать тип партнера");

        if (partner.Rating < 0)
            errors.Add("Рейтинг должен быть неотрицательным числом");

        if (!string.IsNullOrWhiteSpace(partner.Email) && !IsValidEmail(partner.Email))
            errors.Add("Некорректный формат email адреса");

        if (errors.Any())
        {
            throw new ValidationException(string.Join("; ", errors));
        }
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

/// <summary>
/// Исключение валидации
/// </summary>
public class ValidationException : Exception
{
    public ValidationException(string message) : base(message)
    {
    }
}
