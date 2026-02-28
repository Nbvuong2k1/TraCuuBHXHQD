namespace TraCuuBHXH_BHYT.Request
{
    public class RequestUpdateBHXHVN
    {
        // ====== Core info ======
        public long? Id { get; set; }
        // public string? Type { get; set; }

        public string? SoCccd { get; set; }
        public string? HoTen { get; set; }
        public string? NgaySinh { get; set; }
        public string? GioiTinh { get; set; }
        public string? DiaChi { get; set; }

        // ====== BHXH / BHYT ======
        public string? MaSoBHXH { get; set; }
        // public string? MaTheBHXH { get; set; }
        public string? MiCardNum { get; set; }
        // public long? MiCardOldId { get; set; }

        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }
        // public DateTime? NgayPhatHanh { get; set; }
        // public DateTime? NgayIn { get; set; }
        // public DateTime? NgayGiam { get; set; }
        public DateTime? Ngay5NamLienTuc { get; set; }

        // public bool? PhatHanh { get; set; }
        // public bool? DaIn { get; set; }
        // public bool? DaHetHan { get; set; }
        // public bool? ThuHoi { get; set; }

        // ====== Organization / Category ======
        public long? IDTheBHYT { get; set; }
        public long? IDDonVi { get; set; }
        public short? IDDoiTuong { get; set; }
        // public long? IDHoTro { get; set; }
        // public long? IDHangMucYTe { get; set; }
        // public long? IDYteDoiTuong { get; set; }
        // public long? IDYTeTinh { get; set; }
        public string? IDBenhVien { get; set; }
        public string? MaCSKCB { get; set; }

        public string? TenBenhVien { get; set; }
        public string? MaKCB { get; set; }

        // // ====== Address ======
        // public string? MaTinhDangSong { get; set; }
        // public short? AddressProvinceId { get; set; }
        // public short? AddressDistrictId { get; set; }
        // public short? AddressCommuneId { get; set; }

        // // ====== Print / Lock ======
        // public bool? IsLockPrint { get; set; }
        // public long? UserPrintedId { get; set; }
        // public long? UserLockPrintId { get; set; }

        // ====== Status / Error ======
        public int? Status { get; set; }
        // public string? MaLoi { get; set; }
        // public string? GhiChu { get; set; }

        // // ====== Approval ======
        // public byte? TrangThaiPheDuyet { get; set; }
        // public DateTime? NgayPheDuyet { get; set; }
        // public string? ApproveUserId { get; set; }
        // public byte? ApproveMoveStatus { get; set; }
        // public DateTime? ApproveMoveDate { get; set; }
        // public string? ApproveMoveUserId { get; set; }

        // ====== Revoke ======
        // public DateTime? NgayThuHoi { get; set; }
        // public long? NguoiThuHoi { get; set; }

        // // ====== Debt ======
        // public bool? IsDebt { get; set; }
        // public string? DebtUserId { get; set; }
        // public DateTime? DebtDate { get; set; }

        // ====== Sync VSS ======
        // public bool? IsSynVss { get; set; }
        // public DateTime? SynVssDate { get; set; }

        // // ====== Reference / Arrive ======
        // public string? ReferenceNumber { get; set; }
        // public DateTime? ReferenceDate { get; set; }
        // public string? ReferenceNumberOrig { get; set; }
        // public DateTime? ReferenceDateOrig { get; set; }
        // public string? ArriveNumber { get; set; }
        // public string? ArriveDocumentType { get; set; }
        // public string? ArriveDocumentCode { get; set; }

        // ====== Others ======
        // public int? SoThangLienTuc { get; set; }
        // public string? MaGiam { get; set; }
        // public bool? IsChangedInfo { get; set; }
        // public string? ChangedInfoNote { get; set; }
        // public string? RenewalKey { get; set; }
        public byte? IsOnlyBirthYear { get; set; }

        // public long? IDHSCN { get; set; }
        // public long? PersonalProfileCorrectionId { get; set; }
        // public long? UserId { get; set; }

        // ====== Audit ======
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
