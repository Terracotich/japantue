﻿using japantune.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace japantune.Controllers
{
    public class MaterialController : Controller
    {
        private readonly JapanTuneContext _context;
        private readonly ILogger<MaterialController> _logger;

        public MaterialController(JapanTuneContext context, ILogger<MaterialController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var materials = await _context.Materials
                .Include(m => m.Supplier)
                .ToListAsync();
            return View(materials);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var material = await _context.Materials
                .Include(m => m.Supplier)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (material == null)
            {
                return NotFound();
            }

            return View(material);
        }

        public IActionResult Create()
        {
            ViewBag.Suppliers = new SelectList(_context.Suppliers, "Id", "Title"); 
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string title, string price, string quantity, string supplierId)
        {
            try
            {
                if (string.IsNullOrEmpty(title) ||
                    !decimal.TryParse(price, out decimal parsedPrice) ||
                    !int.TryParse(quantity, out int parsedQuantity) ||
                    !int.TryParse(supplierId, out int parsedSupplierId))
                {
                    ViewBag.Suppliers = new SelectList(_context.Suppliers, "Id", "Title"); 
                    ModelState.AddModelError("", "Invalid input data.");
                    return View();
                }

                var material = new Material
                {
                    Title = title,
                    Price = (int)parsedPrice,
                    Quantity = parsedQuantity,
                    SupplierId = parsedSupplierId
                };

                _context.Add(material);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating material");
                ViewBag.Suppliers = new SelectList(_context.Suppliers, "Id", "Title");
                TempData["ErrorMessage"] = "Error creating material.";
                return View();
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var material = await _context.Materials.FindAsync(id);
            if (material == null) return NotFound();

            ViewBag.Suppliers = new SelectList(_context.Suppliers, "Id", "Title");
            return View(material);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string title, string price, string quantity, string supplierId)
        {
            try
            {
                if (!decimal.TryParse(price, out decimal parsedPrice) ||
                    !int.TryParse(quantity, out int parsedQuantity) ||
                    !int.TryParse(supplierId, out int parsedSupplierId) ||
                    parsedPrice <= 0 || parsedQuantity < 0 || parsedSupplierId <= 0)
                {
                    ModelState.AddModelError("", "Invalid input data.");
                    ViewBag.Suppliers = _context.Suppliers.ToList();
                    return RedirectToAction(nameof(Edit), new { id });
                }

                var material = await _context.Materials.FindAsync(id);
                if (material == null)
                {
                    return NotFound();
                }

                material.Title = title;
                material.Price = (int)parsedPrice;
                material.Quantity = parsedQuantity;
                material.SupplierId = parsedSupplierId;

                _context.Update(material);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating material");
                TempData["ErrorMessage"] = "Error updating material.";
                ViewBag.Suppliers = _context.Suppliers.ToList();
                return RedirectToAction(nameof(Edit), new { id });
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var material = await _context.Materials
                .Include(m => m.Supplier)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (material == null)
            {
                return NotFound();
            }

            return View(material);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var material = await _context.Materials.FindAsync(id);
            _context.Materials.Remove(material);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MaterialExists(int id)
        {
            return _context.Materials.Any(e => e.Id == id);
        }
    }
}