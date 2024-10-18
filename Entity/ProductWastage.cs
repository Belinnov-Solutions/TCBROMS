using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class ProductWastage
    {
        public int UserID { get; set; }
        public List<Product> ProductList { get; set; }
        public List<string> Images { get; set; }

        public ProductWastage()
        {
            this.ProductList = new List<Product>();
            this.Images = new List<string>();
        }
    }
}