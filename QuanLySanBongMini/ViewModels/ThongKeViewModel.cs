using QuanLySanBongMini.Helpers;
using QuanLySanBongMini.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using QuanLySanBongMini.Report;
using CrystalDecisions.Shared;

namespace QuanLySanBongMini.ViewModels
{
    // Dùng để hiển thị thống kê theo từng sân bóng
    public class ThongKeSanDTO
    {
        public string TenSan { get; set; }
        public int SoLuotDat { get; set; }
        public decimal DoanhThu { get; set; }
        public string DoanhThuFormatted { get { return DoanhThu.ToString("N0") + " đ"; } }
        public double TyLeSuDung { get; set; }
        public string TyLeSuDungFormatted { get { return TyLeSuDung.ToString("0.0") + "%"; } }
    }

    // Dùng để hiển thị thống kê theo từng ngày (7 ngày gần nhất)
    public class ThongKeNgayDTO
    {
        public string Ngay { get; set; }
        public int SoLuotDat { get; set; }
        public decimal DoanhThu { get; set; }
        public string DoanhThuFormatted { get { return DoanhThu.ToString("N0") + " đ"; } }
    }

    // Dùng để hiển thị top khách hàng
    public class TopKhachHangDTO
    {
        public int STT { get; set; }
        public string TenKhachHang { get; set; }
        public string SoDienThoai { get; set; }
        public int SoLanDat { get; set; }
        public decimal TongChiTieu { get; set; }
        public string TongChiTieuFormatted { get { return TongChiTieu.ToString("N0") + " đ"; } }
    }

    public class ThongKeViewModel : BaseViewModel
    {
        // Bộ lọc thời gian
        private DateTime _tuNgay = DateTime.Today.AddDays(-30);
        public DateTime TuNgay
        {
            get { return _tuNgay; }
            set { _tuNgay = value; OnPropertyChanged(); }
        }

        private DateTime _denNgay = DateTime.Today;
        public DateTime DenNgay
        {
            get { return _denNgay; }
            set { _denNgay = value; OnPropertyChanged(); }
        }

        // Các chỉ số tổng quan (KPI)
        private decimal _tongDoanhThu;
        public decimal TongDoanhThu
        {
            get { return _tongDoanhThu; }
            set { _tongDoanhThu = value; OnPropertyChanged(); OnPropertyChanged(nameof(TongDoanhThuFormatted)); }
        }
        public string TongDoanhThuFormatted { get { return TongDoanhThu.ToString("N0") + " đ"; } }

        private int _tongLuotDat;
        public int TongLuotDat
        {
            get { return _tongLuotDat; }
            set { _tongLuotDat = value; OnPropertyChanged(); }
        }

        private string _doanhThuTrungBinhNgay = "0 đ";
        public string DoanhThuTrungBinhNgayFormatted
        {
            get { return _doanhThuTrungBinhNgay; }
            set { _doanhThuTrungBinhNgay = value; OnPropertyChanged(); }
        }

        private string _sanPhoBienNhat = "-";
        public string SanPhoBienNhat
        {
            get { return _sanPhoBienNhat; }
            set { _sanPhoBienNhat = value; OnPropertyChanged(); }
        }

        private int _soKhachMoi;
        public int SoKhachMoi
        {
            get { return _soKhachMoi; }
            set { _soKhachMoi = value; OnPropertyChanged(); }
        }

        private string _tangTruongDoanhThu = "";
        public string DoanhThuTangTruongFormatted
        {
            get { return _tangTruongDoanhThu; }
            set { _tangTruongDoanhThu = value; OnPropertyChanged(); }
        }

        private string _tangTruongLuotDat = "";
        public string LuotDatTangTruongFormatted
        {
            get { return _tangTruongLuotDat; }
            set { _tangTruongLuotDat = value; OnPropertyChanged(); }
        }

