using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AviationDB_CSharp.Models
{
    /// <summary>
    /// Модель для таблицы seats
    /// </summary>
    [Table("seats", Schema = "bookings")]
    public class Seat
    {
        /// <summary>
        /// Код самолета (часть составного ключа)
        /// </summary>
        [Key]
        [Column("aircraft_code")]
        [StringLength(3)]
        public string AircraftCode { get; set; } = string.Empty;

        /// <summary>
        /// Номер места (часть составного ключа)
        /// </summary>
        [Key]
        [Column("seat_no")]
        [StringLength(4)]
        public string SeatNo { get; set; } = string.Empty;

        /// <summary>
        /// Класс обслуживания
        /// </summary>
        [Column("fare_conditions")]
        [StringLength(10)]
        public string FareConditions { get; set; } = string.Empty;

        /// <summary>
        /// Навигационное свойство к самолету
        /// </summary>
        [ForeignKey("AircraftCode")]
        public virtual Aircraft? Aircraft { get; set; }

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
    }
}