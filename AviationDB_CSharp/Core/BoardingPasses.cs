using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AviationDB_CSharp.Core
{
    [Table("boarding_passes", Schema = "bookings")]
    public class BoardingPasses
    {
        [Key, Column("ticket_no"), MaxLength(13)]
        public string TicketNo { get; set; }

        [Key, Column("flight_id")]
        public int FlightId { get; set; }

        [Required, Column("boarding_no")]
        public int BoardingNo { get; set; }

        [Required, Column("seat_no"), MaxLength(4)]
        public string SeatNo { get; set; }

        // Навигационные свойства
        [ForeignKey("TicketNo, FlightId")]
        public TicketFlights TicketFlight { get; set; }
    }
}
