using Microsoft.EntityFrameworkCore;
using TraCuuBHXH_BHYT.Entities;
using TraCuuBHXH_BHYT.Configuration;

namespace TraCuuBHXH_BHYT.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<ThongTinTheBHYT> ThongTinTheBHYT { get; set; }
        public DbSet<DoituongEntity> DoiTuong { get; set; }
        public DbSet<DMParameterEntity> DMParameter { get; set; }
        public DbSet<DMKhoiKCBEntity> DMKhoiKCB { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new Configuration.ThongTinTheBHYTConfiguration());
            modelBuilder.ApplyConfiguration(new Configuration.DoiTuongConfiguration());
            modelBuilder.ApplyConfiguration(new Configuration.DMParameterConfiguration());
            modelBuilder.ApplyConfiguration(new Configuration.DMKhoiKCBConfiguration());
            base.OnModelCreating(modelBuilder);
        }
    }
}