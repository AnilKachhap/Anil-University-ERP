using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnilUniversity.Models
{
    public class Attendance
    {
        [Key]
        public int AttendanceId { get; set; }

        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        [ValidateNever]
        public Student? Student { get; set; }

        [DataType(DataType.Date)]
        public DateTime AttendanceDate { get; set; } = DateTime.Today;

        [Required]
        public string Status { get; set; } = "Present";
    }
}