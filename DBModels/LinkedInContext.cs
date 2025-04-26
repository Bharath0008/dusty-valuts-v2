using Microsoft.EntityFrameworkCore;

namespace LinkedInAPI.DBModels
{
    public class LoginContext : DbContext
    {
        public LoginContext(DbContextOptions<LoginContext> options)
            : base(options)
        {
        }

        public DbSet<LoginData> LoginData { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LoginData>().ToTable("LoginData");
        }
    }
}
