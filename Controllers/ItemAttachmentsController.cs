using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Online_market.Data;
using Online_market.Models;
using Online_market.Services;

namespace Online_market.Controllers
{
    public class ItemAttachmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;

        public ItemAttachmentsController(ApplicationDbContext context, IImageService imageService)
        {
            _context = context;
            _imageService = imageService;
        }

        // GET: ItemAttachments
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ItemAttachment.Include(i => i.Item);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: ItemAttachments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var itemAttachment = await _context.ItemAttachment
                .Include(i => i.Item)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (itemAttachment == null)
            {
                return NotFound();
            }

            return View(itemAttachment);
        }

        // GET: ItemAttachments/Create
        public IActionResult Create()
        {
            ViewData["ItemId"] = new SelectList(_context.Item, "Id", "Description");
            return View();
        }

        // POST: ItemAttachments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create([Bind("Id,ImageData,Link,ContentType,ItemId,ItemAttachmentTypeId")] ItemAttachment itemAttachment, IFormFile image)
        {
            var slug = _context.Item.FirstOrDefault(i => i.Id == itemAttachment.ItemId).Slug;
            if (ModelState.IsValid)
            {
                itemAttachment.ContentType = _imageService.RecordContentType(image);
                itemAttachment.ImageData = await _imageService.EncodeFileAsync(image);
                
                _context.Add(itemAttachment);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details","Items", new { slug });
            }
            ViewData["ItemId"] = new SelectList(_context.Item, "Id", "Name", itemAttachment.ItemId);
            return RedirectToAction("Details", "Items", new { slug });
        }

        // GET: ItemAttachments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var itemAttachment = await _context.ItemAttachment.FindAsync(id);
            if (itemAttachment == null)
            {
                return NotFound();
            }
            ViewData["ItemId"] = new SelectList(_context.Item, "Id", "Name", itemAttachment.ItemId);
            return View(itemAttachment);
        }

        // POST: ItemAttachments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ImageData,ContentType,ItemId")] ItemAttachment itemAttachment)
        {
            if (id != itemAttachment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(itemAttachment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemAttachmentExists(itemAttachment.Id))
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
            ViewData["ItemId"] = new SelectList(_context.Item, "Id", "Description", itemAttachment.ItemId);
            return View(itemAttachment);
        }

        // GET: ItemAttachments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var itemAttachment = await _context.ItemAttachment
                .Include(i => i.Item)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (itemAttachment == null)
            {
                return NotFound();
            }

            return View(itemAttachment);
        }

        // POST: ItemAttachments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var itemAttachment = await _context.ItemAttachment.FindAsync(id);
            _context.ItemAttachment.Remove(itemAttachment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ItemAttachmentExists(int id)
        {
            return _context.ItemAttachment.Any(e => e.Id == id);
        }
    }
}
