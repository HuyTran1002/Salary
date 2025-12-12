<<<<<<< HEAD
# Salary Calculator - Ứng Dụng Tính Lương

## Mô Tả
Ứng dụng tính lương bằng C# Windows Forms với giao diện thân thiện người dùng. Ứng dụng tính lương từ ngày 21 tháng này đến ngày 20 tháng sau, loại trừ các ngày thứ 7 và chủ nhật.

## Công Thức Tính

**Kỳ tính lương:** Từ ngày 21 tháng này đến ngày 20 tháng sau  
**Ngày công không tính:** Thứ 7 và Chủ nhật (không đổi dù có làm T7, CN hay OT)

### Lương 1 Ngày
Lương 1 ngày = (Lương Cơ Bản + Tiền Ăn) / Số Ngày Công

### Lương Brutto
- **Lương ngày công thường** = Số ngày công × Lương 1 ngày
- **Lương T7, CN (x2)** = Số ngày/tiếng T7, CN × Lương 1 ngày × 2
- **Lương OT trong tuần (x1.5)** = Số tiếng OT × (Lương 1 ngày / 8) × 1.5
- **Lương Brutto** = Lương ngày công + Lương T7, CN + Lương OT

### Khấu Trừ & Lương Net
- **Khấu Trừ Bảo Hiểm** = Lương Brutto × Tỷ Lệ Bảo Hiểm (%)
- **Khấu Trừ Thuế** = (Lương Brutto - Khấu Trừ Bảo Hiểm) × Tỷ Lệ Thuế (%)
- **Lương Net** = Lương Brutto - Khấu Trừ Bảo Hiểm - Khấu Trừ Thuế

## Các Trường Nhập Liệu
✅ **Lương Cơ Bản** - Lương tháng cơ bản (VND)  
✅ **Tiền Ăn** - Tiền ăn hàng ngày (VND)  
✅ **Số Ngày Công (21-20)** - Số ngày công tính lương từ 21 tháng này đến 20 tháng sau (không tính T7, CN) - mặc định 20  
✅ **Ngày/Tiếng T7, CN (x2)** - Số ngày hoặc tiếng làm việc vào T7, CN (nhân 2 lương)  
✅ **Tiếng OT Trong Tuần (x1.5)** - Số tiếng làm thêm vào các ngày bình thường (nhân 1.5 lương)  
✅ **Bảo Hiểm (%)** - Tỷ lệ bảo hiểm - mặc định 10%  
✅ **Thuế Thu Nhập (%)** - Tỷ lệ thuế - mặc định 5%  

**Lưu ý:** Số ngày công (20 ngày) **không thay đổi** dù nhân viên có làm thêm T7, CN hay tiếng OT hay không.  

## Cách Sử Dụng
1. Nhập lương cơ bản
2. Nhập tiền ăn (nếu có)
3. Nhập số ngày công trong tháng (mặc định 20)
4. Nhập số ngày/tiếng làm T7, CN (nếu có)
5. Nhập số tiếng OT trong tuần (nếu có)
6. Nhập tỷ lệ bảo hiểm (%)
7. Nhập tỷ lệ thuế (%)
8. Nhấn nút "TÍNH LƯƠNG" để xem kết quả chi tiết

## Yêu Cầu Hệ Thống
- .NET 6.0 SDK or later
- Windows OS

## Biên Dịch & Chạy
```bash
dotnet build
dotnet run
```

## Cấu Trúc Tệp
- `Program.cs` - Entry point của ứng dụng
- `SalaryCalculatorForm.cs` - Giao diện người dùng
- `SalaryCalculator.csproj` - Tệp cấu hình dự án
=======
# Salary
>>>>>>> 67ac8688c5643cbc4d16556a319684d21579c684
