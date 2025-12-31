using System.ComponentModel.DataAnnotations.Schema;

namespace TraCuuBHXH_BHYT.Entities
{
    public class ThongTinTheBHYT
    {
        public long Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public long IDTheBHYT { get; set; }
        public long IDDonVi { get; set; }

        public short IdDoiTuong { get; set; }

        public short? MaCSKCB { get; set; }   // IDBenhVien
        public long? IDHSCN { get; set; }
        public short? IDHangMucYTe { get; set; }
        public short? IDYTeTinh { get; set; }
        public short? IDHoTro { get; set; }
        public short? IDYteDoiTuon { get; set; }

        public string? UserId { get; set; }

        public string? MaSoBHXH { get; set; }

        public string? MaTheBHYT { get; set; }   // MiCardNum

        public DateOnly? TuNgay { get; set; }
        public DateOnly? DenNgay { get; set; }

        public bool? DaHetHan { get; set; }
        public byte? Status { get; set; }
        public byte? PhatHanh { get; set; }

        public string? Type { get; set; }

        public bool? DaIn { get; set; }
        public DateOnly? NgayIn { get; set; }
        public string? NguoiIn { get; set; }

        public string? MaGiam { get; set; }
        public DateOnly? NgayGiam { get; set; }

        public bool? ThuHoi { get; set; }
        public DateOnly? NgayThuHoi { get; set; }
        public string? NguoiThuHoi { get; set; }

        public int? SoThangLienTuc { get; set; }
        public DateOnly? Ngay5NamLienTuc { get; set; }

        public bool? IsLockPrint { get; set; }
        public string? UserLockPrintId { get; set; }

        public string? HoTen { get; set; }
        public string? NgaySinh { get; set; }
        public string? GioiTinh { get; set; }
        public string? SoCCCD { get; set; }

        public string? DiaChi { get; set; }

        public string? MaTinhDangSong { get; set; }
        public DateOnly? NgayPhatHanh { get; set; }

        public string? MaLoi { get; set; }
        public string? GhiChu { get; set; }

        public byte? TrangThaiPheDuyet { get; set; }
        public DateOnly? NgayPheDuyet { get; set; }

        public byte? ApproveMoveStatus { get; set; }
        public DateOnly? ApproveMoveDate { get; set; }
        public string? ApproveMoveUserId { get; set; }

        public string? ApproveUserId { get; set; }

        public string? RenewalKey { get; set; }
        public bool? IsChangedInfo { get; set; }

        public long? MiCardOldId { get; set; }

        public string? ArriveDocumentType { get; set; }
        public string? ArriveDocumentCode { get; set; }

        public bool? IsDebt { get; set; }
        public DateOnly? DebtDate { get; set; }
        public string? DebtUserId { get; set; }

        public string? ReferenceNumber { get; set; }
        public DateOnly? ReferenceDate { get; set; }

        public string? ArriveNumber { get; set; }

        public long? PersonalProfileCorrectionId { get; set; }

        public DateOnly? SynVssDate { get; set; }
        public bool? IsSynVss { get; set; }

        public short? AddressProvinceId { get; set; }
        public short? AddressDistrictId { get; set; }
        public short? AddressCommuneId { get; set; }

        public string? ReferenceNumberOrig { get; set; }
        public DateOnly? ReferenceDateOrig { get; set; }

        public byte? IsOnlyBirthYear { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string? FormattedUpdatedDate { get; set; }
        public string? SiBookNumOld { get; set; }

        public string? TenBenhVien { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string? MaKCB { get; set; }
        public DMKhoiKCBEntity KhoiKCB { get; set; }

        //public string TenDoiTuong { get; set; }

        // Navigation property đến DMKhoiKCB (link qua 2 ký tự đầu của MiCardNum = Ma)
        //public DMKhoiKCBEntity? DMKhoiKCB { get; set; }
    }
}