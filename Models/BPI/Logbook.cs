using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace chatBotTwilio.Models.BPI
{
    [Table("logbook")]
    public class Logbook
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("id_project")]
        [DefaultValue(0)]
        public int? IdProject { get; set; }

        [Column("id_reporte")]
        [Required]
        [DefaultValue(0)]
        public int IdReporte { get; set; }

        [Column("date", TypeName = "date")]
        [DefaultValue("getdate()")]
        public DateTime? Date { get; set; }

        [Column("timexnote", TypeName = "time(7)")]
        [DefaultValue("sysdatetime()")]
        public TimeSpan? Timexnote { get; set; } // Representa la hora del día

        [Column("typenote", TypeName = "varchar(25)")]
        [DefaultValue("ANTECEDENTES")]
        public string TypeNote { get; set; }

        [Column("supervisor", TypeName = "varchar(250)")]
        [DefaultValue("NAME SUPERVISOR")]
        public string Supervisor { get; set; }

        [Column("image", TypeName = "varchar(320)")]
        [DefaultValue("SIN FOTO")]
        public string ImageUrl { get; set; }

        [Column("description", TypeName = "nvarchar(max)")]
        [DefaultValue("NOTAS")]
        public string? Description { get; set; }

        [Column("imageazure", TypeName = "varchar(320)")]
        [DefaultValue("NO FILE")]
        public string? ImageAzure { get; set; }

        [Column("phone", TypeName = "nvarchar(24)")]
        [DefaultValue("Phone")]
        public string Phone { get; set; }

        [Column("orden")]
        [DefaultValue(0)]
        public int? Orden { get; set; }

        [Column("active")]
        public short? Active { get; set; } = 1;

        // Nuevos campos
        [Column("id_ot")]
        public int? IdOt { get; set; }

        [Column("id_resource")]
        public int? IdResource { get; set; }

        [Column("quantity", TypeName = "decimal(12,3)")]
        public decimal? Quantity { get; set; }

        [Column("start", TypeName = "time(7)")]
        public TimeSpan? Start { get; set; }

        [Column("end", TypeName = "time(7)")]
        public TimeSpan? End { get; set; }

        [Column("position", TypeName = "varchar(50)")]
        public string? Position { get; set; }

        [Column("imageReal", TypeName = "varchar(320)")]
        public string? ImageReal { get; set; }

        [Column("chatid", TypeName = "nvarchar(24)")]
        public string? ChatId { get; set; }

        [Column("cuadrilla", TypeName = "varchar(20)")]
        public string? Cuadrilla { get; set; }

        [Column("metadata", TypeName = "nvarchar(max)")]
        public string? Metadata { get; set; }

        [Column("report_section_id")]
        public int? ReportSectionId { get; set; }

        [Column("task_reference", TypeName = "varchar(50)")]
        public string? TaskReference { get; set; }

        [Column("notes", TypeName = "varchar(500)")]
        public string? Notes { get; set; }

        [Column("fuel_quantity")]
        public int? FuelQuantity { get; set; }

        [Column("report_date", TypeName = "varchar(10)")]
        public string? ReportDate { get; set; }
    }
}