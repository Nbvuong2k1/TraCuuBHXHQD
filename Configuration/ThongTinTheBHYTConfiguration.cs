using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TraCuuBHXH_BHYT.Entities;

namespace TraCuuBHXH_BHYT.Configuration
{
    public class ThongTinTheBHYTConfiguration : IEntityTypeConfiguration<Entities.ThongTinTheBHYT>
    {
        public void Configure(EntityTypeBuilder<Entities.ThongTinTheBHYT> builder)
        {
            builder.ToTable("ThongTinTheBHYT");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("Id");

            builder.Property(x => x.SoCCCD).HasMaxLength(50).HasColumnName("CCCD");
            builder.Property(x => x.HoTen).HasMaxLength(200).HasColumnName("Hoten");
            builder.Property(x => x.NgaySinh).HasColumnName("NgaySinh");
            builder.Property(x => x.GioiTinh).HasColumnName("GioiTinh");
            builder.Property(x => x.MaSoBHXH).HasMaxLength(50).HasColumnName("MaSoBHXH");
            builder.Property(x => x.MaTheBHYT).HasMaxLength(50).HasColumnName("MiCardNum");
            builder.Property(x => x.TuNgay).HasColumnName("TuNgay");
            builder.Property(x => x.DenNgay).HasColumnName("DenNgay");
            builder.Property(x => x.Ngay5NamLienTuc).HasColumnName("Ngay5NamLienTuc");
            builder.Property(x => x.MaCSKCB).HasMaxLength(50).HasColumnName("IDBenhVien");
            builder.Property(x => x.TenBenhVien).HasMaxLength(200).HasColumnName("TenBenhVien");
            builder.Property(x => x.DiaChi).HasMaxLength(300).HasColumnName("DiaChi");
            builder.Property(x => x.IdDoiTuong).HasColumnName("IDDoiTuong");
            builder.Property(x => x.UpdatedDate).HasColumnName("UpdatedDate");
            builder.Property(x => x.MaKCB).HasColumnName("MiCardNumPrefix");

            // Cấu hình Foreign Key relationship với bảng DoiTuong
            builder.HasOne<DoituongEntity>()
                .WithMany()
                .HasForeignKey(x => x.IdDoiTuong)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.KhoiKCB)
               .WithMany()
               .HasForeignKey(x => x.MaKCB)
               .HasPrincipalKey(x => x.Ma);
        }
    }
}