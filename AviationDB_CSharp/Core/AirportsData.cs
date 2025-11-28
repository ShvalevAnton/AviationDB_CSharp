using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Media.Media3D;

namespace AviationDB_CSharp.Core
{
    [Table("airports_data", Schema = "bookings")]
    public class AirportsData
    {
        [Key, Column("airport_code"), MaxLength(3)]
        public string AirportCode { get; set; }

        [Required, Column("airport_name")]
        public string AirportName { get; set; }

        [Required, Column("city")]
        public string City { get; set; }

        [Required, Column("coordinates")]
        public string Coordinates { get; set; } // Geometry type - используем string для простоты

        [Required, Column("timezone")]
        public string Timezone { get; set; }

        // Навигационные свойства
        public ICollection<Flights> DepartureFlights { get; set; }
        public ICollection<Flights> ArrivalFlights { get; set; }
    }
}
