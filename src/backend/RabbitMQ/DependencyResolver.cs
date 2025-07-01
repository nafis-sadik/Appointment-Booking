using Data.Entities;
using RabbitMQ.Client;
using RedBook.Core.AutoMapper;
using RedBook.Core.IoC;
using RedBook.Core.Security;

namespace Rabbit.Configurations
{
    public static class DependencyResolver
    {
        public static void RosolveDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            // DB Context & Other relevant mappings for Blume Core Library
            CoreDependencyResolver<AppDBContext>.RosolveCoreDependencies(services, configuration);

            // Security context mapping
            services.AddScoped<IClaimsPrincipalAccessor, HttpContextClaimsPrincipalAccessor>();

            // Security context mapping
            services.AddObjectMapper(AppDomain.CurrentDomain.GetAssemblies());

            // Register RabbitMQ connection factory
            services.AddSingleton<IConnectionFactory>(provider =>
            {
                return new ConnectionFactory()
                {
                    HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
                    Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
                    UserName = configuration["RabbitMQ:UserName"] ?? "guest",
                    Password = configuration["RabbitMQ:Password"] ?? "guest",
                    VirtualHost = configuration["RabbitMQ:VirtualHost"] ?? "/",
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                    TopologyRecoveryEnabled = true,
                    RequestedHeartbeat = TimeSpan.FromSeconds(60)
                };
            });
        }
    }
}
