using QuanLySanBongMini.Helpers;
using QuanLySanBongMini.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace QuanLySanBongMini.ViewModels
{
    // Lớp trung gian để hiển thị thêm cột "Số lần đặt sân" mà model gốc không có
    public class KhachHangDTO
    {
        public int MaKH { get; set; }
        public string TenKH { get; set; }
        public string SoDienThoai { get; set; }
        public int SoLanDatSan { get; set; }
    }

    public class KhachHangViewModel : BaseViewModel
    {
        private ObservableCollection<KhachHangDTO> _danhSachKhachHang;
        public ObservableCollection<KhachHangDTO> DanhSachKhachHang
        {
            get { return _danhSachKhachHang; }
            set { _danhSachKhachHang = value; OnPropertyChanged(); }
        }

        private KhachHangDTO _khachHangDangChon;
        public KhachHangDTO KhachHangDuocChon
        {
            get { return _khachHangDangChon; }
            set
            {
                _khachHangDangChon = value;
                OnPropertyChanged();
                if (value != null) DienThongTinLenForm(value);
            }
        }

        private string _maKH;
        public string MaKH
        {
            get { return _maKH; }
            set { _maKH = value; OnPropertyChanged(); }
        }

        private string _tenKH;
        public string TenKH
        {
            get { return _tenKH; }
            set { _tenKH = value; OnPropertyChanged(); }
        }

        private string _soDienThoai;
        public string SoDienThoai
        {
            get { return _soDienThoai; }
            set { _soDienThoai = value; OnPropertyChanged(); }
        }

        public RelayCommand ThemCommand { get; set; }
        public RelayCommand SuaCommand { get; set; }
        public RelayCommand XoaCommand { get; set; }
        public RelayCommand LamMoiCommand { get; set; }

        public KhachHangViewModel()
        {
            TaiDuLieu();
            ThemCommand   = new RelayCommand(p => ThemKhachHang(), p => true);
            SuaCommand    = new RelayCommand(p => SuaKhachHang(),  p => true);
            XoaCommand    = new RelayCommand(p => XoaKhachHang(),  p => true);
            LamMoiCommand = new RelayCommand(p => XoaTrang(),      p => true);
        }

        private void TaiDuLieu()
        {
            try
            {
                using (var db = new QLYSANBONGMINIEntities())
                {
                    var danhSach = db.KhachHangs
                        .Select(k => new KhachHangDTO
                        {
                            MaKH        = k.MaKH,
                            TenKH       = k.HoTen,
                            SoDienThoai = k.SoDT,
                            SoLanDatSan = k.DatSans.Count()
                        })
                        .OrderBy(k => k.MaKH)
                        .ToList();

                    DanhSachKhachHang = new ObservableCollection<KhachHangDTO>(danhSach);
                }
            }
            catch
            {
                MessageBox.Show("Không thể tải dữ liệu khách hàng!", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DienThongTinLenForm(KhachHangDTO kh)
        {
            MaKH        = kh.MaKH.ToString();
            TenKH       = kh.TenKH;
            SoDienThoai = kh.SoDienThoai;
        }

        private void ThemKhachHang()
        {
            if (string.IsNullOrWhiteSpace(TenKH))       { MessageBox.Show("Vui lòng nhập tên!"); return; }
            if (string.IsNullOrWhiteSpace(SoDienThoai)) { MessageBox.Show("Vui lòng nhập số điện thoại!"); return; }

            try
            {
                using (var db = new QLYSANBONGMINIEntities())
                {
                    bool daTonTai = db.KhachHangs.Any(k => k.SoDT == SoDienThoai.Trim());
                    if (daTonTai) { MessageBox.Show("Số điện thoại đã tồn tại!"); return; }

                    var khachMoi = new KhachHang();
                    khachMoi.HoTen      = TenKH.Trim();
                    khachMoi.SoDT       = SoDienThoai.Trim();
                    khachMoi.NgayDangKy = DateTime.Now;
                    db.KhachHangs.Add(khachMoi);
                    db.SaveChanges();
                }
                MessageBox.Show("Thêm khách hàng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                XoaTrang(); TaiDuLieu();
            }
            catch { MessageBox.Show("Lỗi khi thêm khách hàng!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void SuaKhachHang()
        {
            if (KhachHangDuocChon == null) { MessageBox.Show("Vui lòng chọn khách hàng cần sửa!"); return; }
            if (string.IsNullOrWhiteSpace(TenKH)) { MessageBox.Show("Vui lòng nhập tên!"); return; }

            try
            {
                using (var db = new QLYSANBONGMINIEntities())
                {
                    var kh = db.KhachHangs.Find(KhachHangDuocChon.MaKH);
                    if (kh == null) return;
                    kh.HoTen = TenKH.Trim();
                    kh.SoDT  = SoDienThoai?.Trim();
                    db.SaveChanges();
                }
                MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                XoaTrang(); TaiDuLieu();
            }
            catch { MessageBox.Show("Lỗi khi cập nhật!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void XoaKhachHang()
        {
            if (KhachHangDuocChon == null) { MessageBox.Show("Vui lòng chọn khách hàng!"); return; }

            string thongBao = KhachHangDuocChon.SoLanDatSan > 0
                ? "Khách \"" + KhachHangDuocChon.TenKH + "\" có " + KhachHangDuocChon.SoLanDatSan +
                  " lần đặt sân. Toàn bộ lịch sử cũng sẽ bị xóa!\nBạn có chắc không?"
                : "Xóa khách hàng \"" + KhachHangDuocChon.TenKH + "\"?";

            if (MessageBox.Show(thongBao, "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning)
                != MessageBoxResult.Yes) return;

            try
            {
                using (var db = new QLYSANBONGMINIEntities())
                {
                    var kh = db.KhachHangs.Find(KhachHangDuocChon.MaKH);
                    if (kh == null) return;

                    // Xóa theo thứ tự: ChiTiet → HoaDon → DatSan → KhachHang
                    var dsDatSan = db.DatSans.Where(d => d.MaKH == kh.MaKH).ToList();
                    foreach (var ds in dsDatSan)
                    {
                        var dsHD = db.HoaDons.Where(h => h.MaDatSan == ds.MaDatSan).ToList();
                        foreach (var hd in dsHD)
                        {
                            var ct = db.ChiTietHoaDons.Where(c => c.MaHD == hd.MaHD).ToList();
                            db.ChiTietHoaDons.RemoveRange(ct);
                        }
                        db.HoaDons.RemoveRange(dsHD);

                        var san = db.SanBongs.Find(ds.MaSan);
                        if (san != null && san.TrangThai != "Trống") san.TrangThai = "Trống";
                    }
                    db.DatSans.RemoveRange(dsDatSan);
                    db.KhachHangs.Remove(kh);
                    db.SaveChanges();
                }
                MessageBox.Show("Xóa thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                XoaTrang(); TaiDuLieu();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void XoaTrang()
        {
            KhachHangDuocChon = null;
            MaKH = string.Empty;
            TenKH = string.Empty;
            SoDienThoai = string.Empty;
        }
    }
}