        // Các bảng dữ liệu
        private ObservableCollection<ThongKeSanDTO> _thongKeSan;
        public ObservableCollection<ThongKeSanDTO> DanhSachThongKeSan
        {
            get { return _thongKeSan; }
            set { _thongKeSan = value; OnPropertyChanged(); }
        }

        private ObservableCollection<ThongKeNgayDTO> _thongKeNgay;
        public ObservableCollection<ThongKeNgayDTO> ThongKeTheoNgay
        {
            get { return _thongKeNgay; }
            set { _thongKeNgay = value; OnPropertyChanged(); }
        }

        private ObservableCollection<TopKhachHangDTO> _topKhachHang;
        public ObservableCollection<TopKhachHangDTO> TopKhachHang
        {
            get { return _topKhachHang; }
            set { _topKhachHang = value; OnPropertyChanged(); }
        }

        // Trạng thái loading
        private bool _dangTai;
        public bool IsLoading
        {
            get { return _dangTai; }
            set { _dangTai = value; OnPropertyChanged(); }
        }

        private string _trangThaiThongBao = "Sẵn sàng";
        public string TrangThaiThongBao
        {
            get { return _trangThaiThongBao; }
            set { _trangThaiThongBao = value; OnPropertyChanged(); }
        }

        // Các nút bấm
        public RelayCommand LamMoiCommand { get; set; }
        public RelayCommand LocTheoKyCommand { get; set; }
        public RelayCommand XuatBaoCaoCommand { get; set; }

        public ThongKeViewModel()
        {
            LamMoiCommand     = new RelayCommand(p => TaiDuLieu(),              p => true);
            LocTheoKyCommand  = new RelayCommand(p => LocTheoKy(p as string),   p => true);
            XuatBaoCaoCommand = new RelayCommand(p => XuatBaoCao(),             p => true);
            TaiDuLieu();
        }

        // Lọc nhanh theo các kỳ định sẵn (Hôm nay, Tuần này, Tháng này...)
        private void LocTheoKy(string ky)
        {
            var homNay = DateTime.Today;
            switch (ky)
            {
                case "Hôm nay":
                    TuNgay  = homNay;
                    DenNgay = homNay;
                    break;
                case "Tuần này":
                    int thu = (int)homNay.DayOfWeek;
                    TuNgay  = homNay.AddDays(-(thu == 0 ? 6 : thu - 1));
                    DenNgay = homNay;
                    break;
                case "Tháng này":
                    TuNgay  = new DateTime(homNay.Year, homNay.Month, 1);
                    DenNgay = homNay;
                    break;
                case "Quý này":
                    int quy = (homNay.Month - 1) / 3;
                    TuNgay  = new DateTime(homNay.Year, quy * 3 + 1, 1);
                    DenNgay = homNay;
                    break;
                case "Năm nay":
                    TuNgay  = new DateTime(homNay.Year, 1, 1);
                    DenNgay = homNay;
                    break;
            }
            TaiDuLieu();
        }

