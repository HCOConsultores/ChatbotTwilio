using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace chatBotTwilio.Models.BPI
{

    [Table("providers")]
    public class Company
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name", TypeName = "varchar(500)")]
        public string Name { get; set; }

        [Column("nameshort", TypeName = "varchar(30)")]
        public string NameShort { get; set; }

        [Column("rfc", TypeName = "varchar(13)")]
        public string RFC { get; set; }

        [Column("address", TypeName = "varchar(100)")]
        public string Address { get; set; }

        [Column("id_state", TypeName = "int")]
        public int? StateId { get; set; }

        [Column("phone", TypeName = "varchar(10)")]
        public string Phone { get; set; }

        [Column("consortium", TypeName = "nvarchar(2)")]
        [DefaultValue("NO")]
        public string Consortium { get; set; }

        [Column("picture", TypeName = "varchar(250)")]
        public string Picture { get; set; }

        [Column("active")]
        public short? Active { get; set; } = 1;
    }
}
