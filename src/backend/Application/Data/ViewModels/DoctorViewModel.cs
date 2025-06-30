namespace Data.ViewModels
{
    public class DoctorViewModel
    {
        public int DoctorId { get; set; }
        public int ClinicId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string? ContactInformation { get; set; }

        // Navigation properties
        public virtual ClinicViewModel Clinic { get; set; } = new ClinicViewModel();
        public virtual ICollection<ScheduleViewModel> Schedules { get; set; } = new List<ScheduleViewModel>();
        public virtual ICollection<AppointmentViewModel> Appointments { get; set; } = new List<AppointmentViewModel>();
    }
}