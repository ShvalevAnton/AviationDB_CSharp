// file name: TicketFlight.cs
// file content begin
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AviationDB_CSharp.Models
{
    /// <summary>
    /// Модель для таблицы ticket_flights (сегменты перелетов по билетам)
    /// </summary>
    [Table("ticket_flights", Schema = "bookings")]
    public class TicketFlight
    {
        /// <summary>
        /// Номер билета (часть составного ключа)
        /// </summary>
        [Key]
        [Column("ticket_no")]
        [StringLength(13)]
        public string TicketNo { get; set; } = string.Empty;

        /// <summary>
        /// ID рейса (часть составного ключа)
        /// </summary>
        [Key]
        [Column("flight_id")]
        public int FlightId { get; set; }

        /// <summary>
        /// Класс обслуживания
        /// </summary>
        [Column("fare_conditions")]
        [StringLength(10)]
        public string FareConditions { get; set; } = string.Empty;

        /// <summary>
        /// Стоимость перелета
        /// </summary>
        [Column("amount")]
        [Range(0, double.MaxValue, ErrorMessage = "Стоимость не может быть отрицательной")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Навигационное свойство к билету
        /// </summary>
        [ForeignKey("TicketNo")]
        public virtual Ticket? Ticket { get; set; }

        /// <summary>
        /// Навигационное свойство к рейсу
        /// </summary>
        [ForeignKey("FlightId")]
        public virtual Flight? Flight { get; set; }

        /// <summary>
        /// Проверка валидности класса обслуживания
        /// </summary>
        public static bool IsValidFareConditions(string fareConditions)
        {
            return fareConditions.ToLower() switch
            {
                "economy" => true,
                "comfort" => true,
                "business" => true,
                _ => false
            };
        }

        /// <summary>
        /// Приведение класса обслуживания к стандартному формату
        /// </summary>
        public static string NormalizeFareConditions(string fareConditions)
        {
            return fareConditions.ToLower() switch
            {
                "economy" => "Economy",
                "comfort" => "Comfort",
                "business" => "Business",
                _ => fareConditions
            };
        }

        /// <summary>
        /// Форматированный вывод стоимости
        /// </summary>
        public string GetFormattedAmount()
        {
            return $"{Amount:C}";
        }

        /// <summary>
        /// Получение информации о перелете
        /// </summary>
        public string GetFlightInfo()
        {
            if (Flight == null)
                return $"Рейс #{FlightId}";

            return $"{Flight.FlightNo}: {Flight.DepartureAirport} → {Flight.ArrivalAirport}";
        }
    }
}
// file content end