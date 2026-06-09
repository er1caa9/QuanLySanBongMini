using QuanLySanBongMini.ViewModels;
using System.Collections.Generic;

namespace QuanLySanBongMini.Report
{
    public class BaoCaoHelper
    {
        public static BaoCaoDataSet TaoBaoCaoDataSet(
            IEnumerable<ThongKeSanDTO> dsSan,
            IEnumerable<ThongKeNgayDTO> dsNgay,
            IEnumerable<TopKhachHangDTO> dsKhach)
        {
            var ds = new BaoCaoDataSet();

            // ── Bảng ThongKeSan ──────────────────────────────────
            foreach (var row in dsSan)
                ds.ThongKeSan.AddThongKeSanRow(
                    row.TenSan,   
                    row.SoLuotDat,
                    row.DoanhThu,
                    row.TyLeSuDung);

            // ── Bảng ThongKeNgay ─────────────────────────────────
            foreach (var row in dsNgay)
                ds.ThongKeNgay.AddThongKeNgayRow(
                    row.Ngay,
                    row.SoLuotDat,
                    row.DoanhThu);

            // ── Bảng TopKhachHang ────────────────────────────────
            foreach (var row in dsKhach)
                ds.TopKhachHang.AddTopKhachHangRow(
                    row.STT,
                    row.TenKhachHang,
                    row.SoDienThoai,
                    row.SoLanDat,
                    row.TongChiTieu);

            return ds;
        }
    }
}