using Entity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class AllProducts
    {
        public AllProducts()
        {
            BuffetMenu = new List<ProductGroupModel>();
        }
       public List<ProductGroup> pGroupList { get; set; }
        public List<Product> productsList { get; set; }

        public List<ProductLinker> productsLinker { get; set; }

        public List<ProductGroupModel> BuffetMenu { get; set; }

    }
}