using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AnilUniversity.Models;

namespace AnilUniversity.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<StudentDocument> StudentDocuments { get; set; }
        public DbSet<StudentPayment> StudentPayments { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        public DbSet<Attendance> Attendances { get; set; }
    }
}