using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnilUniversity.Models
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; }

        [Required]
        public string CourseName { get; set; } = string.Empty;

        [Required]
        public string Duration { get; set; } = string.Empty;

        [Required]
        public decimal Fees { get; set; }

        [Required]
        [Range(1, 10000)]
        public int TotalSeats { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        [NotMapped]
        public int FilledSeats { get; set; }

        [NotMapped]
        public int AvailableSeats { get; set; }
    }
}