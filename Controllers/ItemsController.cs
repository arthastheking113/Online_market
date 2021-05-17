using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Online_market.Data;
using Online_market.Models;
using Online_market.Services;

namespace Online_market.Controllers
{
    public class ItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;
        private readonly UserManager<CustomUser> _userManger;
        private readonly SignInManager<CustomUser> _signInManager;
        private readonly ISlugService _slugService;
        private readonly IUserDetector _userDetector;
        private readonly ICreateTemporaryUser _createTemporaryUser;
        private readonly ILogger<ItemsController> _logger;

        public ItemsController(ApplicationDbContext context,
            IImageService imageService,
            UserManager<CustomUser> userManger,
            SignInManager<CustomUser> signInManager,
            ISlugService slugService,
            IUserDetector userDetector,
            ICreateTemporaryUser createTemporaryUser,
            ILogger<ItemsController> logger)
        {
            _context = context;
            _imageService = imageService;
            _userManger = userManger;
            _signInManager = signInManager;
            _slugService = slugService;
            _userDetector = userDetector;
            _createTemporaryUser = createTemporaryUser;
            _logger = logger;
        }
        [Authorize(Roles = "Administrator")]
        // GET: Items
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = await _context.Item
                .Include(i => i.Category)
                .Include(i => i.ItemStatus)
                .Include(i => i.ItemSaleOff)
                .Include(i => i.Rates).ToListAsync();
      
