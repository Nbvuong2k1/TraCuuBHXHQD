using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TraCuuBHXH_BHYT.Entities;

namespace TraCuuBHXH_BHYT.Configuration
{
    public class DMParameterConfiguration : IEntityTypeConfiguration<DMParameterEntity>
    {
        public void Configure(EntityTypeBuilder<DMParameterEntity> builder)
        {
            builder.ToTable("DMParameters");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("Id");

            builder.Property(x => x.Key).HasMaxLength(200).HasColumnName("Key");
            builder.Property(x => x.Value).HasMaxLength(500).HasColumnName("Value");
            builder.Property(x => x.IsActive).HasColumnName("Active");
        }
    }
}

