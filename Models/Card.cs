using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Online_market.Models
{
    public class Card
    {
        public int Id { get; set; }
        public string CardNumber { get; set; }
        public DateTime Date { get; set; }
        public int CVS { get; set; }

        public string CustomUserId { get; set; }
        public virtual CustomUser CustomUser { get; set; }
    }
}
