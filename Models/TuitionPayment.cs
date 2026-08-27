using System;

namespace EduPath.Avalonia.Models
{
    /// <summary>
    /// Một lần thanh toán học phí.
    /// Một TuitionInvoice có thể có nhiều lần thanh toán.
    /// </summary>
    public class TuitionPayment
    {
        public string PaymentId { get; set; } = string.Empty;

        public string InvoiceId { get; set; } = string.Empty;

        public DateTime PaidAt { get; set; }

        public decimal Amount { get; set; }

        public string Method { get; set; } = string.Empty;

        public string Note { get; set; } = string.Empty;
    }
}