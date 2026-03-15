using Microsoft.EntityFrameworkCore;
using hodotaev_library.Data;
using hodotaev_library.Models;

namespace hodotaev_library.Services;

public class PartnerService : IPartnerService
{
    private readonly HodotaevPraktikaContext? _context;

    public PartnerService(HodotaevPraktikaContext? context = null)
    {
        _context = context;
    }

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

        foreach (var partner in partners)
        {
            partner.Discount = CalculateDiscount(partner.TotalSalesAmount);
        }

        return partners;
    }

    public async Task<HodotaevPartner?> GetPartnerByIdAsync(int id)
    {
        return await _context.HodotaevPartners
            .Include(p => p.PartnerType)
            .FirstOrDefaultAsync(p => p.PartnerId == id);
    }

    public async Task<HodotaevPartner> AddPartnerAsync(HodotaevPartner partner)
    {
        ValidatePartner(partner);

        partner.CreatedAt = DateTime.UtcNow;
        partner.UpdatedAt = DateTime.UtcNow;

        _context.HodotaevPartners.Add(partner);
        await _context.SaveChangesAsync();

        if (partner.PartnerTypeId != 0)
        {
            partner.PartnerType = await _context.HodotaevPartnerTypes
                .FirstOrDefaultAsync(pt => pt.PartnerTypeId == partner.PartnerTypeId);
        }

        return partner;
    }

    public async Task<HodotaevPartner> UpdatePartnerAsync(HodotaevPartner partner)
    {
        ValidatePartner(partner);

        var existingPartner = await _context.HodotaevPartners
            .FirstOrDefaultAsync(p => p.PartnerId == partner.PartnerId);

        if (existingPartner == null)
        {
            throw new InvalidOperationException($"Партнер с ID {partner.PartnerId} не найден");
        }

        if (existingPartner.Rating != partner.Rating)
        {
            await LogRatingChangeAsync(
                partner.PartnerId,
                existingPartner.Rating,
                partner.Rating,
                "Изменение через интерфейс приложения",
                "System");
        }

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

        return await GetPartnerByIdAsync(partner.PartnerId)
            ?? throw new InvalidOperationException("Не удалось загрузить обновленные данные партнера");
    }

    public async Task DeletePartnerAsync(int id)
    {
        var partner = await _context.HodotaevPartners
            .Include(p => p.Sales)
            .FirstOrDefaultAsync(p => p.PartnerId == id);

        if (partner == null)
        {
            throw new InvalidOperationException($"Партнер с ID {id} не найден");
        }

        if (partner.Sales.Any())
        {
            _context.HodotaevSales.RemoveRange(partner.Sales);
        }

        var ratingHistory = await _context.HodotaevPartnerRatingHistories
            .Where(h => h.PartnerId == id)
            .ToListAsync();
        if (ratingHistory.Any())
        {
            _context.HodotaevPartnerRatingHistories.RemoveRange(ratingHistory);
        }

        _context.HodotaevPartners.Remove(partner);
        await _context.SaveChangesAsync();
    }

    public async Task<List<HodotaevPartnerType>> GetPartnerTypesAsync()
    {
        return await _context.HodotaevPartnerTypes
            .OrderBy(pt => pt.TypeName)
            .ToListAsync();
    }

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

    public async Task<decimal> CalculateDiscountAsync(int partnerId)
    {
        var totalSales = await _context.HodotaevSales
            .Where(s => s.PartnerId == partnerId)
            .SumAsync(s => s.Quantity * s.SalePrice);

        return CalculateDiscount(totalSales);
    }

    public async Task<List<HodotaevSale>> GetPartnerSalesHistoryAsync(int partnerId)
    {
        return await _context.HodotaevSales
            .Include(s => s.Product)
            .Where(s => s.PartnerId == partnerId)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();
    }

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

    public async Task<HodotaevSale> AddSaleAsync(HodotaevSale sale)
    {
        if (sale.Quantity <= 0)
            throw new ValidationException("Количество должно быть больше нуля");

        if (sale.SalePrice < 0)
            throw new ValidationException("Цена должна быть неотрицательной");

        var partner = await _context.HodotaevPartners.FindAsync(sale.PartnerId);
        if (partner == null)
            throw new ValidationException($"Партнер с ID {sale.PartnerId} не найден");

        var product = await _context.HodotaevProducts.FindAsync(sale.ProductId);
        if (product == null)
            throw new ValidationException($"Продукт с ID {sale.ProductId} не найден");

        sale.CreatedAt = DateTime.UtcNow;
        if (sale.SaleDate == default)
            sale.SaleDate = DateTime.UtcNow;

        _context.HodotaevSales.Add(sale);
        await _context.SaveChangesAsync();

        sale.Product = await _context.HodotaevProducts.FindAsync(sale.ProductId);
        sale.Partner = await _context.HodotaevPartners.FindAsync(sale.PartnerId);

        return sale;
    }

    public async Task DeleteSaleAsync(int saleId)
    {
        var sale = await _context.HodotaevSales.FindAsync(saleId);
        if (sale == null)
            throw new InvalidOperationException($"Продажа с ID {saleId} не найдена");

        _context.HodotaevSales.Remove(sale);
        await _context.SaveChangesAsync();
    }

    public async Task<List<HodotaevProduct>> GetAllProductsAsync()
    {
        return await _context.HodotaevProducts
            .OrderBy(p => p.ProductName)
            .ToListAsync();
    }

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

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message)
    {
    }
}
