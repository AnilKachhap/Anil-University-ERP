using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AnilUniversity.ViewModels
{
    public class StudentDocumentViewModel
    {
        public int StudentId { get; set; }

        [Required]
        public IFormFile Photo { get; set; }

        [Required]
        public IFormFile Signature { get; set; }

        [Required]
        public IFormFile Aadhaar { get; set; }

        [Required]
        public IFormFile TenthMarksheet { get; set; }

        [Required]
        public IFormFile TwelfthMarksheet { get; set; }

        public IFormFile? GraduationMarksheet { get; set; }
    }
}