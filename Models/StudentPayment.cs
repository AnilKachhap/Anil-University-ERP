using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnilUniversity.Models
{
    public class StudentPayment
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public Student? Student { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Required]
        public string PaymentMode { get; set; } = "Cash";

        public string? TransactionId { get; set; }

        public string PaymentStatus { get; set; } = "Paid";

        public string? Remarks { get; set; }
        public string? RazorpayOrderId { get; set; }

        public string? RazorpayPaymentId { get; set; }

        public string? RazorpaySignature { get; set; }
    }
}