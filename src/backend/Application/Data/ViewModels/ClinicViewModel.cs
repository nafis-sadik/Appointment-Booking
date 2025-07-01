using Data.Entities;

namespace Data.ViewModels
{
    public class ClinicViewModel
    {
        public int ClinicId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public TimeSpan? OperatingHours { get; set; }
        public TimeSpan? ClosingHours { get; set; }

        // Navigation properties
        public virtual ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}