using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.Models
{
    public class CartModel
    {
        public CartModel()
        {
            OrderedItems = new List<Product>();
        }
        public Guid OrderGUID { get; set; }
        public List<Product> OrderedItems { get; set; }
        public float Total { get; set; }
        public float Discount { get; set; }
        public float GrandTotal { get; set; }
    }
}