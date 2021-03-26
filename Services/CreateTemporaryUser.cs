using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Online_market.Data;
using Online_market.Models;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Online_market.Services
{
    public class CreateTemporaryUser : ICreateTemporaryUser
    {
        private readonly UserManager<CustomUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IUserDetector _userDetector;
        private readonly ILogger<CreateTemporaryUser> _logger;
        private readonly IActionContextAccessor _actionContextAccessor;

        public CreateTemporaryUser(UserManager<CustomUser> userManager, 
            ApplicationDbContext context, 
            IUserDetector userDetector, 
            ILogger<CreateTemporaryUser> logger,
            IActionContextAccessor actionContextAccessor)
        {
            _userManager = userManager;
            _context = context;
            _userDetector = userDetector;
            _logger = logger;
            _actionContextAccessor = actionContextAccessor;
        }
        public async Task CreateTemporaryUserAsync()
        {
            string IpAdress = _userDetector.GetUserIpAdress();
            string connectionId = _userDetector.GetUserConnectionId();
            if (_context.Users.FirstOrDefault(u => u.IpAdress == IpAdress && u.ConnectionId == connectionId) == null)
            {
                var NumberOfUser = _context.Users.ToList().Count() + 1;
                var tempEmail = "tempUser" + NumberOfUser.ToString() + "@lanonlinemarket.com";
                var userName = tempEmail;

                var user = new CustomUser
                {
                    UserName = userName,
                    Email = tempEmail,
                    FirstName = "User",
                    LastName = $"#{NumberOfUser}",
                    IpAdress = IpAdress,
                    ConnectionId = connectionId,
                    ImageData = null,
                    ContentType = null,
                    Street = null,
                    State = null,
                    City = null,
                    Zipcode = null,
                    EmailConfirmed = true
                };
                var result = await _userManager.CreateAsync(user, "Abc123!");
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");
                    await _userManager.AddToRoleAsync(user, "TemporaryUser");
                }
            }
      
        }
    }
}
