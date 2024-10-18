using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class OrderedProductsLocation
    {
        public OrderedProductsLocation()
        {
            LocationList = new List<Product>();
        }
        public ProductLocation ProductLocation { get; set; }
        public List<Product> LocationList { get; set; }
    }
}