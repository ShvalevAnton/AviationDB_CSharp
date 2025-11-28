using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AviationDB_CSharp.Core
{
    [Table("bookings", Schema = "bookings")]
    public class Bookings
    {
        [Key, Column("book_ref"), MaxLength(6)]
        public string BookRef { get; set; }

        [Required, Column("book_date")]
        public DateTime BookDate { get; set; }

        [Required, Column("total_amount")]
        public decimal TotalAmount { get; set; }

        // Навигационные свойства
        public ICollection<Tickets> Tickets { get; set; }
    }
}
