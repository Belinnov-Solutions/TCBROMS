using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class ProductLocation
    {
        public ProductLocation()
        {
            ProductList = new List<Product>();
        }
        public string LocationName { get; set; }
        public int Qty { get; set; }
        public List<Product>  ProductList {get; set;}
    }
}