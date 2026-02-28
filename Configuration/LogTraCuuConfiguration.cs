using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TraCuuBHXH_BHYT.Configuration
{
    public class LogTraCuuConfigurtion : IEntityTypeConfiguration<Entities.LogTraCuuEntity>
    {
        public void Configure(EntityTypeBuilder<Entities.LogTraCuuEntity> builder)
        {
            builder.ToTable("LogTraCuu");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("Id");

            builder.Property(x => x.ThoiGianTraCuu).HasMaxLength(50).HasColumnName("ThoiGianTraCuu");
            builder.Property(x => x.MaTraCuu).HasMaxLength(200).HasColumnName("MaTraCuu");
            builder.Property(x => x.HoTenTraCuu).HasMaxLength(200).HasColumnName("HoTenTraCuu");
            builder.Property(x => x.NgaySinhTraCuu).HasMaxLength(200).HasColumnName("NgaySinhTraCuu");
            builder.Property(x => x.GioiTinhTraCuu).HasMaxLength(200).HasColumnName("GioiTinhTraCuu");
            builder.Property(x => x.Type).HasMaxLength(200).HasColumnName("Type");
            builder.Property(x => x.KetQua).HasMaxLength(200).HasColumnName("KetQua");
        }
    }
}
