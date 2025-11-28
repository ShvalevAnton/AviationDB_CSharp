using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AviationDB_CSharp.Core
{
    [Table("spatial_ref_sys", Schema = "bookings")]
    public class SpatialRefSys
    {
        [Key, Column("srid")]
        public int Srid { get; set; }

        [Column("auth_name"), MaxLength(256)]
        public string AuthName { get; set; }

        [Column("auth_srid")]
        public int? AuthSrid { get; set; }

        [Column("srtext"), MaxLength(2048)]
        public string SrText { get; set; }

        [Column("proj4text"), MaxLength(2048)]
        public string Proj4Text { get; set; }
    }
}
