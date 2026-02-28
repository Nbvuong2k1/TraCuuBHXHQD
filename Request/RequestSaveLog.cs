namespace TraCuuBHXH_BHYT.Request
{
    public class RequestSaveLog
    {
        public DateTime ThoiGianTraCuu { get; set; }
        public string? MaTraCuu { get; set; }
        public string? HoTenTraCuu { get; set; }
        public string? NgaySinhTraCuu { get; set; }
        public string? GioiTinhTraCuu { get; set; }
        public string? Type { get; set; }
        public long KetQua { get; set; }
    }
}
