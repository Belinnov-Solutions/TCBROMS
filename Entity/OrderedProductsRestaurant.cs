using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class OrderedProductsRestaurant
    {
        public OrderedProductsRestaurant()
        {
            LocationList = new List<ProductLocation>();
        }
        public Restaurant Restaurant { get; set; }
        public List<ProductLocation> LocationList { get; set; }
    }
}