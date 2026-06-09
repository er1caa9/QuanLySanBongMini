Create Database QLYSANBONGMINI
USE QLYSANBONGMINI

-- 1. Bảng Nhân Viên
CREATE TABLE NhanVien (
    MaNV INT IDENTITY(1,1) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    SoDT VARCHAR(15),
    TenDangNhap VARCHAR(50) UNIQUE NOT NULL,
    MatKhau VARCHAR(255) NOT NULL,
    VaiTro NVARCHAR(50)
);

-- 2. Bảng Khách Hàng
CREATE TABLE KhachHang (
    MaKH INT IDENTITY(1,1) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    SoDT VARCHAR(15) NOT NULL,
    DiaChi NVARCHAR(255),
    Email VARCHAR(100),
    NgayDangKy DATETIME DEFAULT GETDATE()
);

-- 3. Bảng Sân Bóng
CREATE TABLE SanBong (
    MaSan INT IDENTITY(1,1) PRIMARY KEY,
    TenSan NVARCHAR(100) NOT NULL,
    LoaiSan NVARCHAR(50),
    GiaThueGio DECIMAL(18,2) NOT NULL, -- Tương ứng với GiaThue/Gio
    TrangThai NVARCHAR(50),
    HinhAnh NVARCHAR(MAX)
);

-- 4. Bảng Đặt Sân (Phụ thuộc KhachHang và SanBong)
CREATE TABLE DatSan (
    MaDatSan INT IDENTITY(1,1) PRIMARY KEY,
    MaKH INT NOT NULL,
    MaSan INT NOT NULL,
    TGBatDau DATETIME NOT NULL,
    TGKetThuc DATETIME NOT NULL,
    TongTien DECIMAL(18,2),
    TrangThai NVARCHAR(50),
    FOREIGN KEY (MaKH) REFERENCES KhachHang(MaKH),
    FOREIGN KEY (MaSan) REFERENCES SanBong(MaSan)
);

-- 5. Bảng Hóa Đơn (Phụ thuộc DatSan và NhanVien)
CREATE TABLE HoaDon (
    MaHD INT IDENTITY(1,1) PRIMARY KEY,
    MaDatSan INT NULL, -- Có thể NULL nếu khách mua lẻ đồ uống/dịch vụ mà không đặt sân
    MaNV INT NOT NULL,
    NgayLap DATETIME DEFAULT GETDATE(),
    TongTien DECIMAL(18,2),
    LoaiHD NVARCHAR(50),
    FOREIGN KEY (MaDatSan) REFERENCES DatSan(MaDatSan),
    FOREIGN KEY (MaNV) REFERENCES NhanVien(MaNV)
);

-- 6. Bảng Chi Tiết Hóa Đơn (Phụ thuộc HoaDon)
CREATE TABLE ChiTietHoaDon (
    MaCTHD INT IDENTITY(1,1) PRIMARY KEY,
    MaHD INT NOT NULL,
    MoTa NVARCHAR(255),
    DonGia DECIMAL(18,2),
    SoLuong INT,
    ThanhTien DECIMAL(18,2),
    FOREIGN KEY (MaHD) REFERENCES HoaDon(MaHD)
);
-- 7. Bảng món ăn
CREATE TABLE MonAn (
    MaMon    INT IDENTITY(1,1) PRIMARY KEY,
    TenMon   NVARCHAR(100)  NOT NULL,
    LoaiMon  NVARCHAR(50)   NOT NULL,   -- 'Đồ uống' hoặc 'Đồ ăn'
    DonGia   DECIMAL(18,0)  NOT NULL,
    MoTa     NVARCHAR(200)  NULL,
    ConHang  BIT            NOT NULL DEFAULT 1
);


-- ============================================================
-- 1. NHÂN VIÊN (6 người)
-- ============================================================
INSERT INTO dbo.NhanVien (HoTen, SoDT, TenDangNhap, MatKhau, VaiTro) VALUES
('Nguyễn Anh Bằng',   '0901234567', 'anhbang',   '123456', N'Quản lý'),
('Nguyễn Tường An',      '0912345678', 'tuongan',  '123456', N'Nhân viên'),
('Lê Gia Bảo ',       '0923456789', 'lebao',   '123456', N'Nhân viên'),
('Ngô Viết Duy Anh',      '0934567890', 'duyanh',    '123456', N'Nhân viên'),
('Quách Quốc Anh',      '0945678901', 'quocanh',    '123456', N'Nhân viên'),
('Nguyễn Đức Anh',   '0956789012', 'ducanh',   '123456', N'Quản lý');
GO

