using Online_market.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Online_market.Services
{
    public interface ICreateTemporaryUser
    {
        public Task CreateTemporaryUserAsync();
    }
}
