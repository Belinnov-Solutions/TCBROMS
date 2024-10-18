using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.Hushi
{
    public class StorageLocationProductList
    {
        public StorageLocationProductList()
        {
            ProductList = new List<PurchaseProduct>();
            Location = new StorageLocation();
        }
        public StorageLocation Location { get; set; }
        public List<PurchaseProduct> ProductList { get; set; }
    }
}
