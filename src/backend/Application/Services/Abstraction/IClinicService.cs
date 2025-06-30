using Data.ViewModels;

namespace Services.Abstraction
{
    public interface IClinicService
    {
        public Task<IEnumerable<ClinicViewModel>> GetClinics();
        public Task<IEnumerable<DoctorViewModel>> GetClinicDoctors(int ClinicId);
        public Task<ClinicViewModel> Update(ClinicViewModel model);
        public Task<ClinicViewModel> Add(ClinicViewModel model);
    }
}
