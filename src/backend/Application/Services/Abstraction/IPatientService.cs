using Data.ViewModels;

namespace Services.Abstraction
{
    public interface IPatientService
    {
        public Task<PatientViewModel> Add(PatientViewModel model);
        public Task<PatientViewModel> Update(PatientViewModel model);
        public Task<IEnumerable<PatientViewModel>> Search(string SearchString);
        public Task Book(AppointmentViewModel model);
    }
}