-- ============================================================
-- 2. SÂN BÓNG (8 sân)
-- ============================================================
INSERT INTO dbo.SanBong (TenSan, LoaiSan, GiaThueGio, TrangThai, HinhAnh) VALUES
(N'Sân A1', N'Sân 5 người', 150000, N'Trống',       NULL),
(N'Sân A2', N'Sân 5 người', 150000, N'Trống',       NULL),
(N'Sân B1', N'Sân 7 người', 250000, N'Trống',       NULL),
(N'Sân B2', N'Sân 7 người', 250000, N'Đang bảo trì', NULL),
(N'Sân C1', N'Sân 11 người',400000, N'Trống',       NULL),
(N'Sân C2', N'Sân 11 người',400000, N'Trống',       NULL),
(N'Sân VIP1', N'Sân 5 người', 200000, N'Trống',     NULL),
(N'Sân VIP2', N'Sân 7 người', 300000, N'Trống',     NULL);
GO

-- ============================================================
-- 3. KHÁCH HÀNG (12 khách)
-- ============================================================
INSERT INTO dbo.KhachHang (HoTen, SoDT, DiaChi, Email, NgayDangKy) VALUES
(N'Nguyễn Văn An',      '0901111111', N'123 Lê Lợi, Q1, TP.HCM',           'an.nguyen@gmail.com',    '2024-01-10'),
(N'Trần Minh Tuấn',     '0902222222', N'45 Nguyễn Huệ, Q1, TP.HCM',        'tuan.tran@gmail.com',    '2024-01-15'),
(N'Lê Thị Bích',        '0903333333', N'78 Trần Hưng Đạo, Q5, TP.HCM',     'bich.le@gmail.com',      '2024-02-01'),
(N'Phạm Quang Huy',     '0904444444', N'12 Đinh Tiên Hoàng, BT, TP.HCM',   'huy.pham@gmail.com',     '2024-02-14'),
(N'Võ Thành Nam',       '0905555555', N'56 Hai Bà Trưng, Q3, TP.HCM',      'nam.vo@gmail.com',       '2024-03-05'),
(N'Đặng Thị Thu',       '0906666666', N'89 Cách Mạng Tháng 8, Q10, TP.HCM','thu.dang@gmail.com',     '2024-03-20'),
(N'Bùi Văn Khoa',       '0907777777', N'34 Lý Thường Kiệt, Q10, TP.HCM',   'khoa.bui@gmail.com',     '2024-04-01'),
(N'Ngô Thị Hằng',       '0908888888', N'67 Nguyễn Thị Minh Khai, Q3, TP.HCM','hang.ngo@gmail.com',  '2024-04-18'),
(N'Hoàng Minh Đức',     '0909999999', N'23 Pasteur, Q3, TP.HCM',            'duc.hoang@gmail.com',    '2024-05-02'),
(N'Phan Thị Yến',       '0910101010', N'90 Nam Kỳ Khởi Nghĩa, Q3, TP.HCM', 'yen.phan@gmail.com',     '2024-05-15'),
(N'Đinh Quốc Toản',     '0911111110', N'15 Đinh Bộ Lĩnh, BT, TP.HCM',      'toan.dinh@gmail.com',    '2024-06-01'),
(N'Lương Thị Phương',   '0912121212', N'48 Nơ Trang Long, BT, TP.HCM',      'phuong.luong@gmail.com', '2024-06-20');
GO

