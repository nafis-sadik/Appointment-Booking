using WebAPI.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using RedBook.Core.Constants;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;

namespace Inventory.WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // Add services to the container. 
            builder.Services.InitializeCommonConstants(builder.Configuration);

            // Cross-Origin Requests (CORS)
            builder.Services.AddCors(builder.Configuration);

            // IoC Container & DbContext
            builder.Services.RosolveDependencies(builder.Configuration);

            builder.Services.AddControllers(options =>
            {
                options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
                //options.Filters.Add<HttpResponseExceptionFilter>();
            }).ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                    new BadRequestObjectResult(context.ModelState)
                    {
                        ContentTypes =
                        {
                                // using static System.Net.Mime.MediaTypeNames;
                                Application.Json,
                                Application.Xml
                        }
                    };
            }).AddXmlSerializerFormatters();

            // JWT Bearer token based authentication
            builder.Services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(CommonConstants.SecurityConfig.SaltByte),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            // Swagger Configurations
            builder.Services.AddSwaggerGen(x =>
            {
                x.SwaggerDoc("v0.1", new OpenApiInfo
                {
                    Title = "Web API",
                    Version = "v0.1",
                    Description = "A JWT based user management system to authenticate and manage users, their roles, rights and other user related features for all applications developed and maintained by Blume Digital Corp.",
                    TermsOfService = new Uri("https://example.com/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "Example Contact",
                        Url = new Uri("https://example.com/contact")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Example License",
                        Url = new Uri("https://example.com/license")
                    }
                });

                x.CustomOperationIds(apiDesc =>
                {
                    var controllerAction = apiDesc.ActionDescriptor as ControllerActionDescriptor;
                    return controllerAction!.ControllerName + "-" + controllerAction.ActionName;
                });

                foreach (var file in Directory.GetFiles(AppContext.BaseDirectory, "*.xml"))
                {
                    x.IncludeXmlComments(file);
                }

                x.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Format: Bearer {access_token}",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                x.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    {
                        new OpenApiSecurityScheme{
                            Reference=new OpenApiReference{
                                Id="Bearer", //The name of the previously defined security scheme.
                                Type=ReferenceType.SecurityScheme
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                // Configure Swagger to use the XML file that's generated with the preceding instructions.
                // For Linux or non-Windows operating systems, file names and paths can be case-sensitive.
                // For example, a TodoApi.XML file is valid on Windows but not CentOS.
                x.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
            });

            WebApplication app = builder.Build();

            // Database Initialization
            app.InitDatabase(builder.Environment);

            app.UseCors(CorsConfig.Policy);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v0.1/swagger.json", "My API V0.1");
                });
            }

            app.UseHttpsRedirection();

            app.UseMiddleware<ExceptionMiddleware>();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
