using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Data.CommonConstants.CommonAttributeConstants;

namespace Data.Entities
{
    [Table("Appointments")]
    [Index(nameof(DoctorId), nameof(Date), nameof(StartTime), IsUnique = true)]
    public class Appointment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Patient")]
        public int PatientId { get; set; }

        [Required]
        [ForeignKey("Doctor")]
        public int DoctorId { get; set; }

        [Required]
        [ForeignKey("Clinic")]
        public int ClinicId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = AppoinmentStatus.Booked;

        public int CreateBy { get; set; }

        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        public int UpdateBy { get; set; }

        public DateTime UpdateDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Patient Patient { get; set; } = null!;
        public virtual Doctor Doctor { get; set; } = null!;
        public virtual Clinic Clinic { get; set; } = null!;
    }
}