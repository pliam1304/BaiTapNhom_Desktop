using System;

namespace EduPath.Avalonia.Models
{
    /// <summary>
    /// Hóa đơn học phí của một sinh viên trong một học kỳ.
    ///
    /// Hiện tại dữ liệu được lưu trong InMemoryStore.
    /// Sau này có thể chuyển sang Database / Entity Framework.
    /// </summary>
    public class TuitionInvoice
    {
        public string InvoiceId { get; set; } = string.Empty;

        public string StudentId { get; set; } = string.Empty;

        public string Term { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public TuitionInvoiceStatus Status { get; set; }
    }
}