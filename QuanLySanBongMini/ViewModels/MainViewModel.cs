using QuanLySanBongMini.Helpers;
using QuanLySanBongMini.Models;
using QuanLySanBongMini.View;
using System.Windows;

namespace QuanLySanBongMini.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        // Thông tin nhân viên đang đăng nhập
        private NhanVien _nhanVienHienTai;
        public NhanVien CurrentUser
        {
            get { return _nhanVienHienTai; }
            set { _nhanVienHienTai = value; OnPropertyChanged(); }
        }

        // View đang hiển thị ở phần nội dung chính (phần giữa màn hình)
        private object _viewHienTai;
        public object CurrentView
        {
            get { return _viewHienTai; }
            set { _viewHienTai = value; OnPropertyChanged(); }
        }

        // Kiểm tra xem nhân viên có phải quản lý không
        public bool LaQuanLy
        {
            get { return CurrentUser?.VaiTro == "Quản lý"; }
        }

        // Dùng trong XAML để ẩn/hiện các nút chỉ dành cho quản lý
        public Visibility ChiQuanLy
        {
            get { return LaQuanLy ? Visibility.Visible : Visibility.Collapsed; }
        }

        // --- Các command cho menu bên trái ---
        public RelayCommand ShowDatSanCommand { get; set; }
        public RelayCommand ShowChoThanhToanCommand { get; set; }
        public RelayCommand ShowKhachHangCommand { get; set; }
        public RelayCommand ShowThongKeCommand { get; set; }
        public RelayCommand ShowNhanVienCommand { get; set; }
        public RelayCommand ShowSanBongCommand { get; set; }
        public RelayCommand ShowQuanLyMenuCommand { get; set; }
        public RelayCommand DangXuatCommand { get; set; }

        public MainViewModel(NhanVien nguoiDung)
        {
            CurrentUser = nguoiDung;

            // Lưu mã nhân viên để các ViewModel khác dùng khi tạo hóa đơn
            DatSanViewModel.MaNVHienTai = nguoiDung.MaNV;

            // Mặc định hiển thị màn hình đặt sân
            CurrentView = new DatSanViewModel();

            // Các menu nhân viên thường dùng được
            ShowDatSanCommand = new RelayCommand(p => CurrentView = new DatSanViewModel(), p => true);
            ShowChoThanhToanCommand = new RelayCommand(p => CurrentView = new DanhSachChoThanhToanViewModel(), p => true);
            ShowKhachHangCommand = new RelayCommand(p => CurrentView = new KhachHangViewModel(), p => true);

            // Các menu chỉ quản lý mới dùng được
            ShowThongKeCommand = new RelayCommand(p => MoNeuDuQuyen(() => CurrentView = new ThongKeViewModel()));
            ShowNhanVienCommand = new RelayCommand(p => MoNeuDuQuyen(() => CurrentView = new NhanVienViewModel()));
            ShowSanBongCommand = new RelayCommand(p => MoNeuDuQuyen(() => CurrentView = new SanBongViewModel()));
            ShowQuanLyMenuCommand = new RelayCommand(p => MoNeuDuQuyen(() => CurrentView = new QuanLyMenuViewModel()));

            // Nút đăng xuất
            DangXuatCommand = new RelayCommand(p => DangXuat(p as Window), p => true);
        }

        // Kiểm tra quyền trước khi mở màn hình
        private void MoNeuDuQuyen(System.Action hamMo)
        {
            if (LaQuanLy)
            {
                hamMo(); // Có quyền thì mở bình thường
            }
            else
            {
                MessageBox.Show(
                    "Bạn không có quyền truy cập chức năng này!\nChỉ Quản lý mới được phép sử dụng.",
                    "Không có quyền",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }

        private void DangXuat(Window cuaSo)
        {
            // Hỏi xác nhận trước khi đăng xuất
            var ketQua = MessageBox.Show(
                "Bạn có chắc muốn đăng xuất?",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (ketQua == MessageBoxResult.Yes)
            {
                // Mở lại màn hình đăng nhập
                var manHinhDangNhap = new LoginWindow();
                manHinhDangNhap.Show();

                // Đóng màn hình chính
                cuaSo?.Close();
            }
        }
    }
}
