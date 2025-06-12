using japantune.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace japantune.Controllers
{
    public class ReviewController : Controller
    {
        private readonly JapanTuneContext _context;
        private readonly ILogger<ReviewController> _logger;

        public ReviewController(JapanTuneContext context, ILogger<ReviewController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Reviews
                .Include(r => r.User)
                .ToListAsync());
        }

        public IActionResult Create()
        {
            ViewBag.Users = GetUsersSelectList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string title,
            int rating,
            string reviewDate,
            int userId)
        {
            try
            {
                if (string.IsNullOrEmpty(title) ||
                    !DateOnly.TryParse(reviewDate, out DateOnly parsedDate) ||
                    userId <= 0 ||
                    rating < 1 || rating > 5)
                {
                    ModelState.AddModelError("", "Invalid input data");
                    ViewBag.Users = GetUsersSelectList();
                    return View();
                }

                var review = new Review
                {
                    Title = title,
                    Rating = (short)rating,
                    ReviewDate = parsedDate,
                    UserId = userId
                };

                _context.Add(review);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review");
                ViewBag.Users = GetUsersSelectList();
                TempData["ErrorMessage"] = "Error creating review";
                return View();
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return NotFound();

            ViewBag.Users = GetUsersSelectList(review.UserId);
            return View(review);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            string title,
            int rating,
            string reviewDate,
            int userId)
        {
            try
            {
                var review = await _context.Reviews.FindAsync(id);
                if (review == null) return NotFound();

                if (string.IsNullOrEmpty(title) ||
                    !DateOnly.TryParse(reviewDate, out DateOnly parsedDate) ||
                    userId <= 0 ||
                    rating < 1 || rating > 5)
                {
                    ModelState.AddModelError("", "Invalid input data");
                    ViewBag.Users = GetUsersSelectList(userId);
                    return View(review);
                }

                review.Title = title;
                review.Rating = (short)rating;
                review.ReviewDate = parsedDate;
                review.UserId = userId;

                _context.Update(review);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing review");
                ViewBag.Users = GetUsersSelectList(userId);
                TempData["ErrorMessage"] = "Error editing review";
                return RedirectToAction(nameof(Edit), new { id });
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Reviews
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (review == null)
            {
                return NotFound();
            }

            return View(review);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReviewExists(int id)
        {
            return _context.Reviews.Any(e => e.Id == id);
        }

        private SelectList GetUsersSelectList(int? selectedUserId = null)
        {
            return new SelectList(
                _context.Users.Select(u => new {
                    Id = u.Id,
                    FullName = $"{u.FirstName} {u.SurName}"
                }),
                "Id", "FullName", selectedUserId);
        }
    }
}