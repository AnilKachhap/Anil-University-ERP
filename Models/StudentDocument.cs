using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnilUniversity.Models
{
    public class StudentDocument
    {
        [Key]
        public int StudentDocumentId { get; set; }

        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public Student Student { get; set; }

        public string? PhotoPath { get; set; }

        public string? SignaturePath { get; set; }

        public string? AadhaarPath { get; set; }

        public string? TenthMarksheetPath { get; set; }

        public string? TwelfthMarksheetPath { get; set; }

        public string? GraduationMarksheetPath { get; set; }
    }
}