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
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = await _context.Orders.Include(o => o.Category).Include(o => o.CustomUser).Include(o => o.OrderStatus).Include(o => o.Item).OrderByDescending(o => o.Created).ToListAsync();
            List<Order> orderList = new List<Order>();
            foreach (var item in applicationDbContext)
            {
                var trackingNumber = item.TrackingNumber;
                if(!orderList.Select(o => o.TrackingNumber).Contains(trackingNumber))
                {
                    orderList.Add(item);
                }
            }
            return View(orderList);
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Category)
                .Include(o => o.CustomUser)
                .Include(o => o.Item)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Id");
            ViewData["CustomUserId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["ItemId"] = new SelectList(_context.Item, "Id", "Id");
            return View();
        }

        // GET: Orders/Create
        public IActionResult TrackOrder()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> TrackOrderDetailsAsync(string TrackingNumber)
        {
            var order = await _context.Orders.Where(o => o.TrackingNumber == TrackingNumber).Include(o => o.Category).Include(o => o.CustomUser).Include(o => o.Item).Include(o => o.OrderStatus).ToListAsync();
            if (order.Count > 0)
            {
                float total = 0;
                float TotalItem = 0;
                var receivedId = _context.OrderStatuses.FirstOrDefault(o => o.Name == "Received").Id;
                foreach (var items in order)
                {
                    if ((User.IsInRole("Administrator") || User.IsInRole("Moderator")) && items.OrderStatusId == receivedId)
                    {
                        var workingStatus = _context.OrderStatuses.FirstOrDefault(o => o.Name == "Working").Id;
                        items.OrderStatusId = workingStatus;
                        _context.Update(items);
                        await _context.SaveChangesAsync();
                    }
                    total += (float.Parse(items.Price) * (float)items.Quantity);
                    TotalItem += (float)items.Quantity;
                }

                total += (float)15;
                float faxFee = total * (float)7 / (float)100;
                float TotalGrand = total + faxFee;
                ViewData["Date"] = _context.Orders.FirstOrDefault(o => o.TrackingNumber == TrackingNumber).Date;
                ViewData["TotalItem"] = Math.Round(TotalItem * 100f) / 100f ;
                ViewData["faxFee"] = Math.Round(faxFee * 100f) / 100f;
                ViewData["TotalGrand"] = Math.Round(TotalGrand * 100f) / 100f;
                ViewData["TrackingNumber"] = TrackingNumber;
                ViewData["OrderStatus"] = new SelectList(_context.OrderStatuses.Where(o => o.Name != "Received" && o.Name != "Working"), "Id", "Description");
                return View(order);
            }
            return RedirectToAction("CantFindTrackingNumber","Orders");
           
        }

        public IActionResult CantFindTrackingNumber()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatusAsync(string TrackingNumber, int OrderStatusId, string TrackingLink)
        {
            var order = await _context.Orders.Where(o => o.TrackingNumber == TrackingNumber).Include(o => o.Category).Include(o => o.CustomUser).Include(o => o.Item).Include(o => o.OrderStatus).ToListAsync();
            if (order.Count > 0)
            {
                float total = 0;
                float TotalItem = 0;
                foreach (var items in order)
                {
                  
                    items.OrderStatusId = OrderStatusId;
                    items.Updated = DateTimeOffset.Now;
                    if (TrackingLink != null)
                    {
                        items.TrackingLink = TrackingLink;
                    }
                    _context.Update(items);
                    await _context.SaveChangesAsync();
                    
                    total += (float.Parse(items.Price) * (float)items.Quantity);
                    TotalItem += (float)items.Quantity;
                }

                total += (float)15;
                float faxFee = total * (float)7 / (float)100;
                float TotalGrand = total + faxFee;
                ViewData["Date"] = _context.Orders.FirstOrDefault(o => o.TrackingNumber == TrackingNumber).Date;
                ViewData["TotalItem"] = Math.Round(TotalItem * 100f) / 100f;
                ViewData["faxFee"] = Math.Round(faxFee * 100f) / 100f;
                ViewData["TotalGrand"] = Math.Round(TotalGrand * 100f) / 100f;
                ViewData["TrackingNumber"] = TrackingNumber;
                ViewData["OrderStatus"] = new SelectList(_context.OrderStatuses.Where(o => o.Name != "Received" && o.Name != "Working"), "Id", "Description");

                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction("CantFindTrackingNumber", "Orders");
        }



        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TrackingNumber,Name,Price,IsSold,Quantity,ImageData,ContentType,ItemId,CategoryId,CustomUserId")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Id", order.CategoryId);
            ViewData["CustomUserId"] = new SelectList(_context.Users, "Id", "Id", order.CustomUserId);
            ViewData["ItemId"] = new SelectList(_context.Item, "Id", "Id", order.ItemId);
            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Id", order.CategoryId);
            ViewData["CustomUserId"] = new SelectList(_context.Users, "Id", "Id", order.CustomUserId);
            ViewData["ItemId"] = new SelectList(_context.Item, "Id", "Id", order.ItemId);
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TrackingNumber,Name,Price,IsSold,Quantity,ImageData,ContentType,ItemId,CategoryId,CustomUserId")] Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.Id))
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
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Id", order.CategoryId);
            ViewData["CustomUserId"] = new SelectList(_context.Users, "Id", "Id", order.CustomUserId);
            ViewData["ItemId"] = new SelectList(_context.Item, "Id", "Id", order.ItemId);
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Category)
                .Include(o => o.CustomUser)
                .Include(o => o.Item)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}
