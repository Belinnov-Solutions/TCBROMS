using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.Hushi
{
    public class LocationProductList
    {
        public LocationProductList()
        {
            Location = new StorageLocation();
            ProductRestaurantList = new List<ProductRestaurantList>();
        }
        public StorageLocation Location { get; set; }
        public List<ProductRestaurantList> ProductRestaurantList { get; set; }
    }
}