using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class OrderedProductsResponse
    {
        public OrderedProductsResponse()
        {
            RestWiseList = new List<OrderedProductsRestaurant>();
        }
        public List<OrderedProductsRestaurant> RestWiseList { get; set; }
    }
}