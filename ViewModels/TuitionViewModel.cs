// Viewmodel/TuitionViewModel.cs

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using EduPath.Avalonia.Models;
using EduPath.Avalonia.Data;

namespace EduPath.Avalonia.ViewModels
{
    public class TuitionViewModel : ViewModelBase, IRefreshable
    {
        private readonly InMemoryStore _store = InMemoryStore.Instance;
        // =====================================================
        // STUDENT
        // =====================================================
        public Student Student { get; }
        public string StudentName => Student.FullName;
        public string StudentId => Student.StudentId;
        public string StudentClass => Student.ClassCode;
        public string Faculty => Student.Faculty;

        // =====================================================
        // SEMESTER
        // =====================================================
        private string _semesterName = "HK1 2026-2027";

        public string SemesterName => _semesterName;

        // =====================================================
        // TUITION CONFIG
        // =====================================================
        public decimal LectureUnitPrice => 300000m;
        public decimal PracticeUnitPrice => 150000m;

        // =====================================================
        // TUITION DATA
        // =====================================================
        public ObservableCollection<TuitionDetailRow> TuitionDetails { get; } = new();
        public ObservableCollection<PaymentHistoryRow> PaymentHistory { get; } = new();

        private TuitionInvoice? _currentInvoice;

        public TuitionInvoice? CurrentInvoice
        {
            get => _currentInvoice;
            private set
            {
                _currentInvoice = value;

                RaisePropertyChanged(nameof(CurrentInvoice));
                RaisePropertyChanged(nameof(InvoiceStatusText));
                RaisePropertyChanged(nameof(InvoiceStatusDescription));
            }
        }

        public string InvoiceStatusText
        {
            get
            {
                if (CurrentInvoice == null)
                    return "CHƯA CÓ HÓA ĐƠN";

                return CurrentInvoice.Status switch
                {
                    TuitionInvoiceStatus.Paid => "ĐÃ ĐÓNG",
                    TuitionInvoiceStatus.PartiallyPaid => "ĐÃ ĐÓNG MỘT PHẦN",
                    _ => "CHƯA ĐÓNG"
                };
            }
        }

        public string InvoiceStatusDescription
        {
            get
            {
                return CurrentInvoice?.Status switch
                {
                    TuitionInvoiceStatus.Paid => "Học phí của học kỳ này đã được thanh toán đầy đủ.",
                    TuitionInvoiceStatus.PartiallyPaid => "Bạn đã thanh toán một phần học phí.",
                    TuitionInvoiceStatus.Unpaid => "Bạn chưa thực hiện thanh toán học phí.",
                    _ => "Chưa có thông tin hóa đơn."
                };
            }
        }


        // =====================================================
        // SUMMARY
        // =====================================================
        public int LectureCredits => TuitionDetails
            .Where(x => x.Type == "Lý thuyết (LT)")
            .Sum(x => x.Credits);

        public int PracticeCredits => TuitionDetails
            .Where(x => x.Type == "Thực hành (TH)")
            .Sum(x => x.Credits);

        public int TotalCredits => TuitionDetails.Sum(x => x.Credits);
        public decimal TotalTuition => TuitionDetails.Sum(x => x.Amount);
        public string TotalTuitionText => $"{TotalTuition:N0} đ";
        public string LectureUnitPriceText => $"{LectureUnitPrice:N0} đ / TC";
        public string PracticeUnitPriceText => $"{PracticeUnitPrice:N0} đ / TC";

        // =====================================================
        // PAYMENT SUMMARY
        // =====================================================
        public decimal TotalPaid => PaymentHistory
            .Where(x => x.Status == "Đã thanh toán")
            .Sum(x => x.Amount);

        public decimal RemainingAmount => Math.Max(0, TotalTuition - TotalPaid);
        public string RemainingAmountText => $"{RemainingAmount:N0} đ";

        public string PaymentStatusText
        {
            get
            {
                if (RemainingAmount <= 0)
                    return "Đã thanh toán";

                if (TotalPaid > 0)
                    return "Đã thanh toán một phần";

                return "Chưa thanh toán";
            }
        }

