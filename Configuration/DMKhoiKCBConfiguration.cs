using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace TraCuuBHXH_BHYT.Configuration
{
    public class DMKhoiKCBConfiguration : IEntityTypeConfiguration<Entities.DMKhoiKCBEntity>
    {
        public void Configure(EntityTypeBuilder<Entities.DMKhoiKCBEntity> builder)
        {
            builder.ToTable("DMKhoiKCB");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("Id");
            builder.Property(x => x.Ma).HasMaxLength(50).HasColumnName("MA");
            builder.Property(x => x.Ten).HasMaxLength(1000).HasColumnName("TEN");
            builder.Property(x => x.Nhom).HasMaxLength(50).HasColumnName("NHOM");
            builder.Property(x => x.TinhTrang).HasMaxLength(1).HasColumnName("TINH_TRANG");
        }
    }
}
