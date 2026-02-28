namespace TraCuuBHXH_BHYT.Request
{
    public class RequestSearchLog
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string? MaTraCuu { get; set; }
        public string? Hoten { get; set; }
        public string? NgaySinh { get; set; }
    }
}
