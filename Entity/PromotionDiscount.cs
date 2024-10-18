using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class PromotionDiscount
    {
        public PromotionDiscount()
        {
            billableProducts = new List<Product>();
        }
        public List<Product> billableProducts { get; set; }
        public int custCount { get; set; }
    }
}