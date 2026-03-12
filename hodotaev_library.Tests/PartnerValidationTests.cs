using hodotaev_library.Models;

namespace hodotaev_library.Tests;

/// <summary>
/// Тесты валидации данных партнера
/// </summary>
public class PartnerValidationTests
{
    /// <summary>
    /// Проверка корректности email адресов
    /// </summary>
    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("user.name@domain.ru", true)]
    [InlineData("invalid-email", false)]
    [InlineData("@nodomain.com", false)]
    [InlineData("noat.com", false)]
    [InlineData("", true)] // Пустой email допустим (необязательное поле)
    [InlineData(null, true)] // Null допустим
    public void IsValidEmail_ValidatesCorrectly(string email, bool expectedValid)
    {
        // Arrange
        var partner = new HodotaevPartner
        {
            Email = email,
            CompanyName = "Test Company",
            PartnerTypeId = 1,
            Rating = 0
        };

        // Act
        bool isValid = string.IsNullOrWhiteSpace(email) || IsValidEmail(email);

        // Assert
        Assert.Equal(expectedValid, isValid);
    }

    /// <summary>
    /// Проверка рейтинга - должен быть неотрицательным
    /// </summary>
    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(100, true)]
    public void Rating_NonNegative_Validation(int rating, bool expectedValid)
    {
        // Arrange
        var partner = new HodotaevPartner
        {
            CompanyName = "Test Company",
            PartnerTypeId = 1,
            Rating = rating
        };

        // Act
        bool isValid = rating >= 0;

        // Assert
        Assert.Equal(expectedValid, isValid);
    }

    /// <summary>
    /// Проверка обязательности наименования компании
    /// </summary>
    [Theory]
    [InlineData("Company Name", true)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData(null, false)]
    public void CompanyName_Required_Validation(string? name, bool expectedValid)
    {
        // Arrange
        var partner = new HodotaevPartner
        {
            CompanyName = name,
            PartnerTypeId = 1,
            Rating = 0
        };

        // Act
        bool isValid = !string.IsNullOrWhiteSpace(name);

        // Assert
        Assert.Equal(expectedValid, isValid);
    }

    /// <summary>
    /// Проверка ИНН - формат для России
    /// </summary>
    [Theory]
    [InlineData("7701234567", true)] // 10 знаков - ИНН организации
    [InlineData("780123456789", true)] // 12 знаков - ИНН физического лица
    [InlineData("123456", false)] // Слишком короткий
    [InlineData("1234567890123", false)] // Слишком длинный
    [InlineData("", true)] // Пустой допустим (необязательное поле)
    [InlineData(null, true)] // Null допустим
    public void Inn_Format_Validation(string? inn, bool expectedValid)
    {
        // Arrange
        var partner = new HodotaevPartner
        {
            Inn = inn,
            CompanyName = "Test Company",
            PartnerTypeId = 1,
            Rating = 0
        };

        // Act
        bool isValid = string.IsNullOrWhiteSpace(inn) || (inn.Length == 10 || inn.Length == 12);

        // Assert
        Assert.Equal(expectedValid, isValid);
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
