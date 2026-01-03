using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AviationDB_CSharp.Models
{
    /// <summary>
    /// Модель для таблицы boarding_passes
    /// </summary>
    [Table("boarding_passes", Schema = "bookings")]
    public class BoardingPass
    {
        /// <summary>
        /// Номер билета. Часть составного первичного ключа
        /// </summary>
        [Key]
        [Column("ticket_no")]
        [StringLength(13)]
        public string TicketNo { get; set; } = string.Empty;

        /// <summary>
        /// ID рейса. Часть составного первичного ключа
        /// </summary>
        [Key]
        [Column("flight_id")]
        public int FlightId { get; set; }

        /// <summary>
        /// Номер посадочного талона (уникален в пределах рейса)
        /// </summary>
        [Column("boarding_no")]
        public int BoardingNo { get; set; }

        /// <summary>
        /// Номер места (уникален в пределах рейса)
        /// </summary>
        [Column("seat_no")]
        [StringLength(4)]
        public string SeatNo { get; set; } = string.Empty;

        /// <summary>
        /// Проверка валидности номера места
        /// </summary>
        public bool IsValidSeatNo()
        {
            if (string.IsNullOrWhiteSpace(SeatNo))
                return false;

            // Пример: "12A", "1C", "25F"
            if (SeatNo.Length < 2 || SeatNo.Length > 4)
                return false;

            // Последний символ должен быть буквой (ряд)
            char lastChar = SeatNo[SeatNo.Length - 1];
            if (!char.IsLetter(lastChar))
                return false;

            // Первые символы должны быть цифрами (номер места)
            for (int i = 0; i < SeatNo.Length - 1; i++)
            {
                if (!char.IsDigit(SeatNo[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Получение номера ряда места
        /// </summary>
        public string GetSeatRow()
        {
            if (string.IsNullOrWhiteSpace(SeatNo) || SeatNo.Length < 2)
                return "Unknown";

            // Возвращаем цифровую часть (все кроме последнего символа)
            return SeatNo.Substring(0, SeatNo.Length - 1);
        }

        /// <summary>
        /// Получение буквы места
        /// </summary>
        public char GetSeatLetter()
        {
            if (string.IsNullOrWhiteSpace(SeatNo) || SeatNo.Length < 2)
                return '?';

            return SeatNo[SeatNo.Length - 1];
        }

        /// <summary>
        /// Форматированный вывод информации о посадочном талоне
        /// </summary>
        public string GetFormattedInfo()
        {
            return $"Билет: {TicketNo}, Рейс: {FlightId}, Посадка: {BoardingNo}, Место: {SeatNo}";
        }
    }
}