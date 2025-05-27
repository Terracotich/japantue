using japantune.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace japantune.Controllers
{
    public class PaymentController : Controller
    {
        private readonly JapanTuneContext _context;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(JapanTuneContext context, ILogger<PaymentController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Payments (без изменений)
        public async Task<IActionResult> Index()
        {
            return View(await _context.Payments
                .Include(p => p.User)
                .Include(p => p.Orders)
                .ToListAsync());
        }

        // GET: Payments/Create
        public IActionResult Create()
        {
            ViewBag.Users = new SelectList(
                _context.Users.Select(u => new {
                    Id = u.Id,
                    FullName = $"{u.FirstName} {u.SurName}"
                }),
                "Id", "FullName");
            return View();
        }

        // POST: Payments/Create (с параметрами)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string price,
            string payMethod,
            string paymentDate,
            int userId)
        {
            try
            {
                // Валидация
                if (!decimal.TryParse(price, out decimal parsedPrice) ||
                    string.IsNullOrEmpty(payMethod) ||
                    !DateOnly.TryParse(paymentDate, out DateOnly parsedDate) ||
                    userId <= 0)
                {
                    ModelState.AddModelError("", "Invalid input data.");
                    ViewBag.Users = GetUsersSelectList();
                    return View();
                }

                var payment = new Payment
                {
                    Price = (int)parsedPrice,
                    PayMethod = payMethod,
                    PaymentDate = parsedDate,
                    UserId = userId
                };

                _context.Add(payment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment");
                ViewBag.Users = GetUsersSelectList();
                TempData["ErrorMessage"] = "Error creating payment.";
                return View();
            }
        }

        // GET: Payments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return NotFound();

            ViewBag.Users = new SelectList(
                _context.Users.Select(u => new {
                    Id = u.Id,
                    FullName = $"{u.FirstName} {u.SurName}"
                }),
                "Id", "FullName", payment.UserId);
            return View(payment);
        }

        // POST: Payments/Edit/5 (с параметрами)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            string price,
            string payMethod,
            string paymentDate,
            int userId)
        {
            try
            {
                var payment = await _context.Payments.FindAsync(id);
                if (payment == null) return NotFound();

                // Валидация
                if (!decimal.TryParse(price, out decimal parsedPrice) ||
                    string.IsNullOrEmpty(payMethod) ||
                    !DateOnly.TryParse(paymentDate, out DateOnly parsedDate) ||
                    userId <= 0)
                {
                    ModelState.AddModelError("", "Invalid input data.");
                    ViewBag.Users = GetUsersSelectList(userId);
                    return View(payment);
                }

                // Обновление полей
                payment.Price = (int)parsedPrice;
                payment.PayMethod = payMethod;
                payment.PaymentDate = parsedDate;
                payment.UserId = userId;

                _context.Update(payment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing payment");
                ViewBag.Users = GetUsersSelectList(userId);
                TempData["ErrorMessage"] = "Error editing payment.";
                return RedirectToAction(nameof(Edit), new { id });
            }
        }


        // GET: Payments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var payment = await _context.Payments
                .Include(p => p.User)
                .Include(p => p.Orders)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (payment == null) return NotFound();

            ViewBag.OrderCount = payment.Orders?.Count ?? 0;
            return View(payment);
        }

        // POST: Payments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Удаляем связанные заказы
                var orders = await _context.Orders
                    .Where(o => o.PaymentId == id)
                    .ToListAsync();
                _context.Orders.RemoveRange(orders);

                // Удаляем платеж
                var payment = await _context.Payments.FindAsync(id);
                if (payment != null)
                {
                    _context.Payments.Remove(payment);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting payment");
                ModelState.AddModelError("", $"Error deleting payment: {ex.Message}");
                return RedirectToAction(nameof(Delete), new { id });
            }
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

        private bool PaymentExists(int id)
        {
            return _context.Payments.Any(e => e.Id == id);
        }
    }
}
