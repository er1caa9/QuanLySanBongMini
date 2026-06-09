using QuanLySanBongMini.Helpers;
using QuanLySanBongMini.Models;
using QuanLySanBongMini.View;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace QuanLySanBongMini.ViewModels
{
    // Lớp chứa thông tin 1 dòng trong bảng danh sách chờ thanh toán
    public class DatSanRowDTO
    {
        public int MaDatSan { get; set; }
        public string TenKhachHang { get; set; }
        public string SoDienThoai { get; set; }
        public string TenSan { get; set; }
        public string ThoiGian { get; set; }
        public decimal TongTien { get; set; }
        // Hiển thị tiền có định dạng (ví dụ: 150.000 đ)
        public string TongTienFormatted { get { return TongTien.ToString("N0") + " đ"; } }
        public string TrangThai { get; set; }
        // Màu sắc khác nhau cho từng trạng thái
        public string MauTrangThai { get { return TrangThai == "Đã đặt" ? "#3B82F6" : "#F59E0B"; } }

        // Thông tin cần thiết để mở cửa sổ thanh toán
        public int MaSan { get; set; }
        public DateTime TGBatDau { get; set; }
        public DateTime TGKetThuc { get; set; }
    }

    public class DanhSachChoThanhToanViewModel : BaseViewModel
    {
        private ObservableCollection<DatSanRowDTO> _danhSach;
        public ObservableCollection<DatSanRowDTO> DanhSach
        {
            get { return _danhSach; }
            set { _danhSach = value; OnPropertyChanged(); }
        }

        private DatSanRowDTO _dongDangChon;
        public DatSanRowDTO DongDangChon
        {
            get { return _dongDangChon; }
            set { _dongDangChon = value; OnPropertyChanged(); }
        }

        // Ô tìm kiếm
        private string _tuKhoaTimKiem;
        public string TuKhoaTimKiem
        {
            get { return _tuKhoaTimKiem; }
            set { _tuKhoaTimKiem = value; OnPropertyChanged(); TimKiem(); }
        }

        public RelayCommand ThanhToanCommand { get; set; }
        public RelayCommand HuyDatSanCommand { get; set; }
        public RelayCommand LamMoiCommand { get; set; }

        public DanhSachChoThanhToanViewModel()
        {
            ThanhToanCommand = new RelayCommand(p => MoThanhToan(), p => true);
            HuyDatSanCommand = new RelayCommand(p => HuyDatSan(),   p => true);
            LamMoiCommand    = new RelayCommand(p => TaiDuLieu(),   p => true);
            TaiDuLieu();
        }

        private void TaiDuLieu(string tuKhoa = "")
        {
            try
            {
                using (var db = new QLYSANBONGMINIEntities())
                {
                    // Lấy các đặt sân đang chờ thanh toán
                    var danhSach = db.DatSans
                        .Where(d => d.TrangThai == "Đã đặt" || d.TrangThai == "Đang đá")
                        .OrderBy(d => d.TGBatDau)
                        .ToList()
                        .Select(d => new DatSanRowDTO
                        {
                            MaDatSan     = d.MaDatSan,
                            TenKhachHang = d.KhachHang?.HoTen ?? "?",
                            SoDienThoai  = d.KhachHang?.SoDT ?? "",
                            TenSan       = d.SanBong?.TenSan ?? "?",
                            ThoiGian     = d.TGBatDau.ToString("HH:mm") + " - " +
                                           d.TGKetThuc.ToString("HH:mm") + "  " +
                                           d.TGBatDau.ToString("dd/MM/yyyy"),
                            TongTien     = d.TongTien ?? 0,
                            TrangThai    = d.TrangThai,
                            MaSan        = d.MaSan,
                            TGBatDau     = d.TGBatDau,
                            TGKetThuc    = d.TGKetThuc
                        })
                        .Where(d => string.IsNullOrWhiteSpace(tuKhoa)
                            || d.TenKhachHang.Contains(tuKhoa)
                            || d.SoDienThoai.Contains(tuKhoa)
                            || d.TenSan.Contains(tuKhoa))
                        .ToList();

                    DanhSach = new ObservableCollection<DatSanRowDTO>(danhSach);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TimKiem() { TaiDuLieu(TuKhoaTimKiem); }

        private void MoThanhToan()
        {
            if (DongDangChon == null) return;

            var cuaSoThanhToan = new PaymentDialog();
            double soGio = (DongDangChon.TGKetThuc - DongDangChon.TGBatDau).TotalHours;

            var vm = new PaymentDialogViewModel();
            vm.TenSan          = DongDangChon.TenSan;
            vm.ThoiGianDa      = DongDangChon.TGBatDau.ToString("HH:mm") + " - " +
                                  DongDangChon.TGKetThuc.ToString("HH:mm") +
                                  " (" + soGio.ToString("F1") + " giờ)";
            vm.TongTien        = DongDangChon.TongTien;
            vm.MaSan           = DongDangChon.MaSan;
            vm.TenKhachHang    = DongDangChon.TenKhachHang;
            vm.SoDienThoai     = DongDangChon.SoDienThoai;
            vm.TGBatDau        = DongDangChon.TGBatDau;
            vm.TGKetThuc       = DongDangChon.TGKetThuc;
            vm.MaDatSanHienCo  = DongDangChon.MaDatSan;
            vm.OnThanhToanXong = () =>
            {
                DongDangChon = null;
                TaiDuLieu();
            };

            cuaSoThanhToan.DataContext = vm;
            cuaSoThanhToan.ShowDialog();
        }

        private void HuyDatSan()
        {
            if (DongDangChon == null) return;

            var xacNhan = MessageBox.Show(
                "Hủy đặt sân của khách \"" + DongDangChon.TenKhachHang +
                "\" (" + DongDangChon.TenSan + ")?",
                "Xác nhận hủy",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );
            if (xacNhan != MessageBoxResult.Yes) return;

            try
            {
                using (var db = new QLYSANBONGMINIEntities())
                {
                    var datSan = db.DatSans.Find(DongDangChon.MaDatSan);
                    if (datSan == null) return;

                    // Trả sân về Trống
                    var san = db.SanBongs.Find(datSan.MaSan);
                    if (san != null) san.TrangThai = "Trống";

                    db.DatSans.Remove(datSan);
                    db.SaveChanges();
                }
                MessageBox.Show("Đã hủy đặt sân thành công!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                DongDangChon = null;
                TaiDuLieu();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi hủy: " + ex.Message, "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
