using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.Hushi
{
    public class ProductRestaurantList
    {
        public ProductRestaurantList()
        {
            RestaurantList = new List<Restaurant>();
            Product = new PurchaseProduct();
        }
        public PurchaseProduct Product { get; set; }
        public List<Restaurant> RestaurantList { get; set; }
    }
}