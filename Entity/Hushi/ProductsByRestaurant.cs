using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.Hushi
{
    public class ProductsByRestaurant
    {
        public ProductsByRestaurant()
        {
            RestWiseList = new List<RestaurantLocationList>();
        }
        public List<RestaurantLocationList> RestWiseList { get; set; }
    }
}