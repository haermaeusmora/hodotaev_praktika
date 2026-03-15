using hodotaev_library.Models;

namespace hodotaev_library.Services;

public interface IPartnerService
{
    Task<List<HodotaevPartner>> GetAllPartnersAsync();
    Task<HodotaevPartner?> GetPartnerByIdAsync(int id);
    Task<HodotaevPartner> AddPartnerAsync(HodotaevPartner partner);
    Task<HodotaevPartner> UpdatePartnerAsync(HodotaevPartner partner);
    Task DeletePartnerAsync(int id);
    Task<List<HodotaevPartnerType>> GetPartnerTypesAsync();
    Task<decimal> CalculateDiscountAsync(int partnerId);
    Task<List<HodotaevSale>> GetPartnerSalesHistoryAsync(int partnerId);
    Task LogRatingChangeAsync(int partnerId, int oldRating, int newRating, string reason, string changedBy);
    
    Task<HodotaevSale> AddSaleAsync(HodotaevSale sale);
    Task DeleteSaleAsync(int saleId);
    Task<List<HodotaevProduct>> GetAllProductsAsync();
}
