using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace RedBook.Core.AutoMapper
{
    public static class ObjectMapperExtensions
    {
        public static void AddObjectMapper(this IServiceCollection services, Assembly[] assemblies)
        {
            services.AddAutoMapper(assemblies);
            services.AddTransient<IObjectMapper, AutoMapperObjectMapper>();
        }
    }
}