            return View( applicationDbContext);
        }
        public async Task<IActionResult> AllProduct()
        {
            var featureItemList = await _context.Item.Where(i => i.IsProductReady)
                .Include(i => i.Category)
                .Include(i => i.ItemStatus)
                .Include(i => i.ItemSaleOff)
                .Include(i => i.Rates)
                .Include(i => i.Attachments).OrderByDescending(i => i.Number_Of_Sold).ToListAsync();
            string IpAdress = _userDetector.GetUserIpAdress();
            string connectionId = _userDetector.GetUserConnectionId();
            if (!_signInManager.IsSignedIn(User))
            {
                var user = _context.Users.FirstOrDefault(u => u.IpAdress == IpAdress && u.ConnectionId == connectionId);
                if (user != null && (await _userManger.IsInRoleAsync(user, "TemporaryUser")))
                {
                    var result = await _signInManager.PasswordSignInAsync(user.Email, "Abc123!", true, lockoutOnFailure: false);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User logged in.");
                        return View(featureItemList);
                    }
                }
                else
                {
                    await _createTemporaryUser.CreateTemporaryUserAsync();
                    var tempUser = _context.Users.FirstOrDefault(u => u.IpAdress == IpAdress && u.ConnectionId == connectionId);
                    if (tempUser != null && (await _userManger.IsInRoleAsync(tempUser, "TemporaryUser")))
                    {
                        var result = await _signInManager.PasswordSignInAsync(tempUser.Email, "Abc123!", true, lockoutOnFailure: false);
                        if (result.Succeeded)
                        {
                            _logger.LogInformation("User logged in.");
                            return View(featureItemList);
                        }
                    }
                }
            }
            else
            {

                var user = await _userManger.GetUserAsync(User);
                if (user != null)
                {
                    if (user.IpAdress == null && user.ConnectionId == null)
                    {
                        user.ConnectionId = connectionId;
                        user.IpAdress = IpAdress;
                        _context.Update(user);
                        await _context.SaveChangesAsync();
                    }
                    return View(featureItemList);
                }
                await _signInManager.SignOutAsync();
                return RedirectToAction(nameof(AllProduct));
            }

            return View(featureItemList);
        }
        public async Task<IActionResult> DrinkAndSnack()
        {
            var category = _context.Category;
            var drinksId = category.FirstOrDefault(i => i.Name == "Drinks").Id;
            var snacksId = category.FirstOrDefault(i => i.Name == "Snacks").Id;
            var featureItemList = await _context.Item
                .Where(i => i.IsProductReady)
                .Where(i => i.CategoryId == drinksId || i.CategoryId == snacksId)
                .Include(i => i.Category)
                .Include(i => i.ItemStatus)
                .Include(i => i.ItemSaleOff)
                .Include(i => i.Comments)
                .Include(i => i.Attachments)
                .Include(i => i.Rates).ToListAsync();


            string IpAdress = _userDetector.GetUserIpAdress();
            string connectionId = _userDetector.GetUserConnectionId();
            if (!_signInManager.IsSignedIn(User))
            {
                var user = _context.Users.FirstOrDefault(u => u.IpAdress == IpAdress && u.ConnectionId == connectionId);
                if (user != null && (await _userManger.IsInRoleAsync(user, "TemporaryUser")))
                {
                    var result = await _signInManager.PasswordSignInAsync(user.Email, "Abc123!", true, lockoutOnFailure: false);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User logged in.");
                        return View(featureItemList);
                    }
                }
                else
                {
                    await _createTemporaryUser.CreateTemporaryUserAsync();
                    var tempUser = _context.Users.FirstOrDefault(u => u.IpAdress == IpAdress && u.ConnectionId == connectionId);
                    if (tempUser != null && (await _userManger.IsInRoleAsync(tempUser, "TemporaryUser")))
                    {
                        var result = await _signInManager.PasswordSignInAsync(tempUser.Email, "Abc123!", true, lockoutOnFailure: false);
                        if (result.Succeeded)
                        {
                            _logger.LogInformation("User logged in.");
                            return View(featureItemList);
                        }
                    }
                }
            }
            else
            {

                var user = await _userManger.GetUserAsync(User);
                if (user != null)
                {
                    if (user.IpAdress == null && user.ConnectionId == null)
                    {
                        user.ConnectionId = connectionId;
                        user.IpAdress = IpAdress;
                        _context.Update(user);
                        await _context.SaveChangesAsync();
                    }
                    return View(featureItemList);
                }
                await _signInManager.SignOutAsync();
                return RedirectToAction(nameof(DrinkAndSnack));
            }

            return View(featureItemList);
        }

        public JsonResult AddItemToCard()
        {
            var result = new ItemModel();
           
            return Json(result);
        }


        // GET: Items/Details/5
        public async Task<IActionResult> Details(string slug)
        {

            var item = await _context.Item
                .Include(i => i.Category)
                .Include(i => i.ItemStatus)
                .Include(i => i.ItemSaleOff)
                .Include(i => i.Comments)
                .Include(i => i.Attachments)
                .Include(i => i.Rates)
                .FirstOrDefaultAsync(m => m.Slug == slug);
            if (item == null)
            {
                return NotFound();
            }
            item.ViewCount += 1;
            _context.Update(item);
            await _context.SaveChangesAsync();
            ViewData["AttachmentType"] = new SelectList(_context.ItemAttachmentTypes, "Id", "Name");
            ViewData["UserId"] = _userManger.GetUserId(User);
            string IpAdress = _userDetector.GetUserIpAdress();
            string connectionId = _userDetector.GetUserConnectionId();
            if (!_signInManager.IsSignedIn(User))
            {
                var user = _context.Users.FirstOrDefault(u => u.IpAdress == IpAdress && u.ConnectionId == connectionId);
                if (user != null && (await _userManger.IsInRoleAsync(user, "TemporaryUser")))
                {
                    var result = await _signInManager.PasswordSignInAsync(user.Email, "Abc123!", true, lockoutOnFailure: false);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User logged in.");
                        return View(item);
                    }
                }
                else
                {
                    await _createTemporaryUser.CreateTemporaryUserAsync();
                    var tempUser = _context.Users.FirstOrDefault(u => u.IpAdress == IpAdress && u.ConnectionId == connectionId);
                    if (tempUser != null && (await _userManger.IsInRoleAsync(tempUser, "TemporaryUser")))
                    {
                        var result = await _signInManager.PasswordSignInAsync(tempUser.Email, "Abc123!", true, lockoutOnFailure: false);
                        if (result.Succeeded)
                        {
                            _logger.LogInformation("User logged in.");
                            return View(item);
                        }
                    }
                }
            }
            else
            {

                var user = await _userManger.GetUserAsync(User);
                if (user != null)
                {
                    if (user.IpAdress == null && user.ConnectionId == null)
                    {
                        user.ConnectionId = connectionId;
                        user.IpAdress = IpAdress;
                        _context.Update(user);
                        await _context.SaveChangesAsync();
                    }
                    return View(item);
                }
                await _signInManager.SignOutAsync();
                return RedirectToAction("Details", "Items", new { slug = slug});
            }



            return View(item);
        }

        [HttpPost]
        public async Task<IActionResult> SearchAsync(string search)
        {
            var item = await _context.Item.Include(i => i.Category)
                .Include(i => i.ItemSaleOff)
                .Include(i => i.ItemStatus)
                .Include(i => i.Rates)
                .Include(i => i.Attachments)
                .Include(i => i.Comments)
                .Where(i => i.IsProductReady)
                .Where(i => 
                i.Name.ToLower().Contains(search) ||
                i.Description.ToLower().Contains(search) ||
                i.Content.ToLower().Contains(search) ||
                i.Category.Name.ToLower().Contains(search) ||
                i.Category.Description.ToLower().Contains(search)).ToListAsync();
            return View(item);
        }


        // GET: Items/Create
        [Authorize(Roles = "Administrator")]
        public IActionResult Create()
        {
            ViewData["RateId"] = new SelectList(_context.Rate, "Id", "Name");
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Name");
            ViewData["ItemSaleOffId"] = new SelectList(_context.ItemSaleOff, "Id", "Name");
            ViewData["ItemStatusId"] = new SelectList(_context.ItemStatus, "Id", "Name");
            return View();
        }

        // POST: Items/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Content,Price,ImageData,ContentType,IsProductReady,RateValue,CategoryId,ItemSaleOffId,ItemStatusId,Slug")] Item item, IFormFile image)
        {
            if (ModelState.IsValid)
            {
                item.ContentType = _imageService.RecordContentType(image);
                item.ImageData = await _imageService.EncodeFileAsync(image);
                item.RateValue = 5;

                var slug = _slugService.URLFriendly(item.Name);
                if (_slugService.IsUnique(_context, slug))
                {
                    item.Slug = slug;
                }
                else
                {
                    ModelState.AddModelError("Name", "This title cannot be used as it results in a duplicate Slug!");
                    ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Name", item.CategoryId);
                    return View(item);
                }
                _context.Add(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Name", item.CategoryId);
            ViewData["ItemSaleOffId"] = new SelectList(_context.ItemSaleOff, "Id", "Name", item.ItemSaleOffId);
            ViewData["ItemStatusId"] = new SelectList(_context.ItemStatus, "Id", "Name", item.ItemStatusId);
            return View(item);
        }

        // GET: Items/Edit/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Item.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Name", item.CategoryId);
            ViewData["ItemSaleOffId"] = new SelectList(_context.ItemSaleOff, "Id", "Name", item.ItemSaleOffId);
            ViewData["ItemStatusId"] = new SelectList(_context.ItemStatus, "Id", "Name", item.ItemStatusId);
            return View(item);
        }

        // POST: Items/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Content,Price,ImageData,IsProductReady,ContentType,RateValue,CategoryId,ItemSaleOffId,ItemStatusId,Slug")] Item item, IFormFile image, byte[] imageData, string contentType)
        {
            if (id != item.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (image != null)
                    {
                        item.ContentType = _imageService.RecordContentType(image);
                        item.ImageData = await _imageService.EncodeFileAsync(image);
                    }
                    else
                    {
                        item.ContentType = contentType;
                        item.ImageData = imageData;
                    }

                    var slug = _slugService.URLFriendly(item.Name);
                   

                    if (slug != item.Slug)
                    {
                        if (_slugService.IsUnique(_context, slug))
                        {
                            item.Slug = slug;
                        }
                        else
                        {
                            ModelState.AddModelError("Name", "This title cannot be used as it results in a duplicate Slug!");
                            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Name", item.CategoryId);
                            ViewData["ItemSaleOffId"] = new SelectList(_context.ItemSaleOff, "Id", "Name", item.ItemSaleOffId);
                            ViewData["ItemStatusId"] = new SelectList(_context.ItemStatus, "Id", "Name", item.ItemStatusId);
                            return View(item);
                        }
                    }
                    _context.Update(item);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemExists(item.Id))
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
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Name", item.CategoryId);
            ViewData["ItemSaleOffId"] = new SelectList(_context.ItemSaleOff, "Id", "Name", item.ItemSaleOffId);
            ViewData["ItemStatusId"] = new SelectList(_context.ItemStatus, "Id", "Name", item.ItemStatusId);
            return View(item);
        }

        // GET: Items/Delete/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Item
                .Include(i => i.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // POST: Items/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.Item.FindAsync(id);
            _context.Item.Remove(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ItemExists(int id)
        {
            return _context.Item.Any(e => e.Id == id);
        }
    }
}
