using Data.Entities;
using Data.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RedBook.Core.AutoMapper;
using RedBook.Core.Domain;
using RedBook.Core.EntityFramework;
using RedBook.Core.Security;
using RedBook.Core.UnitOfWork;
using System.Text;
using System.Text.Json;

namespace RabbitConsumer
{
    public class BookingConsumer : ServiceBase, IHostedService
    {
        private readonly ILogger<BookingConsumer> _logger;
        private readonly IConnectionFactory _connectionFactory;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private IConnection? _connection;
        private IChannel? _channel;

        public BookingConsumer(
            ILogger<BookingConsumer> logger,
            IObjectMapper mapper,
            IConfiguration configuration,
            IConnectionFactory connectionFactory,
            IServiceProvider serviceProvider
        ) : base(logger, mapper, serviceProvider)
        {
            _logger = logger;
            _connectionFactory = connectionFactory;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Setup RabbitMQ connection
            _connection = await _connectionFactory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            // Declare queue
            await _channel.QueueDeclareAsync(
                queue: _configuration["RabbitMQ:AppointmentQueue"] ?? "appointment-queue",
                durable: true,
                exclusive: false,
                autoDelete: false);

            // Set QoS
            await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

            // Setup consumer
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += ProcessMessage;

            // Start consuming
            await _channel.BasicConsumeAsync(
                queue: _configuration["RabbitMQ:AppointmentQueue"] ?? "appointment-queue",
                autoAck: false,
                consumer: consumer);

            _logger.LogInformation("BookingConsumer started");
        }

        private async Task ProcessMessage(object sender, BasicDeliverEventArgs eventArgs)
        {
            var message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

            try
            {
                AppointmentViewModel? appointmentViewModel = JsonSerializer.Deserialize<AppointmentViewModel>(message,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (appointmentViewModel != null)
                {
                    bool success = await ProcessBooking(appointmentViewModel);

                    if (success)
                    {
                        await _channel!.BasicAckAsync(eventArgs.DeliveryTag, false);
                        _logger.LogInformation("Processed BookingId: {BookingId}", appointmentViewModel.AppointmentId);
                    }
                    else
                    {
                        await _channel!.BasicNackAsync(eventArgs.DeliveryTag, false, true);
                        _logger.LogWarning("Failed to process BookingId: {BookingId}, requeued", appointmentViewModel.AppointmentId);
                    }
                }
                else
                {
                    await _channel!.BasicAckAsync(eventArgs.DeliveryTag, false);
                }
            }
            catch (JsonException)
            {
                _logger.LogError("Invalid JSON message: {Message}", message);
                await _channel!.BasicAckAsync(eventArgs.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
                await _channel!.BasicNackAsync(eventArgs.DeliveryTag, false, true);
            }
        }

        private async Task<bool> ProcessBooking(AppointmentViewModel bookingMessage)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDBContext>();
                    using (var factory = new EFRepositoryFactory(context))
                    {
                        var appointmentRepository = factory.GetRepository<Appointment>();
                        bool appointmentExists = await appointmentRepository.UnTrackableQuery()
                            .Where(appointment =>
                                appointment.DoctorId == bookingMessage.DoctorId
                                && appointment.StartTime <= bookingMessage.StartTime
                                && appointment.EndTime >= bookingMessage.StartTime
                            ).AnyAsync();

                        if (appointmentExists)
                            return false;
                        Appointment entity = Mapper.Map<Appointment>(bookingMessage);
                        await appointmentRepository.InsertAsync(entity);
                        await appointmentRepository.SaveChangesAsync();
                    }
                }

                _logger.LogInformation("Processing BookingId: {BookingId}", bookingMessage.AppointmentId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error for BookingId: {BookingId}", bookingMessage.AppointmentId);
                return false;
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_channel != null)
            {
                await _channel.CloseAsync();
                _channel.Dispose();
            }

            if (_connection != null)
            {
                await _connection.CloseAsync();
                _connection.Dispose();
            }

            _logger.LogInformation("BookingConsumer stopped");
        }
    }
}