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

        // GET: Payments
        public async Task<IActionResult> Index()
        {
            var payments = await _context.Payments
                .Include(p => p.User)
                .Include(p => p.Orders)
                .ToListAsync();
            return View(payments);
        }

        // GET: Payments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var payment = await _context.Payments
                .Include(p => p.User)
                .Include(p => p.Orders)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (payment == null) return NotFound();

            return View(payment);
        }

        // GET: Payments/Create
        public IActionResult Create()
        {
            ViewBag.Users = _context.Users
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = $"{u.FirstName} {u.SurName}"
                })
                .ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Payment payment)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Преобразуем DateTime.Now в DateOnly
                    payment.PaymentDate = DateOnly.FromDateTime(DateTime.Now);

                    _context.Add(payment);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error creating payment");
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            // Заполняем список пользователей для выпадающего списка
            ViewBag.Users = _context.Users
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = $"{u.FirstName} {u.SurName}"
                })
                .ToList();

            return View(payment);
        }

        // GET: Payments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return NotFound();

            ViewBag.Users = _context.Users
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = $"{u.FirstName} {u.SurName}",
                    Selected = u.Id == payment.UserId
                })
                .ToList();
            return View(payment);
        }

        // POST: Payments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Payment payment)
        {
            if (id != payment.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(payment);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!PaymentExists(payment.Id))
                    {
                        return NotFound();
                    }
                    _logger.LogError(ex, "Error editing payment");
                    ModelState.AddModelError("", "Unable to save changes. The payment was updated by another user.");
                }
            }

            ViewBag.Users = _context.Users
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = $"{u.FirstName} {u.SurName}",
                    Selected = u.Id == payment.UserId
                })
                .ToList();
            return View(payment);
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

        private bool PaymentExists(int id)
        {
            return _context.Payments.Any(e => e.Id == id);
        }
    }
}