using System.ComponentModel.DataAnnotations;

namespace AnilUniversity.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Message { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;
    }
}