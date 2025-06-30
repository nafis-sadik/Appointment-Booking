using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    [Table("Patients")]
    public class Patient
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ContactInformation { get; set; } = string.Empty;

        // Null allowed as patients might register them selves in future
        public int? CreateBy { get; set; } = null;

        // Null allowed as patients might register them selves in future
        public DateTime? CreateDate { get; set; } = null;

        public int? UpdateBy { get; set; } = null;

        public DateTime? UpdateDate { get; set; } = null;

        // Navigation properties
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}