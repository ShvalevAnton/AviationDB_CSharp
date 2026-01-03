using AviationDB_CSharp.Data;
using AviationDB_CSharp.Models;
using Microsoft.EntityFrameworkCore;

namespace AviationDB_CSharp.Services
{
    /// <summary>
    /// Сервис для выполнения CRUD операций с таблицей ticket_flights
    /// </summary>
    public class TicketFlightService
    {
        private readonly ApplicationDbContext _context;
        private readonly TicketService _ticketService;
        private readonly FlightService _flightService;

        public TicketFlightService(ApplicationDbContext context, TicketService ticketService, FlightService flightService)
        {
            _context = context;
            _ticketService = ticketService;
            _flightService = flightService;
        }

        /// <summary>
        /// Создание нового сегмента перелета (Create)
        /// </summary>
        public async Task<TicketFlight> CreateAsync(TicketFlight ticketFlight)
        {
            try
            {
                // Валидация номера билета
                if (string.IsNullOrEmpty(ticketFlight.TicketNo) || ticketFlight.TicketNo.Length != 13)
                {
                    throw new ArgumentException("Номер билета должен состоять ровно из 13 символов.");
                }

                // Валидация ID рейса
                if (ticketFlight.FlightId <= 0)
                {
                    throw new ArgumentException("ID рейса должен быть положительным числом.");
                }

                // Валидация класса обслуживания
                if (string.IsNullOrWhiteSpace(ticketFlight.FareConditions))
                {
                    throw new ArgumentException("Класс обслуживания обязателен.");
                }

                // Нормализация класса обслуживания
                ticketFlight.FareConditions = TicketFlight.NormalizeFareConditions(ticketFlight.FareConditions);

                // Проверка валидности класса обслуживания
                if (!TicketFlight.IsValidFareConditions(ticketFlight.FareConditions))
                {
                    throw new ArgumentException("Недопустимый класс обслуживания. Допустимые значения: Economy, Comfort, Business.");
                }

                // Валидация стоимости
                if (ticketFlight.Amount < 0)
                {
                    throw new ArgumentException("Стоимость не может быть отрицательной.");
                }

                // Проверка существования билета
                var ticketExists = await _ticketService.ExistsAsync(ticketFlight.TicketNo);
                if (!ticketExists)
                {
                    throw new ArgumentException($"Билет с номером {ticketFlight.TicketNo} не существует.");
                }

                // Проверка существования рейса
                var flightExists = await _flightService.ExistsAsync(ticketFlight.FlightId);
                if (!flightExists)
                {
                    throw new ArgumentException($"Рейс с ID {ticketFlight.FlightId} не существует.");
                }

                // Проверка уникальности сегмента (составной ключ)
                var existingTicketFlight = await _context.TicketFlights
                    .FirstOrDefaultAsync(tf => tf.TicketNo == ticketFlight.TicketNo && tf.FlightId == ticketFlight.FlightId);

                if (existingTicketFlight != null)
                {
                    throw new ArgumentException($"Сегмент перелета для билета {ticketFlight.TicketNo} и рейса {ticketFlight.FlightId} уже существует.");
                }

                _context.TicketFlights.Add(ticketFlight);
                await _context.SaveChangesAsync();

                return ticketFlight;
            }
            catch (DbUpdateException dbEx)
            {
                var errorMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                throw new Exception($"Ошибка базы данных: {errorMessage}");
            }
        }

        /// <summary>
        /// Получение всех сегментов перелетов (Read)
        /// </summary>
        public async Task<List<TicketFlight>> GetAllAsync()
        {
            return await _context.TicketFlights
                .Include(tf => tf.Ticket)
                .Include(tf => tf.Flight)
                .OrderBy(tf => tf.TicketNo)
                .ThenBy(tf => tf.FlightId)
                .ToListAsync();
        }

        /// <summary>
        /// Получение сегмента по составному ключу
        /// </summary>
        public async Task<TicketFlight?> GetByKeyAsync(string ticketNo, int flightId)
        {
            if (string.IsNullOrEmpty(ticketNo) || ticketNo.Length != 13 || flightId <= 0)
                return null;

            return await _context.TicketFlights
                .Include(tf => tf.Ticket)
                .Include(tf => tf.Flight)
                .FirstOrDefaultAsync(tf => tf.TicketNo == ticketNo && tf.FlightId == flightId);
        }

        /// <summary>
        /// Получение сегментов по номеру билета
        /// </summary>
        public async Task<List<TicketFlight>> GetByTicketNoAsync(string ticketNo)
        {
            if (string.IsNullOrEmpty(ticketNo) || ticketNo.Length != 13)
                return new List<TicketFlight>();

            return await _context.TicketFlights
                .Include(tf => tf.Ticket)
                .Include(tf => tf.Flight)
                .Where(tf => tf.TicketNo == ticketNo)
                .OrderBy(tf => tf.FlightId)
                .ToListAsync();
        }

        /// <summary>
        /// Получение сегментов по ID рейса
        /// </summary>
        public async Task<List<TicketFlight>> GetByFlightIdAsync(int flightId)
        {
            if (flightId <= 0)
                return new List<TicketFlight>();

            return await _context.TicketFlights
                .Include(tf => tf.Ticket)
                .Include(tf => tf.Flight)
                .Where(tf => tf.FlightId == flightId)
                .OrderBy(tf => tf.TicketNo)
                .ToListAsync();
        }

