using Entity.Hushi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class Restaurant
    {
        public Restaurant()
        {
            OrderList = new List<ProductOrder>();
        }
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; }
        public int Qty { get; set; }
        public Guid OrderGUID { get; set; }
        public string PickDate { get; set; }
        public int ProductId { get; set; }
        public List<ProductOrder> OrderList { get; set; }
    }
}