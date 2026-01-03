using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AviationDB_CSharp.Models
{
    /// <summary>
    /// Модель для таблицы bookings (бронирования)
    /// </summary>
    [Table("bookings", Schema = "bookings")]
    public class Booking
    {
        /// <summary>
        /// Номер бронирования. Первичный ключ
        /// </summary>
        [Key]
        [Column("book_ref")]
        [StringLength(6)]
        public string BookRef { get; set; } = string.Empty;

        /// <summary>
        /// Дата и время бронирования (с часовым поясом)
        /// Автоматически конвертируется в UTC через DateTimeToUtcConverter
        /// </summary>
        [Column("book_date")]
        public DateTime BookDate { get; set; }

        /// <summary>
        /// Общая стоимость бронирования (точность: 10 цифр, 2 знака после запятой)
        /// </summary>
        [Column("total_amount", TypeName = "numeric(10,2)")]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Проверка валидности номера бронирования
        /// </summary>
        public bool IsValidBookRef()
        {
            if (string.IsNullOrWhiteSpace(BookRef))
                return false;

            if (BookRef.Length != 6)
                return false;

            foreach (char c in BookRef)
            {
                if (!char.IsLetterOrDigit(c))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Проверка валидности общей суммы
        /// </summary>
        public bool IsValidTotalAmount()
        {
            return TotalAmount > 0;
        }

        /// <summary>
        /// Проверка валидности даты бронирования
        /// </summary>
        public bool IsValidBookDate()
        {
            // Просто проверяем, что дата не слишком старая
            return BookDate.Year > 2000;
        }

        /// <summary>
        /// Форматированный вывод информации о бронировании
        /// </summary>
        public string GetFormattedInfo()
        {
            return $"Бронь: {BookRef}, Дата: {GetFormattedDate()}, Сумма: {TotalAmount:C}";
        }

        /// <summary>
        /// Форматированная дата для отображения (в локальном времени)
        /// </summary>
        public string GetFormattedDate()
        {
            // Конвертер автоматически сохраняет в UTC, при чтении тоже получаем UTC
            // Конвертируем в локальное время для отображения
            return BookDate.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// Форматированная дата в UTC для отображения
        /// </summary>
        public string GetFormattedUtcDate()
        {
            return BookDate.ToString("yyyy-MM-dd HH:mm:ss 'UTC'");
        }

        /// <summary>
        /// Форматированная сумма для отображения
        /// </summary>
        public string GetFormattedAmount()
        {
            return TotalAmount.ToString("N2");
        }

        /// <summary>
        /// Получение даты в локальном времени (удобный метод)
        /// </summary>
        public DateTime GetLocalBookDate()
        {
            return BookDate.ToLocalTime();
        }       

        /// <summary>
        /// Форматированная дата для отображения - для привязки XAML
        /// </summary>
        [NotMapped]
        public string FormattedDate => GetFormattedDate();

        /// <summary>
        /// Форматированная дата в UTC для отображения - для привязки XAML
        /// </summary>
        [NotMapped]
        public string FormattedUtcDate => GetFormattedUtcDate();

        /// <summary>
        /// Форматированная сумма для отображения - для привязки XAML
        /// </summary>
        [NotMapped]
        public string FormattedAmount => GetFormattedAmount();
    }
}