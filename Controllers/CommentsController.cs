using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Online_market.Data;
using Online_market.Models;
using Online_market.Services;

namespace Online_market.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<CustomUser> _userManager;
        private readonly ICanUserComment _canUserComment;

        public CommentsController(ApplicationDbContext context, 
            UserManager<CustomUser> userManager,
            ICanUserComment canUserComment)
        {
            _context = context;
            _userManager = userManager;
            _canUserComment = canUserComment;
        }

        // GET: Comments
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Comment.Include(c => c.CustomUser).Include(c => c.Item).Include(c => c.Rate);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Comments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comment
                .Include(c => c.CustomUser)
                .Include(c => c.Item)
                .Include(c => c.Rate)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // GET: Comments/Create
        public IActionResult Create()
        {
            ViewData["CustomUserId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["ItemId"] = new SelectList(_context.Item, "Id", "Id");
            ViewData["RateId"] = new SelectList(_context.Rate, "Id", "Id");
            return View();
        }

        // POST: Comments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Content,Date,RateId,ItemId,CustomUserId")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var rateid_from_database = _context.Rate.FirstOrDefault(r => r.Value == comment.RateId).Id;
                comment.RateId = rateid_from_database;
                comment.CustomUserId = userId;
                comment.Date = DateTime.Now;
                _context.Add(comment);
                await _context.SaveChangesAsync();
                
                var item = _context.Item.Include(i => i.Comments).Include(i => i.Rates).Include(i => i.Attachments).FirstOrDefault(i => i.Id == comment.ItemId);
                var rateValue = ((_context.Comment.Where(i => i.ItemId == comment.ItemId).Include(i => i.Rate).Include(i => i.Item).Select(i => i.Rate).Select(i => i.Value).ToList().Sum(x => Convert.ToDouble(x)) + 5) / Convert.ToDouble(item.Comments.Count + 1));
                //var rateValue = (item.Comments.Select(x => x.Rate).Select(i => i.Value).ToList().Sum(x => Convert.ToDouble(x)))/ Convert.ToDouble(item.Comments.Count);
                item.RateValue = rateValue;
                await _canUserComment.RemoveUserFromItemAsync(userId, comment.ItemId);
                _context.Update(item);
                await _context.SaveChangesAsync();
                var slug = item.Slug;

                return RedirectToAction("Details","Items", new { slug });
            }
            ViewData["CustomUserId"] = new SelectList(_context.Users, "Id", "Id", comment.CustomUserId);
            ViewData["ItemId"] = new SelectList(_context.Item, "Id", "Id", comment.ItemId);
            ViewData["RateId"] = new SelectList(_context.Rate, "Id", "Id", comment.RateId);
            return View(comment);
        }

        // GET: Comments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comment.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }
            ViewData["CustomUserId"] = new SelectList(_context.Users, "Id", "Id", comment.CustomUserId);
            ViewData["ItemId"] = new SelectList(_context.Item, "Id", "Id", comment.ItemId);
            ViewData["RateId"] = new SelectList(_context.Rate, "Id", "Id", comment.RateId);
            return View(comment);
        }

        // POST: Comments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Content,Date,RateId,ItemId,CustomUserId")] Comment comment)
        {
            if (id != comment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(comment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentExists(comment.Id))
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
            ViewData["CustomUserId"] = new SelectList(_context.Users, "Id", "Id", comment.CustomUserId);
            ViewData["ItemId"] = new SelectList(_context.Item, "Id", "Id", comment.ItemId);
            ViewData["RateId"] = new SelectList(_context.Rate, "Id", "Id", comment.RateId);
            return View(comment);
        }

        // GET: Comments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comment
                .Include(c => c.CustomUser)
                .Include(c => c.Item)
                .Include(c => c.Rate)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // POST: Comments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var comment = await _context.Comment.FindAsync(id);
            _context.Comment.Remove(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CommentExists(int id)
        {
            return _context.Comment.Any(e => e.Id == id);
        }
    }
}
