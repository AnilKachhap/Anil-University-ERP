using AnilUniversity.Data;
using AnilUniversity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Razorpay.Api;
using Rotativa.AspNetCore;

namespace AnilUniversity.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;

        public PaymentController(
     ApplicationDbContext context,
     IConfiguration configuration,
     UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _configuration = configuration;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            var payments = _context.StudentPayments
                .Include(x => x.Student)
                .ThenInclude(x => x.Course)
                .OrderByDescending(x => x.PaymentDate)
                .ToList();

            return View(payments);
        }

        // Payment Page
        [Authorize(Roles = "Admin")]
        public IActionResult Create(int studentId)
        {

            var student = _context.Students
                .Include(x => x.Course)
                .FirstOrDefault(x => x.StudentId == studentId);


            if (student == null)
                return NotFound();



            ViewBag.Student = student;


            return View(new StudentPayment
            {
                StudentId = student.StudentId,
                Amount = student.Course?.Fees ?? 0
            });

        }




        // Cash / Normal Payment Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(StudentPayment payment)
        {


            if (!ModelState.IsValid)
            {

                ViewBag.Student = _context.Students
                    .Include(x => x.Course)
                    .FirstOrDefault(x => x.StudentId == payment.StudentId);


                return View(payment);

            }



            // Cash payment
            payment.PaymentStatus = "Paid";

            payment.PaymentDate = DateTime.Now;



            _context.StudentPayments.Add(payment);


            await _context.SaveChangesAsync();



            TempData["Success"] =
                "Payment Saved Successfully";



            return RedirectToAction(
                "Receipt",
                new { id = payment.PaymentId }
            );


        }





        // Razorpay Order Create

        [HttpPost]
        public IActionResult CreateOrder(decimal amount)
        {


            RazorpayClient client =
                new RazorpayClient(
                    _configuration["Razorpay:Key"],
                    _configuration["Razorpay:Secret"]
                );



            Dictionary<string, object> options =
                new Dictionary<string, object>();


            options.Add(
                "amount",
                Convert.ToInt32(amount * 100)
            );


            options.Add(
                "currency",
                "INR"
            );


            options.Add(
                "receipt",
                Guid.NewGuid().ToString()
            );



            Order order = client.Order.Create(options);



            return Json(new
            {

                orderId = order["id"].ToString(),

                key = _configuration["Razorpay:Key"]

            });


        }





        // Razorpay Verification

        [HttpPost]
        public async Task<IActionResult> VerifyPayment(
            StudentPayment payment)
        {

            try
            {


                Dictionary<string, string> attributes =
                    new Dictionary<string, string>();


                attributes.Add(
                    "razorpay_payment_id",
                    payment.RazorpayPaymentId
                );


                attributes.Add(
                    "razorpay_order_id",
                    payment.RazorpayOrderId
                );


                attributes.Add(
                    "razorpay_signature",
                    payment.RazorpaySignature
                );



                Utils.verifyPaymentSignature(attributes);



                payment.TransactionId =
                    payment.RazorpayPaymentId;



                payment.PaymentStatus =
                    "Paid";


                payment.PaymentDate =
                    DateTime.Now;



                _context.StudentPayments.Add(payment);



                await _context.SaveChangesAsync();



                TempData["Success"] =
                    "Razorpay Payment Successful";



                if (User.IsInRole("Student"))
                {
                    return RedirectToAction("MyPayments", "StudentPortal");
                }

                return RedirectToAction("Receipt", new { id = payment.PaymentId });

            }
            catch (Exception)
            {


                TempData["Error"] =
                    "Payment Verification Failed";


                return RedirectToAction(
                    "Create",
                    new
                    {
                        studentId = payment.StudentId
                    });

            }


        }






        // Payment History
        [Authorize(Roles = "Admin")]
        public IActionResult History(int studentId)
        {


            var payments =
                _context.StudentPayments
                .Include(x => x.Student)
                .Where(x => x.StudentId == studentId)
                .OrderByDescending(x => x.PaymentDate)
                .ToList();



            ViewBag.Student =
                _context.Students
                .FirstOrDefault(x => x.StudentId == studentId);



            return View(payments);


        }





        // Receipt

        [Authorize]
        public async Task<IActionResult> Receipt(int id)
        {
            var payment = await _context.StudentPayments
                .Include(x => x.Student)
                .ThenInclude(x => x.Course)
                .FirstOrDefaultAsync(x => x.PaymentId == id);

            if (payment == null)
                return NotFound();

            if (User.IsInRole("Admin"))
            {
                return View("Receipt", payment);
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null || payment.StudentId != user.StudentId)
                return Forbid();

            return View("StudentReceipt", payment);
        }



        [Authorize]
        public async Task<IActionResult> DownloadReceiptPdf(int id)
        {
            var payment = await _context.StudentPayments
                .Include(x => x.Student)
                .ThenInclude(x => x.Course)
                .FirstOrDefaultAsync(x => x.PaymentId == id);

            if (payment == null)
                return NotFound();

            if (!User.IsInRole("Admin"))
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null || payment.StudentId != user.StudentId)
                    return Forbid();
            }

            return new ViewAsPdf("Receipt", payment)
            {
                FileName = $"Receipt_{payment.PaymentId}.pdf"
            };
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> PayNow()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || user.StudentId == null)
                return Redirect("/Identity/Account/Login");

            var student = await _context.Students
                .Include(x => x.Course)
                .FirstOrDefaultAsync(x => x.StudentId == user.StudentId);

            if (student == null)
                return NotFound();

            ViewBag.Student = student;

            return View("StudentPay", new StudentPayment
            {
                StudentId = student.StudentId,
                Amount = student.Course!.Fees
            });
        }
    }
}