        /// <summary>
        /// Получение сегментов по классу обслуживания
        /// </summary>
        public async Task<List<TicketFlight>> GetByFareConditionsAsync(string fareConditions)
        {
            if (string.IsNullOrWhiteSpace(fareConditions))
                return new List<TicketFlight>();

            var normalizedConditions = TicketFlight.NormalizeFareConditions(fareConditions);

            return await _context.TicketFlights
                .Include(tf => tf.Ticket)
                .Include(tf => tf.Flight)
                .Where(tf => tf.FareConditions == normalizedConditions)
                .OrderBy(tf => tf.TicketNo)
                .ThenBy(tf => tf.FlightId)
                .ToListAsync();
        }

        /// <summary>
        /// Обновление данных сегмента (Update)
        /// </summary>
        public async Task<TicketFlight?> UpdateAsync(string ticketNo, int flightId, TicketFlight ticketFlight)
        {
            try
            {
                var existing = await _context.TicketFlights
                    .FirstOrDefaultAsync(tf => tf.TicketNo == ticketNo && tf.FlightId == flightId);

                if (existing == null)
                {
                    return null;
                }

                // Обновление класса обслуживания
                if (!string.IsNullOrWhiteSpace(ticketFlight.FareConditions))
                {
                    var normalizedConditions = TicketFlight.NormalizeFareConditions(ticketFlight.FareConditions);

                    if (!TicketFlight.IsValidFareConditions(normalizedConditions))
                    {
                        throw new ArgumentException("Недопустимый класс обслуживания. Допустимые значения: Economy, Comfort, Business.");
                    }

                    existing.FareConditions = normalizedConditions;
                }

                // Обновление стоимости
                if (ticketFlight.Amount >= 0)
                {
                    existing.Amount = ticketFlight.Amount;
                }

                await _context.SaveChangesAsync();
                return existing;
            }
            catch (DbUpdateException dbEx)
            {
                var errorMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                throw new Exception($"Ошибка базы данных: {errorMessage}");
            }
        }

        /// <summary>
        /// Удаление сегмента (Delete)
        /// </summary>
        public async Task<bool> DeleteAsync(string ticketNo, int flightId)
        {
            var ticketFlight = await _context.TicketFlights
                .FirstOrDefaultAsync(tf => tf.TicketNo == ticketNo && tf.FlightId == flightId);

            if (ticketFlight == null)
            {
                return false;
            }

            _context.TicketFlights.Remove(ticketFlight);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Проверка существования сегмента
        /// </summary>
        public async Task<bool> ExistsAsync(string ticketNo, int flightId)
        {
            return await _context.TicketFlights
                .AnyAsync(tf => tf.TicketNo == ticketNo && tf.FlightId == flightId);
        }

        /// <summary>
        /// Получение общей стоимости всех сегментов по билету
        /// </summary>
        public async Task<decimal> GetTotalAmountByTicketAsync(string ticketNo)
        {
            if (string.IsNullOrEmpty(ticketNo) || ticketNo.Length != 13)
                return 0;

            return await _context.TicketFlights
                .Where(tf => tf.TicketNo == ticketNo)
                .SumAsync(tf => tf.Amount);
        }

        /// <summary>
        /// Получение общей стоимости всех сегментов по рейсу
        /// </summary>
        public async Task<decimal> GetTotalAmountByFlightAsync(int flightId)
        {
            if (flightId <= 0)
                return 0;

            return await _context.TicketFlights
                .Where(tf => tf.FlightId == flightId)
                .SumAsync(tf => tf.Amount);
        }

        /// <summary>
        /// Получение статистики по классам обслуживания для рейса
        /// </summary>
        public async Task<Dictionary<string, int>> GetFlightStatisticsAsync(int flightId)
        {
            if (flightId <= 0)
                return new Dictionary<string, int>();

            var ticketFlights = await GetByFlightIdAsync(flightId);

            return ticketFlights
                .GroupBy(tf => tf.FareConditions)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        /// <summary>
        /// Получение статистики по классам обслуживания для билета
        /// </summary>
        public async Task<Dictionary<string, decimal>> GetTicketAmountStatisticsAsync(string ticketNo)
        {
            if (string.IsNullOrEmpty(ticketNo) || ticketNo.Length != 13)
                return new Dictionary<string, decimal>();

            var ticketFlights = await GetByTicketNoAsync(ticketNo);

            return ticketFlights
                .GroupBy(tf => tf.FareConditions)
                .ToDictionary(g => g.Key, g => g.Sum(tf => tf.Amount));
        }

        /// <summary>
        /// Получение сегментов с фильтрацией по стоимости
        /// </summary>
        public async Task<List<TicketFlight>> GetByAmountRangeAsync(decimal minAmount, decimal maxAmount)
        {
            return await _context.TicketFlights
                .Include(tf => tf.Ticket)
                .Include(tf => tf.Flight)
                .Where(tf => tf.Amount >= minAmount && tf.Amount <= maxAmount)
                .OrderBy(tf => tf.Amount)
                .ToListAsync();
        }
    }
}