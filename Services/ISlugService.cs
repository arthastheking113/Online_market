using Online_market.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Online_market.Services
{
    public interface ISlugService
    {
        string URLFriendly(string title);

        bool IsUnique(ApplicationDbContext dbContext, string slug);
    }
}
