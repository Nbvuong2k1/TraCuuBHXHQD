using Microsoft.EntityFrameworkCore;
using TraCuuBHXH_BHYT.Data;
using TraCuuBHXH_BHYT.Entities;
using TraCuuBHXH_BHYT.Interface;
using TraCuuBHXH_BHYT.Request;
using TraCuuBHXH_BHYT.Response;
using TraCuuBHXH_BHYT.Constant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;

namespace TraCuuBHXH_BHYT.Service
{
    public class TraCuuBHXHService : ITraCuuBHXHService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        public TraCuuBHXHService(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public async Task<ResponseTraCuuBHXHVN> TraCuuBHXHQDAsync(RequestTraCuuBHXHVN request)
        {
            try
            {
                // ============================
                // 1. Validate bắt buộc
                // ============================
                var missing = new List<string>();

                if (string.IsNullOrWhiteSpace(request.type)) missing.Add("type");
                if (string.IsNullOrWhiteSpace(request.soCccd)) missing.Add("soCccd");
                if (string.IsNullOrWhiteSpace(request.hoTen)) missing.Add("hoTen");
                if (string.IsNullOrWhiteSpace(request.ngaySinh)) missing.Add("ngaySinh");

                if (request.gioiTinh != Constant.Constant.GIOI_TINH_NU && request.gioiTinh != Constant.Constant.GIOI_TINH_NAM)
                    missing.Add("gioiTinh");

                if (missing.Count > 0)
                {
                    return new ResponseTraCuuBHXHVN
                    {
                        maLoi = Constant.Constant.MA_LOI_THAT_BAI,
                        moTaLoi = $"Thiếu hoặc sai trường: {string.Join(", ", missing)}"
                    };
                }

                // type phải = BHYT
                if (!string.Equals(request.type, Constant.Constant.TYPE_BHYT, StringComparison.OrdinalIgnoreCase))
                {
                    return new ResponseTraCuuBHXHVN
                    {
                        maLoi = Constant.Constant.MA_LOI_THAT_BAI,
                        moTaLoi = "Loại yêu cầu không đúng"
                    };
                }

                // ============================
                // 2. Bắt đầu truy vấn
                // ============================
                var query = _db.ThongTinTheBHYT
                    .Join(_db.DoiTuong,
                        thongTin => thongTin.IdDoiTuong,
                        doiTuong => doiTuong.Id,
                        (thongTin, doiTuong) => new { ThongTin = thongTin, DoiTuong = doiTuong })
                    .AsQueryable();

                // So CCCD
                query = query.Where(x => x.ThongTin.SoCCCD.Trim() == request.soCccd.Trim());

                // So tên (không phân biệt hoa thường)
                query = query.Where(x => x.ThongTin.HoTen.Trim().ToUpper() == request.hoTen.Trim().ToUpper());

                // So giới tính
                string gioitinh = request.gioiTinh == Constant.Constant.GIOI_TINH_NAM ? Constant.Constant.GIOI_TINH_NU : Constant.Constant.GIOI_TINH_NAM;
                query = query.Where(x => x.ThongTin.GioiTinh == gioitinh);

                // So ngày sinh
                if (request.ngaySinh.Trim().Length == Constant.Constant.DO_DAI_NGAY_SINH_NAM)
                {
                    query = query.Where(x => x.ThongTin.NgaySinh.Substring(0, Constant.Constant.DO_DAI_NGAY_SINH_NAM) == request.ngaySinh.Trim());
                }
                else if (request.ngaySinh.Trim().Length == Constant.Constant.DO_DAI_NGAY_SINH_THANG_NAM)
                {
                    query = query.Where(x => x.ThongTin.NgaySinh.Substring(0, Constant.Constant.DO_DAI_NGAY_SINH_THANG_NAM) == request.ngaySinh.Trim());
                }
                else if (request.ngaySinh.Trim().Length == Constant.Constant.DO_DAI_NGAY_SINH_DAY_DU)
                {
                    query = query.Where(x => x.ThongTin.NgaySinh.Trim() == request.ngaySinh.Trim());
                }
                else return new ResponseTraCuuBHXHVN
                {
                    maLoi = Constant.Constant.MA_LOI_THAT_BAI,
                    moTaLoi = "Ngày sinh không đúng định dạng"
                };

                //  query = query.Where(x => Convert.ToDateTime(x.NgaySinh) == dob);

                // Nếu có mã số BHXH thì thêm điều kiện
                if (!string.IsNullOrWhiteSpace(request.maSoBHXH))
                {
                    query = query.Where(x => x.ThongTin.MaSoBHXH == request.maSoBHXH.Trim());
                }

                // ============================
                // 3. Lấy dữ liệu (Order by Id desc để lấy bản ghi mới nhất nếu có nhiều bản ghi)
                // ============================
                var item = await query.OrderByDescending(x => x.ThongTin.Id).FirstOrDefaultAsync();

                if (item == null)
                {
                    return new ResponseTraCuuBHXHVN
                    {
                        maLoi = Constant.Constant.MA_LOI_THAT_BAI,
                        moTaLoi = "Không tìm thấy dữ liệu phù hợp"
                    };
                }

                // Load DMKhoiKCB dựa trên 2 ký tự đầu của MaTheBHYT (MiCardNum)
                DMKhoiKCBEntity? dmKhoiKCB = null;
                if (!string.IsNullOrEmpty(item.ThongTin.MaKCB))
                {
                    var maKhoi = item.ThongTin.MaKCB;
                    dmKhoiKCB = await _db.DMKhoiKCB
                        .FirstOrDefaultAsync(x => x.Ma == maKhoi);

                    // Gán vào navigation property
                    item.ThongTin.KhoiKCB.Ten = dmKhoiKCB.Ten.ToString();
                }

                // ============================
                // 4. Xác định loại ngày sinh (0 / 1 / 2)
                // ============================
                int typeBirthDay = GetBirthType(item.ThongTin.NgaySinh);

                // ============================
                // 5. Kiểm tra MaDT để quyết định có lấy địa chỉ hay không
                // ============================
                string? diaChi = null;
                string? maDT = item.DoiTuong.MaDT?.Trim().ToUpper();
                if (Constant.Constant.MA_DT_DUOC_PHEP_LAY_DIA_CHI.Contains(maDT))
                {
                    diaChi = item.ThongTin.DiaChi;
                }

                // ============================
                // 6. Trả về kết quả
                // ============================
                return new ResponseTraCuuBHXHVN
                {
                    soCCCD = item.ThongTin.SoCCCD,
                    hoTen = item.ThongTin.HoTen,
                    ngaySinh = item.ThongTin.NgaySinh,
                    gioiTinh = item.ThongTin.GioiTinh == Constant.Constant.GIOI_TINH_NU ? Constant.Constant.GIOI_TINH_NAM : Constant.Constant.GIOI_TINH_NU,
                    maThe = item.ThongTin.MaTheBHYT,
                    tuNgay = item.ThongTin.TuNgay?.ToString(Constant.Constant.DINH_DANG_NGAY_THANG),
                    denNgay = item.ThongTin.DenNgay?.ToString(Constant.Constant.DINH_DANG_NGAY_THANG),
                    ngay5NamLienTuc = item.ThongTin.Ngay5NamLienTuc?.ToString(Constant.Constant.DINH_DANG_NGAY_THANG),
                    maCSKCB = item.ThongTin.MaCSKCB,
                    tenBenhVien = item.ThongTin.TenBenhVien,
                    diaChi = diaChi,
                    maCQBH = Constant.Constant.MA_CQ_BH,
                    tenCQBH = Constant.Constant.TEN_CQ_BH,
                    namSinh = typeBirthDay.ToString(),
                    maLoi = Constant.Constant.MA_LOI_THANH_CONG,
                    moTaLoi = null,
                    nguoiGui = Constant.Constant.NGUOI_GUI,
                    ngayCapNhat = item.ThongTin.UpdatedDate,
                    maDoiTuong = item.ThongTin.MaKCB,
                    tenDoiTuong = item.ThongTin.KhoiKCB?.Ten,
                };
            }
            catch (DbUpdateException dbEx)
            {
                // Lỗi khi cập nhật database
                return new ResponseTraCuuBHXHVN
                {
                    maLoi = Constant.Constant.MA_LOI_THAT_BAI,
                    moTaLoi = "Lỗi kết nối cơ sở dữ liệu. Vui lòng thử lại sau."
                };
            }
            catch (Exception ex)
            {
                // Lỗi chung
                return new ResponseTraCuuBHXHVN
                {
                    maLoi = Constant.Constant.MA_LOI_THAT_BAI,
                    moTaLoi = "Đã xảy ra lỗi trong quá trình xử lý. Vui lòng thử lại sau."
                };
            }
        }
        private int GetBirthType(string ngaySinh)
        {
            if (string.IsNullOrEmpty(ngaySinh) || ngaySinh.Length != Constant.Constant.DO_DAI_NGAY_SINH_DAY_DU)
                return 0;

            if (ngaySinh.Substring(4) == "0101") return 1;   // Chỉ năm
            if (ngaySinh.EndsWith("01")) return 2;           // Chỉ tháng + năm

            return 0;  // Đủ ngày
        }

        public async Task<ResponseUpdateBHXHVN> ThemHoacCapNhatAsync(RequestUpdateBHXHVN request)
        {
            try
            {
                // ============================
                // 1. Validate bắt buộc
                // ============================
                var missing = new List<string>();

                if (string.IsNullOrWhiteSpace(request.SoCccd)) missing.Add("soCccd");
                if (string.IsNullOrWhiteSpace(request.HoTen)) missing.Add("hoTen");

                if (request.GioiTinh != Constant.Constant.GIOI_TINH_NU && request.GioiTinh != Constant.Constant.GIOI_TINH_NAM)
                    missing.Add("gioiTinh");

                if (missing.Count > 0)
                {
                    return new ResponseUpdateBHXHVN
                    {
                        maLoi = Constant.Constant.MA_LOI_THAT_BAI,
                        moTaLoi = $"Thiếu hoặc sai trường: {string.Join(", ", missing)}"
                    };
                }

                // ============================
                // 2. Kiểm tra xem dữ liệu đã tồn tại chưa (dựa trên CCCD + HỌ tên + giới tính + ngày sinh)
                // ============================
                string gioiTinhInverted = request.GioiTinh == Constant.Constant.GIOI_TINH_NAM ? Constant.Constant.GIOI_TINH_NU : Constant.Constant.GIOI_TINH_NAM;
                
                var existingRecord = await _db.ThongTinTheBHYT
                    .FirstOrDefaultAsync(x =>
                        x.SoCCCD.Trim() == request.SoCccd.Trim() &&
                        x.HoTen.Trim().ToUpper() == request.HoTen.Trim().ToUpper() &&
                        x.GioiTinh == gioiTinhInverted &&
                        x.NgaySinh.Trim() == request.NgaySinh.Trim());

                // ============================
                // 3. Update hoặc Insert
                // ============================
                if (existingRecord != null)
                {
                    existingRecord.UpdatedDate = DateTime.Now;

                    existingRecord.IDTheBHYT = request.IDTheBHYT ?? existingRecord.IDTheBHYT;
                    existingRecord.IDDonVi = request.IDDonVi ?? existingRecord.IDDonVi;
                    existingRecord.IdDoiTuong = request.IDDoiTuong != null ? (short)request.IDDoiTuong : existingRecord.IdDoiTuong;
                    existingRecord.MaCSKCB = request.IDBenhVien != null ? (short)request.IDBenhVien : existingRecord.MaCSKCB;
                    existingRecord.IDHSCN = request.IDHSCN ?? existingRecord.IDHSCN;
                    existingRecord.IDHangMucYTe = request.IDHangMucYTe != null ? (short)request.IDHangMucYTe : existingRecord.IDHangMucYTe;
                    existingRecord.IDYTeTinh = request.IDYTeTinh != null ? (short)request.IDYTeTinh : existingRecord.IDYTeTinh;
                    existingRecord.IDHoTro = request.IDHoTro != null ? (short)request.IDHoTro : existingRecord.IDHoTro;
                    existingRecord.IDYteDoiTuon = request.IDYteDoiTuong != null ? (short)request.IDYteDoiTuong : existingRecord.IDYteDoiTuon;

                    existingRecord.UserId = request.UserId?.ToString() ?? existingRecord.UserId;
                    existingRecord.MaSoBHXH = request.MaSoBHXH ?? existingRecord.MaSoBHXH;
                    existingRecord.MaTheBHYT = request.MiCardNum ?? existingRecord.MaTheBHYT;

                    existingRecord.TuNgay = request.TuNgay != null
                        ? DateOnly.FromDateTime((DateTime)request.TuNgay)
                        : existingRecord.TuNgay;

                    existingRecord.DenNgay = request.DenNgay != null
                        ? DateOnly.FromDateTime((DateTime)request.DenNgay)
                        : existingRecord.DenNgay;

                    existingRecord.DaHetHan = request.DaHetHan ?? existingRecord.DaHetHan;
                    existingRecord.Status = request.Status != null ? (byte)request.Status : existingRecord.Status;
                    existingRecord.PhatHanh = request.PhatHanh != null ? (byte)(request.PhatHanh == true ? 1 : 0) : existingRecord.PhatHanh;

                    existingRecord.DaIn = request.DaIn ?? existingRecord.DaIn;
                    existingRecord.NgayIn = request.NgayIn != null ? DateOnly.FromDateTime((DateTime)request.NgayIn) : existingRecord.NgayIn;
                    existingRecord.NguoiIn = request.UserPrintedId?.ToString() ?? existingRecord.NguoiIn;

                    existingRecord.MaGiam = request.MaLoi ?? existingRecord.MaGiam;
                    existingRecord.NgayGiam = request.NgayGiam != null ? DateOnly.FromDateTime((DateTime)request.NgayGiam) : existingRecord.NgayGiam;

                    existingRecord.ThuHoi = request.ThuHoi ?? existingRecord.ThuHoi;
                    existingRecord.NgayThuHoi = request.NgayThuHoi != null ? DateOnly.FromDateTime((DateTime)request.NgayThuHoi) : existingRecord.NgayThuHoi;
                    existingRecord.NguoiThuHoi = request.UserId?.ToString() ?? existingRecord.NguoiThuHoi;

                    existingRecord.SoThangLienTuc = request.SoThangLienTuc ?? existingRecord.SoThangLienTuc;
                    existingRecord.Ngay5NamLienTuc = request.Ngay5NamLienTuc != null
                        ? DateOnly.FromDateTime((DateTime)request.Ngay5NamLienTuc)
                        : existingRecord.Ngay5NamLienTuc;

                    existingRecord.IsLockPrint = request.IsLockPrint ?? existingRecord.IsLockPrint;
                    existingRecord.UserLockPrintId = request.UserLockPrintId?.ToString() ?? existingRecord.UserLockPrintId;

                    existingRecord.DiaChi = request.DiaChi ?? existingRecord.DiaChi;
                    existingRecord.MaTinhDangSong = request.MaTinhDangSong ?? existingRecord.MaTinhDangSong;

                    existingRecord.SoCCCD = request.SoCccd?.Trim() ?? existingRecord.SoCCCD;
                    existingRecord.HoTen = request.HoTen?.Trim() ?? existingRecord.HoTen;
                    existingRecord.NgaySinh = request.NgaySinh?.Trim() ?? existingRecord.NgaySinh;
                    existingRecord.GioiTinh = gioiTinhInverted;

                    existingRecord.NgayPhatHanh = request.NgayPhatHanh != null
                        ? DateOnly.FromDateTime((DateTime)request.NgayPhatHanh)
                        : existingRecord.NgayPhatHanh;

                    existingRecord.GhiChu = request.GhiChu ?? existingRecord.GhiChu;
                    existingRecord.TrangThaiPheDuyet = request.TrangThaiPheDuyet ?? existingRecord.TrangThaiPheDuyet;
                    existingRecord.NgayPheDuyet = request.NgayPheDuyet != null
                        ? DateOnly.FromDateTime((DateTime)request.NgayPheDuyet)
                        : existingRecord.NgayPheDuyet;

                    existingRecord.ApproveMoveDate = request.ApproveMoveDate != null
                        ? DateOnly.FromDateTime((DateTime)request.ApproveMoveDate)
                        : existingRecord.ApproveMoveDate;

                    existingRecord.ApproveUserId = request.ApproveUserId ?? existingRecord.ApproveUserId;
                    existingRecord.ApproveMoveStatus = request.ApproveMoveStatus ?? existingRecord.ApproveMoveStatus;
                    existingRecord.ApproveMoveUserId = request.ApproveMoveUserId?.ToString() ?? existingRecord.ApproveMoveUserId;

                    existingRecord.RenewalKey = request.RenewalKey ?? existingRecord.RenewalKey;
                    existingRecord.IsChangedInfo = request.IsChangedInfo ?? existingRecord.IsChangedInfo;
                    existingRecord.MiCardOldId = request.MiCardOldId ?? existingRecord.MiCardOldId;

                    existingRecord.ArriveDocumentType = request.ArriveDocumentType ?? existingRecord.ArriveDocumentType;
                    existingRecord.ArriveDocumentCode = request.ArriveDocumentCode ?? existingRecord.ArriveDocumentCode;

                    existingRecord.IsDebt = request.IsDebt ?? existingRecord.IsDebt;
                    existingRecord.DebtDate = request.DebtDate != null
                        ? DateOnly.FromDateTime((DateTime)request.DebtDate)
                        : existingRecord.DebtDate;

                    existingRecord.DebtUserId = request.DebtUserId ?? existingRecord.DebtUserId;
                    existingRecord.ReferenceNumber = request.ReferenceNumber ?? existingRecord.ReferenceNumber;
                    existingRecord.ArriveNumber = request.ArriveNumber ?? existingRecord.ArriveNumber;

                    existingRecord.PersonalProfileCorrectionId =
                        request.PersonalProfileCorrectionId ?? existingRecord.PersonalProfileCorrectionId;

                    existingRecord.IsSynVss = request.IsSynVss ?? existingRecord.IsSynVss;
                    existingRecord.AddressProvinceId = request.AddressProvinceId ?? existingRecord.AddressProvinceId;
                    existingRecord.AddressDistrictId = request.AddressDistrictId ?? existingRecord.AddressDistrictId;
                    existingRecord.AddressCommuneId = request.AddressCommuneId ?? existingRecord.AddressCommuneId;

                    existingRecord.ReferenceNumberOrig = request.ReferenceNumberOrig ?? existingRecord.ReferenceNumberOrig;
                    existingRecord.ReferenceDateOrig = request.ReferenceDateOrig != null
                        ? DateOnly.FromDateTime((DateTime)request.ReferenceDateOrig)
                        : existingRecord.ReferenceDateOrig;

                    existingRecord.IsOnlyBirthYear = request.IsOnlyBirthYear ?? existingRecord.IsOnlyBirthYear;
                    existingRecord.TenBenhVien = request.TenBenhVien ?? existingRecord.TenBenhVien;

                    _db.ThongTinTheBHYT.Update(existingRecord);
                    await _db.SaveChangesAsync();

                    return new ResponseUpdateBHXHVN
                    {
                        maLoi = Constant.Constant.MA_LOI_THANH_CONG,
                        moTaLoi = "Cập nhật dữ liệu thành công",
                        soCCCD = existingRecord.SoCCCD,
                        hoTen = existingRecord.HoTen,
                        ngaySinh = existingRecord.NgaySinh,
                        gioiTinh = existingRecord.GioiTinh == Constant.Constant.GIOI_TINH_NU ? Constant.Constant.GIOI_TINH_NAM : Constant.Constant.GIOI_TINH_NU,
                        maThe = existingRecord.MaTheBHYT,
                        nguoiGui = Constant.Constant.NGUOI_GUI,
                        ngayCapNhat = existingRecord.UpdatedDate
                    };
                }
                else
                {
                    // Thêm mới dữ liệu
                    var newRecord = new ThongTinTheBHYT
                    {
                        Id = (long)request.Id,
                        UpdatedDate = request.UpdatedDate != null ? request.UpdatedDate : DateTime.Now,
                        CreatedDate = request.CreatedDate != null ? request.CreatedDate : DateTime.Now,
                        IDTheBHYT = request.IDTheBHYT != null ? (long)request.IDTheBHYT : 0,
                        IDDonVi = request.IDDonVi != null ? (long)request.IDDonVi : 0,
                        IdDoiTuong = request.IDDoiTuong != null ? (short)request.IDDoiTuong : (short)0,
                        MaCSKCB = request.IDBenhVien != null ? (short)request.IDBenhVien : (short)0,
                        IDHSCN = request.IDHSCN != null ? (long)request.IDHSCN : 0,
                        IDHangMucYTe = request.IDHangMucYTe != null ? (short)request.IDHangMucYTe : (short)0,
                        IDYTeTinh = request.IDYTeTinh != null ? (short)request.IDYTeTinh : (short)0,
                        IDHoTro = request.IDHoTro != null ? (short)request.IDHoTro : (short)0,
                        IDYteDoiTuon = request.IDYteDoiTuong != null ? (short)request.IDYteDoiTuong : (short)0,
                        UserId = request.UserId != null ? request.UserId.ToString() : null,
                        MaSoBHXH = request.MaSoBHXH,
                        MaTheBHYT = request.MiCardNum,
                        TuNgay = request.TuNgay != null ? DateOnly.FromDateTime((DateTime)request.TuNgay) : null,
                        DenNgay = request.DenNgay != null ? DateOnly.FromDateTime((DateTime)request.DenNgay) : null,
                        DaHetHan = request.DaHetHan,
                        Status = request.Status != null ? (byte)request.Status : (byte)0,
                        PhatHanh = request.PhatHanh != null ? (byte)(request.PhatHanh == true ? 1 : 0) : (byte)0,
                        //Type = request.Type,
                        DaIn = request.DaIn,
                        NgayIn = request.NgayIn != null ? DateOnly.FromDateTime((DateTime)request.NgayIn) : null,
                        NguoiIn = request.UserPrintedId != null ? request.UserPrintedId.ToString() : null,
                        MaGiam = request.MaLoi,
                        NgayGiam = request.NgayGiam != null ? DateOnly.FromDateTime((DateTime)request.NgayGiam) : null,
                        ThuHoi = request.ThuHoi,
                        NgayThuHoi = request.NgayGiam != null ? DateOnly.FromDateTime((DateTime)request.NgayGiam) : null,
                        NguoiThuHoi = request.UserId != null ? request.UserId.ToString()
                        : null,
                        SoThangLienTuc = request.SoThangLienTuc != null ? (int)request.SoThangLienTuc : 0,
                        Ngay5NamLienTuc = request.Ngay5NamLienTuc != null ? DateOnly.FromDateTime((DateTime)request.Ngay5NamLienTuc) : null,
                        IsLockPrint = request.IsLockPrint,
                        UserLockPrintId = request.UserLockPrintId != null ? request.UserLockPrintId.ToString() : null,
                        DiaChi = request.DiaChi,
                        MaTinhDangSong = request.MaTinhDangSong,
                        SoCCCD = request.SoCccd.Trim(),
                        HoTen = request.HoTen.Trim(),
                        NgaySinh = request.NgaySinh.Trim(),
                        GioiTinh = gioiTinhInverted,
                        NgayPhatHanh = request.NgayPhatHanh != null ? DateOnly.FromDateTime((DateTime)request.NgayPhatHanh) : null, 
                        MaLoi = request.MaLoi,
                        GhiChu = request.GhiChu,
                        TrangThaiPheDuyet = request.TrangThaiPheDuyet,
                        NgayPheDuyet = request.NgayPheDuyet != null ? DateOnly.FromDateTime((DateTime)request.NgayPheDuyet) : null ,
                        ApproveMoveDate = request.ApproveMoveDate != null ? DateOnly.FromDateTime((DateTime)request.ApproveMoveDate) : null,
                        ApproveUserId = request.ApproveUserId,
                        ApproveMoveStatus = request.ApproveMoveStatus,
                        ApproveMoveUserId = request.ApproveMoveUserId != null ? request.ApproveMoveUserId.ToString() : null,
                        RenewalKey = request.RenewalKey,
                        IsChangedInfo = request.IsChangedInfo,
                        MiCardOldId = request.MiCardOldId,
                        ArriveDocumentType = request.ArriveDocumentType,
                        ArriveDocumentCode = request.ArriveDocumentCode,
                        IsDebt = request.IsDebt,
                        DebtDate = request.DebtDate != null ? DateOnly.FromDateTime((DateTime)request.DebtDate) : null ,
                        DebtUserId = request.DebtUserId,
                        ReferenceNumber = request.ReferenceNumber,
                        ArriveNumber = request.ArriveNumber,
                        PersonalProfileCorrectionId = request.PersonalProfileCorrectionId != null ? (long)request.PersonalProfileCorrectionId : 0,
                        IsSynVss = request.IsSynVss,
                        AddressProvinceId = request.AddressProvinceId,
                        AddressDistrictId = request.AddressDistrictId,
                        AddressCommuneId = request.AddressCommuneId,
                        ReferenceNumberOrig = request.ReferenceNumberOrig,
                        ReferenceDateOrig = request.ReferenceDateOrig != null ? DateOnly.FromDateTime((DateTime)request.ReferenceDateOrig) : null ,
                        IsOnlyBirthYear = request.IsOnlyBirthYear,
                        // FormattedUpdatedDate = request.UpdatedDate.ToString("yyyyMM"),
                        TenBenhVien = request.TenBenhVien,
                    };

                    _db.ThongTinTheBHYT.Add(newRecord);
                    await _db.SaveChangesAsync();

                    return new ResponseUpdateBHXHVN
                    {
                        maLoi = Constant.Constant.MA_LOI_THANH_CONG,
                        moTaLoi = "Thêm dữ liệu thành công",
                        soCCCD = newRecord.SoCCCD,
                        hoTen = newRecord.HoTen,
                        ngaySinh = newRecord.NgaySinh,
                        gioiTinh = newRecord.GioiTinh == Constant.Constant.GIOI_TINH_NU ? Constant.Constant.GIOI_TINH_NAM : Constant.Constant.GIOI_TINH_NU,
                        maThe = newRecord.MaSoBHXH,
                        nguoiGui = Constant.Constant.NGUOI_GUI,
                        ngayCapNhat = newRecord.UpdatedDate
                    };
                }
            }
            catch (DbUpdateException dbEx)
            {
                return new ResponseUpdateBHXHVN
                {
                    maLoi = Constant.Constant.MA_LOI_THAT_BAI,
                    moTaLoi = "Lỗi kết nối cơ sở dữ liệu. Vui lòng thử lại sau."
                };
            }
            catch (Exception ex)
            {
                return new ResponseUpdateBHXHVN
                {
                    maLoi = Constant.Constant.MA_LOI_THAT_BAI,
                    moTaLoi = "Đã xảy ra lỗi trong quá trình xử lý. Vui lòng thử lại sau."
                };
            }
        }
    }
}
