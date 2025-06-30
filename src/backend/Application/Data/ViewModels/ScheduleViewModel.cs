namespace Data.ViewModels
{
    public class ScheduleViewModel
    {
        public int ScheduleId { get; set; }
        public int DoctorId { get; set; }
        public string? DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public TimeSpan VisitTimeSapn { get; set; }
        public virtual DoctorViewModel Doctor { get; set; } = new DoctorViewModel();
    }
}