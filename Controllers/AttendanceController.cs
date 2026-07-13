using AnilUniversity.Data;
using AnilUniversity.Models;
using AnilUniversity.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AnilUniversity.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Attendance List
        public async Task<IActionResult> Index()
        {
            var attendance = await _context.Attendances
                .Include(x => x.Student)
                .OrderByDescending(x => x.AttendanceDate)
                .ToListAsync();

            return View(attendance);
        }
        public IActionResult MarkAttendance()
        {
            var model = new AttendanceSheetVM();

            model.Students = _context.Students
                .OrderBy(x => x.FullName)
                .Select(x => new AttendanceVM
                {
                    StudentId = x.StudentId,
                    StudentName = x.FullName,
                    IsPresent = true
                })
                .ToList();

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAttendance(AttendanceSheetVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            foreach (var item in model.Students)
            {
                bool exists = await _context.Attendances.AnyAsync(x =>
                    x.StudentId == item.StudentId &&
                    x.AttendanceDate.Date == model.AttendanceDate.Date);

                if (exists)
                    continue;

                Attendance attendance = new Attendance
                {
                    StudentId = item.StudentId,
                    AttendanceDate = model.AttendanceDate,
                    Status = item.IsPresent ? "Present" : "Absent"
                };

                _context.Attendances.Add(attendance);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Attendance saved successfully.";

            return RedirectToAction(nameof(Index));
        }
        // GET
        public IActionResult Create()
        {
            ViewBag.StudentId = new SelectList(
                _context.Students.OrderBy(x => x.FullName),
                "StudentId",
                "FullName");

            return View();
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Attendance model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.StudentId = new SelectList(
                    _context.Students,
                    "StudentId",
                    "FullName");

                return View(model);
            }

            // Duplicate attendance check
            bool exists = await _context.Attendances.AnyAsync(x =>
                x.StudentId == model.StudentId &&
                x.AttendanceDate.Date == model.AttendanceDate.Date);

            if (exists)
            {
                TempData["Error"] = "Attendance already marked for today.";

                ViewBag.StudentId = new SelectList(
                    _context.Students,
                    "StudentId",
                    "FullName");

                return View(model);
            }

            _context.Attendances.Add(model);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Attendance saved successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}