-- ============================================================
-- 4. ĐẶT SÂN (20 lượt - trải đều các tháng)
-- ============================================================
INSERT INTO dbo.DatSan (MaKH, MaSan, TGBatDau, TGKetThuc, TongTien, TrangThai) VALUES
-- Tháng 1/2025
(1,  1, '2025-01-05 08:00', '2025-01-05 10:00', 300000,  N'Đã thanh toán'),
(2,  3, '2025-01-08 14:00', '2025-01-08 16:00', 500000,  N'Đã thanh toán'),
(3,  5, '2025-01-12 09:00', '2025-01-12 11:00', 800000,  N'Đã thanh toán'),
-- Tháng 2/2025
(4,  2, '2025-02-02 07:00', '2025-02-02 09:00', 300000,  N'Đã thanh toán'),
(5,  7, '2025-02-10 17:00', '2025-02-10 19:00', 400000,  N'Đã thanh toán'),
(6,  1, '2025-02-15 15:00', '2025-02-15 17:00', 300000,  N'Đã thanh toán'),
-- Tháng 3/2025
(7,  3, '2025-03-03 08:00', '2025-03-03 10:30', 625000,  N'Đã thanh toán'),
(8,  6, '2025-03-10 18:00', '2025-03-10 20:00', 800000,  N'Đã thanh toán'),
(9,  8, '2025-03-20 09:00', '2025-03-20 11:00', 600000,  N'Đã thanh toán'),
-- Tháng 4/2025
(10, 2, '2025-04-05 07:30', '2025-04-05 09:30', 300000,  N'Đã thanh toán'),
(11, 5, '2025-04-12 16:00', '2025-04-12 18:00', 800000,  N'Đã thanh toán'),
(1,  7, '2025-04-18 08:00', '2025-04-18 10:00', 400000,  N'Đã thanh toán'),
-- Tháng 5/2025
(2,  1, '2025-05-06 14:00', '2025-05-06 16:00', 300000,  N'Đã thanh toán'),
(3,  3, '2025-05-14 08:00', '2025-05-14 10:00', 500000,  N'Đã thanh toán'),
(4,  6, '2025-05-22 17:00', '2025-05-22 19:30', 1000000, N'Đã thanh toán'),
-- Tháng 6/2025
(5,  8, '2025-06-01 09:00', '2025-06-01 11:00', 600000,  N'Đã thanh toán'),
(6,  2, '2025-06-08 07:00', '2025-06-08 09:00', 300000,  N'Đã thanh toán'),
(7,  5, '2025-06-15 15:00', '2025-06-15 17:00', 800000,  N'Đã thanh toán'),
-- Đặt trước (chưa thanh toán)
(8,  1, '2025-06-20 08:00', '2025-06-20 10:00', 300000,  N'Đã đặt'),
(9,  3, '2025-06-22 14:00', '2025-06-22 16:00', 500000,  N'Đã đặt');
GO

-- ============================================================
-- 5. HÓA ĐƠN (18 hóa đơn - tương ứng các lượt đã thanh toán)
-- ============================================================
INSERT INTO dbo.HoaDon (MaDatSan, MaNV, NgayLap, TongTien, LoaiHD) VALUES
(1,  1, '2025-01-05 10:05', 300000,  N'Bán'),
(2,  2, '2025-01-08 16:05', 500000,  N'Bán'),
(3,  1, '2025-01-12 11:05', 800000,  N'Bán'),
(4,  3, '2025-02-02 09:05', 300000,  N'Bán'),
(5,  2, '2025-02-10 19:05', 400000,  N'Bán'),
(6,  1, '2025-02-15 17:05', 300000,  N'Bán'),
(7,  4, '2025-03-03 10:35', 625000,  N'Bán'),
(8,  2, '2025-03-10 20:05', 800000,  N'Bán'),
(9,  3, '2025-03-20 11:05', 600000,  N'Bán'),
(10, 1, '2025-04-05 09:35', 300000,  N'Bán'),
(11, 5, '2025-04-12 18:05', 800000,  N'Bán'),
(12, 2, '2025-04-18 10:05', 400000,  N'Bán'),
(13, 3, '2025-05-06 16:05', 300000,  N'Bán'),
(14, 1, '2025-05-14 10:05', 500000,  N'Bán'),
(15, 4, '2025-05-22 19:35', 1000000, N'Bán'),
(16, 2, '2025-06-01 11:05', 600000,  N'Bán'),
(17, 1, '2025-06-08 09:05', 300000,  N'Bán'),
(18, 3, '2025-06-15 17:05', 800000,  N'Bán');
GO

