using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
namespace Online_market.Services
{
    public class UserDetector : IUserDetector
    {
        private readonly IActionContextAccessor _actionContextAccessor;

        public UserDetector(
            IActionContextAccessor actionContextAccessor)
        {;
            _actionContextAccessor = actionContextAccessor;
        }
        public string GetUserIpAdress()
        {

            string ip = _actionContextAccessor.ActionContext.HttpContext.Connection.RemoteIpAddress.ToString();
            return ip;
        }

        public string GetUserConnectionId()
        {
            string ConnectionId = _actionContextAccessor.ActionContext.HttpContext.Connection.Id;
            return ConnectionId;
        }
    }
}
