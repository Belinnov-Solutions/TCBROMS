using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.Models
{
    public class OrderPrintModel
    {
        public OrderPrintModel()
        {
            OrderedProducts = new List<Product>();
        }
        public string TableNumber { get; set; }
        public Guid OrderGUID { get; set; }
        public int TotalItems { get; set; }
        public List<Product> OrderedProducts { get; set; }
    }
}