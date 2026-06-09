using QuanLySanBongMini.Helpers;
using QuanLySanBongMini.Models;
using QuanLySanBongMini.View;
using System.Linq;
using System.Windows;

namespace QuanLySanBongMini.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        // --- Các property để binding với TextBox trên giao diện ---

        private string _maNhanVien;
        public string MaNV
        {
            get { return _maNhanVien; }
            set
            {
                _maNhanVien = value;
                OnPropertyChanged();
                // Cập nhật lại trạng thái nút đăng nhập mỗi khi gõ
                DangNhapCommand.RaiseCanExecuteChanged();
            }
        }

        private string _matKhau;
        public string MatKhau
        {
            get { return _matKhau; }
            set
            {
                _matKhau = value;
                OnPropertyChanged();
                DangNhapCommand.RaiseCanExecuteChanged();
            }
        }

        // Hiển thị thông báo lỗi bên dưới form đăng nhập
        private string _thongBaoLoi;
        public string ThongBao
        {
            get { return _thongBaoLoi; }
            set { _thongBaoLoi = value; OnPropertyChanged(); }
        }

        // Command gắn với nút "Đăng nhập"
        public RelayCommand DangNhapCommand { get; set; }

        public LoginViewModel()
        {
            // Tạo command đăng nhập
            // Tham số thứ 2 là điều kiện: chỉ bấm được khi đã nhập MaNV và MatKhau
            DangNhapCommand = new RelayCommand(
                p => DangNhap(p as Window),
                p => !string.IsNullOrWhiteSpace(MaNV) && !string.IsNullOrWhiteSpace(MatKhau)
            );
        }

        private void DangNhap(Window cuaSoDangNhap)
        {
            try
            {
                using (var db = new QLYSANBONGMINIEntities())
                {
                    NhanVien nguoiDung = null;

                    // Thử đăng nhập bằng mã số nhân viên (ví dụ: "1", "2")
                    int maSo;
                    if (int.TryParse(MaNV, out maSo))
                    {
                        nguoiDung = db.NhanViens.FirstOrDefault(
                            x => x.MaNV == maSo && x.MatKhau == MatKhau
                        );
                    }

                    // Nếu chưa tìm thấy, thử đăng nhập bằng tên đăng nhập
                    if (nguoiDung == null)
                    {
                        nguoiDung = db.NhanViens.FirstOrDefault(
                            x => x.TenDangNhap == MaNV && x.MatKhau == MatKhau
                        );
                    }

                    if (nguoiDung != null)
                    {
                        // Đăng nhập thành công → mở màn hình chính
                        MainView manHinhChinh = new MainView();
                        manHinhChinh.DataContext = new MainViewModel(nguoiDung);
                        manHinhChinh.WindowState = WindowState.Maximized;
                        manHinhChinh.Show();

                        // Đóng cửa sổ đăng nhập
                        cuaSoDangNhap?.Close();
                    }
                    else
                    {
                        // Sai thông tin đăng nhập
                        ThongBao = "Sai mã nhân viên hoặc mật khẩu!";
                    }
                }
            }
            catch
            {
                ThongBao = "Không thể kết nối cơ sở dữ liệu!";
            }
        }
    }
}
