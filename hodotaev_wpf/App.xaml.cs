using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using hodotaev_library.Data;
using hodotaev_library.Services;

namespace hodotaev_wpf;

public static class App
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    [STAThread]
    public static void Main()
    {
        var wpfApp = new Application();

        var services = new ServiceCollection();

        services.AddDbContext<HodotaevPraktikaContext>(options =>
            options.UseNpgsql("Host=localhost;Port=5432;Database=hodotaev_praktika;Username=app;Password=123456789;Include Error Detail=true"));

        services.AddScoped<IPartnerService, PartnerService>();

        ServiceProvider = services.BuildServiceProvider();

        try
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();

            wpfApp.Run();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Ошибка запуска приложения: {ex.GetBaseException().Message}\n\n" +
                $"Убедитесь, что:\n" +
                $"1. PostgreSQL запущен\n" +
                $"2. База данных hodotaev_praktika существует\n" +
                $"3. Скрипт БД выполнен успешно",
                "Ошибка запуска",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
