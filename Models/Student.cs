using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnilUniversity.Models
{
    public class Student
    {
        [Key]
        public int StudentId { get; set; }

        
        [StringLength(20)]
        public string? ApplicationNo { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        public string FatherName { get; set; }

        [Required]
        public string MotherName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DOB { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        [Phone]
        public string Mobile { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public Course? Course { get; set; }

       
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Pending";

        public string? Remarks { get; set; }
        public StudentDocument? StudentDocument { get; set; }
        public string? StudentCode { get; set; }
    }
}