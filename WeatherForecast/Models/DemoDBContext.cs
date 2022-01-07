using Microsoft.EntityFrameworkCore;

namespace WeatherForecast
{
    public interface IDemoDBContext
    {
        DbSet<Forecast> weatherForecasts { get; set; }
    }
    public class DemoDBContext : DbContext, IDemoDBContext
    {
        public DemoDBContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Forecast> weatherForecasts { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
