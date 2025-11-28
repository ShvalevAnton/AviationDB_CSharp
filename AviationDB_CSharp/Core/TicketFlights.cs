using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AviationDB_CSharp.Core
{
    [Table("ticket_flights", Schema = "bookings")]
    public class TicketFlights
    {
        [Key, Column("ticket_no"), MaxLength(13)]
        public string TicketNo { get; set; }

        [Key, Column("flight_id")]
        public int FlightId { get; set; }

        [Required, Column("fare_conditions"), MaxLength(10)]
        public string FareConditions { get; set; }

        [Required, Column("amount")]
        public decimal Amount { get; set; }

        // Навигационные свойства
        [ForeignKey("TicketNo")]
        public Tickets Ticket { get; set; }

        [ForeignKey("FlightId")]
        public Flights Flight { get; set; }

        public ICollection<BoardingPasses> BoardingPasses { get; set; }
    }
}
