using AviationDB_CSharp.Data;
using AviationDB_CSharp.Models;
using Microsoft.EntityFrameworkCore;

namespace AviationDB_CSharp.Services
{
    /// <summary>
    /// Сервис для выполнения CRUD операций с таблицей boarding_passes
    /// </summary>
    public class BoardingPassService
    {
        private readonly ApplicationDbContext _context;

        public BoardingPassService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Создание нового посадочного талона (Create)
        /// </summary>
        public async Task<BoardingPass> CreateAsync(BoardingPass boardingPass)
        {
            try
            {
                // Валидация номера билета
                if (string.IsNullOrEmpty(boardingPass.TicketNo) || boardingPass.TicketNo.Length != 13)
                {
                    throw new ArgumentException("Номер билета должен состоять ровно из 13 символов.");
                }

                // Валидация ID рейса
                if (boardingPass.FlightId <= 0)
                {
                    throw new ArgumentException("ID рейса должен быть положительным числом.");
                }

                // Валидация номера посадочного талона
                if (boardingPass.BoardingNo <= 0)
                {
                    throw new ArgumentException("Номер посадочного талона должен быть положительным числом.");
                }

                // Валидация номера места
                if (string.IsNullOrWhiteSpace(boardingPass.SeatNo) || boardingPass.SeatNo.Length > 4)
                {
                    throw new ArgumentException("Номер места должен содержать от 1 до 4 символов.");
                }

                if (!boardingPass.IsValidSeatNo())
                {
                    throw new ArgumentException("Некорректный формат номера места. Пример: '12A', '1C', '25F'.");
                }

                // Проверка уникальности составного первичного ключа
                var existing = await _context.BoardingPasses
                    .FirstOrDefaultAsync(bp => bp.TicketNo == boardingPass.TicketNo &&
                                               bp.FlightId == boardingPass.FlightId);

                if (existing != null)
                {
                    throw new ArgumentException($"Посадочный талон для билета {boardingPass.TicketNo} и рейса {boardingPass.FlightId} уже существует.");
                }

                // Проверка уникальности boarding_no в пределах рейса
                var boardingNoExists = await _context.BoardingPasses
                    .AnyAsync(bp => bp.FlightId == boardingPass.FlightId &&
                                    bp.BoardingNo == boardingPass.BoardingNo);

                if (boardingNoExists)
                {
                    throw new ArgumentException($"Номер посадочного талона {boardingPass.BoardingNo} уже используется для рейса {boardingPass.FlightId}.");
                }

                // Проверка уникальности seat_no в пределах рейса
                var seatNoExists = await _context.BoardingPasses
                    .AnyAsync(bp => bp.FlightId == boardingPass.FlightId &&
                                    bp.SeatNo == boardingPass.SeatNo);

                if (seatNoExists)
                {
                    throw new ArgumentException($"Место {boardingPass.SeatNo} уже занято для рейса {boardingPass.FlightId}.");
                }

                _context.BoardingPasses.Add(boardingPass);
                await _context.SaveChangesAsync();

                return boardingPass;
            }
            catch (DbUpdateException dbEx)
            {
                var errorMessage = dbEx.InnerException?.Message ?? dbEx.Message;

                // Проверяем нарушение уникальных ограничений
                if (errorMessage.Contains("boarding_passes_pkey"))
                {
                    throw new ArgumentException("Нарушение первичного ключа: билет с таким номером уже существует для этого рейса.");
                }
                else if (errorMessage.Contains("boarding_passes_flight_id_boarding_no_key"))
                {
                    throw new ArgumentException("Нарушение уникальности: номер посадочного талона должен быть уникальным в пределах рейса.");
                }
                else if (errorMessage.Contains("boarding_passes_flight_id_seat_no_key"))
                {
                    throw new ArgumentException("Нарушение уникальности: номер места должен быть уникальным в пределах рейса.");
                }
                else if (errorMessage.Contains("boarding_passes_ticket_no_fkey"))
                {
                    throw new ArgumentException("Нарушение внешнего ключа: билет с таким номером не существует в таблице ticket_flights.");
                }

                throw new Exception($"Ошибка базы данных: {errorMessage}");
            }
        }

        /// <summary>
        /// Получение всех посадочных талонов (Read)
        /// </summary>
        public async Task<List<BoardingPass>> GetAllAsync()
        {
            return await _context.BoardingPasses
                .OrderBy(bp => bp.FlightId)
                .ThenBy(bp => bp.BoardingNo)
                .ToListAsync();
        }

        /// <summary>
        /// Получение посадочного талона по составному ключу (Read)
        /// </summary>
        public async Task<BoardingPass?> GetByKeyAsync(string ticketNo, int flightId)
        {
            if (string.IsNullOrEmpty(ticketNo) || ticketNo.Length != 13 || flightId <= 0)
                return null;

            return await _context.BoardingPasses
                .FirstOrDefaultAsync(bp => bp.TicketNo == ticketNo && bp.FlightId == flightId);
        }

