using AnilUniversity.Data;
using AnilUniversity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AnilUniversity.Controllers
{
    [Authorize(Roles = "Admin")]

    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

      
        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewBag.TotalStudents = _context.Students.Count();
            DateTime today = DateTime.Today;

            ViewBag.TodayStudents = _context.Students
                .Count(x => x.CreatedDate.Date == today);

            ViewBag.MonthStudents = _context.Students
                .Count(x => x.CreatedDate.Month == today.Month &&
                            x.CreatedDate.Year == today.Year);

            ViewBag.YearStudents = _context.Students
                .Count(x => x.CreatedDate.Year == today.Year);
            ViewBag.TotalCourses = _context.Courses.Count();

            ViewBag.Pending = _context.Students.Count(x => x.Status == "Pending");
            ViewBag.Approved = _context.Students.Count(x => x.Status == "Approved");
            ViewBag.Rejected = _context.Students.Count(x => x.Status == "Rejected");

            ViewBag.Jan = _context.Students.Count(x => x.CreatedDate.Month == 1);
            ViewBag.Feb = _context.Students.Count(x => x.CreatedDate.Month == 2);
            ViewBag.Mar = _context.Students.Count(x => x.CreatedDate.Month == 3);
            ViewBag.Apr = _context.Students.Count(x => x.CreatedDate.Month == 4);
            ViewBag.May = _context.Students.Count(x => x.CreatedDate.Month == 5);
            ViewBag.Jun = _context.Students.Count(x => x.CreatedDate.Month == 6);
            ViewBag.Jul = _context.Students.Count(x => x.CreatedDate.Month == 7);
            ViewBag.Aug = _context.Students.Count(x => x.CreatedDate.Month == 8);
            ViewBag.Sep = _context.Students.Count(x => x.CreatedDate.Month == 9);
            ViewBag.Oct = _context.Students.Count(x => x.CreatedDate.Month == 10);
            ViewBag.Nov = _context.Students.Count(x => x.CreatedDate.Month == 11);
            ViewBag.Dec = _context.Students.Count(x => x.CreatedDate.Month == 12);

            var students = _context.Students
                            .Include(x => x.Course)
                            .OrderByDescending(x => x.StudentId)
                            .Take(10)
                            .ToList();

            // Total Fee Collection
            ViewBag.TotalCollection = _context.StudentPayments
                .Where(x => x.PaymentStatus == "Paid")
                .Sum(x => (decimal?)x.Amount) ?? 0;

            // Today's Collection
            ViewBag.TodayCollection = _context.StudentPayments
                .Where(x => x.PaymentStatus == "Paid"
                         && x.PaymentDate.Date == today)
                .Sum(x => (decimal?)x.Amount) ?? 0;

            // Total Seats
            ViewBag.TotalSeats = _context.Courses.Sum(x => x.TotalSeats);

            // Filled Seats
            ViewBag.FilledSeats = _context.Students.Count();

            // Available Seats
            ViewBag.AvailableSeats =
                ViewBag.TotalSeats - ViewBag.FilledSeats;

            // Recent Payments
            ViewBag.RecentPayments = _context.StudentPayments
                .Include(x => x.Student)
                .OrderByDescending(x => x.PaymentDate)
                .Take(5)
                .ToList();
            // Attendance Summary
            ViewBag.TotalAttendance = _context.Attendances.Count();

            ViewBag.TotalPresent = _context.Attendances
                .Count(x => x.Status == "Present");

            ViewBag.TotalAbsent = _context.Attendances
                .Count(x => x.Status == "Absent");

            ViewBag.TodayAttendance = _context.Attendances
                .Count(x => x.AttendanceDate.Date == today);
            ViewBag.FeeJan = _context.StudentPayments
    .Where(x => x.PaymentStatus == "Paid" && x.PaymentDate.Month == 1)
    .Sum(x => (decimal?)x.Amount) ?? 0;

            ViewBag.FeeFeb = _context.StudentPayments
                .Where(x => x.PaymentStatus == "Paid" && x.PaymentDate.Month == 2)
                .Sum(x => (decimal?)x.Amount) ?? 0;

            ViewBag.FeeMar = _context.StudentPayments
                .Where(x => x.PaymentStatus == "Paid" && x.PaymentDate.Month == 3)
                .Sum(x => (decimal?)x.Amount) ?? 0;

            ViewBag.FeeApr = _context.StudentPayments
                .Where(x => x.PaymentStatus == "Paid" && x.PaymentDate.Month == 4)
                .Sum(x => (decimal?)x.Amount) ?? 0;

            ViewBag.FeeMay = _context.StudentPayments
                .Where(x => x.PaymentStatus == "Paid" && x.PaymentDate.Month == 5)
                .Sum(x => (decimal?)x.Amount) ?? 0;

            ViewBag.FeeJun = _context.StudentPayments
                .Where(x => x.PaymentStatus == "Paid" && x.PaymentDate.Month == 6)
                .Sum(x => (decimal?)x.Amount) ?? 0;

            ViewBag.FeeJul = _context.StudentPayments
                .Where(x => x.PaymentStatus == "Paid" && x.PaymentDate.Month == 7)
                .Sum(x => (decimal?)x.Amount) ?? 0;

            ViewBag.FeeAug = _context.StudentPayments
                .Where(x => x.PaymentStatus == "Paid" && x.PaymentDate.Month == 8)
                .Sum(x => (decimal?)x.Amount) ?? 0;

            ViewBag.FeeSep = _context.StudentPayments
                .Where(x => x.PaymentStatus == "Paid" && x.PaymentDate.Month == 9)
                .Sum(x => (decimal?)x.Amount) ?? 0;

            ViewBag.FeeOct = _context.StudentPayments
                .Where(x => x.PaymentStatus == "Paid" && x.PaymentDate.Month == 10)
                .Sum(x => (decimal?)x.Amount) ?? 0;

            ViewBag.FeeNov = _context.StudentPayments
                .Where(x => x.PaymentStatus == "Paid" && x.PaymentDate.Month == 11)
                .Sum(x => (decimal?)x.Amount) ?? 0;

            ViewBag.FeeDec = _context.StudentPayments
                .Where(x => x.PaymentStatus == "Paid" && x.PaymentDate.Month == 12)
                .Sum(x => (decimal?)x.Amount) ?? 0;

            var courseData = _context.Courses
    .Select(c => new
    {
        Course = c.CourseName,
        Count = _context.Students.Count(s => s.CourseId == c.CourseId)
    })
    .ToList();

            ViewBag.CourseNames = courseData.Select(x => x.Course).ToArray();
            ViewBag.CourseCounts = courseData.Select(x => x.Count).ToArray();

            return View(students);
        }
    }

}