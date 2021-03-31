using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Online_market.Data;
using Online_market.Models;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Online_market.Controllers
{
    [Route("create-checkout-session")]
    [ApiController]
    public class PaymentIntentApiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<CustomUser> _userManger;

        public PaymentIntentApiController(ApplicationDbContext context, UserManager<CustomUser> userManger)
        {
            _context = context;
            _userManger = userManger;
        }
        [HttpPost]
        public async Task<ActionResult> CreateAsync()
        {
            string host = "https://" + HttpContext.Request.Host.ToString() + "/";
            var CartList = await _context.Cart.Where(i => i.CustomUserId == _userManger.GetUserId(User) && !i.IsSold).Include(i => i.Category).Include(i => i.CustomUser).ToListAsync();
            foreach (var items in CartList)
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
  
            decimal total = 0;
            decimal TotalItem = 0;
            foreach (var items in CartList)
            {
                total += (Convert.ToDecimal(items.Price) * (decimal)items.Quantity);
                TotalItem += (decimal)items.Quantity;
            }

            total += (decimal)15;
            decimal faxFee = total * (decimal)7 / (decimal)100;
            decimal TotalGrand = total + faxFee;
            TotalGrand = TotalGrand * 100;
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                  "card",
                },
                LineItems = new List<SessionLineItemOptions>
                {
                  new SessionLineItemOptions
                  {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                      UnitAmount = (long)TotalGrand,
                      Currency = "usd",
                      ProductData = new SessionLineItemPriceDataProductDataOptions
                      {
                        Name = "Lan's market",
                      },
                    },
                    Quantity = 1,
                  },
                },
                Mode = "payment",
                SuccessUrl = host + "Home/success",
                CancelUrl = host + "Carts",
            };
            var service = new SessionService();
            Session session = service.Create(options);
            return Json(new { id = session.Id });
        }
        public IActionResult Success()
        {
            return View();
        }

    }
}
