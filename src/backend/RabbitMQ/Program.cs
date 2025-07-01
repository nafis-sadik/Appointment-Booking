using Rabbit.Configurations;

namespace RabbitConsumer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            string? connStr = builder.Configuration.GetSection("ConnectionStrings:RabbitMQ").ToString();
            if (string.IsNullOrWhiteSpace(connStr))
                throw new ArgumentException("Rabbit conn str missing");

            builder.Services.RosolveDependencies(builder.Configuration);

            // Register the BookingConsumer as a hosted service
            builder.Services.AddHostedService<BookingConsumer>();

            // Add any other required services
            builder.Services.AddLogging();
            
            var host = builder.Build();
            host.Run();
        }
    }
}