-- ============================================================
-- 6. CHI TIẾT HÓA ĐƠN
-- ============================================================
INSERT INTO dbo.ChiTietHoaDon (MaHD, MoTa, DonGia, SoLuong, ThanhTien) VALUES
(1,  N'Thuê sân A1 - Sân 5 người (2 giờ)',         150000, 2,  300000),
(2,  N'Thuê sân B1 - Sân 7 người (2 giờ)',         250000, 2,  500000),
(3,  N'Thuê sân C1 - Sân 11 người (2 giờ)',        400000, 2,  800000),
(4,  N'Thuê sân A2 - Sân 5 người (2 giờ)',         150000, 2,  300000),
(5,  N'Thuê sân VIP1 - Sân 5 người (2 giờ)',       200000, 2,  400000),
(6,  N'Thuê sân A1 - Sân 5 người (2 giờ)',         150000, 2,  300000),
(7,  N'Thuê sân B1 - Sân 7 người (2.5 giờ)',       250000, 3,  625000),
(8,  N'Thuê sân C2 - Sân 11 người (2 giờ)',        400000, 2,  800000),
(9,  N'Thuê sân VIP2 - Sân 7 người (2 giờ)',       300000, 2,  600000),
(10, N'Thuê sân A2 - Sân 5 người (2 giờ)',         150000, 2,  300000),
(11, N'Thuê sân C1 - Sân 11 người (2 giờ)',        400000, 2,  800000),
(12, N'Thuê sân VIP1 - Sân 5 người (2 giờ)',       200000, 2,  400000),
(13, N'Thuê sân A1 - Sân 5 người (2 giờ)',         150000, 2,  300000),
(14, N'Thuê sân B1 - Sân 7 người (2 giờ)',         250000, 2,  500000),
(15, N'Thuê sân C2 - Sân 11 người (2.5 giờ)',      400000, 3, 1000000),
(16, N'Thuê sân VIP2 - Sân 7 người (2 giờ)',       300000, 2,  600000),
(17, N'Thuê sân A2 - Sân 5 người (2 giờ)',         150000, 2,  300000),
(18, N'Thuê sân C1 - Sân 11 người (2 giờ)',        400000, 2,  800000);
GO

-- Dữ liệu mẫu đồ uống
INSERT INTO MonAn (TenMon, LoaiMon, DonGia, MoTa, ConHang) VALUES
(N'Nước suối',           N'Đồ uống', 10000, N'Aquafina 500ml',          1),
(N'Revive',              N'Đồ uống', 15000, N'Revive 390ml',            1),
(N'Revive chanh muối',   N'Đồ uống', 15000, N'Revive chanh muối 390ml', 1),
(N'Trà đá',              N'Đồ uống',  5000, N'Trà đá ly lớn',           1),
(N'7Up',                 N'Đồ uống', 15000, N'7Up lon 330ml',           1),
(N'Coca Cola',           N'Đồ uống', 15000, N'Coca Cola lon 330ml',     1),
(N'Sting đỏ',            N'Đồ uống', 15000, N'Sting lon 330ml',         1),
(N'Nước tăng lực Bò Húc',N'Đồ uống', 15000, N'Red Bull lon 250ml',     1),
(N'Nước cam ép',         N'Đồ uống', 25000, N'Cam ép tươi ly',          1),
(N'Cà phê đá',           N'Đồ uống', 20000, N'Cà phê đen đá ly',        1);

-- Dữ liệu mẫu đồ ăn
INSERT INTO MonAn (TenMon, LoaiMon, DonGia, MoTa, ConHang) VALUES
(N'Bánh mì',         N'Đồ ăn', 20000, N'Bánh mì pate',          1),
(N'Mì gói',          N'Đồ ăn', 15000, N'Mì tôm ly',             1),
(N'Xúc xích nướng',  N'Đồ ăn', 20000, N'Xúc xích nướng 1 cái', 1),
(N'Kẹo cao su',      N'Đồ ăn',  5000, N'Doublemint',            1),
(N'Snack',           N'Đồ ăn', 10000, N'Snack Oishi gói lớn',   1);