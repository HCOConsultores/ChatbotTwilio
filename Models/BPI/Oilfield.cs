using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace chatBotTwilio.Models.BPI
{
    [Table("oilfields")]

    public class Oilfield
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        [Column("id", TypeName = "int")]
        public int Id { get; set; }

        [Column("id_contract", TypeName = "int")]
        public int? ContractId { get; set; }

        [Column("name", TypeName = "varchar(100)")]
        public string Name { get; set; }

        [Column("direccion", TypeName = "varchar(100)")]
        public string Direccion { get; set; }

        [Column("coordinates", TypeName = "varchar(100)")]
        public string Coordinates { get; set; }

        [Column("active")]
        public short? Active { get; set; } = 1;
    }
}
