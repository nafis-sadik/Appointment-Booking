using Data.ViewModels;

namespace Services.Abstraction
{
    public interface IDoctorService
    {
        public Task<IEnumerable<DoctorViewModel>> GetDoctorList();
        public Task<DoctorViewModel> AddDoctor(DoctorViewModel model);
        public Task<IEnumerable<AppointmentViewModel>> GetDoctorAppointments(int DoctorId);
        public Task<ScheduleViewModel> AddSchedule(ScheduleViewModel model);
        public Task<ScheduleViewModel> UpdateSchedule(ScheduleViewModel model);
    }
}
