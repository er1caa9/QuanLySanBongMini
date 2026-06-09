using QuanLySanBongMini.Helpers;
using QuanLySanBongMini.Models;
using QuanLySanBongMini.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace QuanLySanBongMini.ViewModels
{
    public class DatSanViewModel : BaseViewModel
    {
        // Mã nhân viên đang đăng nhập, dùng khi tạo hóa đơn
        // static để các ViewModel khác cũng dùng được
        public static int MaNVHienTai { get; set; } = 1;

        // Danh sách sân bóng hiển thị dạng card
        private ObservableCollection<SanBong> _danhSachSan;
        public ObservableCollection<SanBong> DanhSachSan
        {
            get { return _danhSachSan; }
            set { _danhSachSan = value; OnPropertyChanged(); }
        }

        // Sân đang được chọn
        private SanBong _sanDangChon;
        public SanBong SanDangChon
        {
            get { return _sanDangChon; }
            set { _sanDangChon = value; OnPropertyChanged(); }
        }

        // Danh sách giờ cho ComboBox (từ 05:00 đến 23:00, bước 30 phút)
        public List<string> DanhSachGio { get; } = TaoKhungGio();

        private static List<string> TaoKhungGio()
        {
            var danhSach = new List<string>();
            for (int gio = 5; gio <= 23; gio++)
            {
                danhSach.Add(string.Format("{0:D2}:00", gio));
                if (gio < 23)
                    danhSach.Add(string.Format("{0:D2}:30", gio));
            }
            return danhSach;
        }

        // Ngày đặt sân (chọn từ DatePicker)
        private DateTime _ngayDat = DateTime.Today;
        public DateTime NgayLoc
        {
            get { return _ngayDat; }
            set { _ngayDat = value; OnPropertyChanged(); }
        }

        // Thông tin khách hàng
        private string _tenKhachHang;
        public string TenKhachHang
        {
            get { return _tenKhachHang; }
            set { _tenKhachHang = value; OnPropertyChanged(); }
        }

        private string _soDienThoai;
        public string SoDienThoai
        {
            get { return _soDienThoai; }
            set { _soDienThoai = value; OnPropertyChanged(); }
        }

        // Giờ bắt đầu và kết thúc
        private string _gioBatDau;
        public string GioBatDau
        {
            get { return _gioBatDau; }
            set { _gioBatDau = value; OnPropertyChanged(); }
        }

        private string _gioKetThuc;
        public string GioKetThuc
        {
            get { return _gioKetThuc; }
            set { _gioKetThuc = value; OnPropertyChanged(); }
        }

        // Các nút bấm
        public RelayCommand ChonSanCommand { get; set; }
        public RelayCommand DatSanCommand { get; set; }
        public RelayCommand NhanSanCommand { get; set; }
        public RelayCommand MoCuaSoThanhToanCommand { get; set; }

        public DatSanViewModel()
        {
            TaiDanhSachSan();

            ChonSanCommand          = new RelayCommand(p => ChonSan(p as SanBong), p => true);
            DatSanCommand           = new RelayCommand(p => DatSanTruoc(),          p => true);
            NhanSanCommand          = new RelayCommand(p => NhanSan(),              p => true);
            MoCuaSoThanhToanCommand = new RelayCommand(p => MoThanhToan(),          p => true);
        }

        private void TaiDanhSachSan()
        {
            try
            {
                using (var db = new QLYSANBONGMINIEntities())
                {
                    DanhSachSan = new ObservableCollection<SanBong>(
                        db.SanBongs.OrderBy(s => s.MaSan).ToList()
                    );
                }
            }
            catch
            {
                MessageBox.Show("Không thể tải dữ liệu sân!", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChonSan(SanBong san)
        {
            if (san != null)
                SanDangChon = san;
        }

        // Đặt sân trước: lưu vào DB với trạng thái "Đã đặt", chưa thanh toán
        private void DatSanTruoc()
        {
            DateTime tgBatDau, tgKetThuc;
            if (!KiemTraForm(out tgBatDau, out tgKetThuc)) return;

            try
            {
                using (var db = new QLYSANBONGMINIEntities())
                {
                    // Tìm khách hàng theo số điện thoại, nếu chưa có thì tạo mới
                    var khachHang = db.KhachHangs.FirstOrDefault(k => k.SoDT == SoDienThoai.Trim());
                    if (khachHang == null)
                    {
                        khachHang = new KhachHang();
                        khachHang.HoTen      = TenKhachHang.Trim();
                        khachHang.SoDT       = SoDienThoai.Trim();
                        khachHang.NgayDangKy = DateTime.Now;
                        db.KhachHangs.Add(khachHang);
                        db.SaveChanges();
                    }

                    // Tính tiền thuê
                    decimal soGio   = (decimal)(tgKetThuc - tgBatDau).TotalHours;
                    decimal tongTien = soGio * SanDangChon.GiaThueGio;

                    // Tạo bản ghi đặt sân
                    var datSan = new DatSan();
                    datSan.MaKH      = khachHang.MaKH;
                    datSan.MaSan     = SanDangChon.MaSan;
                    datSan.TGBatDau  = tgBatDau;
                    datSan.TGKetThuc = tgKetThuc;
                    datSan.TongTien  = tongTien;
                    datSan.TrangThai = "Đã đặt";
                    db.DatSans.Add(datSan);

                    // Cập nhật trạng thái sân
                    var san = db.SanBongs.Find(SanDangChon.MaSan);
                    if (san != null) san.TrangThai = "Đã đặt";

                    db.SaveChanges();
                }

                MessageBox.Show(
                    "Đặt sân thành công!\nKhách: " + TenKhachHang +
                    "\nThời gian: " + GioBatDau + " - " + GioKetThuc,
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information
                );
                XoaForm();
                TaiDanhSachSan();
            }
            catch
            {
                MessageBox.Show("Lỗi khi đặt sân!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Nhận sân ngay (bắt đầu đá từ bây giờ)
        private void NhanSan()
        {
            if (string.IsNullOrWhiteSpace(TenKhachHang)) { MessageBox.Show("Vui lòng nhập tên khách!"); return; }
            if (string.IsNullOrWhiteSpace(SoDienThoai))  { MessageBox.Show("Vui lòng nhập số điện thoại!"); return; }

            // Đặt giờ bắt đầu = bây giờ, giờ kết thúc = sau 1 tiếng
            NgayLoc    = DateTime.Today;
            GioBatDau  = DateTime.Now.ToString("HH:mm");
            GioKetThuc = DateTime.Now.AddHours(1).ToString("HH:mm");

            DatSanTruoc();
        }

        // Mở cửa sổ thanh toán
        private void MoThanhToan()
        {
            DateTime tgBatDau, tgKetThuc;
            if (!KiemTraForm(out tgBatDau, out tgKetThuc)) return;

            decimal soGio    = (decimal)(tgKetThuc - tgBatDau).TotalHours;
            decimal tongTien = soGio * SanDangChon.GiaThueGio;

            var cuaSoThanhToan = new PaymentDialog();
            var vm = new PaymentDialogViewModel();
            vm.TenSan       = SanDangChon.TenSan;
            vm.ThoiGianDa   = GioBatDau + " - " + GioKetThuc + " (" + soGio.ToString("F1") + " giờ)";
            vm.TongTien     = tongTien;
            vm.MaSan        = SanDangChon.MaSan;
            vm.TenKhachHang = TenKhachHang;
            vm.SoDienThoai  = SoDienThoai;
            vm.TGBatDau     = tgBatDau;
            vm.TGKetThuc    = tgKetThuc;
            vm.OnThanhToanXong = () =>
            {
                XoaForm();
                TaiDanhSachSan();
            };

            cuaSoThanhToan.DataContext = vm;
            cuaSoThanhToan.ShowDialog();
        }

        // Kiểm tra form trước khi thực hiện đặt sân / thanh toán
        private bool KiemTraForm(out DateTime tgBatDau, out DateTime tgKetThuc)
        {
            tgBatDau  = DateTime.MinValue;
            tgKetThuc = DateTime.MinValue;

            if (SanDangChon == null)              { MessageBox.Show("Vui lòng chọn sân!"); return false; }
            if (string.IsNullOrWhiteSpace(TenKhachHang)) { MessageBox.Show("Vui lòng nhập tên khách!"); return false; }
            if (string.IsNullOrWhiteSpace(SoDienThoai))  { MessageBox.Show("Vui lòng nhập số điện thoại!"); return false; }

            TimeSpan ts1, ts2;
            if (!TimeSpan.TryParse(GioBatDau, out ts1))  { MessageBox.Show("Giờ bắt đầu không hợp lệ!"); return false; }
            if (!TimeSpan.TryParse(GioKetThuc, out ts2)) { MessageBox.Show("Giờ kết thúc không hợp lệ!"); return false; }

            tgBatDau  = NgayLoc.Date + ts1;
            tgKetThuc = NgayLoc.Date + ts2;

            if (tgKetThuc <= tgBatDau) { MessageBox.Show("Giờ kết thúc phải sau giờ bắt đầu!"); return false; }

            return true;
        }

        private void XoaForm()
        {
            SanDangChon   = null;
            TenKhachHang  = string.Empty;
            SoDienThoai   = string.Empty;
            GioBatDau     = string.Empty;
            GioKetThuc    = string.Empty;
        }
    }
}
