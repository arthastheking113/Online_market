using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Online_market.Data;
using Online_market.Models;

namespace Online_market.Controllers
{
    [Authorize]
    public class CartsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<CustomUser> _userManger;
        private readonly SignInManager<CustomUser> _signInManager;

        public CartsController(ApplicationDbContext context, UserManager<CustomUser> userManger, SignInManager<CustomUser> signInManager)
        {
            _context = context;
            _userManger = userManger;
            _signInManager = signInManager;
        }

        // GET: Carts
        public async Task<IActionResult> Index()
        {

            var applicationDbContext = await _context.Cart.Where(i => i.CustomUserId == _userManger.GetUserId(User) && !i.IsSold).Include(i => i.Category).Include(i => i.CustomUser).ToListAsync();
            foreach (var item in applicationDbContext)
            {
                var itemId = _context.Item.FirstOrDefault(i => i.Id == item.ItemId).ItemSaleOffId;
                var CurrentPrice = _context.Item.FirstOrDefault(i => i.Id == item.ItemId).ListPrice(_context, item.Price, itemId);
                if ( item.Price != CurrentPrice)
                {
                    item.Price = CurrentPrice;
                    _context.Update(item);
                    await _context.SaveChangesAsync();
                }
            }
            decimal total = 0;
            foreach (var item in applicationDbContext)
            {
                total += Convert.ToDecimal(item.Price) * Convert.ToDecimal(item.Quantity);
            }
            int[] IdArray = new int[applicationDbContext.Count];
            List<int> id_Of_each_Item = applicationDbContext.Select(c => c.Id).ToList();
            for (int runs = 0; runs < applicationDbContext.Count; runs++)
            {
                IdArray[runs] = id_Of_each_Item[runs];
            }

            total += 15;
            decimal faxFee = total * 7 / 100;
            decimal TotalGrand = total + faxFee;
            ViewData["total"] = total;
            ViewData["faxFee"] = faxFee;
            ViewData["TotalGrand"] = TotalGrand;
            ViewData["IdArray"] = IdArray;
            return View(applicationDbContext);
        }

        public async Task<JsonResult> GetIdArray()
        {
            var applicationDbContext = await _context.Cart.Where(i => i.CustomUserId == _userManger.GetUserId(User) && !i.IsSold).Include(i => i.Category).Include(i => i.CustomUser).ToListAsync();
            int[] IdArray = new int[applicationDbContext.Count];
            List<int> id_Of_each_Item = applicationDbContext.Select(c => c.Id).ToList();
            for (int runs = 0; runs < applicationDbContext.Count; runs++)
            {
                IdArray[runs] = id_Of_each_Item[runs];
            }
            return Json(IdArray);
        }
        public async Task<IActionResult> AddItemAsync(int itemId, int quantity)
        {
            var itemInStock = _context.Item.FirstOrDefault(i => i.Id == itemId);

            var number_of_some_item = (await _context.Cart.Where(i => i.CustomUserId == _userManger.GetUserId(User) && !i.IsSold && i.ItemId == itemId).Include(i => i.Category).Include(i => i.CustomUser).ToListAsync()).Count;
            if (number_of_some_item == 0)
            {
                if (quantity > 0)
                {
                    Cart newCart = new Cart
                    {
                        Name = itemInStock.Name,
                        ItemId = itemId,
                        Quantity = quantity,
                        Price = itemInStock.ListPrice(_context, itemInStock.Price, itemInStock.ItemSaleOffId),
                        CategoryId = itemInStock.CategoryId,
                        CustomUserId = _userManger.GetUserId(User),
                        ImageData = itemInStock.ImageData,
                        ContentType = itemInStock.ContentType,
                        Slug = itemInStock.Slug
                    };
                    _context.Add(newCart);
                    await _context.SaveChangesAsync();
                    var slug = itemInStock.Slug;
                    return RedirectToAction("Details","Items", new { slug = slug });
                }
                else
                {
                    Cart newCart = new Cart
                    {
                        Name = itemInStock.Name,
                        ItemId = itemId,
                        Quantity = 1,
                        Price = itemInStock.ListPrice(_context, itemInStock.Price, itemInStock.ItemSaleOffId),
                        CategoryId = itemInStock.CategoryId,
                        CustomUserId = _userManger.GetUserId(User),
                        ImageData = itemInStock.ImageData,
                        ContentType = itemInStock.ContentType,
                        Slug = itemInStock.Slug
                    };
                    _context.Add(newCart);
                    await _context.SaveChangesAsync();
                    var slug = newCart.Slug;
                    return RedirectToAction("Details", "Items", new { slug = slug });
                }                                
            }
            else
            {
                if (quantity <= 0)
                {
                quantity = 1;
                }
                var Item = await _context.Cart
                        .Include(i => i.Category)
                        .Include(i => i.CustomUser)
                        .Include(i => i.Item)
                        .FirstOrDefaultAsync(m => m.ItemId == itemId);

                Item.Quantity += quantity;
                _context.Update(Item);
                await _context.SaveChangesAsync();
                var slug = Item.Slug;
                return RedirectToAction("Details", "Items", new { slug = slug });
            }

        }

