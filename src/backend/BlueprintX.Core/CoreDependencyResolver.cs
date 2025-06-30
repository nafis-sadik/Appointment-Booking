using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RedBook.Core.AutoMapper;
using RedBook.Core.EntityFramework;
using RedBook.Core.Security;
using RedBook.Core.UnitOfWork;

namespace RedBook.Core.IoC
{
    public static class CoreDependencyResolver<TDbContext> where TDbContext : DbContext
    {
        public static void RosolveCoreDependencies(IServiceCollection services, IConfiguration configuration)
        {
            if (string.IsNullOrEmpty(configuration["DbConfig:DatabaseProvider"]))
                throw new ArgumentNullException("DbConfig:DatabaseProvider", "Database provider is not configured.");

            string? connectionString = configuration["DbConfig:ConnectionString"];
            if(string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException("DbConfig:ConnectionString", "Database connection string is not configured.");

            // DbContext Mapping for IOC Container
            if (configuration["DbConfig:DatabaseProvider"] == "MSSQL")
            {
                services.AddDbContext<DbContext, TDbContext>(opts =>
                {
                    opts.UseSqlServer(
                        connectionString,
                        sqlOpts => sqlOpts.MigrationsHistoryTable("__EFMigrationsHistory").UseRelationalNulls()
                    );
                });
            }
            else if (configuration["DbConfig:DatabaseProvider"] == "AZURE_SQL")
            {
                services.AddDbContext<DbContext, TDbContext>(opts =>
                {
                    opts.UseAzureSql(
                        connectionString,
                        sqlOpts => sqlOpts.MigrationsHistoryTable("__EFMigrationsHistory").UseRelationalNulls()
                    );
                });
            }
            else if (configuration["DbConfig:DatabaseProvider"] == "MYSQL")
            {
                services.AddDbContext<DbContext, TDbContext>(opts =>
                {
                    opts.UseMySQL(
                        connectionString,
                        sqlOpts => sqlOpts.MigrationsHistoryTable("__EFMigrationsHistory").UseRelationalNulls()
                    );
                });
            }
            else
            {
                throw new NotImplementedException("Database provider is not implemented yet.");
            }

            // Unit Of Work
            services.AddScoped<IRepositoryFactory, EFRepositoryFactory>();
            services.AddScoped<IUnitOfWorkManager, EFUnitOfWorkManager>();

            // Claims
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IClaimsPrincipalAccessor, HttpContextClaimsPrincipalAccessor>();

            // Object pooling
            services.AddObjectMapper(AppDomain.CurrentDomain.GetAssemblies());
        }
    }
}
