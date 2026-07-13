using AnilUniversity.Data;
using AnilUniversity.Helpers;
using AnilUniversity.Models;
using AnilUniversity.ViewModels;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using AnilUniversity.Services;
using Rotativa.AspNetCore;
using System.IO;
using Microsoft.AspNetCore.Identity;


namespace AnilUniversity.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;

       

        private readonly IWebHostEnvironment _environment;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
       
        private readonly RoleManager<IdentityRole> _roleManager;

        public StudentController(
       ApplicationDbContext context,
       UserManager<ApplicationUser> userManager,
       RoleManager<IdentityRole> roleManager,
       IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _environment = environment;
        }

        // Registration Form

        [AllowAnonymous]
        public IActionResult Register()
        {
            ViewBag.CourseId = new SelectList(_context.Courses.Where(x => x.IsActive),
                                              "CourseId",
                                              "CourseName");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Register(Student student)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CourseId = new SelectList(
                    _context.Courses.Where(x => x.IsActive),
                    "CourseId",
                    "CourseName");

                return View(student);
            }

            // Check Course
            var course = await _context.Courses
                .FirstOrDefaultAsync(x => x.CourseId == student.CourseId);

            if (course == null)
            {
                ModelState.AddModelError("", "Invalid Course.");

                ViewBag.CourseId = new SelectList(
                    _context.Courses.Where(x => x.IsActive),
                    "CourseId",
                    "CourseName");

                return View(student);
            }

            // Check Seat Availability
            int filledSeats = await _context.Students
                .CountAsync(x => x.CourseId == student.CourseId);

            if (filledSeats >= course.TotalSeats)
            {
                ModelState.AddModelError("", "Admission Closed. No seats available.");

                ViewBag.CourseId = new SelectList(
                    _context.Courses.Where(x => x.IsActive),
                    "CourseId",
                    "CourseName");

                return View(student);
            }

            // Check Duplicate Email
            bool emailExists = await _context.Students
                .AnyAsync(x => x.Email == student.Email);

            if (emailExists)
            {
                ModelState.AddModelError("", "This email is already registered.");

                ViewBag.CourseId = new SelectList(
                    _context.Courses.Where(x => x.IsActive),
                    "CourseId",
                    "CourseName");

                return View(student);
            }

            // Default Values
            student.Status = "Pending";
            student.CreatedDate = DateTime.Now;

            // Save Student
            _context.Students.Add(student);
            await _context.SaveChangesAsync();
            // Create Identity User
            var appUser = new ApplicationUser
            {
                UserName = student.Email,
                Email = student.Email,
                EmailConfirmed = true,
                StudentId = student.StudentId
            };

            // Default Password
            string defaultPassword = "Student@123";

            var result = await _userManager.CreateAsync(appUser, defaultPassword);

            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync("Student"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Student"));
                }

                await _userManager.AddToRoleAsync(appUser, "Student");
            }

            // Generate Application No
            student.ApplicationNo = $"ANIL{DateTime.Now.Year}{student.StudentId:D5}";

            // Generate Student Code
            student.StudentCode = $"AU{DateTime.Now.Year}{student.StudentId:D5}";

            _context.Update(student);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(UploadDocuments), new { id = student.StudentId });
        }
        [AllowAnonymous]
        public async Task<IActionResult> UploadDocuments(int id)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(x => x.StudentId == id);

            if (student == null)
                return NotFound();

            bool alreadyUploaded = await _context.StudentDocuments
                .AnyAsync(x => x.StudentId == id);

            if (alreadyUploaded)
            {
                TempData["Error"] = "Documents have already been uploaded.";

                return RedirectToAction(nameof(Preview), new { id });
            }

            StudentDocumentViewModel model = new StudentDocumentViewModel
            {
                StudentId = id
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> UploadDocuments(StudentDocumentViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var student = await _context.Students
                .FirstOrDefaultAsync(x => x.StudentId == model.StudentId);

            if (student == null)
                return NotFound();

            bool alreadyUploaded = await _context.StudentDocuments
                .AnyAsync(x => x.StudentId == model.StudentId);

            if (alreadyUploaded)
            {
                TempData["Error"] = "Documents have already been uploaded.";

                return RedirectToAction(nameof(Preview), new { id = model.StudentId });
            }

            StudentDocument document = new StudentDocument
            {
                StudentId = model.StudentId
            };

            document.PhotoPath = await FileHelper.SaveFileAsync(
                model.Photo,
                Path.Combine(_environment.WebRootPath, "Uploads", "Photos"));

            document.SignaturePath = await FileHelper.SaveFileAsync(
                model.Signature,
                Path.Combine(_environment.WebRootPath, "Uploads", "Signatures"));

            document.AadhaarPath = await FileHelper.SaveFileAsync(
                model.Aadhaar,
                Path.Combine(_environment.WebRootPath, "Uploads", "Aadhaar"));

            document.TenthMarksheetPath = await FileHelper.SaveFileAsync(
                model.TenthMarksheet,
                Path.Combine(_environment.WebRootPath, "Uploads", "MarkSheets"));

            document.TwelfthMarksheetPath = await FileHelper.SaveFileAsync(
                model.TwelfthMarksheet,
                Path.Combine(_environment.WebRootPath, "Uploads", "MarkSheets"));

            if (model.GraduationMarksheet != null)
            {
                document.GraduationMarksheetPath = await FileHelper.SaveFileAsync(
                    model.GraduationMarksheet,
                    Path.Combine(_environment.WebRootPath, "Uploads", "MarkSheets"));
            }

            _context.StudentDocuments.Add(document);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Documents uploaded successfully.";

            return RedirectToAction(nameof(Preview), new { id = model.StudentId });
        }
        [AllowAnonymous]
        public IActionResult Preview(int id)
        {
            var student = _context.Students
                            .Include(x => x.Course)
                            .Include(x => x.StudentDocument)
                            .FirstOrDefault(x => x.StudentId == id);

            return View(student);
        }

        private string GenerateApplicationNo()
        {
            return "ANIL" + DateTime.Now.ToString("yyyyMMddHHmmss");
        }
        [AllowAnonymous]
        public IActionResult Print(int id)
        {
            var student = _context.Students
                            .Include(x => x.Course)
                            .Include(x => x.StudentDocument)
                            .FirstOrDefault(x => x.StudentId == id);

            return View(student);
        }

        public IActionResult Details(int id)
        {
            var student = _context.Students
                .Include(x => x.Course)
                .Include(x => x.StudentDocument)
                .FirstOrDefault(x => x.StudentId == id);

            if (student == null)
                return NotFound();

            return View(student);
        }
        [Authorize]

        public IActionResult Edit(int id)
        {
            var student = _context.Students.Find(id);

            if (student == null)
                return NotFound();

            ViewBag.CourseId = new SelectList(
                _context.Courses.Where(x => x.IsActive),
                "CourseId",
                "CourseName",
                student.CourseId);

            return View(student);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Student student)
        {
            if (id != student.StudentId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }

                ViewBag.CourseId = new SelectList(_context.Courses.Where(x => x.IsActive),
                    "CourseId", "CourseName", student.CourseId);

                return View(student);
            }

            var existingStudent = await _context.Students.FindAsync(id);

            //if (existingStudent == null)
            //{
            //    return NotFound();
            //}

            existingStudent.FullName = student.FullName;
            existingStudent.FatherName = student.FatherName;
            existingStudent.MotherName = student.MotherName;
            existingStudent.DOB = student.DOB;
            existingStudent.Gender = student.Gender;
            existingStudent.Mobile = student.Mobile;
            existingStudent.Email = student.Email;
            existingStudent.Address = student.Address;
            existingStudent.CourseId = student.CourseId;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Student Updated Successfully.";

            return RedirectToAction(nameof(Index));
        }
        [Authorize]
        public async Task<IActionResult> EditDocuments(int id)
        {
            var document = await _context.StudentDocuments
                .FirstOrDefaultAsync(x => x.StudentId == id);

            if (document == null)
                return NotFound();

            EditDocumentViewModel model = new EditDocumentViewModel
            {
                StudentId = document.StudentId,
                StudentDocumentId = document.StudentDocumentId,

                PhotoPath = document.PhotoPath,
                SignaturePath = document.SignaturePath,
                AadhaarPath = document.AadhaarPath,
                TenthMarksheetPath = document.TenthMarksheetPath,
                TwelfthMarksheetPath = document.TwelfthMarksheetPath,
                GraduationMarksheetPath = document.GraduationMarksheetPath
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> EditDocuments(EditDocumentViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var document = await _context.StudentDocuments
                .FirstOrDefaultAsync(x => x.StudentDocumentId == model.StudentDocumentId);

            if (document == null)
                return NotFound();

            if (model.Photo != null)
            {
                DeleteFile(document.PhotoPath);

                document.PhotoPath = await FileHelper.SaveFileAsync(
                    model.Photo,
                    Path.Combine(_environment.WebRootPath, "Uploads", "Photos"));
            }

            if (model.Signature != null)
            {
                DeleteFile(document.SignaturePath);

                document.SignaturePath = await FileHelper.SaveFileAsync(
                    model.Signature,
                    Path.Combine(_environment.WebRootPath, "Uploads", "Signatures"));
            }

            if (model.Aadhaar != null)
            {
                DeleteFile(document.AadhaarPath);

                document.AadhaarPath = await FileHelper.SaveFileAsync(
                    model.Aadhaar,
                    Path.Combine(_environment.WebRootPath, "Uploads", "Aadhaar"));
            }

            if (model.TenthMarksheet != null)
            {
                DeleteFile(document.TenthMarksheetPath);

                document.TenthMarksheetPath = await FileHelper.SaveFileAsync(
                    model.TenthMarksheet,
                    Path.Combine(_environment.WebRootPath, "Uploads", "MarkSheets"));
            }

            if (model.TwelfthMarksheet != null)
            {
                DeleteFile(document.TwelfthMarksheetPath);

                document.TwelfthMarksheetPath = await FileHelper.SaveFileAsync(
                    model.TwelfthMarksheet,
                    Path.Combine(_environment.WebRootPath, "Uploads", "MarkSheets"));
            }

            if (model.GraduationMarksheet != null)
            {
                DeleteFile(document.GraduationMarksheetPath);

                document.GraduationMarksheetPath = await FileHelper.SaveFileAsync(
                    model.GraduationMarksheet,
                    Path.Combine(_environment.WebRootPath, "Uploads", "MarkSheets"));
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Documents updated successfully.";

            return RedirectToAction(nameof(Details), new { id = model.StudentId });
        }
        [Authorize]
        [AllowAnonymous]
        public IActionResult DownloadPdf(int id)
        {
            return new ViewAsPdf("Print",
                _context.Students
                .Include(x => x.Course)
                .Include(x => x.StudentDocument)
                .FirstOrDefault(x => x.StudentId == id))
            {
                FileName = "AdmissionForm.pdf"
            };
        }
        public async Task<IActionResult> Approve(int id)
        {
            var student = _context.Students.Find(id);

            if (student == null)
                return NotFound();

            student.Status = "Approved";

            await CreateStudentLoginAsync(student);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Student approved successfully. Login credentials have been sent to the student's email.";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Reject(int id)
        {
            var student = _context.Students.Find(id);

            if (student == null)
                return NotFound();

            student.Status = "Rejected";

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var student = await _context.Students
                .Include(x => x.StudentDocument)
                .FirstOrDefaultAsync(x => x.StudentId == id);

            if (student == null)
                return NotFound();

            // Delete Identity User
            var user = await _userManager.FindByEmailAsync(student.Email);

            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }

            // Delete Uploaded Files
            if (student.StudentDocument != null)
            {
                DeleteFile(student.StudentDocument.PhotoPath);
                DeleteFile(student.StudentDocument.SignaturePath);
                DeleteFile(student.StudentDocument.AadhaarPath);
                DeleteFile(student.StudentDocument.TenthMarksheetPath);
                DeleteFile(student.StudentDocument.TwelfthMarksheetPath);
                DeleteFile(student.StudentDocument.GraduationMarksheetPath);

                _context.StudentDocuments.Remove(student.StudentDocument);
            }

            _context.Students.Remove(student);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Student deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Index(string? searchString, int? courseId, string? status)
        {
            // Course Dropdown
            ViewBag.CourseId = new SelectList(
                await _context.Courses
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.CourseName)
                    .ToListAsync(),
                "CourseId",
                "CourseName",
                courseId);

            // Preserve Filter Values
            ViewBag.SearchString = searchString;
            ViewBag.Status = status;

            var students = _context.Students
                .AsNoTracking()
                .Include(x => x.Course)
                .AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim();

                students = students.Where(x =>
                    x.FullName.Contains(searchString) ||
                    x.ApplicationNo.Contains(searchString) ||
                    x.Mobile.Contains(searchString) ||
                    x.Email.Contains(searchString));
            }

            // Course Filter
            if (courseId.HasValue)
            {
                students = students.Where(x => x.CourseId == courseId);
            }

            // Status Filter
            if (!string.IsNullOrWhiteSpace(status))
            {
                students = students.Where(x => x.Status == status);
            }

            var result = await students
                .OrderByDescending(x => x.StudentId)
                .ToListAsync();

            return View(result);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportExcel()
        {
            var students = await _context.Students
                .AsNoTracking()
                .Include(x => x.Course)
                .OrderBy(x => x.FullName)
                .ToListAsync();

            using var workbook = new XLWorkbook();

            var worksheet = workbook.Worksheets.Add("Students");

            // Header
            worksheet.Cell(1, 1).Value = "Application No";
            worksheet.Cell(1, 2).Value = "Student Code";
            worksheet.Cell(1, 3).Value = "Student Name";
            worksheet.Cell(1, 4).Value = "Father Name";
            worksheet.Cell(1, 5).Value = "Course";
            worksheet.Cell(1, 6).Value = "Mobile";
            worksheet.Cell(1, 7).Value = "Email";
            worksheet.Cell(1, 8).Value = "Status";
            worksheet.Cell(1, 9).Value = "Admission Date";

            // Header Style
            var header = worksheet.Range("A1:I1");

            header.Style.Font.Bold = true;
            header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            header.Style.Fill.BackgroundColor = XLColor.LightBlue;

            int row = 2;

            foreach (var student in students)
            {
                worksheet.Cell(row, 1).Value = student.ApplicationNo;
                worksheet.Cell(row, 2).Value = student.StudentCode;
                worksheet.Cell(row, 3).Value = student.FullName;
                worksheet.Cell(row, 4).Value = student.FatherName;
                worksheet.Cell(row, 5).Value = student.Course?.CourseName;
                worksheet.Cell(row, 6).Value = student.Mobile;
                worksheet.Cell(row, 7).Value = student.Email;
                worksheet.Cell(row, 8).Value = student.Status;
                worksheet.Cell(row, 9).Value = student.CreatedDate.ToString("dd-MM-yyyy");

                row++;
            }

            // Excel Formatting
            worksheet.Columns().AdjustToContents();
            worksheet.SheetView.FreezeRows(1);
            worksheet.RangeUsed().SetAutoFilter();

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Students_{DateTime.Now:yyyyMMdd}.xlsx");
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> StudentReport()
        {
            var students = await _context.Students
                .AsNoTracking()
                .Include(x => x.Course)
                .OrderByDescending(x => x.StudentId)
                .ToListAsync();

            return new ViewAsPdf("StudentReport", students)
            {
                FileName = $"StudentReport_{DateTime.Now:yyyyMMdd}.pdf",

                PageSize = Rotativa.AspNetCore.Options.Size.A4,

                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,

                CustomSwitches =
                    "--print-media-type " +
                    "--enable-local-file-access " +
                    "--footer-center \"Page [page] of [toPage]\" " +
                    "--footer-font-size 9 " +
                    "--footer-spacing 5"
            };
        }
        [AllowAnonymous]
        public IActionResult TrackApplication()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public IActionResult TrackApplication(string applicationNo)
        {
            var student = _context.Students
                .Include(x => x.Course)
                .Include(x => x.StudentDocument)
                .FirstOrDefault(x => x.ApplicationNo == applicationNo);

            if (student == null)
            {
                ViewBag.Message = "Application Not Found.";
                return View();
            }

            return View(student);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int id)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(x => x.StudentId == id);

            if (student == null)
                return NotFound();

            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(Student model)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(x => x.StudentId == model.StudentId);

            if (student == null)
                return NotFound();

            // Valid Status Check
            if (model.Status != "Pending" &&
                model.Status != "Approved" &&
                model.Status != "Rejected")
            {
                TempData["Error"] = "Invalid status selected.";
                return RedirectToAction(nameof(Index));
            }

            // Status already same
            if (student.Status == model.Status &&
                student.Remarks == model.Remarks)
            {
                TempData["Info"] = "No changes were made.";
                return RedirectToAction(nameof(Index));
            }

            student.Status = model.Status;
            student.Remarks = model.Remarks;

            if (student.Status == "Approved")
            {
                await CreateStudentLoginAsync(student);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Student status updated successfully.";

            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CourseReport()
        {
            ViewBag.CourseId = new SelectList(
                await _context.Courses
                    .AsNoTracking()
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.CourseName)
                    .ToListAsync(),
                "CourseId",
                "CourseName");

            return View();
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CourseReportResult(int courseId)
        {
            var students = await _context.Students
                .AsNoTracking()
                .Include(x => x.Course)
                .Where(x => x.CourseId == courseId)
                .OrderBy(x => x.FullName)
                .ToListAsync();

            ViewBag.CourseName = students.FirstOrDefault()?.Course?.CourseName ?? "N/A";
            ViewBag.TotalStudents = students.Count;

            return View(students);
        }
        [Authorize(Roles = "Admin")]
        public IActionResult DateReport()
        {
            return View();
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DateReportResult(DateTime fromDate, DateTime toDate)
        {
            if (fromDate > toDate)
            {
                TempData["Error"] = "From Date cannot be greater than To Date.";
                return RedirectToAction(nameof(DateReport));
            }

            var students = await _context.Students
                .AsNoTracking()
                .Include(x => x.Course)
                .Where(x => x.CreatedDate.Date >= fromDate.Date &&
                            x.CreatedDate.Date <= toDate.Date)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            ViewBag.FromDate = fromDate.ToString("dd MMM yyyy");
            ViewBag.ToDate = toDate.ToString("dd MMM yyyy");
            ViewBag.TotalStudents = students.Count;

            return View(students);
        }


        [Authorize(Roles = "Admin,Student")]
        public IActionResult IdCard(int id)
        {
            var student = _context.Students
                .Include(x => x.Course)
                .Include(x => x.StudentDocument)
                .FirstOrDefault(x => x.StudentId == id);

            if (student == null)
                return NotFound();

            if (student.Status != "Approved")
            {
                TempData["Error"] = "ID Card is available only after approval.";
                return RedirectToAction(nameof(Index));
            }

            return View(student);
        }

        [Authorize(Roles = "Admin,Student")]
        public IActionResult DownloadIdCard(int id)
        {
            var student = _context.Students
                .Include(x => x.Course)
                .Include(x => x.StudentDocument)
                .FirstOrDefault(x => x.StudentId == id);

            if (student == null)
                return NotFound();

            if (student.Status != "Approved")
            {
                TempData["Error"] = "ID Card is available only after approval.";
                return RedirectToAction(nameof(Index));
            }

            return new ViewAsPdf("IdCard", student)
            {
                FileName = $"IDCard_{student.StudentCode}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                CustomSwitches = "--enable-local-file-access"
            };
        }
        public IActionResult GenerateQr(int id)
        {
            string url = Url.Action("Verify", "Student",
                new { id = id }, Request.Scheme);

            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);

                PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);

                byte[] qrCodeImage = qrCode.GetGraphic(20);

                return File(qrCodeImage, "image/png");
            }
        }
        public IActionResult Verify(int id)
        {
            var student = _context.Students
                .Include(x => x.Course)
                .Include(x => x.StudentDocument)
                .FirstOrDefault(x => x.StudentId == id);

            if (student == null)
                return NotFound();

            return View(student);
        }
        private async Task CreateStudentLoginAsync(Student student)
        {
            // Check if login already exists
            var existingUser = await _userManager.FindByEmailAsync(student.Email);

            if (existingUser != null)
                return;

            const string tempPassword = "AnilUni@2026";

            var user = new ApplicationUser
            {
                UserName = student.Email,
                Email = student.Email,
                EmailConfirmed = true,
                StudentId = student.StudentId
            };

            var result = await _userManager.CreateAsync(user, tempPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(x => x.Description));
                throw new Exception($"Unable to create student login. {errors}");
            }

            // Assign Student Role
            if (!await _userManager.IsInRoleAsync(user, "Student"))
            {
                await _userManager.AddToRoleAsync(user, "Student");
            }

            string loginUrl = Url.Action(
                "Login",
                "Account",
                new { area = "Identity" },
                Request.Scheme)!;

            try
            {
                await _emailService.SendEmailAsync(
                    student.Email,
                    "Admission Approved - Anil University",
                    $@"
<div style='font-family:Segoe UI,Arial,sans-serif;
            max-width:700px;
            margin:auto;
            border:1px solid #ddd;
            border-radius:8px;
            padding:30px;'>

    <h2 style='color:#0d6efd;margin-top:0;'>
        🎓 Congratulations, {student.FullName}!
    </h2>

    <p>
        Your admission to <b>Anil University</b> has been
        <span style='color:green;font-weight:bold;'>APPROVED</span>.
    </p>

    <hr/>

    <h3>Student Login Credentials</h3>

    <table style='border-collapse:collapse;'>

        <tr>
            <td style='padding:8px;'><b>Email</b></td>
            <td style='padding:8px;'>{student.Email}</td>
        </tr>

        <tr>
            <td style='padding:8px;'><b>Temporary Password</b></td>
            <td style='padding:8px;'>{tempPassword}</td>
        </tr>

    </table>

    <br/>

    <a href='{loginUrl}'
       style='background:#0d6efd;
              color:white;
              padding:12px 25px;
              text-decoration:none;
              border-radius:5px;
              display:inline-block;'>

        Login to Student Portal

    </a>

    <br/><br/>

    <p style='color:#d9534f;'>
        <b>Important:</b> Please change your password immediately after your first login.
    </p>

    <hr/>

    <p>
        Regards,<br/>
        <b>Anil University</b>
    </p>

</div>");
            }
            catch (Exception)
            {
                // Login account is created successfully.
                // Email sending failed, but don't stop the approval process.
            }
        }
        private void DeleteFile(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            string fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }
        private async Task<Student?> GetStudentByIdAsync(int id)
        {
            return await _context.Students
                .AsNoTracking()
                .Include(x => x.Course)
                .Include(x => x.StudentDocument)
                .FirstOrDefaultAsync(x => x.StudentId == id);
        }
    }


}