        // =====================================================
        // COMMANDS
        // =====================================================
        public RelayCommand PayCommand { get; }
        public RelayCommand PrintInvoiceCommand { get; }
        public RelayCommand ExportTuitionExcelCommand { get; }
        private void ExportTuitionExcel()
        {
            string folder = GetExportFolder();

            string filePath = Path.Combine(
                folder,
                $"BangTinhHocPhi_{Student.StudentId}.xlsx"
            );

            using var workbook = new XLWorkbook();

            var worksheet =
                workbook.Worksheets.Add("Bảng học phí");

            worksheet.Cell(1, 1).Value =
                "BẢNG TÍNH HỌC PHÍ";

            worksheet.Cell(2, 1).Value =
                $"Sinh viên: {Student.FullName}";

            worksheet.Cell(3, 1).Value =
                $"Mã sinh viên: {Student.StudentId}";

            // Header
            worksheet.Cell(5, 1).Value =
                "Loại tín chỉ";

            worksheet.Cell(5, 2).Value =
                "Số tín chỉ";

            worksheet.Cell(5, 3).Value =
                "Đơn giá";

            worksheet.Cell(5, 4).Value =
                "Thành tiền";

            int row = 6;

            foreach (var item in TuitionDetails)
            {
                worksheet.Cell(row, 1).Value =
                    item.Type;

                worksheet.Cell(row, 2).Value =
                    item.Credits;

                worksheet.Cell(row, 3).Value =
                    item.UnitPrice;

                worksheet.Cell(row, 4).Value =
                    item.Amount;

                row++;
            }

            // Tổng cộng
            worksheet.Cell(row + 1, 1).Value =
                "TỔNG CỘNG";

            worksheet.Cell(row + 1, 2).Value =
                TuitionDetails.Sum(x => x.Credits);

            worksheet.Cell(row + 1, 4).Value =
                TotalTuition;

            // Định dạng tiền
            worksheet.Column(3)
                .Style
                .NumberFormat
                .Format = "#,##0 \"đ\"";

            worksheet.Column(4)
                .Style
                .NumberFormat
                .Format = "#,##0 \"đ\"";

            // In đậm tiêu đề
            worksheet.Range("A1:D1")
                .Merge();

            worksheet.Cell(1, 1)
                .Style
                .Font
                .Bold = true;

            worksheet.Cell(1, 1)
                .Style
                .Font
                .FontSize = 18;

            worksheet.Range("A5:D5")
                .Style
                .Font
                .Bold = true;

            worksheet.Range(
                row + 1,
                1,
                row + 1,
                4
            )
            .Style
            .Font
            .Bold = true;

            // Tự động chỉnh độ rộng cột
            worksheet.Columns()
                .AdjustToContents();

            workbook.SaveAs(filePath);

            OpenFile(filePath);
        }

        // private string GetExportFolder()
        // {
        //     string desktop =
        //         Environment.GetFolderPath(
        //             Environment.SpecialFolder.Desktop
        //         );

        //     string folder =
        //         Path.Combine(
        //             desktop,
        //             "EduPath_Exports"
        //         );

        //     if (!Directory.Exists(folder))
        //     {
        //         Directory.CreateDirectory(folder);
        //     }

        //     return folder;
        // }

