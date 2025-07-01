using Data.CommonConstants;
using Data.Entities;
using Data.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RedBook.Core.AutoMapper;
using RedBook.Core.Constants;
using RedBook.Core.Domain;
using RedBook.Core.Security;
using RedBook.Core.UnitOfWork;
using Services.Abstraction;
using System.Text;
using System.Text.Json;

namespace Services.Implementation
{
    public class PatientService : ServiceBase, IPatientService
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly IConfiguration _configuration;
        public PatientService(
            ILogger<PatientService> logger,
            IObjectMapper mapper,
            IClaimsPrincipalAccessor claimsPrincipalAccessor,
            IUnitOfWorkManager unitOfWork,
            IHttpContextAccessor httpContextAccessor,
            IConnectionFactory connectionFactory,
            IConfiguration configuration
        ) : base(logger, mapper, claimsPrincipalAccessor, unitOfWork, httpContextAccessor)
        {
            _connectionFactory = connectionFactory;
            _configuration = configuration;
        }

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
                Patient entity = new Patient();
                entity.Name = model.Name;
                entity.ContactInformation = entity.ContactInformation;
                entity.CreateBy = User.UserId;
                entity.CreateDate = DateTime.UtcNow;
                entity = await patientRepository.InsertAsync(entity);
                await patientRepository.SaveChangesAsync();
                model.Id = entity.Id;
                return model;
            }
        }

        public async Task Book(AppointmentViewModel model)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            // Declare the same queue (ensures it exists)
            await channel.QueueDeclareAsync(
                queue: "booking-queue",
                durable: true,
                exclusive: false,
                autoDelete: false);

            // Serialize message to JSON
            string messageJson = JsonSerializer.Serialize(model);
            byte[] messageBody = Encoding.UTF8.GetBytes(messageJson);

            // Send the message
            await channel.BasicPublishAsync(
                exchange: string.Empty,    // Use default exchange
                routingKey: _configuration["RabbitMQ:AppointmentQueue"] ?? "appointment-queue",
                body: messageBody);

            _logger.LogInformation("Sent booking message for BookingId: {BookingId}", model.AppointmentId);
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
