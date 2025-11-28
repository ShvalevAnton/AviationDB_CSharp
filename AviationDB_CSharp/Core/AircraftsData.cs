using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AviationDB_CSharp.Core
{
    [Table("aircrafts_data", Schema = "bookings")]
    public class AircraftsData
    {
        [Key, Column("aircraft_code"), MaxLength(3)]
        public string AircraftCode { get; set; }

        [Required, Column("model")]
        public string Model { get; set; } // JSONB в БД, но будем использовать string

        [Required, Column("range")]
        public int Range { get; set; }

        // Навигационные свойства
        public ICollection<Flights> Flights { get; set; }
        public ICollection<Seats> Seats { get; set; }
    }
}
