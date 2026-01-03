using AviationDB_CSharp.Data;
using AviationDB_CSharp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite.Geometries;

namespace AviationDB_CSharp.Services
{
    /// <summary>
    /// Сервис для выполнения CRUD операций с таблицей airports_data
    /// </summary>
    public class AirportService
    {
        private readonly IServiceProvider _serviceProvider;

        public AirportService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Создание нового аэропорта (Create)
        /// </summary>
        public async Task<Airport> CreateAsync(Airport airport)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                // Валидация кода аэропорта
                if (string.IsNullOrEmpty(airport.AirportCode) || airport.AirportCode.Length != 3)
                {
                    throw new ArgumentException("Код аэропорта должен состоять ровно из 3 символов.");
                }

                // Проверка существования
                var existing = await context.Airports
                    .FirstOrDefaultAsync(a => a.AirportCode == airport.AirportCode);

                if (existing != null)
                {
                    throw new ArgumentException($"Аэропорт с кодом {airport.AirportCode} уже существует.");
                }

                // Валидация часового пояса
                if (string.IsNullOrWhiteSpace(airport.Timezone))
                {
                    throw new ArgumentException("Часовой пояс обязателен.");
                }

                // Обработка JSON полей
                if (string.IsNullOrWhiteSpace(airport.AirportName) || !Airport.IsValidJson(airport.AirportName))
                {
                    airport.AirportName = Airport.CreateLocalizedJson(airport.AirportName?.Trim() ?? "Unknown Airport");
                }

                if (string.IsNullOrWhiteSpace(airport.City) || !Airport.IsValidJson(airport.City))
                {
                    airport.City = Airport.CreateLocalizedJson(airport.City?.Trim() ?? "Unknown City");
                }

                // Валидация координат
                if (airport.Coordinates == null)
                {
                    throw new ArgumentException("Координаты обязательны.");
                }

                // Устанавливаем SRID для координат
                airport.Coordinates.SRID = 4326;

                context.Airports.Add(airport);
                await context.SaveChangesAsync();

                return airport;
            }
            catch (DbUpdateException dbEx)
            {
                var errorMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                throw new Exception($"Ошибка базы данных: {errorMessage}");
            }
        }

        /// <summary>
        /// Получение всех аэропортов (Read)
        /// </summary>
        public async Task<List<Airport>> GetAllAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            return await context.Airports
                .OrderBy(a => a.AirportCode)
                .ToListAsync();
        }

        /// <summary>
        /// Получение аэропорта по коду (Read)
        /// </summary>
        public async Task<Airport?> GetByCodeAsync(string airportCode)
        {
            if (string.IsNullOrEmpty(airportCode) || airportCode.Length != 3)
                return null;

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            return await context.Airports
                .FirstOrDefaultAsync(a => a.AirportCode == airportCode.ToUpper());
        }

        /// <summary>
        /// Обновление данных аэропорта (Update)
        /// </summary>
        public async Task<Airport?> UpdateAsync(Airport airport)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                var existing = await context.Airports
                    .FirstOrDefaultAsync(a => a.AirportCode == airport.AirportCode);

                if (existing == null)
                {
                    return null;
                }

                // Обновление JSON полей
                if (!string.IsNullOrWhiteSpace(airport.AirportName))
                {
                    existing.AirportName = Airport.IsValidJson(airport.AirportName)
                        ? airport.AirportName
                        : Airport.CreateLocalizedJson(airport.AirportName.Trim());
                }

                if (!string.IsNullOrWhiteSpace(airport.City))
                {
                    existing.City = Airport.IsValidJson(airport.City)
                        ? airport.City
                        : Airport.CreateLocalizedJson(airport.City.Trim());
                }

                // Обновление координат
                if (airport.Coordinates != null)
                {
                    airport.Coordinates.SRID = 4326;
                    existing.Coordinates = airport.Coordinates;
                }

                // Обновление часового пояса
                if (!string.IsNullOrWhiteSpace(airport.Timezone))
                {
                    existing.Timezone = airport.Timezone;
                }

                await context.SaveChangesAsync();
                return existing;
            }
            catch (DbUpdateException dbEx)
            {
                var errorMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                throw new Exception($"Ошибка базы данных: {errorMessage}");
            }
        }

        /// <summary>
        /// Удаление аэропорта (Delete)
        /// </summary>
        public async Task<bool> DeleteAsync(string airportCode)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var airport = await context.Airports
                .FirstOrDefaultAsync(a => a.AirportCode == airportCode);

            if (airport == null)
            {
                return false;
            }

            context.Airports.Remove(airport);
            await context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Проверка существования аэропорта по коду
        /// </summary>
        public async Task<bool> ExistsAsync(string airportCode)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            return await context.Airports
                .AnyAsync(a => a.AirportCode == airportCode.ToUpper());
        }

        /// <summary>
        /// Получение аэропортов с пагинацией
        /// </summary>
        public async Task<(List<Airport> airports, int totalCount)> GetPaginatedAsync(int page, int pageSize)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var query = context.Airports.AsQueryable();

            var totalCount = await query.CountAsync();

            var airports = await query
                .OrderBy(a => a.AirportCode)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (airports, totalCount);
        }

        /// <summary>
        /// Поиск аэропортов с пагинацией
        /// </summary>
        public async Task<(List<Airport> airports, int totalCount)> SearchPaginatedAsync(
            string searchTerm, int page, int pageSize)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var query = context.Airports.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                // Поиск только по коду аэропорта (точное или частичное совпадение)
                var code = searchTerm.ToUpper().Trim();
                query = query.Where(a => a.AirportCode.Contains(code));
            }

            var totalCount = await query.CountAsync();

            var airports = await query
                .OrderBy(a => a.AirportCode)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (airports, totalCount);
        }
    }
}