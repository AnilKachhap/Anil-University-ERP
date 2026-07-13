using AnilUniversity.Data;
using AnilUniversity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Student")]
public class StudentPortalController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public StudentPortalController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Dashboard()
    {
        await LoadStudentInfo();
        var student = await GetCurrentStudentAsync();

        if (student == null)
            return Redirect("/Identity/Account/Login");

        ViewBag.Notifications = await _context.Notifications
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.CreatedDate)
            .Take(5)
            .ToListAsync();
        var payment = await _context.StudentPayments
    .Where(x => x.StudentId == student.StudentId)
    .SumAsync(x => (decimal?)x.Amount) ?? 0;

        var attendance = await _context.Attendances
            .Where(x => x.StudentId == student.StudentId)
            .ToListAsync();

        var present = attendance.Count(x => x.Status == "Present");

        double percentage = attendance.Count == 0
            ? 0
            : Math.Round((double)present * 100 / attendance.Count, 2);

        ViewBag.TotalPayment = payment;
        ViewBag.AttendancePercentage = percentage;

        ViewBag.DocumentCount =
            (student.StudentDocument?.PhotoPath != null ? 1 : 0) +
            (student.StudentDocument?.SignaturePath != null ? 1 : 0) +
            (student.StudentDocument?.AadhaarPath != null ? 1 : 0) +
            (student.StudentDocument?.TenthMarksheetPath != null ? 1 : 0) +
            (student.StudentDocument?.TwelfthMarksheetPath != null ? 1 : 0) +
            (student.StudentDocument?.GraduationMarksheetPath != null ? 1 : 0);
        return View(student);
    }

    public async Task<IActionResult> MyProfile()
    {
        await LoadStudentInfo();
        var student = await GetCurrentStudentAsync();

        if (student == null)
            return Redirect("/Identity/Account/Login");

        return View(student);
    }

    public async Task<IActionResult> EditProfile()
    {
        await LoadStudentInfo();
        var student = await GetCurrentStudentAsync();

        if (student == null)
            return Redirect("/Identity/Account/Login");

        return View(student);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(Student model)
    {
        await LoadStudentInfo();
        var student = await GetCurrentStudentAsync();

        if (student == null)
            return Redirect("/Identity/Account/Login");

        student.Mobile = model.Mobile;
        student.Address = model.Address;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Profile Updated Successfully.";

        return RedirectToAction(nameof(MyProfile));
    }

    public async Task<IActionResult> MyIdCard()
    {
        await LoadStudentInfo();
        var student = await GetCurrentStudentAsync();

        if (student == null)
            return Redirect("/Identity/Account/Login");

        return View(student);
    }

    public async Task<IActionResult> MyDocuments()
    {
        await LoadStudentInfo();
        var student = await GetCurrentStudentAsync();

        if (student == null)
            return Redirect("/Identity/Account/Login");

        var document = await _context.StudentDocuments
            .Include(x => x.Student)
            .FirstOrDefaultAsync(x => x.StudentId == student.StudentId);

        return View(document);
    }

    public async Task<IActionResult> MyPayments()
    {
        await LoadStudentInfo();
        var student = await GetCurrentStudentAsync();

        if (student == null)
            return Redirect("/Identity/Account/Login");

        var payments = await _context.StudentPayments
            .Include(x => x.Student)
            .Where(x => x.StudentId == student.StudentId)
            .OrderByDescending(x => x.PaymentDate)
            .ToListAsync();

        return View(payments);
    }

    public async Task<IActionResult> MyAttendance()
    {
        await LoadStudentInfo();
        var student = await GetCurrentStudentAsync();

        if (student == null)
            return Redirect("/Identity/Account/Login");

        var attendance = await _context.Attendances
            .Where(x => x.StudentId == student.StudentId)
            .OrderByDescending(x => x.AttendanceDate)
            .ToListAsync();

        ViewBag.Total = attendance.Count;
        ViewBag.Present = attendance.Count(x => x.Status == "Present");
        ViewBag.Absent = attendance.Count(x => x.Status == "Absent");

        ViewBag.Percentage = attendance.Count == 0
            ? 0
            : Math.Round(
                attendance.Count(x => x.Status == "Present") * 100.0 / attendance.Count,
                2);

        return View(attendance);
    }

    private async Task<Student?> GetCurrentStudentAsync()
    {
        await LoadStudentInfo();
        var user = await _userManager.GetUserAsync(User);

        if (user == null || user.StudentId == null)
            return null;

        return await _context.Students
            .Include(x => x.Course)
            .Include(x => x.StudentDocument)
            .FirstOrDefaultAsync(x => x.StudentId == user.StudentId);
    }
    private async Task LoadStudentInfo()
    {
      
        var user = await _userManager.GetUserAsync(User);

        if (user == null || user.StudentId == null)
            return;

        var student = await _context.Students
            .Include(x => x.StudentDocument)
            .FirstOrDefaultAsync(x => x.StudentId == user.StudentId);

        if (student != null)
        {
            ViewBag.StudentName = student.FullName;
            ViewBag.StudentEmail = student.Email;
            ViewBag.StudentPhoto = student.StudentDocument?.PhotoPath;
        }
    }
}