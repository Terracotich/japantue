using japantune.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace japantune.Controllers
{
    public class OrderController : Controller
    {
        private readonly JapanTuneContext _context;
        private readonly ILogger<OrderController> _logger;

        public OrderController(JapanTuneContext context, ILogger<OrderController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Material)
                .Include(o => o.Payment)
                .Include(o => o.Review)
                .ToListAsync());
        }

        public IActionResult Create()
        {
            LoadSelectLists();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string orderDate,
            string status,
            int userId,
            int materialId,
            int paymentId,
            int? reviewId)
        {
            try
            {
                if (!DateOnly.TryParse(orderDate, out DateOnly parsedDate) ||
                    string.IsNullOrEmpty(status) ||
                    userId <= 0 ||
                    materialId <= 0 ||
                    paymentId <= 0)
                {
                    ModelState.AddModelError("", "Invalid input data");
                    LoadSelectLists();
                    return View();
                }

                var order = new Order
                {
                    OrderDate = parsedDate,
                    Status = status,
                    UserId = userId,
                    MaterialId = materialId,
                    PaymentId = paymentId,
                    ReviewId = reviewId
                };

                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                LoadSelectLists();
                TempData["ErrorMessage"] = "Error creating order";
                return View();
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            LoadSelectLists(order.UserId, order.MaterialId, order.PaymentId, order.ReviewId);
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            string orderDate,
            string status,
            int userId,
            int materialId,
            int paymentId,
            int? reviewId)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null) return NotFound();

                if (!DateOnly.TryParse(orderDate, out DateOnly parsedDate) ||
                    string.IsNullOrEmpty(status) ||
                    userId <= 0 ||
                    materialId <= 0 ||
                    paymentId <= 0)
                {
                    ModelState.AddModelError("", "Invalid input data");
                    LoadSelectLists(userId, materialId, paymentId, reviewId);
                    return View(order);
                }

                order.OrderDate = parsedDate;
                order.Status = status;
                order.UserId = userId;
                order.MaterialId = materialId;
                order.PaymentId = paymentId;
                order.ReviewId = reviewId;

                _context.Update(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing order");
                LoadSelectLists(userId, materialId, paymentId, reviewId);
                TempData["ErrorMessage"] = "Error editing order";
                return RedirectToAction(nameof(Edit), new { id });
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Material)
                .Include(o => o.Payment)
                .Include(o => o.Review)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private void LoadSelectLists(
            int? selectedUserId = null,
            int? selectedMaterialId = null,
            int? selectedPaymentId = null,
            int? selectedReviewId = null)
        {
            ViewBag.Users = new SelectList(
                _context.Users.Select(u => new {
                    Id = u.Id,
                    FullName = $"{u.FirstName} {u.SurName}"
                }),
                "Id", "FullName", selectedUserId);

            ViewBag.Materials = new SelectList(
                _context.Materials, "Id", "Title", selectedMaterialId);

            ViewBag.Payments = new SelectList(
                _context.Payments, "Id", "PayMethod", selectedPaymentId);

            ViewBag.Reviews = new SelectList(
                _context.Reviews, "Id", "Title", selectedReviewId);
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}