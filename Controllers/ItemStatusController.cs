using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Online_market.Data;
using Online_market.Models;

namespace Online_market.Controllers
{
    public class ItemStatusController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ItemStatusController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ItemStatus
        public async Task<IActionResult> Index()
        {
            return View(await _context.ItemStatus.ToListAsync());
        }

        // GET: ItemStatus/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var itemStatus = await _context.ItemStatus
                .FirstOrDefaultAsync(m => m.Id == id);
            if (itemStatus == null)
            {
                return NotFound();
            }

            return View(itemStatus);
        }

        // GET: ItemStatus/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ItemStatus/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] ItemStatus itemStatus)
        {
            if (ModelState.IsValid)
            {
                _context.Add(itemStatus);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(itemStatus);
        }

        // GET: ItemStatus/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var itemStatus = await _context.ItemStatus.FindAsync(id);
            if (itemStatus == null)
            {
                return NotFound();
            }
            return View(itemStatus);
        }

        // POST: ItemStatus/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] ItemStatus itemStatus)
        {
            if (id != itemStatus.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(itemStatus);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemStatusExists(itemStatus.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(itemStatus);
        }

        // GET: ItemStatus/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var itemStatus = await _context.ItemStatus
                .FirstOrDefaultAsync(m => m.Id == id);
            if (itemStatus == null)
            {
                return NotFound();
            }

            return View(itemStatus);
        }

        // POST: ItemStatus/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var itemStatus = await _context.ItemStatus.FindAsync(id);
            _context.ItemStatus.Remove(itemStatus);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ItemStatusExists(int id)
        {
            return _context.ItemStatus.Any(e => e.Id == id);
        }
    }
}
