using AviationDB_CSharp.Data;
using AviationDB_CSharp.Models;
using Microsoft.EntityFrameworkCore;

namespace AviationDB_CSharp.Services
{
    /// <summary>
    /// Сервис для выполнения CRUD операций с таблицей seats
    /// </summary>
    public class SeatService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory; // Изменен: используем фабрику DbContext

        public SeatService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Создание нового места (Create)
        /// </summary>
        public async Task<Seat> CreateAsync(Seat seat)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            try
            {
                // Валидация кода самолета
                if (string.IsNullOrEmpty(seat.AircraftCode) || seat.AircraftCode.Length != 3)
                {
                    throw new ArgumentException("Код самолета должен состоять ровно из 3 символов.");
                }

                // Валидация номера места
                if (string.IsNullOrEmpty(seat.SeatNo) || seat.SeatNo.Length > 4)
                {
                    throw new ArgumentException("Номер места должен быть не более 4 символов.");
                }

                // Валидация класса обслуживания
                if (string.IsNullOrWhiteSpace(seat.FareConditions))
                {
                    throw new ArgumentException("Класс обслуживания обязателен.");
                }

                // Нормализация класса обслуживания
                seat.FareConditions = Seat.NormalizeFareConditions(seat.FareConditions);

                // Проверка валидности класса обслуживания
                if (!Seat.IsValidFareConditions(seat.FareConditions))
                {
                    throw new ArgumentException("Недопустимый класс обслуживания. Допустимые значения: Economy, Comfort, Business.");
                }

                // Проверка существования самолета
                var aircraftExists = await context.Aircrafts
                    .AnyAsync(a => a.AircraftCode == seat.AircraftCode);

                if (!aircraftExists)
                {
                    throw new ArgumentException($"Самолет с кодом {seat.AircraftCode} не существует.");
                }

                // Проверка уникальности места (составной ключ)
                var existingSeat = await context.Seats
                    .FirstOrDefaultAsync(s => s.AircraftCode == seat.AircraftCode && s.SeatNo == seat.SeatNo);

                if (existingSeat != null)
                {
                    throw new ArgumentException($"Место {seat.SeatNo} в самолете {seat.AircraftCode} уже существует.");
                }

                context.Seats.Add(seat);
                await context.SaveChangesAsync();

                // Загружаем связанные данные
                await context.Entry(seat)
                    .Reference(s => s.Aircraft)
                    .LoadAsync();

                return seat;
            }
            catch (DbUpdateException dbEx)
            {
                var errorMessage = dbEx.InnerException?.Message ?? dbEx.Message;

                if (errorMessage.Contains("seats_pkey"))
                {
                    throw new ArgumentException("Нарушение первичного ключа: место с таким номером уже существует.");
                }
                else if (errorMessage.Contains("seats_aircraft_code_fkey"))
                {
                    throw new ArgumentException("Нарушение внешнего ключа: самолет с таким кодом не существует.");
                }
                else if (errorMessage.Contains("seats_check"))
                {
                    throw new ArgumentException("Нарушение проверочного ограничения: некорректный класс обслуживания.");
                }

                throw new Exception($"Ошибка базы данных: {errorMessage}");
            }
        }

        /// <summary>
        /// Получение всех мест с пагинацией (Изменен/Добавлен)
        /// </summary>
        public async Task<(List<Seat> seats, int totalCount)> GetPaginatedAsync(int page, int pageSize)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            var query = context.Seats
                .Include(s => s.Aircraft)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var seats = await query
                .OrderBy(s => s.AircraftCode)
                .ThenBy(s => s.SeatNo)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (seats, totalCount);
        }

        /// <summary>
        /// Получение места по составному ключу (Изменен)
        /// </summary>
        public async Task<Seat?> GetByKeyAsync(string aircraftCode, string seatNo)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            if (string.IsNullOrEmpty(aircraftCode) || aircraftCode.Length != 3 ||
                string.IsNullOrEmpty(seatNo))
                return null;

            return await context.Seats
                .Include(s => s.Aircraft)
                .FirstOrDefaultAsync(s => s.AircraftCode == aircraftCode.ToUpper() && s.SeatNo == seatNo);
        }

        /// <summary>
        /// Получение мест по коду самолета с пагинацией (Изменен/Добавлен)
        /// </summary>
        public async Task<(List<Seat> seats, int totalCount)> GetByAircraftCodePaginatedAsync(
            string aircraftCode, int page, int pageSize)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            if (string.IsNullOrEmpty(aircraftCode) || aircraftCode.Length != 3)
                return (new List<Seat>(), 0);

            var query = context.Seats
                .Include(s => s.Aircraft)
                .Where(s => s.AircraftCode == aircraftCode.ToUpper())
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var seats = await query
                .OrderBy(s => s.SeatNo)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (seats, totalCount);
        }

        /// <summary>
        /// Обновление данных места (Update) (Изменен)
        /// </summary>
        public async Task<Seat?> UpdateAsync(string aircraftCode, string seatNo, Seat seat)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            try
            {
                var existing = await context.Seats
                    .Include(s => s.Aircraft)
                    .FirstOrDefaultAsync(s => s.AircraftCode == aircraftCode && s.SeatNo == seatNo);

                if (existing == null)
                {
                    return null;
                }

                // Валидация класса обслуживания
                if (!string.IsNullOrWhiteSpace(seat.FareConditions))
                {
                    var normalizedConditions = Seat.NormalizeFareConditions(seat.FareConditions);

                    if (!Seat.IsValidFareConditions(normalizedConditions))
                    {
                        throw new ArgumentException("Недопустимый класс обслуживания. Допустимые значения: Economy, Comfort, Business.");
                    }

                    existing.FareConditions = normalizedConditions;
                }

                await context.SaveChangesAsync();
                return existing;
            }
            catch (DbUpdateException dbEx)
            {
                var errorMessage = dbEx.InnerException?.Message ?? dbEx.Message;

                if (errorMessage.Contains("seats_check"))
                {
                    throw new ArgumentException("Нарушение проверочного ограничения: некорректный класс обслуживания.");
                }

                throw new Exception($"Ошибка базы данных: {errorMessage}");
            }
        }

        /// <summary>
        /// Удаление места (Delete) (Изменен)
        /// </summary>
        public async Task<bool> DeleteAsync(string aircraftCode, string seatNo)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            var seat = await context.Seats
                .FirstOrDefaultAsync(s => s.AircraftCode == aircraftCode && s.SeatNo == seatNo);

            if (seat == null)
            {
                return false;
            }

            context.Seats.Remove(seat);
            await context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Поиск мест с пагинацией (Изменен/Добавлен)
        /// </summary>
        public async Task<(List<Seat> seats, int totalCount)> SearchPaginatedAsync(
            string searchTerm, int page, int pageSize)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            var query = context.Seats
                .Include(s => s.Aircraft)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var search = searchTerm.ToUpper().Trim();
                query = query.Where(s =>
                    s.AircraftCode.Contains(search) ||
                    s.SeatNo.Contains(search) ||
                    s.FareConditions.Contains(search));
            }

            var totalCount = await query.CountAsync();

            var seats = await query
                .OrderBy(s => s.AircraftCode)
                .ThenBy(s => s.SeatNo)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (seats, totalCount);
        }

        /// <summary>
        /// Получение мест по классу обслуживания с пагинацией (Изменен/Добавлен)
        /// </summary>
        public async Task<(List<Seat> seats, int totalCount)> GetByFareConditionsPaginatedAsync(
            string fareConditions, int page, int pageSize)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            var normalizedConditions = Seat.NormalizeFareConditions(fareConditions);

            var query = context.Seats
                .Include(s => s.Aircraft)
                .Where(s => s.FareConditions == normalizedConditions)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var seats = await query
                .OrderBy(s => s.AircraftCode)
                .ThenBy(s => s.SeatNo)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (seats, totalCount);
        }

        /// <summary>
        /// Проверка существования места (Изменен)
        /// </summary>
        public async Task<bool> ExistsAsync(string aircraftCode, string seatNo)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            return await context.Seats
                .AnyAsync(s => s.AircraftCode == aircraftCode && s.SeatNo == seatNo);
        }

        /// <summary>
        /// Получение статистики мест по самолету (Изменен)
        /// </summary>
        public async Task<Dictionary<string, int>> GetSeatStatisticsByAircraftAsync(string aircraftCode)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            var seats = await context.Seats
                .Where(s => s.AircraftCode == aircraftCode)
                .ToListAsync();

            return seats
                .GroupBy(s => s.FareConditions)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        /// <summary>
        /// Получение всех самолетов для фильтрации (Изменен/Добавлен)
        /// </summary>
        public async Task<List<string>> GetAllAircraftCodesAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            return await context.Aircrafts
                .Select(a => a.AircraftCode)
                .OrderBy(code => code)
                .ToListAsync();
        }

        /// <summary>
        /// Получение всех классов обслуживания для фильтрации (Изменен/Добавлен)
        /// </summary>
        public async Task<List<string>> GetAllFareConditionsAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            return await context.Seats
                .Select(s => s.FareConditions)
                .Distinct()
                .OrderBy(condition => condition)
                .ToListAsync();
        }
    }
}