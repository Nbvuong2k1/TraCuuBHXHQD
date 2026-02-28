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
            RequestSaveLog log = new RequestSaveLog();
            try
            {
                log.Type = request.type;
                log.MaTraCuu = request.maTraCuu;
                log.HoTenTraCuu = request.hoTen;
                log.NgaySinhTraCuu = request.ngaySinh;
                log.GioiTinhTraCuu = request.gioiTinh;
                log.ThoiGianTraCuu = DateTime.Now;
                
                
                // ============================
                // 1. Validate bắt buộc
                // ============================
                var missing = new List<string>();

                if (string.IsNullOrWhiteSpace(request.type)) missing.Add("type");
                if (string.IsNullOrWhiteSpace(request.maTraCuu)) missing.Add("maTraCuu");
                if (string.IsNullOrWhiteSpace(request.hoTen)) missing.Add("hoTen");
                if (string.IsNullOrWhiteSpace(request.ngaySinh)) missing.Add("ngaySinh");

                if (missing.Count > 0)
                {
                    log.KetQua = 0;
                    await TrySaveLogAsync(log);
                    return new ResponseTraCuuBHXHVN
                    {
                        maLoi = Constant.Constant.MA_LOI_THAT_BAI,
                        moTaLoi = $"Thiếu hoặc sai trường: {string.Join(", ", missing)}"
                    };
                }

                // type phải = BHYT
                if (!string.Equals(request.type, Constant.Constant.TYPE_BHYT, StringComparison.OrdinalIgnoreCase))
                {
                    log.KetQua = 0;
                    await TrySaveLogAsync(log);
                    return new ResponseTraCuuBHXHVN
                    {
                        maLoi = Constant.Constant.MA_LOI_THAT_BAI,
                        moTaLoi = "Loại yêu cầu không đúng"
                    };
                }

                if(!string.IsNullOrEmpty(request.maTraCuu) && request.maTraCuu.Length != 10 && request.maTraCuu.Length != 12 && request.maTraCuu.Length != 15)
                {
                    log.KetQua = 0;
                    await TrySaveLogAsync(log);
                    return new ResponseTraCuuBHXHVN
                    {
                        maLoi = Constant.Constant.MA_LOI_THAT_BAI,
                        moTaLoi = "maTraCuu không đúng định dạng"
                    };
                }    
                // ============================
                // 2. Bắt đầu truy vấn
                // ============================
                var query = _db.ThongTinTheBHYT
                    .Select(thongTin => new
                    {
                        ThongTin = thongTin
                    })
                    .AsQueryable();

                // So CCCD với maTraCuu (12)
                if (request.maTraCuu.Length == 12)query = query.Where(x => x.ThongTin.SoCCCD.Trim() == request.maTraCuu.Trim());

                // So MASOBHXH với maTraCuu (10)
                if (request.maTraCuu.Length == 10) query = query.Where(x => x.ThongTin.MaSoBHXH.Trim() == request.maTraCuu.Trim());

                // So Mathe với maTraCuu (15)
                if (request.maTraCuu.Length == 15) query = query.Where(x => x.ThongTin.MaTheBHYT.Trim() == request.maTraCuu.Trim());

                // So tên (không phân biệt hoa thường)
                query = query.Where(x => x.ThongTin.HoTen.Trim().ToUpper() == request.hoTen.Trim().ToUpper());

                // So giới tính
                if (!string.IsNullOrEmpty(request.gioiTinh))
                {
                    if (request.gioiTinh.Trim() == "1" || request.gioiTinh.Trim() == "0")
                    {
                        string gioitinh = request.gioiTinh == Constant.Constant.GIOI_TINH_NAM ? Constant.Constant.GIOI_TINH_NU : Constant.Constant.GIOI_TINH_NAM;
                        query = query.Where(x => x.ThongTin.GioiTinh == gioitinh);
                    }
                    else
                    {
                        log.KetQua = 0;
                        await TrySaveLogAsync(log);
                        return new ResponseTraCuuBHXHVN
                        {
                            maLoi = Constant.Constant.MA_LOI_THAT_BAI,
                            moTaLoi = "gioiTinh không đúng định dạng"
                        };
                    }
                     
                }

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
                else
                {
                    log.KetQua = 0;
                    await TrySaveLogAsync(log);
                    return new ResponseTraCuuBHXHVN
                    {
                        maLoi = Constant.Constant.MA_LOI_THAT_BAI,
                        moTaLoi = "ngaySinh không đúng định dạng"
                    };
                }
                // ============================
                // 3. Lấy dữ liệu (Order by Id desc để lấy bản ghi mới nhất nếu có nhiều bản ghi)
                // ============================
                var item = await query.OrderByDescending(x => x.ThongTin.Id).FirstOrDefaultAsync();

                if (item == null)
                {
                    log.KetQua = 0;
                    await TrySaveLogAsync(log);
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
                //string? maDT = item.DoiTuong.MaDT?.Trim().ToUpper();
                //if (Constant.Constant.MA_DT_DUOC_PHEP_LAY_DIA_CHI.Contains(maDT))
                //{
                diaChi = item.ThongTin.DiaChi;
                //}

                // ============================
                // 6. Trả về kết quả
                // ============================
                log.KetQua = item.ThongTin.Id;
                await TrySaveLogAsync(log);
                return new ResponseTraCuuBHXHVN
                {
                    soCCCD = item.ThongTin.SoCCCD,
                    hoTen = item.ThongTin.HoTen,
                    ngaySinh = item.ThongTin.NgaySinh,
                    gioiTinh = item.ThongTin.GioiTinh == Constant.Constant.GIOI_TINH_NU ? Constant.Constant.GIOI_TINH_NAM : Constant.Constant.GIOI_TINH_NU,
                    maThe = item.ThongTin.MaTheBHYT,
                    maSoBhxh = item.ThongTin.MaSoBHXH,
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
                log.KetQua = 0;
                await TrySaveLogAsync(log);
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
                log.KetQua = 0;
                await TrySaveLogAsync(log);
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

        private async Task TrySaveLogAsync(RequestSaveLog log)
        {
            try
            {
                var entity = new LogTraCuuEntity
                {
                    ThoiGianTraCuu = log.ThoiGianTraCuu,
                    MaTraCuu = log.MaTraCuu,
                    HoTenTraCuu = log.HoTenTraCuu,
                    NgaySinhTraCuu = log.NgaySinhTraCuu,
                    GioiTinhTraCuu = log.GioiTinhTraCuu,
                    Type = log.Type,
                    KetQua = log.KetQua
                };
                await _db.LogTraCuu.AddAsync(entity);
                await _db.SaveChangesAsync();
            }
            catch
            {
            }
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
                        x.Id == request.Id);

                // ============================
                // 3. Update hoặc Insert
                // ============================
                if (existingRecord != null)
                {
                    existingRecord.UpdatedDate = DateTime.Now;

                    existingRecord.IDTheBHYT = request.IDTheBHYT ?? existingRecord.IDTheBHYT;
                    //existingRecord.IDDonVi = request.IDDonVi ?? existingRecord.IDDonVi;
                    //existingRecord.IdDoiTuong = request.IDDoiTuong != null ? (short)request.IDDoiTuong : existingRecord.IdDoiTuong;
                    existingRecord.MaCSKCB = request.MaCSKCB.ToString();
                    existingRecord.MaSoBHXH = request.MaSoBHXH ?? existingRecord.MaSoBHXH;
                    existingRecord.MaTheBHYT = request.MiCardNum ?? existingRecord.MaTheBHYT;

                    existingRecord.TuNgay = request.TuNgay != null
                        ? DateOnly.FromDateTime((DateTime)request.TuNgay)
                        : existingRecord.TuNgay;

                    existingRecord.DenNgay = request.DenNgay != null
                        ? DateOnly.FromDateTime((DateTime)request.DenNgay)
                        : existingRecord.DenNgay;

                    existingRecord.Status = request.Status != null ? (byte)request.Status : existingRecord.Status;

                    existingRecord.Ngay5NamLienTuc = request.Ngay5NamLienTuc != null
                        ? DateOnly.FromDateTime((DateTime)request.Ngay5NamLienTuc)
                        : existingRecord.Ngay5NamLienTuc;


                    existingRecord.DiaChi = request.DiaChi ?? existingRecord.DiaChi;

                    existingRecord.SoCCCD = request.SoCccd?.Trim() ?? existingRecord.SoCCCD;
                    existingRecord.HoTen = request.HoTen?.Trim() ?? existingRecord.HoTen;
                    existingRecord.NgaySinh = request.NgaySinh?.Trim() ?? existingRecord.NgaySinh;
                    existingRecord.GioiTinh = gioiTinhInverted;

                    existingRecord.IsOnlyBirthYear = request.IsOnlyBirthYear ?? existingRecord.IsOnlyBirthYear;
                    existingRecord.TenBenhVien = request.TenBenhVien ?? existingRecord.TenBenhVien;
                    existingRecord.MaKCB = request.MaKCB ?? existingRecord.MaKCB;
                    existingRecord.IdDoiTuong = request.IDDoiTuong ?? existingRecord.IdDoiTuong;
                    existingRecord.IDDonVi = request.IDDonVi ?? existingRecord.IDDonVi;

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
                        ngayCapNhat = existingRecord.UpdatedDate,
                        diaChi = existingRecord.DiaChi,
                        maDoiTuong = existingRecord.MaKCB
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
                        MaCSKCB = request.IDBenhVien.ToString(),
                        MaSoBHXH = request.MaSoBHXH,
                        MaTheBHYT = request.MiCardNum,
                        TuNgay = request.TuNgay != null ? DateOnly.FromDateTime((DateTime)request.TuNgay) : null,
                        DenNgay = request.DenNgay != null ? DateOnly.FromDateTime((DateTime)request.DenNgay) : null,
                        Status = request.Status != null ? (byte)request.Status : (byte)0,
                        Ngay5NamLienTuc = request.Ngay5NamLienTuc != null ? DateOnly.FromDateTime((DateTime)request.Ngay5NamLienTuc) : null,
                        DiaChi = request.DiaChi,
                        SoCCCD = request.SoCccd.Trim(),
                        HoTen = request.HoTen.Trim(),
                        NgaySinh = request.NgaySinh.Trim(),
                        GioiTinh = gioiTinhInverted,
                        IsOnlyBirthYear = request.IsOnlyBirthYear,
                        TenBenhVien = request.TenBenhVien,
                        MaKCB = request.MaKCB,
                        IDDonVi = request.IDDonVi != null ? (long)request.IDDonVi : 0,
                        IdDoiTuong = request.IDDoiTuong != null ? (short)request.IDDoiTuong : (short)0
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
                        ngayCapNhat = newRecord.UpdatedDate,
                        diaChi = newRecord.DiaChi,
                        maDoiTuong = newRecord.MaKCB
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

        public async Task<ResponseSearchLog> SearchLogTraCuu(string role, RequestSearchLog request)
        {
            var Roles = _db.DMParameter
                   .AsNoTracking()
                   .FirstOrDefault(x => x.Key == "Role" && x.IsActive == true);
            if(role.Trim() != Roles.Value.ToString().Trim())
            {
                return new ResponseSearchLog
                {
                    Total = -1,
                    Message = "Bạn không có quyền truy cập chức năng này"
                };
            }    
            var from = request.FromDate;
            var to = request.ToDate;
            if (from > to)
            {
                var tmp = from;
                from = to;
                to = tmp;
            }
            // Bao trùm nguyên ngày ToDate
            var endExclusive = to.Date.AddDays(1);

            var query = _db.LogTraCuu.AsNoTracking()
                .Where(x => x.ThoiGianTraCuu >= from && x.ThoiGianTraCuu < endExclusive);

            if (!string.IsNullOrWhiteSpace(request.MaTraCuu))
            {
                var ma = request.MaTraCuu.Trim();
                query = query.Where(x => x.MaTraCuu != null && x.MaTraCuu.Contains(ma));
            }

            if (!string.IsNullOrWhiteSpace(request.Hoten))
            {
                var name = request.Hoten.Trim().ToUpper();
                query = query.Where(x => x.HoTenTraCuu != null && x.HoTenTraCuu.ToUpper().Contains(name));
            }

            if (!string.IsNullOrWhiteSpace(request.NgaySinh))
            {
                var ns = request.NgaySinh.Trim();
                query = query.Where(x => x.NgaySinhTraCuu != null && x.NgaySinhTraCuu.Contains(ns));
            }

            var list = await query
                .OrderByDescending(x => x.ThoiGianTraCuu)
                .Select(x => new ResponseSearchLogItem
                {
                    Id = x.Id,
                    ThoiGianTraCuu = x.ThoiGianTraCuu,
                    MaTraCuu = x.MaTraCuu,
                    HoTenTraCuu = x.HoTenTraCuu,
                    NgaySinhTraCuu = x.NgaySinhTraCuu,
                    GioiTinhTraCuu = x.GioiTinhTraCuu,
                    Type = x.Type,
                    KetQua = x.KetQua
                })
                .ToListAsync();

            return new ResponseSearchLog
            {
                Total = list.Count,
                Data = list
            };
        }
    }
}
