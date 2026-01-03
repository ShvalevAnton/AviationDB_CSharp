using AviationDB_CSharp.Data;
using AviationDB_CSharp.Models;
using Microsoft.EntityFrameworkCore;

namespace AviationDB_CSharp.Services
{
    /// <summary>
    /// Сервис для выполнения CRUD операций с таблицей flights
    /// </summary>
    public class FlightService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly AircraftService _aircraftService;
        private readonly AirportService _airportService;

        public FlightService(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            AircraftService aircraftService,
            AirportService airportService)
        {
            _contextFactory = contextFactory;
            _aircraftService = aircraftService;
            _airportService = airportService;
        }

        /// <summary>
        /// Создание нового рейса (Create) с пагинацией
        /// </summary>
        public async Task<Flight> CreateAsync(Flight flight)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            try
            {
                // Валидация номера рейса
                if (string.IsNullOrEmpty(flight.FlightNo) || flight.FlightNo.Length != 6)
                {
                    throw new ArgumentException("Номер рейса должен состоять ровно из 6 символов.");
                }

                // Валидация статуса
                if (!flight.IsValidStatus())
                {
                    throw new ArgumentException($"Некорректный статус рейса. Допустимые значения: 'Scheduled', 'On Time', 'Delayed', 'Departed', 'Arrived', 'Cancelled'.");
                }

                // Валидация временных ограничений
                if (!flight.IsValidSchedule())
                {
                    throw new ArgumentException("Время прилета должно быть позже времени вылета.");
                }

                // Проверка, что аэропорты разные
                if (!flight.AreAirportsDifferent())
                {
                    throw new ArgumentException("Аэропорт вылета и прилета не могут быть одинаковыми.");
                }

                // Проверка существования самолета
                var aircraft = await _aircraftService.GetByCodeAsync(flight.AircraftCode);
                if (aircraft == null)
                {
                    throw new ArgumentException($"Самолет с кодом {flight.AircraftCode} не существует.");
                }

                // Проверка существования аэропорта вылета
                var departureAirport = await _airportService.GetByCodeAsync(flight.DepartureAirport);
                if (departureAirport == null)
                {
                    throw new ArgumentException($"Аэропорт вылета {flight.DepartureAirport} не существует.");
                }

                // Проверка существования аэропорта прилета
                var arrivalAirport = await _airportService.GetByCodeAsync(flight.ArrivalAirport);
                if (arrivalAirport == null)
                {
                    throw new ArgumentException($"Аэропорт прилета {flight.ArrivalAirport} не существует.");
                }

                // Проверка уникальности комбинации flight_no + scheduled_departure
                var existingFlight = await context.Flights
                    .FirstOrDefaultAsync(f => f.FlightNo == flight.FlightNo &&
                                              f.ScheduledDeparture == flight.ScheduledDeparture);

                if (existingFlight != null)
                {
                    throw new ArgumentException($"Рейс {flight.FlightNo} на {flight.ScheduledDeparture:yyyy-MM-dd HH:mm} уже существует.");
                }

                // ЯВНО конвертируем даты в UTC
                var scheduledDepartureUtc = flight.ScheduledDeparture.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(flight.ScheduledDeparture, DateTimeKind.Utc)
                    : flight.ScheduledDeparture.ToUniversalTime();

                var scheduledArrivalUtc = flight.ScheduledArrival.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(flight.ScheduledArrival, DateTimeKind.Utc)
                    : flight.ScheduledArrival.ToUniversalTime();

                flight.ScheduledDeparture = scheduledDepartureUtc;
                flight.ScheduledArrival = scheduledArrivalUtc;

                context.Flights.Add(flight);
                await context.SaveChangesAsync();

                // Загружаем связанные данные
                await context.Entry(flight)
                    .Reference(f => f.Aircraft)
                    .LoadAsync();
                await context.Entry(flight)
                    .Reference(f => f.DepartureAirportInfo)
                    .LoadAsync();
                await context.Entry(flight)
                    .Reference(f => f.ArrivalAirportInfo)
                    .LoadAsync();

                return flight;
            }
            catch (DbUpdateException dbEx)
            {
                var errorMessage = dbEx.InnerException?.Message ?? dbEx.Message;

                if (errorMessage.Contains("flights_pkey"))
                {
                    throw new ArgumentException("Нарушение первичного ключа.");
                }
                else if (errorMessage.Contains("flights_flight_no_scheduled_departure_key"))
                {
                    throw new ArgumentException("Нарушение уникальности: комбинация номера рейса и времени вылета должна быть уникальной.");
                }
                else if (errorMessage.Contains("flights_aircraft_code_fkey"))
                {
                    throw new ArgumentException("Нарушение внешнего ключа: самолет с таким кодом не существует.");
                }
                else if (errorMessage.Contains("flights_arrival_airport_fkey"))
                {
                    throw new ArgumentException("Нарушение внешнего ключа: аэропорт прилета не существует.");
                }
                else if (errorMessage.Contains("flights_departure_airport_fkey"))
                {
                    throw new ArgumentException("Нарушение внешнего ключа: аэропорт вылета не существует.");
                }
                else if (errorMessage.Contains("flights_check"))
                {
                    throw new ArgumentException("Нарушение проверочного ограничения: время прилета должно быть позже времени вылета.");
                }
                else if (errorMessage.Contains("flights_status_check"))
                {
                    throw new ArgumentException("Нарушение проверочного ограничения: некорректный статус рейса.");
                }

                throw new Exception($"Ошибка базы данных: {errorMessage}");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Cannot write DateTime with Kind=Unspecified"))
                {
                    throw new ArgumentException("Ошибка формата даты. Используется конвертер DateTimeToUtcConverter.");
                }
                throw;
            }
        }

        /// <summary>
        /// Получение всех рейсов (Read) с пагинацией (Изменен/Добавлен)
        /// </summary>
        public async Task<(List<Flight> flights, int totalCount)> GetPaginatedAsync(int page, int pageSize)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            var query = context.Flights
                .Include(f => f.Aircraft)
                .Include(f => f.DepartureAirportInfo)
                .Include(f => f.ArrivalAirportInfo)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var flights = await query
                .OrderByDescending(f => f.ScheduledDeparture)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (flights, totalCount);
        }

        /// <summary>
        /// Получение рейса по ID (Read)
        /// </summary>
        public async Task<Flight?> GetByIdAsync(int flightId)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            return await context.Flights
                .Include(f => f.Aircraft)
                .Include(f => f.DepartureAirportInfo)
                .Include(f => f.ArrivalAirportInfo)
                .FirstOrDefaultAsync(f => f.FlightId == flightId);
        }

        /// <summary>
        /// Получение рейса по номеру и дате вылета (Read)
        /// </summary>
        public async Task<Flight?> GetByFlightNoAndDateAsync(string flightNo, DateTime scheduledDeparture)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            return await context.Flights
                .Include(f => f.Aircraft)
                .Include(f => f.DepartureAirportInfo)
                .Include(f => f.ArrivalAirportInfo)
                .FirstOrDefaultAsync(f => f.FlightNo == flightNo &&
                                         f.ScheduledDeparture.Date == scheduledDeparture.Date);
        }

        /// <summary>
        /// Обновление данных рейса (Update)
        /// </summary>
        public async Task<Flight?> UpdateAsync(Flight flight)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            try
            {
                var existing = await context.Flights
                    .Include(f => f.Aircraft)
                    .Include(f => f.DepartureAirportInfo)
                    .Include(f => f.ArrivalAirportInfo)
                    .FirstOrDefaultAsync(f => f.FlightId == flight.FlightId);

                if (existing == null)
                {
                    return null;
                }

                // Валидация статуса
                if (!string.IsNullOrWhiteSpace(flight.Status) && !flight.IsValidStatus())
                {
                    throw new ArgumentException($"Некорректный статус рейса. Допустимые значения: 'Scheduled', 'On Time', 'Delayed', 'Departed', 'Arrived', 'Cancelled'.");
                }

                // Обновление полей (только разрешенные для изменения)
                if (!string.IsNullOrWhiteSpace(flight.Status))
                {
                    existing.Status = flight.Status;
                }

                if (flight.ActualDeparture.HasValue)
                {
                    existing.ActualDeparture = flight.ActualDeparture.Value.ToUniversalTime();
                }

                if (flight.ActualArrival.HasValue)
                {
                    existing.ActualArrival = flight.ActualArrival.Value.ToUniversalTime();
                }

                // Если установлены фактические времена, автоматически обновляем статус
                if (flight.ActualDeparture.HasValue || flight.ActualArrival.HasValue)
                {
                    existing.UpdateStatusBasedOnTime();
                }

                // Валидация временных ограничений после обновления
                if (!existing.IsValidSchedule())
                {
                    throw new ArgumentException("Время прилета должно быть позже времени вылета.");
                }

                await context.SaveChangesAsync();
                return existing;
            }
            catch (DbUpdateException dbEx)
            {
                var errorMessage = dbEx.InnerException?.Message ?? dbEx.Message;

                if (errorMessage.Contains("flights_check"))
                {
                    throw new ArgumentException("Нарушение проверочного ограничения: время прилета должно быть позже времени вылета.");
                }
                else if (errorMessage.Contains("flights_status_check"))
                {
                    throw new ArgumentException("Нарушение проверочного ограничения: некорректный статус рейса.");
                }

                throw new Exception($"Ошибка базы данных: {errorMessage}");
            }
        }

        /// <summary>
        /// Удаление рейса (Delete)
        /// </summary>
        public async Task<bool> DeleteAsync(int flightId)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            var flight = await context.Flights
                .FirstOrDefaultAsync(f => f.FlightId == flightId);

            if (flight == null)
            {
                return false;
            }

            // Нельзя удалить рейсы, которые уже вылетели или имеют связанные данные
            if (flight.IsCompleted())
            {
                throw new InvalidOperationException($"Невозможно удалить рейс {flight.FlightNo} со статусом {flight.Status}.");
            }

            context.Flights.Remove(flight);
            await context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Поиск рейсов с пагинацией (Изменен/Добавлен)
        /// </summary>
        public async Task<(List<Flight> flights, int totalCount)> SearchPaginatedAsync(
            string searchTerm, int page, int pageSize)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            var query = context.Flights
                .Include(f => f.Aircraft)
                .Include(f => f.DepartureAirportInfo)
                .Include(f => f.ArrivalAirportInfo)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var search = searchTerm.ToUpper().Trim();
                query = query.Where(f =>
                    f.FlightNo.Contains(search) ||
                    f.DepartureAirport.Contains(search) ||
                    f.ArrivalAirport.Contains(search) ||
                    f.Status.Contains(search) ||
                    f.AircraftCode.Contains(search));
            }

            var totalCount = await query.CountAsync();

            var flights = await query
                .OrderByDescending(f => f.ScheduledDeparture)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (flights, totalCount);
        }

        /// <summary>
        /// Поиск рейсов по аэропорту вылета с пагинацией (Изменен/Добавлен)
        /// </summary>
        public async Task<(List<Flight> flights, int totalCount)> FindByDepartureAirportPaginatedAsync(
            string airportCode, int page, int pageSize)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            var query = context.Flights
                .Include(f => f.Aircraft)
                .Include(f => f.DepartureAirportInfo)
                .Include(f => f.ArrivalAirportInfo)
                .Where(f => f.DepartureAirport == airportCode.ToUpper())
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var flights = await query
                .OrderBy(f => f.ScheduledDeparture)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (flights, totalCount);
        }

        /// <summary>
        /// Поиск рейсов по аэропорту прилета с пагинацией (Изменен/Добавлен)
        /// </summary>
        public async Task<(List<Flight> flights, int totalCount)> FindByArrivalAirportPaginatedAsync(
            string airportCode, int page, int pageSize)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            var query = context.Flights
                .Include(f => f.Aircraft)
                .Include(f => f.DepartureAirportInfo)
                .Include(f => f.ArrivalAirportInfo)
                .Where(f => f.ArrivalAirport == airportCode.ToUpper())
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var flights = await query
                .OrderBy(f => f.ScheduledArrival)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (flights, totalCount);
        }

        /// <summary>
        /// Поиск рейсов по дате вылета с пагинацией (Изменен/Добавлен)
        /// </summary>
        public async Task<(List<Flight> flights, int totalCount)> FindByDepartureDatePaginatedAsync(
            DateTime date, int page, int pageSize)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            var startDate = date.Date.ToUniversalTime();
            var endDate = date.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

            var query = context.Flights
                .Include(f => f.Aircraft)
                .Include(f => f.DepartureAirportInfo)
                .Include(f => f.ArrivalAirportInfo)
                .Where(f => f.ScheduledDeparture >= startDate && f.ScheduledDeparture <= endDate)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var flights = await query
                .OrderBy(f => f.ScheduledDeparture)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (flights, totalCount);
        }

        /// <summary>
        /// Поиск рейсов по статусу с пагинацией (Изменен/Добавлен)
        /// </summary>
        public async Task<(List<Flight> flights, int totalCount)> FindByStatusPaginatedAsync(
            string status, int page, int pageSize)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            var query = context.Flights
                .Include(f => f.Aircraft)
                .Include(f => f.DepartureAirportInfo)
                .Include(f => f.ArrivalAirportInfo)
                .Where(f => f.Status == status)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var flights = await query
                .OrderBy(f => f.ScheduledDeparture)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (flights, totalCount);
        }

        /// <summary>
        /// Проверка существования рейса
        /// </summary>
        public async Task<bool> ExistsAsync(int flightId)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            return await context.Flights
                .AnyAsync(f => f.FlightId == flightId);
        }

        /// <summary>
        /// Обновление статусов рейсов на основе текущего времени
        /// </summary>
        public async Task UpdateAllStatusesAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            var flights = await context.Flights
                .Where(f => f.Status != "Cancelled" && f.Status != "Arrived")
                .ToListAsync();

            foreach (var flight in flights)
            {
                flight.UpdateStatusBasedOnTime();
            }

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Получение статистики рейсов
        /// </summary>
        public async Task<FlightStatistics> GetStatisticsAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            var flights = await context.Flights.ToListAsync();

            if (flights.Count == 0)
            {
                return new FlightStatistics();
            }

            return new FlightStatistics
            {
                TotalFlights = flights.Count,
                ScheduledFlights = flights.Count(f => f.Status == "Scheduled"),
                OnTimeFlights = flights.Count(f => f.Status == "On Time"),
                DelayedFlights = flights.Count(f => f.Status == "Delayed"),
                DepartedFlights = flights.Count(f => f.Status == "Departed"),
                ArrivedFlights = flights.Count(f => f.Status == "Arrived"),
                CancelledFlights = flights.Count(f => f.Status == "Cancelled"),
                EarliestDeparture = flights.Min(f => f.ScheduledDeparture),
                LatestDeparture = flights.Max(f => f.ScheduledDeparture)
            };
        }
    }

    /// <summary>
    /// Класс для статистики рейсов
    /// </summary>
    public class FlightStatistics
    {
        public int TotalFlights { get; set; }
        public int ScheduledFlights { get; set; }
        public int OnTimeFlights { get; set; }
        public int DelayedFlights { get; set; }
        public int DepartedFlights { get; set; }
        public int ArrivedFlights { get; set; }
        public int CancelledFlights { get; set; }
        public DateTime EarliestDeparture { get; set; }
        public DateTime LatestDeparture { get; set; }

        public FlightStatistics()
        {
            TotalFlights = 0;
            ScheduledFlights = 0;
            OnTimeFlights = 0;
            DelayedFlights = 0;
            DepartedFlights = 0;
            ArrivedFlights = 0;
            CancelledFlights = 0;
            EarliestDeparture = DateTime.MinValue;
            LatestDeparture = DateTime.MinValue;
        }
    }
}