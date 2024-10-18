using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.Models
{
    public class OrderMenuItemsResponse
    {
        public OrderMenuItemsResponse()
        {
            MenuItems = new List<List<OrderPart>>();
        }
        public string OrderTime { get; set; }
        public int BatchNo { get; set; }
        public bool Processed { get; set; }
        public List<List<OrderPart>> MenuItems { get; set; }

        public string TableNumber { get; set; }
    }
}