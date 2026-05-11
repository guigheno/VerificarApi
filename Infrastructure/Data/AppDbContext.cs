using Microsoft.EntityFrameworkCore;
using VerificarApi.Domain.Entities;
using VerificarApi.Infrastructure.Data.Configurations;

namespace VerificarApi.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<FipeMarca> FipeMarcas { get; set; }
        public DbSet<FipeModelo> FipeModelos { get; set; }
        public DbSet<VehicleHistory> VehicleHistories { get; set; }
        public DbSet<VehicleAnalysis> VehicleAnalyses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new VehicleConfiguration());
        }
    }
}