        public async Task UpdateItemAsync(int id, int value)
        {
            var applicationDbContext = await _context.Cart.Where(i => i.CustomUserId == _userManger.GetUserId(User) && !i.IsSold).Include(i => i.Category).Include(i => i.CustomUser).ToListAsync();

            var item = applicationDbContext.FirstOrDefault(i => i.Id == id);
            if (value == 1)
            {
                item.Quantity += 1;
            }
            else
            {
                item.Quantity -= 1;
            }
            _context.Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task<JsonResult> DeleteItemAsync(int id)
        {
            var item = await _context.Cart.FindAsync(id);
            _context.Cart.Remove(item);
            await _context.SaveChangesAsync();
            var applicationDbContext = await _context.Cart.Where(i => i.CustomUserId == _userManger.GetUserId(User) && !i.IsSold).Include(i => i.Category).Include(i => i.CustomUser).ToListAsync();
            foreach (var items in applicationDbContext)
            {
                var saleOffId = _context.Item.FirstOrDefault(i => i.Id == items.ItemId).ItemSaleOffId;
                var price = _context.Item.FirstOrDefault(i => i.Id == items.ItemId).Price;
                var CurrentPrice = _context.Item.FirstOrDefault(i => i.Id == items.ItemId).ListPrice(_context, price, saleOffId);
                if (items.Price != CurrentPrice)
                {
                    items.Price = CurrentPrice;
                    _context.Update(items);
                    await _context.SaveChangesAsync();
                }
            }
            float total = 0;
            float TotalItem = 0;
            foreach (var items in applicationDbContext)
            {
                total += (float.Parse(items.Price) * (float)items.Quantity);
                TotalItem += (float)items.Quantity;
            }

            total += (float)15;
            float faxFee = total * (float)7 / (float)100;
            float TotalGrand = total + faxFee;
            if (TotalItem == 0)
            {
                TotalGrand = 0.00f;
            }
            float[] CartValueArray = new float[3];
            CartValueArray[0] = TotalGrand;
            CartValueArray[1] = TotalItem;
            CartValueArray[2] = faxFee;
            return Json(CartValueArray);
        }

        // GET: Carts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cart = await _context.Cart
                .Include(i => i.Category)
                .Include(i => i.CustomUser)
                .Include(i => i.Item)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cart == null)
            {
                return NotFound();
            }

