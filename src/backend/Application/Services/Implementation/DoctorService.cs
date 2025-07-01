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
                Doctor entity = new Doctor();
                entity.Name = model.Name;
                entity.ClinicId = model.ClinicId;
                entity.Specialization = model.Specialization;
                entity.ContactInformation = model.ContactInformation;
                entity.CreateBy = User.UserId;
                entity.CreateDate = DateTime.UtcNow;
                entity = await doctorRepository.InsertAsync(entity);
                await doctorRepository.SaveChangesAsync();
                model.DoctorId = entity.Id;
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

                if (userRole != CommonAttributeConstants.Roles.Doctor && userRole != CommonAttributeConstants.Roles.Admin)
                    throw new ArgumentException(CommonConstants.HttpResponseMessages.AdminAccessRequired);

                // Check if the schedule is valid for clinic
                var doctorRepository = repositoryFactory.GetRepository<Doctor>();
                bool validForClinic = await doctorRepository.UnTrackableQuery()
                    .Where(doctor => 
                        doctor.Id == model.DoctorId 
                        && doctor.Clinic.OperatingHours <= model.StartTime 
                        && doctor.Clinic.ClosingHours >= model.EndTime
                    )
                    .AnyAsync();

                if (!validForClinic)
                    throw new ArgumentException(CommonConstants.HttpResponseMessages.InvalidInput);

                // Conflict with own schedule
                var scheduleRepository = repositoryFactory.GetRepository<Schedule>();
                bool scheduleExists = await scheduleRepository.UnTrackableQuery()
                    .Where(sch =>
                        sch.DoctorId == model.DoctorId
                        && sch.DayOfWeek == model.DayOfWeek
                        && sch.StartTime <= model.StartTime
                        && sch.EndTime >= model.StartTime
                    ).AnyAsync();

                if (scheduleExists)
                    throw new ArgumentException(CommonConstants.HttpResponseMessages.InvalidInput);

                // Insert schedule
                Schedule entity = new Schedule();
                entity.DoctorId = model.DoctorId;
                entity.DayOfWeek = model.DayOfWeek;
                entity.StartTime = model.StartTime;
                entity.EndTime = model.EndTime;
                entity.VisitTimeSapn = model.VisitTimeSapn;
                entity = await scheduleRepository.InsertAsync(entity);
                await scheduleRepository.SaveChangesAsync();
                model.ScheduleId = entity.Id;
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
                // Only doctors can update schedules
                var userRepository = repositoryFactory.GetRepository<User>();
                string? userRole = await userRepository.UnTrackableQuery()
                    .Where(u => u.UserId == User.UserId)
                    .Select(u => u.Role)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(userRole))
                    throw new ArgumentException(CommonConstants.HttpResponseMessages.InvalidToken);

                if (userRole != CommonAttributeConstants.Roles.Doctor)
                    throw new ArgumentException(CommonConstants.HttpResponseMessages.AdminAccessRequired);

                // A doctor can update only his own schedule
                var scheduleRepository = repositoryFactory.GetRepository<Schedule>();
                bool isvalid = await scheduleRepository.TrackableQuery()
                    .Where(sch => sch.DoctorId == User.UserId && sch.Id == model.ScheduleId)
                    .AnyAsync();

                if (!isvalid)
                    throw new ArgumentException("You can not change other people's schedule");

                // Match schedule clash
                bool clashExists = await scheduleRepository.UnTrackableQuery()
                    .Where(sch => sch.DoctorId == User.UserId
                        && model.StartTime <= model.StartTime
                        && model.EndTime >= model.StartTime
                    ).AnyAsync();

                if (clashExists)
                    throw new ArgumentException("Schedule clash detected");

                // Update schedule
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
