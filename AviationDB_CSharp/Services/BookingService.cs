using AviationDB_CSharp.Data;
using AviationDB_CSharp.Models;
using Microsoft.EntityFrameworkCore;

namespace AviationDB_CSharp.Services
{
    /// <summary>
    /// Сервис для выполнения CRUD операций с таблицей bookings
    /// </summary>
    public class BookingService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public BookingService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Создание новой брони (Create)
        /// </summary>
        public async Task<Booking> CreateAsync(Booking booking)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            try
            {
                // Валидация номера бронирования
                if (string.IsNullOrEmpty(booking.BookRef) || booking.BookRef.Length != 6)
                {
                    throw new ArgumentException("Номер бронирования должен состоять ровно из 6 символов.");
                }

                if (!booking.IsValidBookRef())
                {
                    throw new ArgumentException("Некорректный формат номера бронирования. Допустимы только буквы и цифры.");
                }

                // Валидация общей суммы
                if (booking.TotalAmount <= 0)
                {
                    throw new ArgumentException("Общая сумма должна быть больше 0.");
                }

                // Проверка существования
                var existing = await context.Bookings
                    .FirstOrDefaultAsync(b => b.BookRef == booking.BookRef);

                if (existing != null)
                {
                    throw new ArgumentException($"Бронь с номером {booking.BookRef} уже существует.");
                }

                // Автоматически устанавливаем текущую дату, если не указана
                if (booking.BookDate == DateTime.MinValue)
                {
                    booking.BookDate = DateTime.UtcNow; // Устанавливаем UTC
                }
                // Конвертер автоматически конвертирует в UTC при сохранении

                // Валидация даты бронирования
                if (!booking.IsValidBookDate())
                {
                    throw new ArgumentException("Некорректная дата бронирования.");
                }

                context.Bookings.Add(booking);
                await context.SaveChangesAsync();

                return booking;
            }
            catch (DbUpdateException dbEx)
            {
                var errorMessage = dbEx.InnerException?.Message ?? dbEx.Message;

                if (errorMessage.Contains("bookings_pkey"))
                {
                    throw new ArgumentException("Нарушение первичного ключа: бронь с таким номером уже существует.");
                }

                throw new Exception($"Ошибка базы данных: {errorMessage}");
            }
        }

        /// <summary>
        /// Получение всех броней (Read)
        /// </summary>
        public async Task<List<Booking>> GetAllAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Bookings
                .OrderByDescending(b => b.BookDate) // Сначала новые брони
                .ToListAsync();
        }

        /// <summary>
        /// Получение брони по номеру (Read)
        /// </summary>
        public async Task<Booking?> GetByBookRefAsync(string bookRef)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            if (string.IsNullOrEmpty(bookRef) || bookRef.Length != 6)
                return null;

            return await context.Bookings
                .FirstOrDefaultAsync(b => b.BookRef == bookRef.ToUpper());
        }

        /// <summary>
        /// Обновление данных брони (Update)
        /// </summary>
        public async Task<Booking?> UpdateAsync(Booking booking)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            try
            {
                var existing = await context.Bookings
                    .FirstOrDefaultAsync(b => b.BookRef == booking.BookRef);

                if (existing == null)
                {
                    return null;
                }

                // Валидация общей суммы
                if (booking.TotalAmount <= 0)
                {
                    throw new ArgumentException("Общая сумма должна быть больше 0.");
                }

                // Обновление полей
                // Конвертер автоматически конвертирует дату в UTC
                if (booking.BookDate != DateTime.MinValue)
                {
                    existing.BookDate = booking.BookDate;
                }

                // Валидация даты бронирования
                if (!existing.IsValidBookDate())
                {
                    throw new ArgumentException("Некорректная дата бронирования.");
                }

                existing.TotalAmount = booking.TotalAmount;

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
        /// Удаление брони (Delete)
        /// </summary>
        public async Task<bool> DeleteAsync(string bookRef)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var booking = await context.Bookings
                .FirstOrDefaultAsync(b => b.BookRef == bookRef);

            if (booking == null)
            {
                return false;
            }

            context.Bookings.Remove(booking);
            await context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Проверка существования брони по номеру
        /// </summary>
        public async Task<bool> ExistsAsync(string bookRef)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Bookings
                .AnyAsync(b => b.BookRef == bookRef.ToUpper());
        }

        /// <summary>
        /// Поиск броней по дате (в локальном времени)
        /// </summary>
        public async Task<List<Booking>> FindByDateAsync(DateTime date)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            // Конвертируем локальную дату в UTC для поиска
            var startDate = date.Date.ToUniversalTime();
            var endDate = date.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

            return await context.Bookings
                .Where(b => b.BookDate >= startDate && b.BookDate <= endDate)
                .OrderBy(b => b.BookDate)
                .ToListAsync();
        }

        /// <summary>
        /// Поиск броней по диапазону дат (в локальном времени)
        /// </summary>
        public async Task<List<Booking>> FindByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            // Конвертируем даты в UTC для сравнения
            var utcStart = startDate.ToUniversalTime();
            var utcEnd = endDate.ToUniversalTime();

            return await context.Bookings
                .Where(b => b.BookDate >= utcStart && b.BookDate <= utcEnd)
                .OrderBy(b => b.BookDate)
                .ToListAsync();
        }

        /// <summary>
        /// Поиск броней по сумме (больше или равно)
        /// </summary>
        public async Task<List<Booking>> FindByMinAmountAsync(decimal minAmount)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Bookings
                .Where(b => b.TotalAmount >= minAmount)
                .OrderByDescending(b => b.TotalAmount)
                .ToListAsync();
        }

        /// <summary>
        /// Получение статистики броней
        /// </summary>
        public async Task<BookingStatistics> GetStatisticsAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var bookings = await context.Bookings.ToListAsync();

            if (bookings.Count == 0)
            {
                return new BookingStatistics();
            }

            return new BookingStatistics
            {
                TotalBookings = bookings.Count,
                TotalAmount = bookings.Sum(b => b.TotalAmount),
                AverageAmount = bookings.Average(b => b.TotalAmount),
                MinAmount = bookings.Min(b => b.TotalAmount),
                MaxAmount = bookings.Max(b => b.TotalAmount),
                EarliestBooking = bookings.Min(b => b.BookDate),
                LatestBooking = bookings.Max(b => b.BookDate)
            };
        }

        /// <summary>
        /// Генерация нового уникального номера бронирования
        /// </summary>
        public string GenerateBookRef()
        {
            // Простая генерация: первые 3 символа - дата, последние 3 - случайные
            var datePart = DateTime.UtcNow.ToString("ddMM").Substring(0, 3);
            var randomPart = new Random().Next(100, 999).ToString();
            return (datePart + randomPart).ToUpper();
        }

        /// <summary>
        /// Получение броней с пагинацией (Изменен/Добавлен)
        /// </summary>
        public async Task<(List<Booking> bookings, int totalCount)> GetPaginatedAsync(int page, int pageSize)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.Bookings.AsQueryable();

            var totalCount = await query.CountAsync();

            var bookings = await query
                .OrderByDescending(b => b.BookDate) // Сначала новые
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (bookings, totalCount);
        }

        /// <summary>
        /// Поиск броней с пагинацией (только по book_ref) (Изменен/Добавлен)
        /// </summary>
        public async Task<(List<Booking> bookings, int totalCount)> SearchPaginatedAsync(
            string searchTerm, int page, int pageSize)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.Bookings.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                // Поиск только по номеру бронирования
                var bookRef = searchTerm.ToUpper().Trim();
                query = query.Where(b => b.BookRef.Contains(bookRef));
            }

            var totalCount = await query.CountAsync();

            var bookings = await query
                .OrderByDescending(b => b.BookDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (bookings, totalCount);
        }
    }

    /// <summary>
    /// Класс для статистики броней
    /// </summary>
    public class BookingStatistics
    {
        public int TotalBookings { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageAmount { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public DateTime EarliestBooking { get; set; }
        public DateTime LatestBooking { get; set; }

        public BookingStatistics()
        {
            TotalBookings = 0;
            TotalAmount = 0;
            AverageAmount = 0;
            MinAmount = 0;
            MaxAmount = 0;
            EarliestBooking = DateTime.MinValue;
            LatestBooking = DateTime.MinValue;
        }

        /// <summary>
        /// Получение самой ранней брони в локальном времени
        /// </summary>
        public DateTime GetLocalEarliestBooking()
        {
            return EarliestBooking.ToLocalTime();
        }

        /// <summary>
        /// Получение самой поздней брони в локальном времени
        /// </summary>
        public DateTime GetLocalLatestBooking()
        {
            return LatestBooking.ToLocalTime();
        }
    }
}