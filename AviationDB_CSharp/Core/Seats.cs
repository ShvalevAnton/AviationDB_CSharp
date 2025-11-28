using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AviationDB_CSharp.Core
{
    [Table("seats", Schema = "bookings")]
    public class Seats
    {
        [Key, Column("aircraft_code"), MaxLength(3)]
        public string AircraftCode { get; set; }

        [Key, Column("seat_no"), MaxLength(4)]
        public string SeatNo { get; set; }

        [Required, Column("fare_conditions"), MaxLength(10)]
        public string FareConditions { get; set; }

        // Навигационные свойства
        [ForeignKey("AircraftCode")]
        public AircraftsData Aircraft { get; set; }
    }
}
