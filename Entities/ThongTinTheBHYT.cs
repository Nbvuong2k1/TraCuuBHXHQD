namespace TraCuuBHXH_BHYT.Entities
{
    public class ThongTinTheBHYT
    {
        public long Id { get; set; }
        public string? SoCCCD { get; set; }
        public string? HoTen { get; set; }
        public string? NgaySinh { get; set; }
        public string? GioiTinh { get; set; }
        public string? MaSoBHXH { get; set; }
        public string? MaTheBHYT { get; set; }
        public DateOnly? TuNgay { get; set; }
        public DateOnly? DenNgay { get; set; }
        public DateOnly? Ngay5NamLienTuc { get; set; }
        public short? MaCSKCB { get; set; }
        public string? TenBenhVien { get; set; }
        public string? DiaChi { get; set; }
        public short IdDoiTuong { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string MaKCB { get; set; }
        public DMKhoiKCBEntity KhoiKCB { get; set; }

        //public string TenDoiTuong { get; set; }

        // Navigation property đến DMKhoiKCB (link qua 2 ký tự đầu của MiCardNum = Ma)
        //public DMKhoiKCBEntity? DMKhoiKCB { get; set; }
    }
}