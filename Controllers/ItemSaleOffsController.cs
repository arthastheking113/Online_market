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
    public class ItemSaleOffsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ItemSaleOffsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ItemSaleOffs
        public async Task<IActionResult> Index()
        {
            return View(await _context.ItemSaleOff.ToListAsync());
        }

        // GET: ItemSaleOffs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var itemSaleOff = await _context.ItemSaleOff
                .FirstOrDefaultAsync(m => m.Id == id);
            if (itemSaleOff == null)
            {
                return NotFound();
            }

            return View(itemSaleOff);
        }

        // GET: ItemSaleOffs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ItemSaleOffs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,SalePersentAmount")] ItemSaleOff itemSaleOff)
        {
            if (ModelState.IsValid)
            {
                _context.Add(itemSaleOff);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(itemSaleOff);
        }

        // GET: ItemSaleOffs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var itemSaleOff = await _context.ItemSaleOff.FindAsync(id);
            if (itemSaleOff == null)
            {
                return NotFound();
            }
            return View(itemSaleOff);
        }

        // POST: ItemSaleOffs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,SalePersentAmount")] ItemSaleOff itemSaleOff)
        {
            if (id != itemSaleOff.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(itemSaleOff);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemSaleOffExists(itemSaleOff.Id))
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
            return View(itemSaleOff);
        }

        // GET: ItemSaleOffs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var itemSaleOff = await _context.ItemSaleOff
                .FirstOrDefaultAsync(m => m.Id == id);
            if (itemSaleOff == null)
            {
                return NotFound();
            }

            return View(itemSaleOff);
        }

        // POST: ItemSaleOffs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var itemSaleOff = await _context.ItemSaleOff.FindAsync(id);
            _context.ItemSaleOff.Remove(itemSaleOff);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ItemSaleOffExists(int id)
        {
            return _context.ItemSaleOff.Any(e => e.Id == id);
        }
    }
}