        /// <summary>
        /// Обновление данных посадочного талона (Update)
        /// </summary>
        public async Task<BoardingPass?> UpdateAsync(BoardingPass boardingPass)
        {
            try
            {
                var existing = await _context.BoardingPasses
                    .FirstOrDefaultAsync(bp => bp.TicketNo == boardingPass.TicketNo &&
                                               bp.FlightId == boardingPass.FlightId);

                if (existing == null)
                {
                    return null;
                }

                // Валидация номера посадочного талона
                if (boardingPass.BoardingNo <= 0)
                {
                    throw new ArgumentException("Номер посадочного талона должен быть положительным числом.");
                }

                // Валидация номера места
                if (string.IsNullOrWhiteSpace(boardingPass.SeatNo) || boardingPass.SeatNo.Length > 4)
                {
                    throw new ArgumentException("Номер места должен содержать от 1 до 4 символов.");
                }

                if (!boardingPass.IsValidSeatNo())
                {
                    throw new ArgumentException("Некорректный формат номера места. Пример: '12A', '1C', '25F'.");
                }

                // Проверка уникальности boarding_no в пределах рейса (кроме текущей записи)
                var boardingNoExists = await _context.BoardingPasses
                    .AnyAsync(bp => bp.FlightId == boardingPass.FlightId &&
                                    bp.BoardingNo == boardingPass.BoardingNo &&
                                    bp.TicketNo != boardingPass.TicketNo);

                if (boardingNoExists)
                {
                    throw new ArgumentException($"Номер посадочного талона {boardingPass.BoardingNo} уже используется для рейса {boardingPass.FlightId}.");
                }

                // Проверка уникальности seat_no в пределах рейса (кроме текущей записи)
                var seatNoExists = await _context.BoardingPasses
                    .AnyAsync(bp => bp.FlightId == boardingPass.FlightId &&
                                    bp.SeatNo == boardingPass.SeatNo &&
                                    bp.TicketNo != boardingPass.TicketNo);

                if (seatNoExists)
                {
                    throw new ArgumentException($"Место {boardingPass.SeatNo} уже занято для рейса {boardingPass.FlightId}.");
                }

                // Обновление полей
                existing.BoardingNo = boardingPass.BoardingNo;
                existing.SeatNo = boardingPass.SeatNo;

                await _context.SaveChangesAsync();
                return existing;
            }
            catch (DbUpdateException dbEx)
            {
                var errorMessage = dbEx.InnerException?.Message ?? dbEx.Message;

                if (errorMessage.Contains("boarding_passes_flight_id_boarding_no_key"))
                {
                    throw new ArgumentException("Нарушение уникальности: номер посадочного талона должен быть уникальным в пределах рейса.");
                }
                else if (errorMessage.Contains("boarding_passes_flight_id_seat_no_key"))
                {
                    throw new ArgumentException("Нарушение уникальности: номер места должен быть уникальным в пределах рейса.");
                }

                throw new Exception($"Ошибка базы данных: {errorMessage}");
            }
        }

        /// <summary>
        /// Удаление посадочного талона (Delete)
        /// </summary>
        public async Task<bool> DeleteAsync(string ticketNo, int flightId)
        {
            var boardingPass = await _context.BoardingPasses
                .FirstOrDefaultAsync(bp => bp.TicketNo == ticketNo && bp.FlightId == flightId);

            if (boardingPass == null)
            {
                return false;
            }

            _context.BoardingPasses.Remove(boardingPass);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Проверка существования посадочного талона
        /// </summary>
        public async Task<bool> ExistsAsync(string ticketNo, int flightId)
        {
            return await _context.BoardingPasses
                .AnyAsync(bp => bp.TicketNo == ticketNo && bp.FlightId == flightId);
        }

        /// <summary>
        /// Поиск посадочных талонов по номеру рейса
        /// </summary>
        public async Task<List<BoardingPass>> FindByFlightIdAsync(int flightId)
        {
            return await _context.BoardingPasses
                .Where(bp => bp.FlightId == flightId)
                .OrderBy(bp => bp.BoardingNo)
                .ToListAsync();
        }

        /// <summary>
        /// Поиск посадочных талонов по номеру билета
        /// </summary>
        public async Task<List<BoardingPass>> FindByTicketNoAsync(string ticketNo)
        {
            return await _context.BoardingPasses
                .Where(bp => bp.TicketNo == ticketNo)
                .OrderBy(bp => bp.FlightId)
                .ToListAsync();
        }

        /// <summary>
        /// Получение списка свободных номеров посадочных талонов для рейса
        /// </summary>
        public async Task<List<int>> GetAvailableBoardingNosAsync(int flightId, int maxNumber = 500)
        {
            var usedNumbers = await _context.BoardingPasses
                .Where(bp => bp.FlightId == flightId)
                .Select(bp => bp.BoardingNo)
                .ToListAsync();

            var availableNumbers = Enumerable.Range(1, maxNumber)
                .Where(n => !usedNumbers.Contains(n))
                .ToList();

            return availableNumbers;
        }
    }
}