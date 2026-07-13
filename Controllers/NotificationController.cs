using AnilUniversity.Data;
using AnilUniversity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AnilUniversity.Controllers
{
    [Authorize(Roles = "Admin")]
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // List
        public async Task<IActionResult> Index()
        {
            return View(await _context.Notifications
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync());
        }

        // Create GET
        public IActionResult Create()
        {
            return View();
        }

        // Create POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Notification model)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.CreatedDate = DateTime.Now;

            _context.Notifications.Add(model);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Notification Added Successfully.";

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Edit(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);

            if (notification == null)
                return NotFound();

            return View(notification);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Notification model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _context.Notifications.Update(model);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Notification Updated Successfully.";

            return RedirectToAction(nameof(Index));
        }
        // Delete
        public async Task<IActionResult> Delete(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);

            if (notification == null)
                return NotFound();

            _context.Notifications.Remove(notification);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Notification Deleted Successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}