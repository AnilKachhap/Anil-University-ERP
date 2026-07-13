using AnilUniversity.Data;
using AnilUniversity.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
namespace AnilUniversity.Controllers
{

    [Authorize(Roles = "Admin")]
    public class CourseController : Controller
    {
        private readonly ApplicationDbContext _context;

       
        public CourseController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Display Courses

        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            var courses = _context.Courses.ToList();

            foreach (var course in courses)
            {
                course.FilledSeats = _context.Students.Count(x => x.CourseId == course.CourseId);

                course.AvailableSeats = course.TotalSeats - course.FilledSeats;
            }

            return View(courses);
        }
        // Create Course
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Course course)
        {
            if (ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();

                TempData["success"] = "Course Added Successfully";

                return RedirectToAction(nameof(Index));
            }

            return View(course);
        }

        // GET: Edit
        [Authorize(Roles = "Admin")]
        
        public IActionResult Edit(int id)
        {
            var course = _context.Courses.Find(id);

            if (course == null)
                return NotFound();

            return View(course);
        }

        // POST: Edit
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Course course)
        {
            if (id != course.CourseId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(course);

            _context.Update(course);

            await _context.SaveChangesAsync();

            TempData["success"] = "Course Updated Successfully.";

            return RedirectToAction(nameof(Index));
        }
        // GET: Delete
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var course = _context.Courses.Find(id);

            if (course == null)
                return NotFound();

            return View(course);
        }

        // POST: Delete
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);

            if (course == null)
                return NotFound();

            // Check if students are admitted in this course
            bool hasStudents = _context.Students.Any(x => x.CourseId == id);

            if (hasStudents)
            {
                TempData["Error"] = "Course cannot be deleted because students are already admitted.";

                return RedirectToAction(nameof(Index));
            }

            _context.Courses.Remove(course);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Course deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = "Admin")]
        public IActionResult ToggleStatus(int id)
        {
            var course = _context.Courses.Find(id);

            if (course == null)
                return NotFound();

            course.IsActive = !course.IsActive;

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
        [AllowAnonymous]
        public IActionResult PublicCourses()
        {
            var courses = _context.Courses
                                  .Where(x => x.IsActive)
                                  .ToList();

            foreach (var course in courses)
            {
                course.FilledSeats = _context.Students.Count(s => s.CourseId == course.CourseId);
                course.AvailableSeats = course.TotalSeats - course.FilledSeats;
            }

            return View(courses);
        }
    }

}