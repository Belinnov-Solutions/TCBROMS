using Entity.Hushi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class OrderProductsDelivery
    {
        public OrderProductsDelivery()
        {
            ProductList = new List<PurchaseProduct>();
        }            
        public Restaurant Restaurant { get; set; }
        public List<PurchaseProduct> ProductList { get; set; }
    }
}