using hodotaev_library.Services;

namespace hodotaev_library.Tests;

/// <summary>
/// Тесты для расчета скидок партнеров
/// </summary>
public class DiscountCalculationTests
{
    private readonly PartnerService _partnerService;

    public DiscountCalculationTests()
    {
        // Создаем сервис без контекста БД для тестирования только логики расчета скидки
        _partnerService = CreateMockService();
    }

    /// <summary>
    /// Скидка 0% для объема продаж менее 10000
    /// </summary>
    [Fact]
    public void CalculateDiscount_LessThan10000_ReturnsZero()
    {
        // Arrange
        var testCases = new[] { 0m, 1000m, 5000m, 9999.99m };

        foreach (var salesAmount in testCases)
        {
            // Act
            var discount = _partnerService.CalculateDiscount(salesAmount);

            // Assert
            Assert.Equal(0, discount);
        }
    }

    /// <summary>
    /// Скидка 5% для объема продаж от 10000 до 50000
    /// </summary>
    [Fact]
    public void CalculateDiscount_From10000To50000_ReturnsFivePercent()
    {
        // Arrange
        var testCases = new[] { 10000m, 20000m, 30000m, 49999.99m };

        foreach (var salesAmount in testCases)
        {
            // Act
            var discount = _partnerService.CalculateDiscount(salesAmount);

            // Assert
            Assert.Equal(5, discount);
        }
    }

    /// <summary>
    /// Скидка 10% для объема продаж от 50000 до 300000
    /// </summary>
    [Fact]
    public void CalculateDiscount_From50000To300000_ReturnsTenPercent()
    {
        // Arrange
        var testCases = new[] { 50000m, 100000m, 200000m, 299999.99m };

        foreach (var salesAmount in testCases)
        {
            // Act
            var discount = _partnerService.CalculateDiscount(salesAmount);

            // Assert
            Assert.Equal(10, discount);
        }
    }

    /// <summary>
    /// Скидка 15% для объема продаж более 300000
    /// </summary>
    [Fact]
    public void CalculateDiscount_MoreThan300000_ReturnsFifteenPercent()
    {
        // Arrange
        var testCases = new[] { 300000m, 500000m, 1000000m };

        foreach (var salesAmount in testCases)
        {
            // Act
            var discount = _partnerService.CalculateDiscount(salesAmount);

            // Assert
            Assert.Equal(15, discount);
        }
    }

    /// <summary>
    /// Граничные значения расчета скидки
    /// </summary>
    [Fact]
    public void CalculateDiscount_BoundaryValues_CorrectCalculation()
    {
        // Arrange & Act & Assert
        Assert.Equal(0, _partnerService.CalculateDiscount(9999.99m));
        Assert.Equal(5, _partnerService.CalculateDiscount(10000m));
        Assert.Equal(5, _partnerService.CalculateDiscount(49999.99m));
        Assert.Equal(10, _partnerService.CalculateDiscount(50000m));
        Assert.Equal(10, _partnerService.CalculateDiscount(299999.99m));
        Assert.Equal(15, _partnerService.CalculateDiscount(300000m));
    }

    private static PartnerService CreateMockService()
    {
        // Для тестирования логики расчета скидки создаем сервис
        // без реальной БД (тестируем только метод CalculateDiscount)
        return new PartnerService(null!);
    }
}