            return View(cart);
        }

        public async Task<IActionResult> CheckOutAsync()
        {
            var applicationDbContext = await _context.Cart.Where(i => i.CustomUserId == _userManger.GetUserId(User) && !i.IsSold).Include(i => i.Category).Include(i => i.CustomUser).ToListAsync();
            foreach (var items in applicationDbContext)
            {
                var saleOffId = _context.Item.FirstOrDefault(i => i.Id == items.ItemId).ItemSaleOffId;
                var price = _context.Item.FirstOrDefault(i => i.Id == items.ItemId).Price;
                var CurrentPrice = _context.Item.FirstOrDefault(i => i.Id == items.ItemId).ListPrice(_context, price, saleOffId);
                if (items.Price != CurrentPrice)
                {
                    items.Price = CurrentPrice;
                    _context.Update(items);
                    await _context.SaveChangesAsync();
                }
            }

            float total = 0;
            float TotalItem = 0;
            foreach (var items in applicationDbContext)
            {
                total += (float.Parse(items.Price) * (float)items.Quantity);
                TotalItem += (float)items.Quantity;
            }

            total += (float)15;
            float faxFee = total * (float)7 / (float)100;
            float TotalGrand = total + faxFee;
            ViewData["faxFee"] = faxFee;
            ViewData["TotalGrand"] = TotalGrand;
            return View(applicationDbContext);
        }
        // GET: Carts/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Id");
            ViewData["CustomUserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }


        public async Task<IActionResult> Information(string FirstName, string LastName, string Email, string PhoneNumber, string Adress, string State, string City, string Zipcode, string Notes)
        {
            var applicationDbContext = await _context.Cart.Where(i => i.CustomUserId == _userManger.GetUserId(User) && !i.IsSold).Include(i => i.Category).Include(i => i.CustomUser).ToListAsync();
            foreach (var items in applicationDbContext)
            {
                items.Notes = Notes;
                var saleOffId = _context.Item.FirstOrDefault(i => i.Id == items.ItemId).ItemSaleOffId;
                var price = _context.Item.FirstOrDefault(i => i.Id == items.ItemId).Price;
                var CurrentPrice = _context.Item.FirstOrDefault(i => i.Id == items.ItemId).ListPrice(_context, price, saleOffId);
                if (items.Price != CurrentPrice)
                {
                    items.Price = CurrentPrice;                  
                }
                _context.Update(items);
                await _context.SaveChangesAsync();
            }
            try
            {
                var user = await _userManger.GetUserAsync(User);
                user.FirstName = FirstName;
                user.LastName = LastName;
                user.Email = Email;
                user.PhoneNumber = PhoneNumber;
                user.Street = Adress;
                user.State = State;
                user.City = City;
                user.Zipcode = Zipcode;
                _context.Update(user);
                await _context.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"*** ERROR *** - message:{ex.Message}");
                throw;
            }
            float total = 0;
            float TotalItem = 0;
            foreach (var items in applicationDbContext)
            {
                total += (float.Parse(items.Price) * (float)items.Quantity);
                TotalItem += (float)items.Quantity;
            }

            total += (float)15;
            float faxFee = total * (float)7 / (float)100;
            float TotalGrand = total + faxFee;
            ViewData["faxFee"] = faxFee;
            ViewData["TotalGrand"] = TotalGrand;
            return View(applicationDbContext);

        }
        // POST: Carts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ItemId,Name,Description,Price,IsSold,ImageData,ContentType,CategoryId,CustomUserId,Quantity,Slug")] Cart cart,string returnUrl)
        {
            var number_of_some_item = (await _context.Cart.Where(i => i.CustomUserId == _userManger.GetUserId(User) && !i.IsSold && i.ItemId == cart.ItemId).Include(i => i.Category).Include(i => i.CustomUser).ToListAsync()).Count;
            returnUrl ??= Url.Content("~/");
            if (number_of_some_item == 0)
            {
                if (ModelState.IsValid)
                {
                    if (cart.Quantity < 0)
                    {
                        cart.Quantity = 1;
                    }

                    cart.CustomUserId = _userManger.GetUserId(User);
                    _context.Add(cart);
                    await _context.SaveChangesAsync();
                    return LocalRedirect(returnUrl);
                }
            }
            else
            {
                if (ModelState.IsValid)
                {
                    if (cart.Quantity < 0)
                    {
                        cart.Quantity = 1;
                    }
                    var Item = await _context.Cart
                            .Include(i => i.Category)
                            .Include(i => i.CustomUser)
                            .Include(i => i.Item)
                            .FirstOrDefaultAsync(m => m.ItemId == cart.ItemId);

                    Item.Quantity += cart.Quantity;
                    _context.Update(Item);
                    await _context.SaveChangesAsync();
                    return LocalRedirect(returnUrl);
                }
            }          
            return RedirectToAction("Index","Home");
        }

        // GET: Carts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var itemInCart = await _context.Cart.FindAsync(id);
            if (itemInCart == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Id", itemInCart.CategoryId);
            ViewData["CustomUserId"] = new SelectList(_context.Users, "Id", "Id", itemInCart.CustomUserId);
            return View(itemInCart);
        }

        // POST: Carts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ItemId,Name,Description,Price,IsSold,ImageData,ContentType,CategoryId,CustomUserId,Quantity")] Cart cart)
        {
            if (id != cart.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    cart.IsSold = false;
                    _context.Update(cart);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CartExists(cart.Id))
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
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Id", cart.CategoryId);
            ViewData["CustomUserId"] = new SelectList(_context.Users, "Id", "Id", cart.CustomUserId);
            return RedirectToAction(nameof(Index));
        }

        // GET: Carts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cart = await _context.Cart
                .Include(i => i.Category)
                .Include(i => i.CustomUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cart == null)
            {
                return NotFound();
            }

            return View(cart);
        }

        // POST: Carts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cart = await _context.Cart.FindAsync(id);
            _context.Cart.Remove(cart);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CartExists(int id)
        {
            return _context.Cart.Any(e => e.Id == id);
        }
    }
}
