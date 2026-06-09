using QuanLySanBongMini.Helpers;
using QuanLySanBongMini.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace QuanLySanBongMini.ViewModels
{
    public class NhanVienViewModel : BaseViewModel
    {
        private ObservableCollection<NhanVien> _danhSachNhanVien;
        public ObservableCollection<NhanVien> DanhSachNhanVien
        {
            get { return _danhSachNhanVien; }
            set { _danhSachNhanVien = value; OnPropertyChanged(); }
        }

        private NhanVien _nhanVienDangChon;
        public NhanVien NhanVienDangChon
        {
            get { return _nhanVienDangChon; }
            set
            {
                _nhanVienDangChon = value;
                OnPropertyChanged();
                if (value != null) DienThongTinLenForm(value);
            }
        }

        // Các ô nhập trên form
        // MaNV chỉ hiển thị (không nhập tay vì do DB tự tạo)
        private string _maNV;
        public string MaNV
        {
            get { return _maNV; }
            set { _maNV = value; OnPropertyChanged(); }
        }

        private string _tenDangNhap;
        public string TenDangNhap
        {
            get { return _tenDangNhap; }
            set { _tenDangNhap = value; OnPropertyChanged(); }
        }

        private string _hoTen;
        public string HoTen
        {
            get { return _hoTen; }
            set { _hoTen = value; OnPropertyChanged(); }
        }

        private string _matKhau;
        public string MatKhau
        {
            get { return _matKhau; }
            set { _matKhau = value; OnPropertyChanged(); }
        }

        private string _vaiTro;
        public string VaiTro
        {
            get { return _vaiTro; }
            set { _vaiTro = value; OnPropertyChanged(); }
        }

        private string _tuKhoaTimKiem;
        public string TuKhoaTimKiem
        {
            get { return _tuKhoaTimKiem; }
            set { _tuKhoaTimKiem = value; OnPropertyChanged(); TimKiem(); }
        }

        public RelayCommand ThemCommand { get; set; }
        public RelayCommand SuaCommand { get; set; }
        public RelayCommand XoaCommand { get; set; }
        public RelayCommand LamMoiCommand { get; set; }

        public NhanVienViewModel()
        {
            TaiDuLieu();
            XoaTrang(); // Form trắng khi mới mở

            ThemCommand   = new RelayCommand(p => ThemNhanVien(), p => true);
            SuaCommand    = new RelayCommand(p => SuaNhanVien(),  p => true);
            XoaCommand    = new RelayCommand(p => XoaNhanVien(),  p => true);
            LamMoiCommand = new RelayCommand(p => XoaTrang(),     p => true);
        }

        private void TaiDuLieu(string tuKhoa = "")
        {
            try
            {
                using (var db = new QLYSANBONGMINIEntities())
                {
                    var ds = db.NhanViens.AsQueryable();
                    if (!string.IsNullOrWhiteSpace(tuKhoa))
                        ds = ds.Where(nv => nv.HoTen.Contains(tuKhoa));

                    DanhSachNhanVien = new ObservableCollection<NhanVien>(
                        ds.OrderBy(nv => nv.MaNV).ToList()
                    );
                }
            }
            catch
            {
                MessageBox.Show("Không thể tải dữ liệu nhân viên!", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TimKiem() { TaiDuLieu(TuKhoaTimKiem); }

        private void DienThongTinLenForm(NhanVien nv)
        {
            MaNV        = nv.MaNV.ToString();
            TenDangNhap = nv.TenDangNhap;
            HoTen       = nv.HoTen;
            MatKhau     = nv.MatKhau;
            VaiTro      = nv.VaiTro;
        }

        private void ThemNhanVien()
        {
            if (string.IsNullOrWhiteSpace(HoTen))   { MessageBox.Show("Vui lòng nhập họ tên!"); return; }
            if (string.IsNullOrWhiteSpace(MatKhau)) { MessageBox.Show("Vui lòng nhập mật khẩu!"); return; }

            try
            {
                using (var db = new QLYSANBONGMINIEntities())
                {
                    var nv = new NhanVien();
                    nv.HoTen       = HoTen.Trim();
                    nv.TenDangNhap = TenDangNhap?.Trim();
                    nv.MatKhau     = MatKhau.Trim();
                    nv.VaiTro      = string.IsNullOrWhiteSpace(VaiTro) ? "Nhân viên" : VaiTro;
                    db.NhanViens.Add(nv);
                    db.SaveChanges();

                    // Sau SaveChanges thì nv.MaNV đã có giá trị từ DB (auto increment)
                    MessageBox.Show("Thêm thành công! Mã NV: " + nv.MaNV, "Thành công",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                XoaTrang(); TaiDuLieu();
            }
            catch
            {
                MessageBox.Show("Lỗi khi thêm nhân viên!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SuaNhanVien()
        {
            if (NhanVienDangChon == null) { MessageBox.Show("Vui lòng chọn nhân viên cần sửa!"); return; }
            if (string.IsNullOrWhiteSpace(HoTen)) { MessageBox.Show("Vui lòng nhập họ tên!"); return; }

            try
            {
                using (var db = new QLYSANBONGMINIEntities())
                {
                    var nv = db.NhanViens.Find(NhanVienDangChon.MaNV);
                    if (nv == null) return;

                    nv.HoTen       = HoTen.Trim();
                    nv.TenDangNhap = TenDangNhap?.Trim();
                    nv.VaiTro      = VaiTro;
                    // Chỉ đổi mật khẩu nếu người dùng có nhập
                    if (!string.IsNullOrWhiteSpace(MatKhau))
                        nv.MatKhau = MatKhau.Trim();
                    db.SaveChanges();
                }
                MessageBox.Show("Cập nhật thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                XoaTrang(); TaiDuLieu();
            }
            catch
            {
                MessageBox.Show("Lỗi khi cập nhật nhân viên!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void XoaNhanVien()
        {
            if (NhanVienDangChon == null) { MessageBox.Show("Vui lòng chọn nhân viên cần xóa!"); return; }

            if (MessageBox.Show("Xóa nhân viên \"" + NhanVienDangChon.HoTen + "\"?",
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning)
                != MessageBoxResult.Yes) return;

            try
            {
                using (var db = new QLYSANBONGMINIEntities())
                {
                    var nv = db.NhanViens.Find(NhanVienDangChon.MaNV);
                    if (nv == null) return;
                    db.NhanViens.Remove(nv);
                    db.SaveChanges();
                }
                MessageBox.Show("Xóa thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                XoaTrang(); TaiDuLieu();
            }
            catch
            {
                MessageBox.Show("Không thể xóa! Nhân viên đã có hóa đơn liên quan.", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void XoaTrang()
        {
            // Dùng field trực tiếp để tránh trigger DieuHuongLenForm
            _nhanVienDangChon = null;
            OnPropertyChanged(nameof(NhanVienDangChon));

            MaNV        = "(Tự động)";
            TenDangNhap = string.Empty;
            HoTen       = string.Empty;
            MatKhau     = string.Empty;
            VaiTro      = "Nhân viên";
        }
    }
}
