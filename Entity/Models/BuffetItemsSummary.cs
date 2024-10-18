using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.Models
{
    public class BuffetItemsSummary
    {
        public BuffetItemsSummary()
        {
            PlacedOrders = new List<Product>();
            PrintedOrders = new List<Product>();
        }
        public List<Product> PlacedOrders { get; set; }
        public List<Product> PrintedOrders { get; set; }
        public HeadCounts HeadCount { get; set; }
    }
}