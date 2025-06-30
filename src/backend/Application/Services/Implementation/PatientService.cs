using Data.CommonConstants;
using Data.Entities;
using Data.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RedBook.Core.AutoMapper;
using RedBook.Core.Constants;
using RedBook.Core.Domain;
using RedBook.Core.Security;
using RedBook.Core.UnitOfWork;
using Services.Abstraction;
using System.Text;
using RabbitMQ.Client;
using System.Text;

namespace Services.Implementation
{
    public class PatientService : ServiceBase, IPatientService
    {
        public PatientService(
            ILogger<PatientService> logger,
            IObjectMapper mapper,
            IClaimsPrincipalAccessor claimsPrincipalAccessor,
            IUnitOfWorkManager unitOfWork,
            IHttpContextAccessor httpContextAccessor
        ) : base(logger, mapper, claimsPrincipalAccessor, unitOfWork, httpContextAccessor)
        { }

        public async Task<PatientViewModel> Add(PatientViewModel model)
        {
            using (var repositoryFactory = UnitOfWorkManager.GetRepositoryFactory())
            {
                // Check user role permissions
                var userRepository = repositoryFactory.GetRepository<User>();
                string? userRole = userRepository.UnTrackableQuery()
                    .Where(u => u.UserId == User.UserId)
                    .Select(u => u.Role)
                    .FirstOrDefault();
                if (string.IsNullOrEmpty(userRole))
                    throw new ArgumentException(CommonConstants.HttpResponseMessages.InvalidToken);

                if (userRole != CommonAttributeConstants.Roles.Admin 
                    && userRole != CommonAttributeConstants.Roles.Clinic
                    && userRole != CommonAttributeConstants.Roles.Receptionist)
                    throw new ArgumentException(CommonConstants.HttpResponseMessages.AdminAccessRequired);

                // Insert the patient
                var patientRepository = repositoryFactory.GetRepository<Patient>();
                Patient entity = Mapper.Map<Patient>(model);
                entity.CreateBy = User.UserId;
                entity.CreateDate = DateTime.UtcNow;
                entity = await patientRepository.InsertAsync(entity);
                await patientRepository.SaveChangesAsync();
                model = Mapper.Map<PatientViewModel>(entity);
                return model;
            }
        }

        public Task<AppointmentViewModel> Book(AppointmentViewModel model)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "hello",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            string message = "Hello, RabbitMQ!";
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "",
                                 routingKey: "hello",
                                 basicProperties: null,
                                 body: body);

            Console.WriteLine(" [x] Sent '{0}'", message);

        }

        public async Task<IEnumerable<PatientViewModel>> Search(string SearchString)
        {
            if (string.IsNullOrEmpty(SearchString))
                throw new ArgumentException(CommonConstants.HttpResponseMessages.InvalidInput);

            using (var repositoryFactory = UnitOfWorkManager.GetRepositoryFactory())
            {
                // Check user role permissions
                var userRepository = repositoryFactory.GetRepository<User>();
                string? userRole = await userRepository.UnTrackableQuery()
                    .Where(u => u.UserId == User.UserId)
                    .Select(u => u.Role)
                    .FirstOrDefaultAsync();
                if (string.IsNullOrEmpty(userRole))
                    throw new ArgumentException(CommonConstants.HttpResponseMessages.InvalidToken);

                if (userRole != CommonAttributeConstants.Roles.Admin
                    && userRole != CommonAttributeConstants.Roles.Clinic
                    && userRole != CommonAttributeConstants.Roles.Receptionist)
                    throw new ArgumentException(CommonConstants.HttpResponseMessages.AdminAccessRequired);

                // Insert the patient
                var patientRepository = repositoryFactory.GetRepository<Patient>();
                IEnumerable<PatientViewModel> model = await patientRepository
                    .UnTrackableQuery()
                    .Where(patient =>
                        patient.Name.Contains(SearchString.Trim())
                        || patient.ContactInformation.Contains(SearchString.Trim())
                        || patient.CreateBy.HasValue && userRepository.UnTrackableQuery().Any(u => u.UserId == patient.CreateBy)
                    )
                    .Select(patient => new PatientViewModel {
                        Id = patient.Id,
                        Name = patient.Name,
                        ContactInformation = patient.ContactInformation,
                    })
                    .ToListAsync();

                return model;
            }
        }

        public Task<PatientViewModel> Update(PatientViewModel model)
        {
            throw new NotImplementedException();
        }
    }
}
