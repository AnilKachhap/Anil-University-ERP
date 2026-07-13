using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AnilUniversity.ViewModels
{
    public class EditDocumentViewModel
    {
        public int StudentId { get; set; }

        public int StudentDocumentId { get; set; }

        public string? PhotoPath { get; set; }
        public string? SignaturePath { get; set; }
        public string? AadhaarPath { get; set; }
        public string? TenthMarksheetPath { get; set; }
        public string? TwelfthMarksheetPath { get; set; }
        public string? GraduationMarksheetPath { get; set; }

        public IFormFile? Photo { get; set; }
        public IFormFile? Signature { get; set; }
        public IFormFile? Aadhaar { get; set; }
        public IFormFile? TenthMarksheet { get; set; }
        public IFormFile? TwelfthMarksheet { get; set; }
        public IFormFile? GraduationMarksheet { get; set; }
    }
}