        private void OpenFile(string filePath)
        {
            try
            {
                Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    }
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Không thể mở file: {ex.Message}"
                );
            }
        }


        private void ExportPaymentExcel()
        {
            try
            {
                string folder = GetExportFolder();

                string filePath = Path.Combine(
                    folder,
                    $"LichSuThanhToan_{StudentId}.xlsx"
                );

                using var workbook = new XLWorkbook();

                var worksheet =
                    workbook.Worksheets.Add("Lịch sử thanh toán");


                // ============================
                // TIÊU ĐỀ
                // ============================

                worksheet.Range("A1:E1").Merge();

                worksheet.Cell("A1").Value =
                    "LỊCH SỬ THANH TOÁN HỌC PHÍ";

                worksheet.Cell("A1").Style.Font.Bold = true;
                worksheet.Cell("A1").Style.Font.FontSize = 16;

                worksheet.Cell("A1")
                    .Style.Alignment.Horizontal =
                        XLAlignmentHorizontalValues.Center;


                // ============================
                // THÔNG TIN SINH VIÊN
                // ============================

                worksheet.Cell("A3").Value =
                    "Sinh viên:";

                worksheet.Cell("B3").Value =
                    StudentName;

                worksheet.Cell("A4").Value =
                    "MSSV:";

                worksheet.Cell("B4").Value =
                    StudentId;

                worksheet.Cell("A5").Value =
                    "Học kỳ:";

                worksheet.Cell("B5").Value =
                    SemesterName;


                // ============================
                // HEADER
                // ============================

                int headerRow = 7;

                worksheet.Cell(headerRow, 1).Value =
                    "Mã biên lai";

                worksheet.Cell(headerRow, 2).Value =
                    "Ngày giao dịch";

                worksheet.Cell(headerRow, 3).Value =
                    "Số tiền";

                worksheet.Cell(headerRow, 4).Value =
                    "Phương thức";

                worksheet.Cell(headerRow, 5).Value =
                    "Trạng thái";


                worksheet.Range(
                    headerRow,
                    1,
                    headerRow,
                    5
                ).Style.Font.Bold = true;

                worksheet.Range(
                    headerRow,
                    1,
                    headerRow,
                    5
                ).Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;


                // ============================
                // DỮ LIỆU THANH TOÁN
                // ============================

                int row = headerRow + 1;

                foreach (var item in PaymentHistory)
                {
                    worksheet.Cell(row, 1).Value =
                        item.ReceiptCode;

                    worksheet.Cell(row, 2).Value =
                        item.TransactionDate;

                    worksheet.Cell(row, 3).Value =
                        item.Amount;

                    worksheet.Cell(row, 4).Value =
                        item.Method;

                    worksheet.Cell(row, 5).Value =
                        item.Status;

                    row++;
                }


                // ============================
                // ĐỊNH DẠNG
                // ============================

                worksheet.Column(2)
                    .Style.DateFormat.Format =
                        "dd/MM/yyyy";

                worksheet.Column(3)
                    .Style.NumberFormat.Format =
                        "#,##0 \"đ\"";


                // ============================
                // BORDER
                // ============================

                if (row > headerRow + 1)
                {
                    worksheet.Range(
                        headerRow,
                        1,
                        row - 1,
                        5
                    ).Style.Border.OutsideBorder =
                        XLBorderStyleValues.Thin;

                    worksheet.Range(
                        headerRow,
                        1,
                        row - 1,
                        5
                    ).Style.Border.InsideBorder =
                        XLBorderStyleValues.Thin;
                }


                // ============================
                // TỰ ĐỘNG ĐIỀU CHỈNH CỘT
                // ============================

                worksheet.Columns().AdjustToContents();


                // ============================
                // LƯU FILE
                // ============================

                workbook.SaveAs(filePath);


                // MỞ FILE
                Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    }
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Lỗi xuất Excel lịch sử thanh toán: {ex.Message}"
                );
            }
        }

        public RelayCommand PrintTuitionCommand { get; }
        public RelayCommand ExportPaymentExcelCommand { get; }
        public RelayCommand PrintPaymentCommand { get; }
        public RelayCommand RefreshCommand { get; }

        // =====================================================
        // CONSTRUCTOR
        // =====================================================
        public TuitionViewModel(Student student)
        {
            Student = student;
            PayCommand = new RelayCommand(PayTuition);
            PrintInvoiceCommand = new RelayCommand(PrintInvoice);
            ExportTuitionExcelCommand = new RelayCommand(ExportTuitionExcel);
            PrintTuitionCommand = new RelayCommand(PrintTuition);
            ExportPaymentExcelCommand = new RelayCommand(ExportPaymentExcel);
            PrintPaymentCommand = new RelayCommand(PrintPayment);
            RefreshCommand = new RelayCommand(Refresh);

            Load();
        }

        // =====================================================
        // LOAD
        // =====================================================
        private void Load()
        {

            Console.WriteLine("========== TUITION LOAD ==========");

            Console.WriteLine(
                $"Student: {Student.StudentId}"
            );

            Console.WriteLine(
                $"Enrollments total: {_store.Enrollments.Count}"
            );

            Console.WriteLine(
                $"Invoices total: {_store.TuitionInvoices.Count}"
            );

            Console.WriteLine(
                $"Payments total: {_store.TuitionPayments.Count}"
            );

            TuitionDetails.Clear();
            PaymentHistory.Clear();

            TuitionDetails.Clear();
            PaymentHistory.Clear();

            // =====================================================
            // 1. LẤY HÓA ĐƠN CỦA SINH VIÊN
            // =====================================================
            CurrentInvoice = _store.TuitionInvoices
                .FirstOrDefault(x =>
                    x.StudentId == Student.StudentId
                    && x.Term == SemesterName
                );

            // Nếu chưa có hóa đơn thì vẫn hiển thị danh sách học phần đã đăng ký.
            if (CurrentInvoice == null)
            {
                LoadTuitionFromEnrollments();
                RaiseAllProperties();
                return;
            }

            // =====================================================
            // 2. LẤY HỌC KỲ TỪ HÓA ĐƠN
            // =====================================================
            _semesterName = CurrentInvoice.Term;

            // =====================================================
            // 3. TÍNH HỌC PHÍ TỪ ĐƠN ĐĂNG KÝ HỌC PHẦN
            // =====================================================
            LoadTuitionFromEnrollments();

            // =====================================================
            // 4. LẤY LỊCH SỬ THANH TOÁN
            // =====================================================
            var payments = _store.TuitionPayments
                .Where(x => x.InvoiceId == CurrentInvoice.InvoiceId)
                .OrderByDescending(x => x.PaidAt)
                .ToList();

            foreach (var payment in payments)
            {
                PaymentHistory.Add(
                    new PaymentHistoryRow
                    {
                        ReceiptCode = payment.PaymentId,
                        TransactionDate = payment.PaidAt,
                        Amount = payment.Amount,
                        Method = payment.Method,
                        Status = GetPaymentStatusText()
                    }
                );
            }

            // =====================================================
            // 5. TỰ ĐỘNG CẬP NHẬT TRẠNG THÁI HÓA ĐƠN
            // =====================================================
            UpdateInvoiceStatus();

            RaiseAllProperties();
        }

        private void LoadTuitionFromEnrollments()
        {
            // =====================================================
            // LẤY CÁC LƯỢT ĐĂNG KÝ CÒN HIỆU LỰC
            // =====================================================
            var activeEnrollments = _store.Enrollments
                .Where(x =>
                    x.StudentId == Student.StudentId
                    && x.Status == EnrollmentStatus.Enrolled
                )
                .ToList();
            Console.WriteLine(
                $"SemesterName: {SemesterName}"
            );

            Console.WriteLine(
                $"StudentId: {Student.StudentId}"
            );
            Console.WriteLine(
    $"Active enrollments: {activeEnrollments.Count}"
);

foreach (var enrollment in activeEnrollments)
{
    Console.WriteLine(
        $"Enrollment: {enrollment.StudentId} - {enrollment.SectionId}"
    );
}
            // =====================================================
            // LẤY SECTION
            // =====================================================
            var sectionIds = activeEnrollments
                .Select(x => x.SectionId)
                .ToHashSet();

            var sections = _store.Sections
                .Where(x =>
                    sectionIds.Contains(x.SectionId)
                    && x.Term == SemesterName
                )
                .ToList();
Console.WriteLine(
    $"Sections found: {sections.Count}"
);

foreach (var section in sections)
{
    Console.WriteLine(
        $"Section: {section.SectionId} - Term: {section.Term}"
    );
}
            // =====================================================
            // LẤY COURSE
            // =====================================================
            var courseCodes = sections
                .Select(x => x.CourseCode)
                .Distinct()
                .ToHashSet();

            var courses = _store.Courses
                .Where(x => courseCodes.Contains(x.CourseCode))
                .ToList();

            // =====================================================
            // TÍNH TỔNG TÍN CHỈ LT / TH
            // =====================================================
            int lectureCredits = 0;
            int practiceCredits = 0;

            foreach (var course in courses)
            {
                int lecture = course.LectureCredits;
                int practice = course.PracticeCredits;

                // =================================================
                // FALLBACK
                //
                // Các Course cũ chưa có dữ liệu LT/TH.
                // Tạm coi toàn bộ tín chỉ là lý thuyết.
                //
                // Sau này khi chuyển Database:
                // lấy trực tiếp LectureCredits / PracticeCredits.
                // =================================================
                if (lecture == 0 && practice == 0)
                {
                    lecture = course.Credits;
                }

                lectureCredits += lecture;
                practiceCredits += practice;
            }

            // =====================================================
            // HIỂN THỊ BẢNG HỌC PHÍ
            // =====================================================
            if (lectureCredits > 0)
            {
                TuitionDetails.Add(
                    new TuitionDetailRow
                    {
                        Type = "Lý thuyết (LT)",
                        Credits = lectureCredits,
                        UnitPrice = LectureUnitPrice,
                        Amount = lectureCredits * LectureUnitPrice
                    }
                );
            }

            if (practiceCredits > 0)
            {
                TuitionDetails.Add(
                    new TuitionDetailRow
                    {
                        Type = "Thực hành (TH)",
                        Credits = practiceCredits,
                        UnitPrice = PracticeUnitPrice,
                        Amount = practiceCredits * PracticeUnitPrice
                    }
                );
            }
            Console.WriteLine(
    $"TuitionDetails count: {TuitionDetails.Count}"
);
        }

        private void UpdateInvoiceStatus()
        {
            if (CurrentInvoice == null)
                return;

            if (TotalTuition <= 0)
            {
                CurrentInvoice.Status = TuitionInvoiceStatus.Unpaid;
                return;
            }

            if (TotalPaid <= 0)
            {
                CurrentInvoice.Status = TuitionInvoiceStatus.Unpaid;
            }
            else if (TotalPaid < TotalTuition)
            {
                CurrentInvoice.Status = TuitionInvoiceStatus.PartiallyPaid;
            }
            else
            {
                CurrentInvoice.Status = TuitionInvoiceStatus.Paid;
            }

            RaisePropertyChanged(nameof(InvoiceStatusText));
            RaisePropertyChanged(nameof(InvoiceStatusDescription));
        }


        private string GetPaymentStatusText()
        {
            if (TotalPaid >= TotalTuition && TotalTuition > 0)
            {
                return "Đã đóng";
            }

            if (TotalPaid > 0)
            {
                return "Đóng một phần";
            }

            return "Chưa đóng";
        }


        // =====================================================
        // REFRESH
        // =====================================================
        public void Refresh()
        {
            Load();
        }

        // =====================================================
        // PRINT INVOICE
        // =====================================================
        private void PrintInvoice()
        {
            try
            {
                string folder = GetExportFolder();
                string filePath = Path.Combine(folder, $"HoaDonHocPhi_{Student.StudentId}_{SemesterName}.html");
                string html = BuildInvoiceHtml();

                File.WriteAllText(filePath, html);
                OpenFile(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Không thể tạo hóa đơn: {ex.Message}");
            }
        }

        // =====================================================
        // PRINT TUITION TABLE
        // =====================================================
        private void PrintTuition()
        {
            try
            {
                string folder = GetExportFolder();
                string filePath = Path.Combine(folder, $"BangTinhHocPhi_{Student.StudentId}_{SemesterName}.html");
                string html = BuildTuitionHtml();

                File.WriteAllText(filePath, html);
                OpenFile(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Không thể tạo file in học phí: {ex.Message}");
            }
        }

        // =====================================================
        // PRINT PAYMENT
        // =====================================================
        private void PrintPayment()
        {
            try
            {
                string folder = GetExportFolder();
                string filePath = Path.Combine(folder, $"LichSuThanhToan_{Student.StudentId}_{SemesterName}.html");
                string html = BuildPaymentHtml();

                File.WriteAllText(filePath, html);
                OpenFile(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Không thể tạo lịch sử thanh toán: {ex.Message}");
            }
        }

        // =====================================================
        // PAYMENT SIMULATION (MÔ PHỎNG THANH TOÁN)
        // =====================================================
        private void PayTuition()
        {
            // 1. Kiểm tra: Hóa đơn chưa có hoặc nợ đã trả hết (RemainingAmount <= 0) thì không làm gì cả
            if (CurrentInvoice == null || RemainingAmount <= 0)
            {
                Console.WriteLine("Hóa đơn đã được thanh toán hoặc không có khoản nợ.");
                return;
            }

            // 2. Tạo một giao dịch thanh toán mới (Mô phỏng)
            // Lấy trực tiếp số tiền còn nợ (RemainingAmount) để thanh toán toàn bộ
            var newPayment = new EduPath.Avalonia.Models.TuitionPayment
            {
                PaymentId = $"PAY-{DateTime.Now:yyyyMMddHHmmss}", // Tạo mã biên lai động theo thời gian thực
                InvoiceId = CurrentInvoice.InvoiceId,
                Amount = RemainingAmount, 
                PaidAt = DateTime.Now,
                Method = "Thanh toán trực tuyến" 
            };

            // 3. Lưu vào Database giả lập (InMemoryStore)
            _store.TuitionPayments.Add(newPayment);

            Console.WriteLine($"[Thanh toán thành công] Mã biên lai: {newPayment.PaymentId} - Số tiền: {newPayment.Amount:N0}đ");

            // 4. Reload lại toàn bộ dữ liệu (Hàm Load hiện tại của bạn đã xử lý mọi thứ)
            // - PaymentHistory tự động lấy thêm dòng giao dịch mới
            // - TotalPaid tự tăng lên
            // - RemainingAmount tự về 0
            // - UpdateInvoiceStatus() tự chuyển CurrentInvoice.Status thành Paid
            // - Các property tự động RaisePropertyChanged để XAML đổi màu Badge
            Load(); 
        }

        // =====================================================
        // OPEN FILE & FOLDER HELPER
        // =====================================================
        // private static void OpenFile(string filePath)
        // {
        //     Process.Start(new ProcessStartInfo
        //     {
        //         FileName = filePath,
        //         UseShellExecute = true
        //     });
        // }

        private static string GetExportFolder()
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "EduPath"
            );

            Directory.CreateDirectory(folder);
            return folder;
        }

        // =====================================================
        // EXCEL EXPORT (PLACEHOLDERS)
        // =====================================================




        // =====================================================
        // HTML BUILDERS
        // =====================================================
        private string BuildInvoiceHtml()
        {
            return $$"""
<!DOCTYPE html>
<html lang="vi">
<head>
<meta charset="utf-8">
<title>Hóa đơn học phí</title>
<style>
    body { font-family: Arial, sans-serif; margin: 40px; color: #172033; }
    h1 { text-align: center; font-size: 24px; }
    .info { margin-bottom: 20px; }
    table { width: 100%; border-collapse: collapse; margin-top: 20px; }
    th, td { border: 1px solid #d9e0ea; padding: 10px; text-align: left; }
    th { background: #eef2f7; }
    .total { font-size: 20px; font-weight: bold; margin-top: 20px; text-align: right; }
</style>
</head>
<body>

<h1>PHIẾU / HÓA ĐƠN HỌC PHÍ</h1>

<div class="info">
    <p><b>Sinh viên:</b> {{StudentName}}</p>
    <p><b>MSSV:</b> {{StudentId}}</p>
    <p><b>Lớp:</b> {{StudentClass}}</p>
    <p><b>Khoa:</b> {{Faculty}}</p>
    <p><b>Học kỳ:</b> {{SemesterName}}</p>
</div>

<table>
    <thead>
        <tr>
            <th>Loại tín chỉ</th>
            <th>Số tín chỉ</th>
            <th>Đơn giá</th>
            <th>Thành tiền</th>
        </tr>
    </thead>
    <tbody>
        {{string.Join("", TuitionDetails.Select(x => $"<tr><td>{x.Type}</td><td>{x.Credits}</td><td>{x.UnitPrice:N0} đ</td><td>{x.Amount:N0} đ</td></tr>"))}}
    </tbody>
</table>

<div class="total">
    Tổng cộng: {{TotalTuition:N0}} đ
</div>

<p>Ngày lập: {{DateTime.Now:dd/MM/yyyy}}</p>

</body>
</html>
""";
        }

        private string BuildTuitionHtml()
        {
            return $$"""
<!DOCTYPE html>
<html lang="vi">
<head>
<meta charset="utf-8">
<title>Bảng tính học phí</title>
<style>
    body { font-family: Arial, sans-serif; margin: 40px; color: #172033; }
    h1 { text-align: center; }
    table { width: 100%; border-collapse: collapse; }
    th, td { border: 1px solid #d9e0ea; padding: 10px; }
    th { background: #eef2f7; }
    .total { margin-top: 20px; font-size: 18px; font-weight: bold; text-align: right; }
</style>
</head>
<body>

<h1>BẢNG TÍNH HỌC PHÍ</h1>

<p>
    <b>Sinh viên:</b> {{StudentName}}<br/>
    <b>MSSV:</b> {{StudentId}}<br/>
    <b>Học kỳ:</b> {{SemesterName}}
</p>

<table>
    <thead>
        <tr>
            <th>Loại tín chỉ</th>
            <th>Số tín chỉ</th>
            <th>Đơn giá</th>
            <th>Thành tiền</th>
        </tr>
    </thead>
    <tbody>
        {{string.Join("", TuitionDetails.Select(x => $"<tr><td>{x.Type}</td><td>{x.Credits}</td><td>{x.UnitPrice:N0} đ</td><td>{x.Amount:N0} đ</td></tr>"))}}
    </tbody>
</table>

<div class="total">
    Tổng cộng: {{TotalTuition:N0}} đ
</div>

</body>
</html>
""";
        }

        private string BuildPaymentHtml()
        {
            return $$"""
<!DOCTYPE html>
<html lang="vi">
<head>
<meta charset="utf-8">
<title>Lịch sử thanh toán</title>
<style>
    body { font-family: Arial, sans-serif; margin: 40px; }
    table { width: 100%; border-collapse: collapse; }
    th, td { border: 1px solid #d9e0ea; padding: 10px; }
    th { background: #eef2f7; }
</style>
</head>
<body>

<h1>LỊCH SỬ THANH TOÁN</h1>

<p>
    <b>Sinh viên:</b> {{StudentName}}<br/>
    <b>MSSV:</b> {{StudentId}}<br/>
    <b>Học kỳ:</b> {{SemesterName}}
</p>

<table>
    <thead>
        <tr>
            <th>Mã biên lai</th>
            <th>Ngày giao dịch</th>
            <th>Số tiền</th>
            <th>Phương thức</th>
            <th>Trạng thái</th>
        </tr>
    </thead>
    <tbody>
        {{string.Join("", PaymentHistory.Select(x => $"<tr><td>{x.ReceiptCode}</td><td>{x.TransactionDate:dd/MM/yyyy}</td><td>{x.Amount:N0} đ</td><td>{x.Method}</td><td>{x.Status}</td></tr>"))}}
    </tbody>
</table>

</body>
</html>
""";
        }

        // =====================================================
        // PROPERTY CHANGE NOTIFICATION
        // =====================================================
        private void RaiseAllProperties()
        {
            RaisePropertyChanged(nameof(SemesterName));

            RaisePropertyChanged(nameof(LectureCredits));
            RaisePropertyChanged(nameof(PracticeCredits));

            RaisePropertyChanged(nameof(TotalCredits));

            RaisePropertyChanged(nameof(TotalTuition));
            RaisePropertyChanged(nameof(TotalTuitionText));

            RaisePropertyChanged(nameof(TotalPaid));

            RaisePropertyChanged(nameof(RemainingAmount));
            RaisePropertyChanged(nameof(RemainingAmountText));

            RaisePropertyChanged(nameof(PaymentStatusText));

            RaisePropertyChanged(nameof(InvoiceStatusText));
            RaisePropertyChanged(nameof(InvoiceStatusDescription));
        }
    }

    // =========================================================
    // MODEL CLASSES
    // =========================================================
    public class TuitionDetailRow
    {
        public string Type { get; set; } = string.Empty;
        public int Credits { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
    }

    public class PaymentHistoryRow
    {
        public string ReceiptCode { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}