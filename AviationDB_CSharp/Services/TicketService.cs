using AviationDB_CSharp.Data;
using AviationDB_CSharp.Models;
using Microsoft.EntityFrameworkCore;

namespace AviationDB_CSharp.Services
{
    /// <summary>
    /// Сервис для выполнения CRUD операций с таблицей tickets
    /// </summary>
    public class TicketService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory; // Изменен: используем фабрику DbContext

        public TicketService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Создание нового билета (Create)
        /// </summary>
        public async Task<Ticket> CreateAsync(Ticket ticket)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            try
            {
                // Валидация номера билета
                if (string.IsNullOrEmpty(ticket.TicketNo) || ticket.TicketNo.Length != 13)
                {
                    throw new ArgumentException("Номер билета должен состоять ровно из 13 символов.");
                }

                // Проверка существования билета
                var existingTicket = await context.Tickets
                    .FirstOrDefaultAsync(t => t.TicketNo == ticket.TicketNo);

                if (existingTicket != null)
                {
                    throw new ArgumentException($"Билет с номером {ticket.TicketNo} уже существует.");
                }

                // Валидация номера бронирования
                if (string.IsNullOrEmpty(ticket.BookRef) || ticket.BookRef.Length != 6)
                {
                    throw new ArgumentException("Номер бронирования должен состоять ровно из 6 символов.");
                }

                // Проверка существования бронирования
                var bookingExists = await context.Bookings
                    .AnyAsync(b => b.BookRef == ticket.BookRef);

                if (!bookingExists)
                {
                    throw new ArgumentException($"Бронирование с номером {ticket.BookRef} не существует.");
                }

                // Валидация идентификатора пассажира
                if (string.IsNullOrWhiteSpace(ticket.PassengerId) || ticket.PassengerId.Length > 20)
                {
                    throw new ArgumentException("Идентификатор пассажира обязателен и не должен превышать 20 символов.");
                }

                // Валидация имени пассажира
                if (string.IsNullOrWhiteSpace(ticket.PassengerName))
                {
                    throw new ArgumentException("Имя пассажира обязательно.");
                }

                // Обработка контактных данных
                if (!string.IsNullOrWhiteSpace(ticket.ContactData) && !Ticket.IsValidJson(ticket.ContactData))
                {
                    throw new ArgumentException("Контактные данные должны быть в формате JSON.");
                }

                context.Tickets.Add(ticket);
                await context.SaveChangesAsync();

                return ticket;
            }
            catch (DbUpdateException dbEx)
            {
                var errorMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                throw new Exception($"Ошибка базы данных: {errorMessage}");
            }
        }

        /// <summary>
        /// Получение всех билетов с пагинацией (Изменен/Добавлен)
        /// </summary>
        public async Task<(List<Ticket> tickets, int totalCount)> GetPaginatedAsync(int page, int pageSize)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            var query = context.Tickets
                .Include(t => t.Booking)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var tickets = await query
                .OrderBy(t => t.TicketNo)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (tickets, totalCount);
        }

        /// <summary>
        /// Получение всех билетов (Read)
        /// </summary>
        public async Task<List<Ticket>> GetAllAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            return await context.Tickets
                .Include(t => t.Booking)
                .OrderBy(t => t.TicketNo)
                .ToListAsync();
        }

        /// <summary>
        /// Получение билета по номеру (Read)
        /// </summary>
        public async Task<Ticket?> GetByTicketNoAsync(string ticketNo)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            if (string.IsNullOrEmpty(ticketNo) || ticketNo.Length != 13)
                return null;

            return await context.Tickets
                .Include(t => t.Booking)
                .FirstOrDefaultAsync(t => t.TicketNo == ticketNo);
        }

        /// <summary>
        /// Получение билетов по номеру бронирования с пагинацией (Изменен/Добавлен)
        /// </summary>
        public async Task<(List<Ticket> tickets, int totalCount)> GetByBookingRefPaginatedAsync(
            string bookRef, int page, int pageSize)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            if (string.IsNullOrEmpty(bookRef) || bookRef.Length != 6)
                return (new List<Ticket>(), 0);

            var query = context.Tickets
                .Include(t => t.Booking)
                .Where(t => t.BookRef == bookRef)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var tickets = await query
                .OrderBy(t => t.TicketNo)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (tickets, totalCount);
        }

        /// <summary>
        /// Получение билетов по идентификатору пассажира с пагинацией (Изменен/Добавлен)
        /// </summary>
        public async Task<(List<Ticket> tickets, int totalCount)> GetByPassengerIdPaginatedAsync(
            string passengerId, int page, int pageSize)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            if (string.IsNullOrWhiteSpace(passengerId))
                return (new List<Ticket>(), 0);

            var query = context.Tickets
                .Include(t => t.Booking)
                .Where(t => t.PassengerId.Contains(passengerId))
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var tickets = await query
                .OrderByDescending(t => t.Booking != null ? t.Booking.BookDate : DateTime.MinValue)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (tickets, totalCount);
        }

        /// <summary>
        /// Получение билетов по имени пассажира с пагинацией (Изменен/Добавлен)
        /// </summary>
        public async Task<(List<Ticket> tickets, int totalCount)> GetByPassengerNamePaginatedAsync(
            string passengerName, int page, int pageSize)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            if (string.IsNullOrWhiteSpace(passengerName))
                return (new List<Ticket>(), 0);

            var query = context.Tickets
                .Include(t => t.Booking)
                .Where(t => t.PassengerName.Contains(passengerName, StringComparison.OrdinalIgnoreCase))
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var tickets = await query
                .OrderBy(t => t.PassengerName)
                .ThenBy(t => t.TicketNo)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (tickets, totalCount);
        }

        /// <summary>
        /// Обновление данных билета (Update) (Изменен)
        /// </summary>
        public async Task<Ticket?> UpdateAsync(Ticket ticket)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            try
            {
                var existing = await context.Tickets
                    .FirstOrDefaultAsync(t => t.TicketNo == ticket.TicketNo);

                if (existing == null)
                {
                    return null;
                }

                // Обновление номера бронирования (если указан)
                if (!string.IsNullOrWhiteSpace(ticket.BookRef))
                {
                    if (ticket.BookRef.Length != 6)
                    {
                        throw new ArgumentException("Номер бронирования должен состоять ровно из 6 символов.");
                    }

                    // Проверка существования нового бронирования
                    var bookingExists = await context.Bookings
                        .AnyAsync(b => b.BookRef == ticket.BookRef);

                    if (!bookingExists)
                    {
                        throw new ArgumentException($"Бронирование с номером {ticket.BookRef} не существует.");
                    }

                    existing.BookRef = ticket.BookRef;
                }

                // Обновление идентификатора пассажира
                if (!string.IsNullOrWhiteSpace(ticket.PassengerId))
                {
                    if (ticket.PassengerId.Length > 20)
                    {
                        throw new ArgumentException("Идентификатор пассажира не должен превышать 20 символов.");
                    }
                    existing.PassengerId = ticket.PassengerId;
                }

                // Обновление имени пассажира
                if (!string.IsNullOrWhiteSpace(ticket.PassengerName))
                {
                    existing.PassengerName = ticket.PassengerName;
                }

                // Обновление контактных данных
                if (ticket.ContactData != null)
                {
                    if (string.IsNullOrWhiteSpace(ticket.ContactData))
                    {
                        existing.ContactData = null;
                    }
                    else if (Ticket.IsValidJson(ticket.ContactData))
                    {
                        existing.ContactData = ticket.ContactData;
                    }
                    else
                    {
                        throw new ArgumentException("Контактные данные должны быть в формате JSON.");
                    }
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
        /// Удаление билета (Delete) (Изменен)
        /// </summary>
        public async Task<bool> DeleteAsync(string ticketNo)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            var ticket = await context.Tickets
                .FirstOrDefaultAsync(t => t.TicketNo == ticketNo);

            if (ticket == null)
            {
                return false;
            }

            context.Tickets.Remove(ticket);
            await context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Проверка существования билета (Изменен)
        /// </summary>
        public async Task<bool> ExistsAsync(string ticketNo)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            return await context.Tickets
                .AnyAsync(t => t.TicketNo == ticketNo);
        }

        /// <summary>
        /// Поиск билетов с пагинацией (Изменен/Добавлен)
        /// </summary>
        public async Task<(List<Ticket> tickets, int totalCount)> SearchPaginatedAsync(
            string searchTerm, int page, int pageSize)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            var query = context.Tickets
                .Include(t => t.Booking)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var search = searchTerm.Trim();

                // Изменен: Используем ToLower для регистронезависимого поиска
                query = query.Where(t =>
                    t.TicketNo.Contains(search) ||
                    t.BookRef.Contains(search) ||
                    t.PassengerId.Contains(search) ||
                    t.PassengerName.ToLower().Contains(search.ToLower())); // Изменено: убрали StringComparison
            }

            var totalCount = await query.CountAsync();

            var tickets = await query
                .OrderBy(t => t.TicketNo)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (tickets, totalCount);
        }

        /// <summary>
        /// Получение билетов с контактными данными с пагинацией (Изменен/Добавлен)
        /// </summary>
        public async Task<(List<Ticket> tickets, int totalCount)> GetTicketsWithContactDataPaginatedAsync(
            int page, int pageSize)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            var query = context.Tickets
                .Include(t => t.Booking)
                .Where(t => t.ContactData != null && !string.IsNullOrWhiteSpace(t.ContactData))
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var tickets = await query
                .OrderBy(t => t.TicketNo)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (tickets, totalCount);
        }

        /// <summary>
        /// Получение статистики по бронированиям (Изменен)
        /// </summary>
        public async Task<Dictionary<string, int>> GetStatisticsByBookingAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            var tickets = await context.Tickets
                .Include(t => t.Booking)
                .ToListAsync();

            return tickets
                .GroupBy(t => t.BookRef)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        /// <summary>
        /// Получение количества билетов по бронированию (Изменен)
        /// </summary>
        public async Task<int> GetTicketCountByBookingAsync(string bookRef)
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            if (string.IsNullOrEmpty(bookRef) || bookRef.Length != 6)
                return 0;

            return await context.Tickets
                .CountAsync(t => t.BookRef == bookRef);
        }

        /// <summary>
        /// Получение всех номеров бронирований для фильтрации (Изменен/Добавлен)
        /// </summary>
        public async Task<List<string>> GetAllBookingReferencesAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync(); // Изменен: используем фабрику DbContext
            return await context.Tickets
                .Select(t => t.BookRef)
                .Distinct()
                .OrderBy(refCode => refCode)
                .ToListAsync();
        }

        /// <summary>
        /// Простой поиск билетов (Изменен/Добавлен)
        /// </summary>
        public async Task<List<Ticket>> SearchAsync(string searchTerm)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.Tickets
                .Include(t => t.Booking)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var search = searchTerm.Trim();
                query = query.Where(t =>
                    t.TicketNo.Contains(search) ||
                    t.BookRef.Contains(search) ||
                    t.PassengerId.Contains(search) ||
                    t.PassengerName.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            return await query
                .OrderBy(t => t.TicketNo)
                .ToListAsync();
        }
    }
}