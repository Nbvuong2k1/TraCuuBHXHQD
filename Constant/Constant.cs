namespace TraCuuBHXH_BHYT.Constant
{
    public static class Constant
    {
        /// <summary>
        /// Mã lỗi thành công
        /// </summary>
        public const int MA_LOI_THANH_CONG = 1;

        /// <summary>
        /// Mã lỗi thất bại
        /// </summary>
        public const int MA_LOI_THAT_BAI = 2;

        /// <summary>
        /// Mã cơ quan bảo hiểm
        /// </summary>
        public const string MA_CQ_BH = "97";

        /// <summary>
        /// Tên cơ quan bảo hiểm
        /// </summary>
        public const string TEN_CQ_BH = "Bảo hiểm xã hội Quân Đội";

        /// <summary>
        /// Mã người gửi
        /// </summary>
        public const string NGUOI_GUI = "97";

        /// <summary>
        /// Loại yêu cầu tra cứu BHYT
        /// </summary>
        public const string TYPE_BHYT = "BHYT";

        /// <summary>
        /// Mã đối tượng - Thanh niên
        /// </summary>
        public const string MA_DT_TN = "TN";

        /// <summary>
        /// Mã đối tượng - Lao động hợp đồng
        /// </summary>
        public const string MA_DT_LDHD = "LDHD";

        /// <summary>
        /// Mã đối tượng - Sinh viên
        /// </summary>
        public const string MA_DT_SV = "SV";

        /// <summary>
        /// Danh sách mã đối tượng được phép lấy địa chỉ
        /// </summary>
        public static readonly string[] MA_DT_DUOC_PHEP_LAY_DIA_CHI = { MA_DT_TN, MA_DT_LDHD, MA_DT_SV };

        /// <summary>
        /// Giới tính - Nam
        /// </summary>
        public const string GIOI_TINH_NAM = "1";

        /// <summary>
        /// Giới tính - Nữ
        /// </summary>
        public const string GIOI_TINH_NU = "0";

        /// <summary>
        /// Độ dài ngày sinh - Chỉ năm (YYYY)
        /// </summary>
        public const int DO_DAI_NGAY_SINH_NAM = 4;

        /// <summary>
        /// Độ dài ngày sinh - Năm và tháng (YYYYMM)
        /// </summary>
        public const int DO_DAI_NGAY_SINH_THANG_NAM = 6;

        /// <summary>
        /// Độ dài ngày sinh - Đầy đủ (YYYYMMDD)
        /// </summary>
        public const int DO_DAI_NGAY_SINH_DAY_DU = 8;

        /// <summary>
        /// Định dạng ngày tháng
        /// </summary>
        public const string DINH_DANG_NGAY_THANG = "yyyyMMdd";
    }
}

