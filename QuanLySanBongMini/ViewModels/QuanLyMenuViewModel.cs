using QuanLySanBongMini.Helpers;
using QuanLySanBongMini.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace QuanLySanBongMini.ViewModels
{
    public class QuanLyMenuViewModel : BaseViewModel
    {
        private ObservableCollection<MonAn> _danhSachMon;
        public ObservableCollection<MonAn> DanhSachMon
        {
            get { return _danhSachMon; }
            set { _danhSachMon = value; OnPropertyChanged(); }
        }

        private MonAn _monDangChon;
        public MonAn MonDangChon
        {
            get { return _monDangChon; }
            set
            {
                _monDangChon = value;
                OnPropertyChanged();
                if (value != null) DienThongTinLenForm(value);
            }
        }

        // Các ô nhập trên form
        private string _tenMon;
        public string TenMon
        {
            get { return _tenMon; }
            set { _tenMon = value; OnPropertyChanged(); }
        }

        private string _loaiMon = "Đồ uống";
        public string LoaiMon
        {
            get { return _loaiMon; }
            set { _loaiMon = value; OnPropertyChanged(); }
        }

        private decimal _donGia;
        public decimal DonGia
        {
            get { return _donGia; }
            set { _donGia = value; OnPropertyChanged(); }
        }

        private string _moTa;
        public string MoTa
        {
            get { return _moTa; }
            set { _moTa = value; OnPropertyChanged(); }
        }

        private bool _conHang = true;
        public bool ConHang
        {
            get { return _conHang; }
            set { _conHang = value; OnPropertyChanged(); }
        }

        private string _tuKhoa;
        public string TuKhoa
        {
            get { return _tuKhoa; }
            set { _tuKhoa = value; OnPropertyChanged(); TaiDuLieu(_tuKhoa); }
        }

        public RelayCommand ThemCommand { get; set; }
        public RelayCommand SuaCommand { get; set; }
        public RelayCommand XoaCommand { get; set; }
        public RelayCommand LamMoiCommand { get; set; }

        public QuanLyMenuViewModel()
        {
            TaiDuLieu();
            ThemCommand   = new RelayCommand(p => ThemMon(),    p => true);
            SuaCommand    = new RelayCommand(p => SuaMon(),     p => true);
            XoaCommand    = new RelayCommand(p => XoaMon(),     p => true);
            LamMoiCommand = new RelayCommand(p => XoaTrang(),   p => true);
        }

        private void TaiDuLieu(string tuKhoa = "")
        {
            try
            {
                using (var db = new QLYSANBONGMINIEntities())
                {
                    var ds = db.MonAns.AsQueryable();
                    if (!string.IsNullOrWhiteSpace(tuKhoa))
                        ds = ds.Where(m => m.TenMon.Contains(tuKhoa) || m.LoaiMon.Contains(tuKhoa));

                    DanhSachMon = new ObservableCollection<MonAn>(
                        ds.OrderBy(m => m.LoaiMon).ThenBy(m => m.TenMon).ToList()
                    );
                }
            }
            catch
            {
                MessageBox.Show("Không thể tải danh sách món!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DienThongTinLenForm(MonAn m)
        {
            TenMon  = m.TenMon;
            LoaiMon = m.LoaiMon;
            DonGia  = m.DonGia;
            MoTa    = m.MoTa;
            ConHang = m.ConHang;
        }

        private void ThemMon()
        {
            if (string.IsNullOrWhiteSpace(TenMon)) { MessageBox.Show("Vui lòng nhập tên món!"); return; }
            if (DonGia <= 0) { MessageBox.Show("Đơn giá phải lớn hơn 0!"); return; }

            try
            {
                using (var db = new QLYSANBONGMINIEntities())
                {
                    var monMoi = new MonAn();
                    monMoi.TenMon  = TenMon.Trim();
                    monMoi.LoaiMon = LoaiMon;
                    monMoi.DonGia  = DonGia;
                    monMoi.MoTa    = MoTa?.Trim();
                    monMoi.ConHang = ConHang;
                    db.MonAns.Add(monMoi);
                    db.SaveChanges();
                }
                MessageBox.Show("Thêm món thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                XoaTrang(); TaiDuLieu();
            }
            catch { MessageBox.Show("Lỗi khi thêm món!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void SuaMon()
        {
            if (MonDangChon == null) { MessageBox.Show("Vui lòng chọn món cần sửa!"); return; }
            if (string.IsNullOrWhiteSpace(TenMon)) { MessageBox.Show("Vui lòng nhập tên món!"); return; }
            if (DonGia <= 0) { MessageBox.Show("Đơn giá phải lớn hơn 0!"); return; }

            try
            {
                using (var db = new QLYSANBONGMINIEntities())
                {
                    var m = db.MonAns.Find(MonDangChon.MaMon);
                    if (m == null) return;
                    m.TenMon  = TenMon.Trim();
                    m.LoaiMon = LoaiMon;
                    m.DonGia  = DonGia;
                    m.MoTa    = MoTa?.Trim();
                    m.ConHang = ConHang;
                    db.SaveChanges();
                }
                MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                XoaTrang(); TaiDuLieu();
            }
            catch { MessageBox.Show("Lỗi khi cập nhật món!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void XoaMon()
        {
            if (MonDangChon == null) { MessageBox.Show("Vui lòng chọn món cần xóa!"); return; }

            if (MessageBox.Show("Xóa món \"" + MonDangChon.TenMon + "\"?",
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning)
                != MessageBoxResult.Yes) return;

            try
            {
                using (var db = new QLYSANBONGMINIEntities())
                {
                    var m = db.MonAns.Find(MonDangChon.MaMon);
                    if (m == null) return;
                    db.MonAns.Remove(m);
                    db.SaveChanges();
                }
                MessageBox.Show("Xóa thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                XoaTrang(); TaiDuLieu();
            }
            catch { MessageBox.Show("Lỗi khi xóa món!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void XoaTrang()
        {
            _monDangChon = null;
            OnPropertyChanged(nameof(MonDangChon));
            TenMon  = string.Empty;
            LoaiMon = "Đồ uống";
            DonGia  = 0;
            MoTa    = string.Empty;
            ConHang = true;
        }
    }
}
