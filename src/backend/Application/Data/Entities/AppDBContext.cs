using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Data.Entities
{
    public class AppDBContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public AppDBContext(DbContextOptions<AppDBContext> options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration;
        }

        public virtual DbSet<User> Users { get; set; }
        public DbSet<Clinic> Clinics { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        //        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        //       => optionsBuilder.UseSqlServer("Data Source=localhost;Initial Catalog=AppDB;User Id=sa;Password=ABC123abc.;TrustServerCertificate=True;Encrypt=False;Trusted_Connection=False;Connection Timeout=30;");

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string? connectionString = _configuration["DbConfig:ConnectionString"];
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("Database connection string is not provided in configuration.");

            if (_configuration["DbConfig:DatabaseProvider"] == "MYSQL")
                optionsBuilder.UseMySQL(connectionString);
            else if (_configuration["DbConfig:DatabaseProvider"] == "MSSQL")
                optionsBuilder.UseSqlServer(connectionString);
            else if (_configuration["DbConfig:DatabaseProvider"] == "AZURE_SQL")
                optionsBuilder.UseSqlServer(connectionString);
            else
                throw new ArgumentException("Invalid database connection string provided.");
        }
    }
}