        private void TaiDuLieu()
        {
            IsLoading = true;
            TrangThaiThongBao = "Đang tải dữ liệu...";

            try
            {
                using (var db = new QLYSANBONGMINIEntities())
                {
                    // Lấy các đặt sân đã thanh toán trong khoảng thời gian
                    var ngayDenKetThuc = DenNgay.Date.AddDays(1);
                    var danhSachDatSan = db.DatSans
                        .Where(d => d.TGBatDau >= TuNgay.Date
                                 && d.TGBatDau < ngayDenKetThuc
                                 && d.TrangThai == "Đã thanh toán")
                        .ToList();

                    // Tính tổng doanh thu và lượt đặt
                    TongDoanhThu = danhSachDatSan.Sum(d => d.TongTien ?? 0);
                    TongLuotDat  = danhSachDatSan.Count;

                    // Doanh thu trung bình mỗi ngày
                    int soNgay = Math.Max(1, (DenNgay.Date - TuNgay.Date).Days + 1);
                    decimal trungBinhNgay = TongDoanhThu / soNgay;
                    DoanhThuTrungBinhNgayFormatted = trungBinhNgay.ToString("N0") + " đ";

                    // Tìm sân được đặt nhiều nhất
                    var sanPhoBien = danhSachDatSan
                        .GroupBy(d => d.MaSan)
                        .OrderByDescending(g => g.Count())
                        .FirstOrDefault();

                    if (sanPhoBien != null)
                    {
                        var san = db.SanBongs.Find(sanPhoBien.Key);
                        SanPhoBienNhat = san?.TenSan ?? "-";
                    }
                    else
                    {
                        SanPhoBienNhat = "-";
                    }

                    // Đếm khách hàng mới trong kỳ
                    SoKhachMoi = db.KhachHangs
                        .Count(k => k.NgayDangKy >= TuNgay.Date && k.NgayDangKy < ngayDenKetThuc);

                    // So sánh tăng trưởng với kỳ trước
                    var kyTruocTuNgay  = TuNgay.AddDays(-soNgay);
                    var kyTruocDenNgay = TuNgay.Date;
                    var dsKyTruoc = db.DatSans
                        .Where(d => d.TGBatDau >= kyTruocTuNgay
                                 && d.TGBatDau < kyTruocDenNgay
                                 && d.TrangThai == "Đã thanh toán")
                        .ToList();

                    decimal dtKyTruoc = dsKyTruoc.Sum(d => d.TongTien ?? 0);
                    if (dtKyTruoc > 0)
                    {
                        double phanTram = (double)((TongDoanhThu - dtKyTruoc) / dtKyTruoc * 100);
                        DoanhThuTangTruongFormatted = (phanTram >= 0 ? "▲ " : "▼ ") +
                            Math.Abs(phanTram).ToString("0.0") + "%";

                        double phanTramLuot = dsKyTruoc.Count > 0
                            ? (double)(TongLuotDat - dsKyTruoc.Count) / dsKyTruoc.Count * 100
                            : 0;
                        LuotDatTangTruongFormatted = (phanTramLuot >= 0 ? "▲ " : "▼ ") +
                            Math.Abs(phanTramLuot).ToString("0.0") + "%";
                    }
                    else
                    {
                        DoanhThuTangTruongFormatted = "";
                        LuotDatTangTruongFormatted = "";
                    }

                    // Bảng thống kê theo từng sân
                    int tongLuot = danhSachDatSan.Count > 0 ? danhSachDatSan.Count : 1;
                    var thongKeSan = danhSachDatSan
                        .GroupBy(d => d.MaSan)
                        .Select(g => new ThongKeSanDTO
                        {
                            TenSan     = db.SanBongs.Find(g.Key)?.TenSan ?? "?",
                            SoLuotDat  = g.Count(),
                            DoanhThu   = g.Sum(d => d.TongTien ?? 0),
                            TyLeSuDung = Math.Round((double)g.Count() / tongLuot * 100, 1)
                        })
                        .OrderByDescending(x => x.DoanhThu)
                        .ToList();
                    DanhSachThongKeSan = new ObservableCollection<ThongKeSanDTO>(thongKeSan);

                    // Bảng 7 ngày gần nhất
                    var cacNgayGanNhat = Enumerable.Range(0, 7)
                        .Select(i => DateTime.Today.AddDays(-6 + i))
                        .ToList();

                    var thongKeNgay = cacNgayGanNhat.Select(ngay =>
                    {
                        var ngaySau = ngay.AddDays(1);
                        return new ThongKeNgayDTO
                        {
                            Ngay      = ngay.ToString("dd/MM"),
                            SoLuotDat = db.DatSans.Count(d =>
                                d.TGBatDau >= ngay && d.TGBatDau < ngaySau
                                && d.TrangThai == "Đã thanh toán"),
                            DoanhThu  = db.DatSans
                                .Where(d => d.TGBatDau >= ngay && d.TGBatDau < ngaySau
                                         && d.TrangThai == "Đã thanh toán")
                                .Sum(d => (decimal?)d.TongTien) ?? 0
                        };
                    }).ToList();
                    ThongKeTheoNgay = new ObservableCollection<ThongKeNgayDTO>(thongKeNgay);

                    // Top 5 khách hàng chi tiêu nhiều nhất
                    var topKH = db.DatSans
                        .Where(d => d.TrangThai == "Đã thanh toán")
                        .GroupBy(d => d.MaKH)
                        .Select(g => new
                        {
                            MaKH        = g.Key,
                            SoLanDat    = g.Count(),
                            TongChiTieu = g.Sum(d => d.TongTien ?? 0)
                        })
                        .OrderByDescending(x => x.TongChiTieu)
                        .Take(5)
                        .ToList()
                        .Select((x, viTri) => new TopKhachHangDTO
                        {
                            STT          = viTri + 1,
                            TenKhachHang = db.KhachHangs.Find(x.MaKH)?.HoTen ?? "?",
                            SoDienThoai  = db.KhachHangs.Find(x.MaKH)?.SoDT ?? "",
                            SoLanDat     = x.SoLanDat,
                            TongChiTieu  = x.TongChiTieu
                        })
                        .ToList();
                    TopKhachHang = new ObservableCollection<TopKhachHangDTO>(topKH);

                    TrangThaiThongBao = "Cập nhật lúc " + DateTime.Now.ToString("HH:mm:ss") +
                                        " · " + TongLuotDat + " lượt đặt trong kỳ";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải thống kê: " + ex.Message, "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                TrangThaiThongBao = "Lỗi tải dữ liệu";
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Xuất báo cáo đơn giản (hiện MessageBox, có thể nâng cấp sau)
        private void XuatBaoCao()
        {
            try
            {
                // Kiểm tra có dữ liệu không
                if (DanhSachThongKeSan == null || DanhSachThongKeSan.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu để xuất!\nVui lòng tải dữ liệu trước.",
                                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                BaoCaoDataSet ds = BaoCaoHelper.TaoBaoCaoDataSet(DanhSachThongKeSan, ThongKeTheoNgay, TopKhachHang);

                BaoCaoCrystalReport report = new BaoCaoCrystalReport();
                report.SetDataSource(ds);

                // Bước 4: Chọn nơi lưu file PDF
                Microsoft.Win32.SaveFileDialog saveDialog = new Microsoft.Win32.SaveFileDialog();
                saveDialog.Title = "Lưu báo cáo";
                saveDialog.FileName = "BaoCaoThongKe_" + DateTime.Now.ToString("ddMMyyyy");
                saveDialog.DefaultExt = ".pdf";
                saveDialog.Filter = "PDF file (*.pdf)|*.pdf";

                // Nếu người dùng bấm Cancel thì thôi
                if (saveDialog.ShowDialog() != true)
                    return;

                // Bước 5: Xuất ra PDF
                ExportOptions options = new ExportOptions();
                options.ExportFormatType = ExportFormatType.PortableDocFormat;
                options.ExportDestinationType = ExportDestinationType.DiskFile;

                DiskFileDestinationOptions fileOptions = new DiskFileDestinationOptions();
                fileOptions.DiskFileName = saveDialog.FileName;
                options.ExportDestinationOptions = fileOptions;

                report.Export(options);

                // Bước 6: Hỏi có muốn mở file vừa xuất không
                MessageBoxResult ketQua = MessageBox.Show(
                    "Xuất báo cáo thành công!\n\nBạn có muốn mở file PDF ngay bây giờ không?",
                    "Thành công",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (ketQua == MessageBoxResult.Yes)
                {
                    // Mở bằng phần mềm đọc PDF mặc định của máy (Acrobat, Edge, v.v.)
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                    {
                        FileName = saveDialog.FileName,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xuất báo cáo:\n" + ex.Message,
                                "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
