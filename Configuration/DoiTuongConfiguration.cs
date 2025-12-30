using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TraCuuBHXH_BHYT.Entities;

namespace TraCuuBHXH_BHYT.Configuration
{
    public class DoiTuongConfiguration : IEntityTypeConfiguration<Entities.DoituongEntity>
    {
        public void Configure(EntityTypeBuilder<Entities.DoituongEntity> builder)
        {
            builder.ToTable("DMDoiTuong");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("Id");

            builder.Property(x => x.MaDT).HasMaxLength(50).HasColumnName("MaDT");
            builder.Property(x => x.TenDT).HasMaxLength(200).HasColumnName("TenDT");
        }
    }
}