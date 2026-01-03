using AviationDB_CSharp.Data;
using AviationDB_CSharp.Models;
using Microsoft.EntityFrameworkCore;

namespace AviationDB_CSharp.Services
{
    public class AircraftService
    {
        // Изменен: используем фабрику DbContext вместо прямого экземпляра
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public AircraftService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<Aircraft> CreateAsync(Aircraft aircraft)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            try
            {
                // Валидация кода самолета
                if (string.IsNullOrEmpty(aircraft.AircraftCode) || aircraft.AircraftCode.Length != 3)
                {
                    throw new ArgumentException("Код самолета должен состоять ровно из 3 символов.");
                }

                // Проверка существования
                var existing = await context.Aircrafts
                    .FirstOrDefaultAsync(a => a.AircraftCode == aircraft.AircraftCode);

                if (existing != null)
                {
                    throw new ArgumentException($"Самолет с кодом {aircraft.AircraftCode} уже существует.");
                }

                // Валидация дальности
                if (aircraft.Range <= 0)
                {
                    throw new ArgumentException("Дальность должна быть больше 0.");
                }

                // Обработка модели
                if (string.IsNullOrWhiteSpace(aircraft.Model))
                {
                    aircraft.Model = Aircraft.CreateJsonFromText("Unknown");
                }
                else if (!Aircraft.IsValidJson(aircraft.Model))
                {
                    // Преобразуем текст в JSON
                    aircraft.Model = Aircraft.CreateJsonFromText(aircraft.Model.Trim());
                }

                context.Aircrafts.Add(aircraft);
                await context.SaveChangesAsync();

                return aircraft;
            }
            catch (DbUpdateException dbEx)
            {
                // Получаем более детальное сообщение об ошибке
                var errorMessage = dbEx.InnerException?.Message ?? dbEx.Message;

                if (errorMessage.Contains("aircrafts_range_check"))
                {
                    throw new ArgumentException("Дальность должна быть больше 0.");
                }
                else if (errorMessage.Contains("jsonb"))
                {
                    throw new ArgumentException("Некорректный формат JSON для модели самолета.");
                }

                throw new Exception($"Ошибка базы данных: {errorMessage}");
            }
        }

        // Изменен: Добавлены методы с пагинацией
        public async Task<(List<Aircraft> aircrafts, int totalCount)> GetPaginatedAsync(int page, int pageSize)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            var query = context.Aircrafts
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var aircrafts = await query
                .OrderBy(a => a.AircraftCode)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (aircrafts, totalCount);
        }

        public async Task<List<Aircraft>> GetAllAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            return await context.Aircrafts
                .OrderBy(a => a.AircraftCode)
                .ToListAsync();
        }

        public async Task<Aircraft?> GetByCodeAsync(string aircraftCode)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            if (string.IsNullOrEmpty(aircraftCode) || aircraftCode.Length != 3)
                return null;

            return await context.Aircrafts
                .FirstOrDefaultAsync(a => a.AircraftCode == aircraftCode.ToUpper());
        }

        public async Task<Aircraft?> UpdateAsync(Aircraft aircraft)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            try
            {
                var existing = await context.Aircrafts
                    .FirstOrDefaultAsync(a => a.AircraftCode == aircraft.AircraftCode);

                if (existing == null)
                {
                    return null;
                }

                // Валидация дальности
                if (aircraft.Range <= 0)
                {
                    throw new ArgumentException("Дальность должна быть больше 0.");
                }

                // Обработка модели
                if (!string.IsNullOrWhiteSpace(aircraft.Model))
                {
                    if (Aircraft.IsValidJson(aircraft.Model))
                    {
                        existing.Model = aircraft.Model;
                    }
                    else
                    {
                        existing.Model = Aircraft.CreateJsonFromText(aircraft.Model.Trim());
                    }
                }

                existing.Range = aircraft.Range;

                await context.SaveChangesAsync();
                return existing;
            }
            catch (DbUpdateException dbEx)
            {
                var errorMessage = dbEx.InnerException?.Message ?? dbEx.Message;

                if (errorMessage.Contains("aircrafts_range_check"))
                {
                    throw new ArgumentException("Дальность должна быть больше 0.");
                }

                throw new Exception($"Ошибка базы данных: {errorMessage}");
            }
        }

        public async Task<bool> DeleteAsync(string aircraftCode)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            var aircraft = await context.Aircrafts
                .FirstOrDefaultAsync(a => a.AircraftCode == aircraftCode);

            if (aircraft == null)
            {
                return false;
            }

            context.Aircrafts.Remove(aircraft);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(string aircraftCode)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            return await context.Aircrafts
                .AnyAsync(a => a.AircraftCode == aircraftCode.ToUpper());
        }

        // Изменен: Метод поиска только по aircraft_code
        public async Task<(List<Aircraft> aircrafts, int totalCount)> SearchPaginatedAsync(
            string searchTerm, int page, int pageSize)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            var query = context.Aircrafts
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var search = searchTerm.Trim().ToUpper(); // Изменен: приводим к верхнему регистру для поиска кода

                // Изменен: Поиск только по aircraft_code
                query = query.Where(a => a.AircraftCode.Contains(search));
            }

            var totalCount = await query.CountAsync();

            var aircrafts = await query
                .OrderBy(a => a.AircraftCode)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (aircrafts, totalCount);
        }

        // Изменен: Простой поиск только по aircraft_code
        public async Task<List<Aircraft>> SearchAsync(string searchTerm)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            var query = context.Aircrafts
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var search = searchTerm.Trim().ToUpper(); // Изменен: приводим к верхнему регистру
                query = query.Where(a => a.AircraftCode.Contains(search));
            }

            return await query
                .OrderBy(a => a.AircraftCode)
                .ToListAsync();
        }

        // Изменен/Добавлен: Получение количества записей
        public async Task<int> GetCountAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            return await context.Aircrafts.CountAsync();
        }

        // Изменен/Добавлен: Получение всех кодов самолетов
        public async Task<List<string>> GetAllAircraftCodesAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            return await context.Aircrafts
                .Select(a => a.AircraftCode)
                .OrderBy(code => code)
                .ToListAsync();
        }

        // Изменен/Добавлен: Поиск самолетов по точному совпадению кода
        public async Task<List<Aircraft>> SearchByExactCodeAsync(string aircraftCode)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            if (string.IsNullOrWhiteSpace(aircraftCode))
                return new List<Aircraft>();

            var search = aircraftCode.Trim().ToUpper();
            return await context.Aircrafts
                .Where(a => a.AircraftCode == search)
                .OrderBy(a => a.AircraftCode)
                .ToListAsync();
        }

        // Изменен/Добавлен: Поиск самолетов по части кода
        public async Task<List<Aircraft>> SearchByPartialCodeAsync(string partialCode)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            if (string.IsNullOrWhiteSpace(partialCode))
                return await GetAllAsync();

            var search = partialCode.Trim().ToUpper();
            return await context.Aircrafts
                .Where(a => a.AircraftCode.Contains(search))
                .OrderBy(a => a.AircraftCode)
                .ToListAsync();
        }
    }
}