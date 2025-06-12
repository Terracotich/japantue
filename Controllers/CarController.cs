using japantune.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace japantune.Controllers
{
    public class CarController : Controller
    {
        private readonly JapanTuneContext _context;
        private readonly ILogger<CarController> _logger;

        public CarController(JapanTuneContext context, ILogger<CarController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var cars = await _context.Cars
                .Include(c => c.User)
                .ToListAsync();
            return View(cars);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await _context.Cars
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (car == null)
            {
                return NotFound();
            }

            return View(car);
        }

        public IActionResult Create()
        {
            try
            {
                if (!_context.Users.Any())
                {
                    TempData["ErrorMessage"] = "No users available. Please create users first.";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.Users = new SelectList(_context.Users.Select(u => new
                {
                    Id = u.Id,
                    FullName = $"{u.FirstName} {u.SurName}"
                }), "Id", "FullName");

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Create GET");
                TempData["ErrorMessage"] = "An error occurred while loading the form.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(string mark, string model, int releaseDate, string licensePlate, int userId)
        {
            var car = new Car
            {
                Mark = mark,
                Model = model,
                ReleaseDate = releaseDate,
                LicensePlate = licensePlate,
                UserId = userId
            };

            _context.Add(car);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await _context.Cars.FindAsync(id);
            if (car == null)
            {
                return NotFound();
            }

            ViewBag.Users = new SelectList(
                _context.Users.Select(u => new {
                    Id = u.Id,
                    FullName = $"{u.FirstName} {u.SurName}"
                }),
                "Id", "FullName");

            return View(car);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, string mark, string model, int releaseDate, string licensePlate, int userId)
        {
            try
            {
                var existingCar = await _context.Cars.FindAsync(id);
                if (existingCar == null)
                {
                    return NotFound();
                }

                existingCar.Mark = mark;
                existingCar.Model = model;
                existingCar.ReleaseDate = releaseDate;
                existingCar.LicensePlate = licensePlate;
                existingCar.UserId = userId;

                _context.Update(existingCar);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing car");
                TempData["ErrorMessage"] = "Произошла ошибка при редактировании";
                return RedirectToAction(nameof(Edit), new { id });
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await _context.Cars
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (car == null)
            {
                return NotFound();
            }

            return View(car);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null)
            {
                return NotFound();
            }

            try
            {
                _context.Cars.Remove(car);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error deleting car");
                return RedirectToAction(nameof(Delete), new { id, saveChangesError = true });
            }
        }

        private bool CarExists(int id)
        {
            return _context.Cars.Any(e => e.Id == id);
        }
    }
}