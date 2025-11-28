using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AviationDB_CSharp.Core
{
    [Table("tickets", Schema = "bookings")]
    public class Tickets
    {
        [Key, Column("ticket_no"), MaxLength(13)]
        public string TicketNo { get; set; }

        [Required, Column("book_ref"), MaxLength(6)]
        public string BookRef { get; set; }

        [Required, Column("passenger_id"), MaxLength(20)]
        public string PassengerId { get; set; }

        [Required, Column("passenger_name")]
        public string PassengerName { get; set; }

        [Column("contact_data")]
        public string ContactData { get; set; } // JSONB

        // Навигационные свойства
        [ForeignKey("BookRef")]
        public Bookings Booking { get; set; }

        public ICollection<TicketFlights> TicketFlights { get; set; }
    }
}
