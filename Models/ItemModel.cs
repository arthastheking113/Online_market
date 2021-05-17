using Online_market.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Online_market.Models
{
    public class ItemModel
    {
        public string name { get; set; }

        public string price { get; set; }

        public string image { get; set; }

        public int quantity { get; set; }
        public string listPrice { get; set; }
        public string totalPrice { get; set; }

        public string totalItem { get; set; }

        public string totalCartPrice { get; set; }

        public string javascriptCode { get; set; }
    }
}
