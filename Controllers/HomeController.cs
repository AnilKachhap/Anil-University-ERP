using AnilUniversity.Data;
using AnilUniversity.Models;
using AnilUniversity.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AnilUniversity.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public HomeController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            var courses = _context.Courses.ToList();
            return View(courses);
        }

        public IActionResult Privacy()
        {
            return View();
        }

      

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }
        public IActionResult StatusCode(int code)
        {
            if (code == 404)
            {
                return View("NotFound");
            }

            if (code == 403)
            {
                return View("AccessDenied");
            }

            return View("Error");
        }
    }
}