using TraCuuBHXH_BHYT.Entities;

namespace TraCuuBHXH_BHYT.Response
{
    public class ResponseSearchLog
    {
        public int Total { get; set; }
        public List<ResponseSearchLogItem> Data { get; set; } = new List<ResponseSearchLogItem>();
        public string Message { get; set; } = string.Empty;
    }

    public class ResponseSearchLogItem
    {
        public int Id { get; set; }
        public DateTime ThoiGianTraCuu { get; set; }
        public string? MaTraCuu { get; set; }
        public string? HoTenTraCuu { get; set; }
        public string? NgaySinhTraCuu { get; set; }
        public string? GioiTinhTraCuu { get; set; }
        public string? Type { get; set; }
        public long? KetQua { get; set; }
    }
}
