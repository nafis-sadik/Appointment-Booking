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
    public class DoctorService : ServiceBase, IDoctorService
    {
        public DoctorService(
            ILogger<DoctorService> logger,
            IObjectMapper mapper,
            IClaimsPrincipalAccessor claimsPrincipalAccessor,
            IUnitOfWorkManager unitOfWork,
            IHttpContextAccessor httpContextAccessor
        ) : base(logger, mapper, claimsPrincipalAccessor, unitOfWork, httpContextAccessor)
        { }

        public async Task<DoctorViewModel> AddDoctor(DoctorViewModel model)
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

                if (userRole != CommonAttributeConstants.Roles.Admin && userRole != CommonAttributeConstants.Roles.Clinic)
                    throw new ArgumentException(CommonConstants.HttpResponseMessages.AdminAccessRequired);

                var doctorRepository = repositoryFactory.GetRepository<Doctor>();
                Doctor entity = Mapper.Map<Doctor>(model);
                entity.CreateBy = User.UserId;
                entity.CreateDate = DateTime.UtcNow;
                entity = await doctorRepository.InsertAsync(entity);
                model = Mapper.Map<DoctorViewModel>(entity);
                return model;
            }
        }

        public async Task<ScheduleViewModel> AddSchedule(ScheduleViewModel model)
        {
            using (var repositoryFactory = UnitOfWorkManager.GetRepositoryFactory())
            {
                // Check if the user is valid
                var userRepository = repositoryFactory.GetRepository<User>();
                string? userRole = await userRepository.UnTrackableQuery()
                    .Where(u => u.UserId == User.UserId)
                    .Select(u => u.Role)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(userRole))
                    throw new ArgumentException(CommonConstants.HttpResponseMessages.InvalidToken);

                if (userRole != CommonAttributeConstants.Roles.Doctor)
                    throw new ArgumentException(CommonConstants.HttpResponseMessages.AdminAccessRequired);

                // Check if the schedule is valid
                var doctorRepository = repositoryFactory.GetRepository<Doctor>();
                bool isValidSchedule = await doctorRepository.UnTrackableQuery()
                    .Where(doctor => 
                        doctor.Id == model.DoctorId 
                        && doctor.Clinic.OperatingHours <= model.StartTime 
                        && doctor.Clinic.ClosingHours >= model.StartTime + model.VisitTimeSapn
                    )
                    .AnyAsync();

                if (!isValidSchedule)
                    throw new ArgumentException(CommonConstants.HttpResponseMessages.InvalidInput);

                // Insert schedule
                var scheduleRepository = repositoryFactory.GetRepository<Schedule>();
                Schedule entity = Mapper.Map<Schedule>(model);
                entity = await scheduleRepository.InsertAsync(entity);
                model = Mapper.Map<ScheduleViewModel>(entity);
                return model;
            }
        }

        public async Task<IEnumerable<AppointmentViewModel>> GetDoctorAppointments(int DoctorId)
        {
            using (var repositoryFactory = UnitOfWorkManager.GetRepositoryFactory())
            {
                var appointmentRepository = repositoryFactory.GetRepository<Appointment>();
                return await appointmentRepository.UnTrackableQuery()
                    .Where(appointment => appointment.DoctorId == DoctorId)
                    .Select(appointment => new AppointmentViewModel
                    {
                        AppointmentId = appointment.Id,
                        StartTime = appointment.StartTime,
                        EndTime = appointment.EndTime,
                        PatientId = appointment.PatientId,
                        PatientName = appointment.Patient.Name,
                    })
                    .ToListAsync();
            }
        }

        public Task<IEnumerable<DoctorViewModel>> GetDoctorList()
        {
            throw new NotImplementedException();
        }

        public async Task<ScheduleViewModel> UpdateSchedule(ScheduleViewModel model)
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

                if (userRole != CommonAttributeConstants.Roles.Doctor)
                    throw new ArgumentException(CommonConstants.HttpResponseMessages.AdminAccessRequired);

                var scheduleRepository = repositoryFactory.GetRepository<Schedule>();
                bool isvalid = await scheduleRepository.TrackableQuery()
                    .Where(sch => sch.DoctorId == User.UserId && sch.Id == model.ScheduleId)
                    .CountAsync() == 1;

                if (!isvalid)
                    throw new ArgumentException("You can not change other people's schedule");

                scheduleRepository.ColumnUpdate(model.ScheduleId, new Dictionary<string, object>
                {
                    { nameof(Schedule.StartTime), model.StartTime },
                    { nameof(Schedule.EndTime), model.EndTime },
                    { nameof(Schedule.VisitTimeSapn), model.VisitTimeSapn },
                });

                await scheduleRepository.SaveChangesAsync();

                return model;
            }
        }
    }
}
