using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AviationDB_CSharp.Models
{
    /// <summary>
    /// Модель для таблицы flights (рейсы)
    /// </summary>
    [Table("flights", Schema = "bookings")]
    public class Flight
    {
        /// <summary>
        /// ID рейса. Первичный ключ, автогенерируемый
        /// </summary>
        [Key]
        [Column("flight_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FlightId { get; set; }

        /// <summary>
        /// Номер рейса (6 символов)
        /// </summary>
        [Column("flight_no")]
        [StringLength(6)]
        public string FlightNo { get; set; } = string.Empty;

        /// <summary>
        /// Запланированное время вылета (UTC)
        /// </summary>
        [Column("scheduled_departure")]
        public DateTime ScheduledDeparture { get; set; }

        /// <summary>
        /// Запланированное время прилета (UTC)
        /// </summary>
        [Column("scheduled_arrival")]
        public DateTime ScheduledArrival { get; set; }

        /// <summary>
        /// Аэропорт вылета (IATA код, 3 символа)
        /// </summary>
        [Column("departure_airport")]
        [StringLength(3)]
        public string DepartureAirport { get; set; } = string.Empty;

        /// <summary>
        /// Аэропорт прилета (IATA код, 3 символа)
        /// </summary>
        [Column("arrival_airport")]
        [StringLength(3)]
        public string ArrivalAirport { get; set; } = string.Empty;

        /// <summary>
        /// Статус рейса
        /// </summary>
        [Column("status")]
        [StringLength(20)]
        public string Status { get; set; } = "Scheduled";

        /// <summary>
        /// Код самолета (IATA код, 3 символа)
        /// </summary>
        [Column("aircraft_code")]
        [StringLength(3)]
        public string AircraftCode { get; set; } = string.Empty;

        /// <summary>
        /// Фактическое время вылета (UTC, может быть null)
        /// </summary>
        [Column("actual_departure")]
        public DateTime? ActualDeparture { get; set; }

        /// <summary>
        /// Фактическое время прилета (UTC, может быть null)
        /// </summary>
        [Column("actual_arrival")]
        public DateTime? ActualArrival { get; set; }

        /// <summary>
        /// Навигационное свойство для самолета
        /// </summary>
        [ForeignKey("AircraftCode")]
        public virtual Aircraft? Aircraft { get; set; }

        /// <summary>
        /// Навигационное свойство для аэропорта вылета
        /// </summary>
        [ForeignKey("DepartureAirport")]
        public virtual Airport? DepartureAirportInfo { get; set; }

        /// <summary>
        /// Навигационное свойство для аэропорта прилета
        /// </summary>
        [ForeignKey("ArrivalAirport")]
        public virtual Airport? ArrivalAirportInfo { get; set; }

        /// <summary>
        /// Проверка валидности номера рейса
        /// </summary>
        public bool IsValidFlightNo()
        {
            if (string.IsNullOrWhiteSpace(FlightNo))
                return false;

            if (FlightNo.Length != 6)
                return false;

            // Пример: SU1234, AA1234
            return true;
        }

        /// <summary>
        /// Проверка валидности статуса рейса
        /// </summary>
        public bool IsValidStatus()
        {
            var validStatuses = new[] { "On Time", "Delayed", "Departed", "Arrived", "Scheduled", "Cancelled" };
            return validStatuses.Contains(Status);
        }

        /// <summary>
        /// Проверка временных ограничений
        /// </summary>
        public bool IsValidSchedule()
        {
            // scheduled_arrival > scheduled_departure
            if (ScheduledArrival <= ScheduledDeparture)
                return false;

            // Если есть фактические времена, проверяем их
            if (ActualDeparture.HasValue && ActualArrival.HasValue)
            {
                if (ActualArrival <= ActualDeparture)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Проверка, что аэропорты разные
        /// </summary>
        public bool AreAirportsDifferent()
        {
            return DepartureAirport != ArrivalAirport;
        }

        /// <summary>
        /// Расчет продолжительности полета (в минутах)
        /// </summary>
        public int GetScheduledDurationMinutes()
        {
            return (int)(ScheduledArrival - ScheduledDeparture).TotalMinutes;
        }

        /// <summary>
        /// Расчет фактической продолжительности полета (в минутах)
        /// </summary>
        public int? GetActualDurationMinutes()
        {
            if (!ActualDeparture.HasValue || !ActualArrival.HasValue)
                return null;

            return (int)(ActualArrival.Value - ActualDeparture.Value).TotalMinutes;
        }

        /// <summary>
        /// Проверка, является ли рейс завершенным
        /// </summary>
        public bool IsCompleted()
        {
            return Status == "Arrived" || Status == "Cancelled";
        }

        /// <summary>
        /// Форматированное время вылета (локальное)
        /// </summary>
        public string GetFormattedScheduledDeparture()
        {
            return ScheduledDeparture.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
        }

        /// <summary>
        /// Форматированное время прилета (локальное)
        /// </summary>
        public string GetFormattedScheduledArrival()
        {
            return ScheduledArrival.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
        }

        /// <summary>
        /// Форматированное фактическое время вылета (локальное)
        /// </summary>
        public string GetFormattedActualDeparture()
        {
            return ActualDeparture?.ToLocalTime().ToString("yyyy-MM-dd HH:mm") ?? "N/A";
        }

        /// <summary>
        /// Форматированное фактическое время прилета (локальное)
        /// </summary>
        public string GetFormattedActualArrival()
        {
            return ActualArrival?.ToLocalTime().ToString("yyyy-MM-dd HH:mm") ?? "N/A";
        }

        /// <summary>
        /// Форматированная информация о рейсе
        /// </summary>
        public string GetFormattedInfo()
        {
            return $"{FlightNo} {DepartureAirport}→{ArrivalAirport} ({Status})";
        }

        /// <summary>
        /// Обновление статуса рейса на основе текущего времени
        /// </summary>
        public void UpdateStatusBasedOnTime()
        {
            var now = DateTime.UtcNow;

            if (Status == "Cancelled")
                return;

            if (ActualArrival.HasValue)
            {
                Status = "Arrived";
            }
            else if (ActualDeparture.HasValue)
            {
                Status = "Departed";
            }
            else if (ScheduledDeparture <= now.AddMinutes(-15))
            {
                Status = "Delayed";
            }
            else if (ScheduledDeparture <= now)
            {
                Status = "On Time";
            }
            else
            {
                Status = "Scheduled";
            }
        }              

        /// <summary>
        /// Форматированное время вылета (локальное) - для привязки XAML
        /// </summary>
        [NotMapped]
        public string FormattedScheduledDeparture => GetFormattedScheduledDeparture();

        /// <summary>
        /// Форматированное время прилета (локальное) - для привязки XAML
        /// </summary>
        [NotMapped]
        public string FormattedScheduledArrival => GetFormattedScheduledArrival();

        /// <summary>
        /// Форматированное фактическое время вылета (локальное) - для привязки XAML
        /// </summary>
        [NotMapped]
        public string FormattedActualDeparture => GetFormattedActualDeparture();

        /// <summary>
        /// Форматированное фактическое время прилета (локальное) - для привязки XAML
        /// </summary>
        [NotMapped]
        public string FormattedActualArrival => GetFormattedActualArrival();

        /// <summary>
        /// Продолжительность полета в формате для отображения - для привязки XAML
        /// </summary>
        [NotMapped]
        public string FormattedDuration => $"{GetScheduledDurationMinutes()} мин";

        /// <summary>
        /// Форматированная информация о рейсе - для привязки XAML
        /// </summary>
        [NotMapped]
        public string FormattedInfo => GetFormattedInfo();

        /// <summary>
        /// Маршрут в формате для отображения - для привязки XAML
        /// </summary>
        [NotMapped]
        public string FormattedRoute => $"{DepartureAirport} → {ArrivalAirport}";
    }
}