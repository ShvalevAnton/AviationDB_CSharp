using AviationDB_CSharp.Views;
using AviationDB_CSharp.Core;
using AviationDB_CSharp.Core;
using AviationDB_CSharp.Infrastructure;
using AviationDB_CSharp.Infrastructure;
using AviationDB_CSharp.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Windows;


namespace AviationDB_CSharp
{
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;

        public App()
        {
            // Настройка сервисов при запуске приложения
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            //// Загрузка конфигурации
            //var configuration = new ConfigurationBuilder()
            //    .AddJsonFile("appsettings.json")
            //    .Build();
            //// Конфигурация DbContext с строкой подключения из конфигурации
            //services.AddDbContext<AviationDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            // Конфигурация DbContext с строкой подключения
            services.AddDbContext<AviationDbContext>(options => options.UseNpgsql("Host=localhost;Database=demo;Username=anton;Password=q1"));

            // Регистрация репозиториев (остается без изменений)
            services.AddScoped<IAircraftRepository, AircraftRepository>();
            services.AddScoped<IAirportRepository, AirportRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IFlightRepository, FlightRepository>();
            services.AddScoped<ISeatRepository, SeatRepository>();
            services.AddScoped<ITicketRepository, TicketRepository>();
            services.AddScoped<ITicketFlightRepository, TicketFlightRepository>();
            services.AddScoped<IBoardingPassRepository, BoardingPassRepository>();

            // Регистрация ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<AircraftViewModel>();
            services.AddTransient<AirportViewModel>();
            services.AddTransient<BookingViewModel>();
            services.AddTransient<FlightViewModel>();
            services.AddTransient<SeatViewModel>();
            services.AddTransient<TicketViewModel>();
            services.AddTransient<TicketFlightViewModel>();
            services.AddTransient<BoardingPassViewModel>();

            // Регистрация главного окна
            services.AddSingleton<MainWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Получаем главное окно из контейнера и отображаем его
            var mainWindow = _serviceProvider.GetService<MainWindow>();
            mainWindow?.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Освобождаем ресурсы при выходе
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}