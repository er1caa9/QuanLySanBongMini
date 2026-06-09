using QuanLySanBongMini.Helpers;
using QuanLySanBongMini.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace QuanLySanBongMini.ViewModels
{
    internal class SanBongViewModel : BaseViewModel
    {
        // Danh sách sân bóng hiển thị trong DataGrid
        private ObservableCollection<SanBong> _danhSachSan;
        public ObservableCollection<SanBong> DanhSachSan
        {
            get { return _danhSachSan; }
            set { _danhSachSan = value; OnPropertyChanged(); }
        }

        // Sân đang được chọn trong DataGrid
        private SanBong _sanDangChon;
        public SanBong SanDangChon
        {
            get { return _sanDangChon; }
            set
            {
                _sanDangChon = value;
                OnPropertyChanged();
                // Khi chọn sân, tự động điền thông tin lên form
                if (value != null)
                    DienThongTinLenForm(value);
            }
        }

        // --- Các ô nhập liệu trên form ---
        private int _maSan;
        public int MaSan
        {
            get { return _maSan; }
            set { _maSan = value; OnPropertyChanged(); }
        }

        private string _tenSan;
        public string TenSan
        {
            get { return _tenSan; }
            set { _tenSan = value; OnPropertyChanged(); }
        }

        private string _loaiSan;
        public string LoaiSan
        {
            get { return _loaiSan; }
            set { _loaiSan = value; OnPropertyChanged(); }
        }

        private decimal _giaThue;
        public decimal GiaThue
        {
            get { return _giaThue; }
            set { _giaThue = value; OnPropertyChanged(); }
        }

        private string _trangThai;
        public string TrangThai
        {
            get { return _trangThai; }
            set { _trangThai = value; OnPropertyChanged(); }
        }

        // Ô tìm kiếm - khi gõ tự động lọc danh sách
        private string _tuKhoaTimKiem;
        public string TuKhoaTimKiem
        {
            get { return _tuKhoaTimKiem; }
            set
            {
                _tuKhoaTimKiem = value;
                OnPropertyChanged();
                TimKiem(); // Tự động tìm kiếm khi gõ
            }
        }

        // Các nút bấm
        public RelayCommand ThemCommand { get; set; }
        public RelayCommand SuaCommand { get; set; }
        public RelayCommand XoaCommand { get; set; }

        public SanBongViewModel()
        {
            TaiDuLieu();

            ThemCommand = new RelayCommand(p => ThemSan(), p => true);
            SuaCommand = new RelayCommand(p => SuaSan(), p => true);
            XoaCommand = new RelayCommand(p => XoaSan(), p => true);
        }

        // Đọc dữ liệu từ database
        private void TaiDuLieu(string tuKhoa = "")
        {
            try
            {
                using (var db = new QLYSANBONGMINIEntities())
                {
                    // Lấy tất cả sân, nếu có từ khóa thì lọc thêm
                    var danhSach = db.SanBongs.AsQueryable();

                    if (!string.IsNullOrWhiteSpace(tuKhoa))
                    {
                        danhSach = danhSach.Where(s =>
                            s.TenSan.Contains(tuKhoa) || s.LoaiSan.Contains(tuKhoa)
                        );
                    }

                    DanhSachSan = new ObservableCollection<SanBong>(
                        danhSach.OrderBy(s => s.MaSan).ToList()
                    );
                }
            }
            catch
            {
                MessageBox.Show("Không thể tải dữ liệu sân bóng!", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TimKiem()
        {
            TaiDuLieu(TuKhoaTimKiem);
        }

        // Khi click vào hàng trong DataGrid, điền thông tin lên các ô nhập
        private void DienThongTinLenForm(SanBong san)
        {
            MaSan = san.MaSan;
            TenSan = san.TenSan;
            LoaiSan = san.LoaiSan;
            GiaThue = san.GiaThueGio;
            TrangThai = san.TrangThai;
        }

        private void ThemSan()
        {
            // Kiểm tra dữ liệu trước khi thêm
            if (string.IsNullOrWhiteSpace(TenSan))
            {
                MessageBox.Show("Vui lòng nhập tên sân!");
                return;
            }
            if (GiaThue <= 0)
            {
                MessageBox.Show("Giá thuê phải lớn hơn 0!");
                return;
            }

            try
            {
                using (var db = new QLYSANBONGMINIEntities())
                {
                    var sanMoi = new SanBong();
                    sanMoi.TenSan = TenSan.Trim();
                    sanMoi.LoaiSan = LoaiSan ?? "Sân 5 người";
                    sanMoi.GiaThueGio = GiaThue;
                    sanMoi.TrangThai = TrangThai ?? "Trống";

                    db.SanBongs.Add(sanMoi);
                    db.SaveChanges();
                }

                MessageBox.Show("Thêm sân thành công!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                XoaTrang();
                TaiDuLieu();
            }
            catch
            {
                MessageBox.Show("Lỗi khi thêm sân!", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SuaSan()
        {
            if (SanDangChon == null)
            {
                MessageBox.Show("Vui lòng chọn sân cần sửa!");
                return;
            }
            if (string.IsNullOrWhiteSpace(TenSan))
            {
                MessageBox.Show("Vui lòng nhập tên sân!");
                return;
            }
            if (GiaThue <= 0)
            {
                MessageBox.Show("Giá thuê phải lớn hơn 0!");
                return;
            }

            try
            {
                using (var db = new QLYSANBONGMINIEntities())
                {
                    // Tìm sân trong database theo mã
                    var san = db.SanBongs.Find(SanDangChon.MaSan);
                    if (san == null) return;

                    // Cập nhật thông tin
                    san.TenSan = TenSan.Trim();
                    san.LoaiSan = LoaiSan;
                    san.GiaThueGio = GiaThue;
                    san.TrangThai = TrangThai;

                    db.SaveChanges();
                }

                MessageBox.Show("Cập nhật sân thành công!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                XoaTrang();
                TaiDuLieu();
            }
            catch
            {
                MessageBox.Show("Lỗi khi cập nhật sân!", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void XoaSan()
        {
            if (SanDangChon == null)
            {
                MessageBox.Show("Vui lòng chọn sân cần xóa!");
                return;
            }

            // Hỏi xác nhận trước khi xóa
            var xacNhan = MessageBox.Show(
                "Bạn có chắc muốn xóa sân \"" + SanDangChon.TenSan + "\"?",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (xacNhan != MessageBoxResult.Yes) return;

            try
            {
                using (var db = new QLYSANBONGMINIEntities())
                {
                    var san = db.SanBongs.Find(SanDangChon.MaSan);
                    if (san == null) return;

                    db.SanBongs.Remove(san);
                    db.SaveChanges();
                }

                MessageBox.Show("Xóa sân thành công!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                XoaTrang();
                TaiDuLieu();
            }
            catch
            {
                MessageBox.Show("Không thể xóa sân này! Có thể đã có lịch đặt liên quan.", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Xóa trắng form nhập liệu
        private void XoaTrang()
        {
            SanDangChon = null;
            MaSan = 0;
            TenSan = string.Empty;
            LoaiSan = null;
            GiaThue = 0;
            TrangThai = null;
        }
    }
}
