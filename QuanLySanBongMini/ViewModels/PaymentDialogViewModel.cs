using QuanLySanBongMini.Helpers;
using QuanLySanBongMini.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace QuanLySanBongMini.ViewModels
{
    // Đại diện cho 1 món trong giỏ hàng (đồ ăn/uống)
    public class GioHangItem : BaseViewModel
    {
        public int    MaMon  { get; set; }
        public string TenMon { get; set; }
        public decimal DonGia { get; set; }

        private int _soLuong;
        public int SoLuong
        {
            get { return _soLuong; }
            set
            {
                _soLuong = value;
                OnPropertyChanged();
                // Khi số lượng thay đổi thì thành tiền cũng thay đổi
                OnPropertyChanged(nameof(ThanhTien));
            }
        }

        // Thành tiền = đơn giá × số lượng
        public decimal ThanhTien { get { return DonGia * SoLuong; } }
    }

    internal class PaymentDialogViewModel : BaseViewModel
    {
        // Thông tin hiển thị ở đầu cửa sổ
        public string TenSan { get; set; }
        public string ThoiGianDa { get; set; }

        // Tiền thuê sân
        private decimal _tienThueSan;
        public decimal TienThueSan
        {
            get { return _tienThueSan; }
            set { _tienThueSan = value; OnPropertyChanged(); CapNhatTong(); }
        }

        // TongTien là alias để tương thích với code cũ từ ngoài truyền vào
        public decimal TongTien
        {
            get { return TienThueSan; }
            set { TienThueSan = value; }
        }

        // Thông tin để lưu vào database
        public int      MaSan          { get; set; }
        public string   TenKhachHang   { get; set; }
        public string   SoDienThoai    { get; set; }
        public DateTime TGBatDau       { get; set; }
        public DateTime TGKetThuc      { get; set; }
        public int      MaDatSanHienCo { get; set; } // > 0 nếu đã có lịch đặt từ trước
        public Action   OnThanhToanXong { get; set; } // Callback khi thanh toán xong

        // --- Tab đồ uống / đồ ăn ---
        private bool _hienDoUong = true;
        public bool HienDoUong
        {
            get { return _hienDoUong; }
            set { _hienDoUong = value; OnPropertyChanged(); LocTheoLoai(); }
        }

        private bool _hienDoAn;
        public bool HienDoAn
        {
            get { return _hienDoAn; }
            set { _hienDoAn = value; OnPropertyChanged(); LocTheoLoai(); }
        }

        // Toàn bộ món (load 1 lần từ DB)
        private ObservableCollection<MonAn> _tatCaMon = new ObservableCollection<MonAn>();

        // Danh sách món đang hiển thị (sau khi lọc theo tab)
        private ObservableCollection<MonAn> _danhSachMonHienThi;
        public ObservableCollection<MonAn> DanhSachMonHienThi
        {
            get { return _danhSachMonHienThi; }
            set { _danhSachMonHienThi = value; OnPropertyChanged(); }
        }

        // Giỏ hàng (danh sách các món đã thêm vào)
        private ObservableCollection<GioHangItem> _gioHang = new ObservableCollection<GioHangItem>();
        public ObservableCollection<GioHangItem> GioHang
        {
            get { return _gioHang; }
            set { _gioHang = value; OnPropertyChanged(); }
        }

        // Tổng tiền đồ ăn/uống trong giỏ
        public decimal TienDoAnUong { get { return GioHang.Sum(g => g.ThanhTien); } }

        // Tổng cộng = tiền thuê sân + tiền đồ ăn/uống
        public decimal TongCong { get { return TienThueSan + TienDoAnUong; } }

        // Tiền khách đưa
        private decimal _tienKhachDua;
        public decimal TienKhachDua
        {
            get { return _tienKhachDua; }
            set
            {
                _tienKhachDua = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TienThoi)); // Cập nhật tiền thối
            }
        }

        // Tiền thối = tiền khách đưa - tổng cộng (nếu đủ tiền)
        public decimal TienThoi
        {
            get { return TienKhachDua > TongCong ? TienKhachDua - TongCong : 0; }
        }

        // Các nút bấm
        public RelayCommand XacNhanThanhToanCommand { get; set; }
        public RelayCommand DongCuaSoCommand { get; set; }
        public RelayCommand ThemMonCommand { get; set; }
        public RelayCommand XoaMonGioCommand { get; set; }

        public PaymentDialogViewModel()
        {
            TaiDanhSachMon();

            // Nút xác nhận: chỉ bấm được khi đã đủ tiền
            XacNhanThanhToanCommand = new RelayCommand(
                p => XacNhanThanhToan(p as Window),
                p => TienKhachDua >= TongCong && TongCong > 0
            );
            DongCuaSoCommand  = new RelayCommand(p => (p as Window)?.Close(), p => true);
            ThemMonCommand    = new RelayCommand(p => ThemVaoGio(p as MonAn),     p => true);
            XoaMonGioCommand  = new RelayCommand(p => XoaKhoiGio(p as GioHangItem), p => true);
        }

        private void TaiDanhSachMon()
        {
            try
            {
                using (var db = new QLYSANBONGMINIEntities())
                {
                    _tatCaMon = new ObservableCollection<MonAn>(
                        db.MonAns.Where(m => m.ConHang).OrderBy(m => m.TenMon).ToList()
                    );
                }
            }
            catch
            {
                _tatCaMon = new ObservableCollection<MonAn>();
            }
            LocTheoLoai(); // Lọc theo tab mặc định (Đồ uống)
        }

        private void LocTheoLoai()
        {
            string loai = HienDoUong ? "Đồ uống" : "Đồ ăn";
            DanhSachMonHienThi = new ObservableCollection<MonAn>(
                _tatCaMon.Where(m => m.LoaiMon == loai)
            );
        }

        private void ThemVaoGio(MonAn mon)
        {
            if (mon == null) return;

            // Nếu đã có trong giỏ thì tăng số lượng, nếu chưa thì thêm mới
            var daTonTai = GioHang.FirstOrDefault(g => g.MaMon == mon.MaMon);
            if (daTonTai != null)
            {
                daTonTai.SoLuong++;
            }
            else
            {
                GioHang.Add(new GioHangItem
                {
                    MaMon  = mon.MaMon,
                    TenMon = mon.TenMon,
                    DonGia = mon.DonGia,
                    SoLuong = 1
                });
            }
            CapNhatTong();
        }

        private void XoaKhoiGio(GioHangItem mon)
        {
            if (mon == null) return;

            if (mon.SoLuong > 1)
                mon.SoLuong--;    // Giảm số lượng nếu còn nhiều hơn 1
            else
                GioHang.Remove(mon); // Xóa hẳn nếu chỉ còn 1

            CapNhatTong();
        }

        // Thông báo cho giao diện cập nhật lại các tổng tiền
        private void CapNhatTong()
        {
            OnPropertyChanged(nameof(TienDoAnUong));
            OnPropertyChanged(nameof(TongCong));
            OnPropertyChanged(nameof(TienThoi));
        }

        private void XacNhanThanhToan(Window cuaSo)
        {
            if (TienKhachDua < TongCong)
            {
                MessageBox.Show("Tiền khách đưa không đủ!");
                return;
            }

            try
            {
                using (var db = new QLYSANBONGMINIEntities())
                {
                    DatSan datSan;

                    if (MaDatSanHienCo > 0)
                    {
                        // Đã có lịch đặt từ trước → chỉ cần cập nhật trạng thái
                        datSan = db.DatSans.Find(MaDatSanHienCo);
                        if (datSan == null) { MessageBox.Show("Không tìm thấy bản ghi đặt sân!"); return; }
                        datSan.TrangThai = "Đã thanh toán";
                        datSan.TongTien  = TongCong;
                        db.SaveChanges();
                    }
                    else
                    {
                        // Chưa có lịch đặt → tạo mới khách hàng và đặt sân
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

                        datSan = new DatSan();
                        datSan.MaKH      = khachHang.MaKH;
                        datSan.MaSan     = MaSan;
                        datSan.TGBatDau  = TGBatDau;
                        datSan.TGKetThuc = TGKetThuc;
                        datSan.TongTien  = TongCong;
                        datSan.TrangThai = "Đã thanh toán";
                        db.DatSans.Add(datSan);
                        db.SaveChanges();
                    }

                    // Tạo hóa đơn
                    var hoaDon = new HoaDon();
                    hoaDon.MaDatSan = datSan.MaDatSan;
                    hoaDon.MaNV     = DatSanViewModel.MaNVHienTai;
                    hoaDon.NgayLap  = DateTime.Now;
                    hoaDon.TongTien = TongCong;
                    hoaDon.LoaiHD   = "Bán";
                    db.HoaDons.Add(hoaDon);
                    db.SaveChanges();

                    // Chi tiết hóa đơn: tiền thuê sân
                    var sanBong = db.SanBongs.Find(MaSan);
                    double soGio = (TGKetThuc - TGBatDau).TotalHours;
                    var chiTietSan = new ChiTietHoaDon();
                    chiTietSan.MaHD      = hoaDon.MaHD;
                    chiTietSan.MoTa      = "Thuê sân " + (sanBong?.TenSan ?? "") +
                                           " (" + soGio.ToString("F1") + " giờ)";
                    chiTietSan.DonGia    = sanBong?.GiaThueGio;
                    chiTietSan.SoLuong   = (int)Math.Ceiling(soGio);
                    chiTietSan.ThanhTien = TienThueSan;
                    db.ChiTietHoaDons.Add(chiTietSan);

                    // Chi tiết hóa đơn: từng món đồ ăn/uống
                    foreach (var mon in GioHang)
                    {
                        var chiTietMon = new ChiTietHoaDon();
                        chiTietMon.MaHD      = hoaDon.MaHD;
                        chiTietMon.MoTa      = mon.TenMon;
                        chiTietMon.DonGia    = mon.DonGia;
                        chiTietMon.SoLuong   = mon.SoLuong;
                        chiTietMon.ThanhTien = mon.ThanhTien;
                        db.ChiTietHoaDons.Add(chiTietMon);
                    }

                    // Trả sân về Trống
                    if (sanBong != null) sanBong.TrangThai = "Trống";

                    db.SaveChanges();
                }

                MessageBox.Show(
                    "Thanh toán thành công!\nTiền thối: " + TienThoi.ToString("N0") + " VNĐ",
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information
                );

                OnThanhToanXong?.Invoke(); // Báo cho màn hình cha cập nhật lại
                cuaSo?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu hóa đơn: " + ex.Message, "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
