using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AviationDB_CSharp.Core
{
    [Table("flights", Schema = "bookings")]
    public class Flights
    {
        [Key, Column("flight_id")]
        public int FlightId { get; set; }

        [Required, Column("flight_no"), MaxLength(6)]
        public string FlightNo { get; set; }

        [Required, Column("scheduled_departure")]
        public DateTime ScheduledDeparture { get; set; }

        [Required, Column("scheduled_arrival")]
        public DateTime ScheduledArrival { get; set; }

        [Required, Column("departure_airport"), MaxLength(3)]
        public string DepartureAirport { get; set; }

        [Required, Column("arrival_airport"), MaxLength(3)]
        public string ArrivalAirport { get; set; }

        [Required, Column("status"), MaxLength(20)]
        public string Status { get; set; }

        [Required, Column("aircraft_code"), MaxLength(3)]
        public string AircraftCode { get; set; }

        [Column("actual_departure")]
        public DateTime? ActualDeparture { get; set; }

        [Column("actual_arrival")]
        public DateTime? ActualArrival { get; set; }

        // Навигационные свойства
        [ForeignKey("AircraftCode")]
        public AircraftsData Aircraft { get; set; }

        [ForeignKey("DepartureAirport")]
        public AirportsData DepartureAirportData { get; set; }

        [ForeignKey("ArrivalAirport")]
        public AirportsData ArrivalAirportData { get; set; }

        public ICollection<TicketFlights> TicketFlights { get; set; }
        public ICollection<BoardingPasses> BoardingPasses { get; set; }
    }
}
