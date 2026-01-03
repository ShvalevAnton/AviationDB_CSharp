using AviationDB_CSharp.Data;
using AviationDB_CSharp.Services;
using AviationDB_CSharp.ViewModels;
using AviationDB_CSharp.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Configuration;
using System.Data;
using System.Windows;

namespace AviationDB_CSharp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            // Настройка конфигурации
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Настройка DI контейнера
            var services = new ServiceCollection();
             ConfigureServices(services, configuration);

            ServiceProvider = services.BuildServiceProvider();

            // Создание БД если не существует
            EnsureDatabaseCreated();

            // Показываем главное окно
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Регистрируем фабрику DbContext
            services.AddDbContextFactory<ApplicationDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    npgsqlOptions => npgsqlOptions.UseNetTopologySuite()));

            // DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    npgsqlOptions => npgsqlOptions.UseNetTopologySuite()),
                    ServiceLifetime.Transient);


            // Сервисы (те же самые из консольного приложения)
            services.AddScoped<AircraftService>();
            services.AddScoped<AirportService>();
            services.AddScoped<BookingService>();
            services.AddScoped<FlightService>();
            services.AddScoped<SeatService>();
            services.AddScoped<TicketService>();
            services.AddScoped<TicketFlightService>();

            // ViewModels
            services.AddSingleton<MainViewModel>();
            //services.AddTransient<AircraftViewModel>();
            //services.AddTransient<AirportViewModel>();
            //services.AddTransient<BookingViewModel>();
            //services.AddTransient<FlightViewModel>();
            services.AddTransient<SeatsViewModel>();
            services.AddTransient<TicketsViewModel>();

            // Окна
            services.AddSingleton<MainWindow>();
            services.AddTransient<AircraftEditWindow>();
            services.AddTransient<AircraftsViewModel>();
            services.AddTransient<AirportEditWindow>();
            services.AddTransient<BookingEditWindow>();
            services.AddTransient<BookingViewWindow>();
            services.AddTransient<FlightEditWindow>();
            services.AddTransient<FlightViewWindow>();
            services.AddTransient<SeatEditWindow>(); 
            services.AddTransient<SeatViewWindow>();
            services.AddTransient<TicketEditWindow>();
            services.AddTransient<TicketViewWindow>();
        }

        private void EnsureDatabaseCreated()
        {
            using var scope = ServiceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.EnsureCreated();
        }
    }
    // Вспомогательный класс для доступа к ServiceProvider
    public static class ServiceProviderHelper
    {
        public static IServiceProvider GetServiceProvider()
        {
            return App.ServiceProvider;
        }
    }

}
