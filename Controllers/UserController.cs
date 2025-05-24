using japantune.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace japantune.Controllers
{
    public class UserController : Controller
    {
        private readonly JapanTuneContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(JapanTuneContext context, ILogger<UserController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .ToListAsync();
            return View(users);
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: User/Create
        public IActionResult Create()
        {
            ViewBag.Roles = _context.Roles
                .Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = r.Title
                })
                .ToList();

            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user) // Убрали [Bind]
        {
            // Временно пропускаем валидацию
            if (user.RoleId == 0) // Если роль не выбрана
            {
                user.RoleId = _context.Roles.First().Id; // Установите роль по умолчанию
            }

            _context.Add(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            ViewBag.Roles = _context.Roles
                .Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = r.Title
                })
                .ToList();

            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            try
            {
                // Получаем существующего пользователя из БД
                var existingUser = await _context.Users.FindAsync(id);
                if (existingUser == null)
                {
                    return NotFound();
                }

                // Обновляем только изменяемые поля
                existingUser.FirstName = user.FirstName;
                existingUser.SurName = user.SurName;
                existingUser.LastName = user.LastName;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.ClientLogin = user.ClientLogin;
                existingUser.CardNum = user.CardNum;

                // Обработка роли (аналогично Create)
                existingUser.RoleId = user.RoleId == 0
                    ? _context.Roles.First().Id
                    : user.RoleId;

                // Обновляем пароль только если он был изменен
                if (!string.IsNullOrEmpty(user.ClientPassword))
                {
                    existingUser.ClientPassword = user.ClientPassword;
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!UserExists(user.Id))
                {
                    return NotFound();
                }
                _logger.LogError(ex, "Ошибка при редактировании пользователя");
                ModelState.AddModelError("", "Запись была изменена другим пользователем");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Ошибка БД при редактировании");
                ModelState.AddModelError("", "Ошибка при сохранении изменений");
            }

            // Всегда перезагружаем список ролей
            ViewBag.Roles = await _context.Roles
                .Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = r.Title
                })
                .ToListAsync();

            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var user = await _context.Users
                    .Include(u => u.Payments)
                        .ThenInclude(p => p.Orders)
                    .Include(u => u.Orders)
                    .Include(u => u.Cars)
                    .Include(u => u.Reviews)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return NotFound();
                }

                // Удаляем заказы, связанные с платежами пользователя
                foreach (var payment in user.Payments)
                {
                    _context.Orders.RemoveRange(payment.Orders);
                }

                // Удаляем платежи пользователя
                _context.Payments.RemoveRange(user.Payments);

                // Удаляем заказы пользователя
                _context.Orders.RemoveRange(user.Orders);

                // Удаляем автомобили пользователя
                _context.Cars.RemoveRange(user.Cars);

                // Удаляем отзывы пользователя
                _context.Reviews.RemoveRange(user.Reviews);

                // Удаляем самого пользователя
                _context.Users.Remove(user);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting user");
                ModelState.AddModelError("", $"Error deleting user: {ex.Message}");
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        [HttpGet]
        public IActionResult TestAddUser()
        {
            try
            {
                var testUser = new User
                {
                    FirstName = "Test",
                    SurName = "User",
                    PhoneNumber = "1234567890",
                    ClientLogin = "test_" + Guid.NewGuid().ToString()[..8],
                    ClientPassword = "testpass",
                    RoleId = _context.Roles.First().Id // Берём первую доступную роль
                };

                _context.Users.Add(testUser);
                _context.SaveChanges();

                return Content($"Тестовый пользователь добавлен! ID: {testUser.Id}");
            }
            catch (Exception ex)
            {
                return Content($"Ошибка: {ex.Message}\n\nStackTrace: {ex.StackTrace}");
            }
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}