using Entity.Hushi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class ProductsSaveRequest
    {
        public ProductsSaveRequest()
        {
            PickedProducts = new List<PurchaseProduct>();
        }
        public List<PurchaseProduct> PickedProducts { get; set; }
        public string Signature { get; set; }
    }
}