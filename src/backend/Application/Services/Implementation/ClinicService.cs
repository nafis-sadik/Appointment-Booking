using Data.CommonConstants;
using Data.Entities;
using Data.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RedBook.Core.AutoMapper;
using RedBook.Core.Constants;
using RedBook.Core.Domain;
using RedBook.Core.Security;
using RedBook.Core.UnitOfWork;
using Services.Abstraction;

namespace Services.Implementation
{
    public class ClinicService : ServiceBase, IClinicService
    {
        public ClinicService(
            ILogger<ClinicService> logger,
            IObjectMapper mapper,
            IClaimsPrincipalAccessor claimsPrincipalAccessor,
            IUnitOfWorkManager unitOfWork,
            IHttpContextAccessor httpContextAccessor
        ) : base(logger, mapper, claimsPrincipalAccessor, unitOfWork, httpContextAccessor)
        { }

        public async Task<ClinicViewModel> Add(ClinicViewModel model)
        {
            using(var repositoryFactory = UnitOfWorkManager.GetRepositoryFactory())
            {
                var userRepository = repositoryFactory.GetRepository<User>();
                string? userRole = await userRepository.UnTrackableQuery()
                    .Where(u => u.UserId == User.UserId)
                    .Select(u => u.Role)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(userRole))
                    throw new ArgumentException(CommonConstants.HttpResponseMessages.InvalidToken);

                if (userRole != CommonAttributeConstants.Roles.Admin)
                    throw new ArgumentException(CommonConstants.HttpResponseMessages.AdminAccessRequired);

                var clinicRepository = repositoryFactory.GetRepository<Clinic>();
                Clinic entity = Mapper.Map<Clinic>(model);
                entity.CreateBy = User.UserId;
                entity.CreateDate = DateTime.UtcNow;
                entity = await clinicRepository.InsertAsync(entity);
                await clinicRepository.SaveChangesAsync();
                model = Mapper.Map<ClinicViewModel>(entity);
                return model;
            }
        }

        public async Task<IEnumerable<DoctorViewModel>> GetClinicDoctors(int ClinicId)
        {
            using (var repositoryFactory = UnitOfWorkManager.GetRepositoryFactory())
            {
                var doctorRepo = repositoryFactory.GetRepository<Doctor>();
                List<DoctorViewModel> doctors = await doctorRepo.UnTrackableQuery()
                    .Where(doctor => doctor.ClinicId == ClinicId)
                    .Select(doctor => new DoctorViewModel
                    {
                        DoctorId = doctor.Id,
                    })
                    .ToListAsync();
                return doctors;
            }
        }

        public async Task<IEnumerable<ClinicViewModel>> GetClinics()
        {
            using (var repositoryFactory = UnitOfWorkManager.GetRepositoryFactory())
            {
                var clinicRepository = repositoryFactory.GetRepository<Clinic>();
                List<ClinicViewModel> model = await clinicRepository.UnTrackableQuery()
                    .Where(clinic => clinic.Id > 0)
                    .Select(clinic => new ClinicViewModel
                    {
                        ClinicId = clinic.Id,
                        Name = clinic.Name,
                        Address = clinic.Address,
                        ContactNumber = clinic.ContactNumber,
                        OperatingHours = clinic.OperatingHours,
                        ClosingHours = clinic.ClosingHours,
                    })
                    .ToListAsync();
                return model;
            }
        }

        public async Task<ClinicViewModel> Update(ClinicViewModel model)
        {
            using (var repositoryFactory = UnitOfWorkManager.GetRepositoryFactory())
            {
                var userRepository = repositoryFactory.GetRepository<User>();
                string? userRole = await userRepository.UnTrackableQuery()
                    .Where(u => u.UserId == User.UserId)
                    .Select(u => u.Role)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(userRole))
                    throw new ArgumentException(CommonConstants.HttpResponseMessages.InvalidToken);

                if (userRole != CommonAttributeConstants.Roles.Admin)
                    throw new ArgumentException(CommonConstants.HttpResponseMessages.AdminAccessRequired);

                var clinicRepository = repositoryFactory.GetRepository<Clinic>();
                Clinic entity = Mapper.Map<Clinic>(model);
                entity.CreateBy = User.UserId;
                entity.CreateDate = DateTime.UtcNow;
                entity = clinicRepository.Update(entity);
                await clinicRepository.SaveChangesAsync();
                model = Mapper.Map<ClinicViewModel>(entity);
                return model;
            }
        }
    }
}
