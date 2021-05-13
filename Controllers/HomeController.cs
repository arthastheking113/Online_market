using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Online_market.Data;
using Online_market.Models;
using Online_market.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Online_market.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IEmailSender _emailSender;
        private readonly SignInManager<CustomUser> _signInManager;
        private readonly UserManager<CustomUser> _userManager;
        private readonly ICreateTemporaryUser _createTemporaryUSer;
        private readonly IUserDetector _userDetector;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, 
            IEmailSender emailSender, 
            SignInManager<CustomUser> signInManager, 
            UserManager<CustomUser> userManager,
            ICreateTemporaryUser createTemporaryUSer,
            IUserDetector userDetector,
            ApplicationDbContext context)
        {
            _logger = logger;
            _emailSender = emailSender;
            _signInManager = signInManager;
            _userManager = userManager;
            _createTemporaryUSer = createTemporaryUSer;
            _userDetector = userDetector;
            _context = context;
        }

        public async Task<IActionResult> IndexAsync()
        
        {
            var featureItemList = _context.Item
                .Include(i => i.Attachments)
                .Include(i => i.Rates)
                .Include(i => i.ItemStatus)
                .Include(i => i.ItemSaleOff)
                .Include(i => i.Category).OrderByDescending(i => i.Number_Of_Sold).ToList();
            string IpAdress = _userDetector.GetUserIpAdress();
            string connectionId = _userDetector.GetUserConnectionId();
            if (!_signInManager.IsSignedIn(User))
            {
                var user = _context.Users.FirstOrDefault(u => u.IpAdress == IpAdress && u.ConnectionId == connectionId);
                if (user != null && (await _userManager.IsInRoleAsync(user, "TemporaryUser")))
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
                    await _createTemporaryUSer.CreateTemporaryUserAsync();
                    var tempUser = _context.Users.FirstOrDefault(u => u.IpAdress == IpAdress && u.ConnectionId == connectionId);
                    if (tempUser != null && (await _userManager.IsInRoleAsync(tempUser, "TemporaryUser")))
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

                var user = await _userManager.GetUserAsync(User);
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
                return RedirectToAction(nameof(Index));
            }

            return View(featureItemList);
        }


     

        public async Task<IActionResult> SuccessAsync()
        {
            Random rand = new Random();
            var RandomNumber = Enumerable.Range(0, 10)
                                         .Select(i => new Tuple<int, int>(rand.Next(10), i))
                                         .OrderBy(i => i.Item1)
                                         .Select(i => i.Item2);
            var TrackingNumber = String.Join("", string.Join(";", RandomNumber).Split('@', ',', '.', ';', '\''));


            var applicationDbContext = await _context.Orders.Include(o => o.Category).Include(o => o.CustomUser).Include(o => o.OrderStatus).Include(o => o.Item).OrderByDescending(o => o.Updated).ToListAsync();
            List<Order> orderList = new List<Order>();
            foreach (var item in applicationDbContext)
            {
                var trackingNumber = item.TrackingNumber;
                if (!orderList.Select(o => o.TrackingNumber).Contains(trackingNumber))
                {
                    orderList.Add(item);
                }
            }
            var currentTime = DateTimeOffset.Now;
            var StatusFinish = _context.OrderStatuses.FirstOrDefault(o => o.Name == "Finish").Id;
            var Order_Complete = await _context.Orders.Where(t => t.Updated >= currentTime.AddDays(-180) && t.OrderStatusId == StatusFinish).ToListAsync();
            foreach(var item2 in Order_Complete)
            {
                _context.Orders.Remove(item2);
                await _context.SaveChangesAsync();
            }

            while (_context.Orders.Where(i => i.TrackingNumber == TrackingNumber).ToList().Count > 0)
            {
                RandomNumber = Enumerable.Range(0, 10)
                                         .Select(i => new Tuple<int, int>(rand.Next(10), i))
                                         .OrderBy(i => i.Item1)
                                         .Select(i => i.Item2);
                TrackingNumber = String.Join("", string.Join(";", RandomNumber).Split('@', ',', '.', ';', '\''));
            }
            var CartList = await _context.Cart.Where(i => i.CustomUserId == _userManager.GetUserId(User) && !i.IsSold).Include(i => i.Category).Include(i => i.CustomUser).ToListAsync();
            foreach (var items in CartList)
            {
                //check if the price if change or not
                var saleOffId = _context.Item.FirstOrDefault(i => i.Id == items.ItemId).ItemSaleOffId;
                var price = _context.Item.FirstOrDefault(i => i.Id == items.ItemId).Price;
                var CurrentPrice = _context.Item.FirstOrDefault(i => i.Id == items.ItemId).ListPrice(_context, price, saleOffId);
                if (items.Price != CurrentPrice)
                {
                    items.Price = CurrentPrice;
                    _context.Update(items);
                    await _context.SaveChangesAsync();
                }
                // change sold to true to make it disapear in the cart
                items.IsSold = true;
                _context.Update(items);
                await _context.SaveChangesAsync();

                //update number of sold item
                var item_of_shop = _context.Item.FirstOrDefault(i => i.Id == items.ItemId);
                item_of_shop.Number_Of_Sold += 1;
                _context.Update(item_of_shop);
                await _context.SaveChangesAsync();
            }
            var ReceivedStatus = _context.OrderStatuses.FirstOrDefault(o => o.Name == "Received").Id;
            var user = await _userManager.FindByIdAsync(CartList.First().CustomUserId);
            foreach (var item in CartList)
            {
                Order newOrder = new Order
                {
                    TrackingNumber = TrackingNumber,
                    Name = item.Name,
                    Price = item.Price,
                    CustomUserId = item.CustomUserId,
                    CategoryId = item.CategoryId,
                    ItemId = item.ItemId,
                    Quantity = item.Quantity,
                    IsSold = true,
                    ImageData = item.ImageData,
                    ContentType = item.ContentType,
                    Date = DateTimeOffset.Now,
                    Notes = item.Notes,
                    Slug = item.Slug,
                    IsViewByOwner = false,
                    OrderStatusId = ReceivedStatus,
                    TrackingLink = "We are currently working on tracking link!",
                    Created = DateTimeOffset.Now,
                    Updated = DateTimeOffset.Now,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    Email = user.Email,
                    Adress = user.Street,
                    State = user.State,
                    City = user.City,
                    Zipcode = user.Zipcode
                };
                _context.Add(newOrder);
                await _context.SaveChangesAsync();
            }
  

            var callbackUrl = Url.Action(
                                    "TrackOrderDetails",
                                    "Orders",
                                    values: new { TrackingNumber = TrackingNumber },
                                    protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(user.Email, "Lan's Market received your order",
                $"<h1>You succescfully placed an order on Lan's Market at {(DateTimeOffset.Now).ToString("dd MMMM yyyy - hh:mm tt")}</h1> <br> <a style='background-color: #555555;border: none;color: white;padding: 15px 32px;text-align: center;text-decoration: none;display: inline-block;font-size: 16px;' href='{HtmlEncoder.Default.Encode(callbackUrl)}'>Clicking here to track your order</a>  <br> <h3>Your Tracking Number is: {TrackingNumber} </h3> <br>");


            await _emailSender.SendEmailAsync("arthastheking113@gmail.com", "Lan's Market received a new order",
               $"<h1>A new order have been placed at {(DateTimeOffset.Now).ToString("dd MMMM yyyy - hh:mm tt")}</h1> <br> <a style='background-color: #555555;border: none;color: white;padding: 15px 32px;text-align: center;text-decoration: none;display: inline-block;font-size: 16px;' href='{HtmlEncoder.Default.Encode(callbackUrl)}'>Clicking here to go to order</a>  <br> <h3>Tracking Number is: {TrackingNumber} </h3> <br>");


            ViewData["TrackingNumber"] = TrackingNumber;
            return View();
        }
        public IActionResult HowToContact()
        {
            return View();
        }

        public IActionResult ManageShop()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> SignInAsync(string returnUrl,string Email, string Password, bool RememberMe)
        {
            var ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                if ((await _userManager.FindByEmailAsync(Email)) != null)
                {
                    await _signInManager.SignOutAsync();
                    // This doesn't count login failures towards account lockout
                    // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                    var result = await _signInManager.PasswordSignInAsync(Email, Password, RememberMe, lockoutOnFailure: false);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User logged in.");
                        return RedirectToAction(nameof(Index));
                    }
                    if (result.RequiresTwoFactor)
                    {
                        return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = RememberMe });
                    }
                    if (result.IsLockedOut)
                    {
                        _logger.LogWarning("User account locked out.");
                        return RedirectToPage("./Lockout");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        return RedirectToAction(nameof(Index));
                    }
                }
               
            }
            // If we got this far, something failed, redisplay form
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> RegisterAsync(string returnUrl, string Email, string FirstName, string LastName, string Password)
        {
            var ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = new CustomUser { UserName = Email, Email = Email, FirstName = FirstName, LastName = LastName };
                var result = await _userManager.CreateAsync(user, Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToAction(nameof(Index));
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Contact(string Name, string Email, string Subject, string Message)
        {
            string myEmail = "arthastheking113@gmail.com";

            string subject = $"{Name} Just Send You A Message About {Subject}";

            string body = $"Message From {Email}: {Message}";

            string customberSubject = $"I Just Received A Message From Lan's Blog With Name {Name} About {Subject}";
            string customberBody = $"I Received Message From {Email} About: {Subject}. I will contact back to you as soon as possible.";

            await _emailSender.SendEmailAsync(myEmail, subject, body);

            await _emailSender.SendEmailAsync(Email, customberSubject, customberBody);

            ModelState.Clear();
            return View("SendMessageSuccess");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
