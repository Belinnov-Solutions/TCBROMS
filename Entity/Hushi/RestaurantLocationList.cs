using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.Hushi
{
    public class RestaurantLocationList
    {
        public RestaurantLocationList()
        {
            LocationList = new List<StorageLocationProductList>();
        }
        public Restaurant Restaurant { get; set; }
        public List<StorageLocationProductList> LocationList { get; set; }
    }
}