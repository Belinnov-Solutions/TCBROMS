using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.Models
{
    public class ProductGroupModel
    {
        public ProductGroupModel()
        {
            GroupProducts = new List<Product>();
            ChildGroups = new List<ProductGroup>();
        }
        public ProductGroup Group { get; set; }
        public List<Product> GroupProducts { get; set; }

        public List<ProductGroup> ChildGroups { get; set; }

    }
}