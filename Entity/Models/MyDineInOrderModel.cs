using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.Models
{
    public class MyDineInOrderModel
    {
        public MyDineInOrderModel()
        {
            MyOrder = new List<OrderBuffetItem>();
            TableOrder = new List<OrderBuffetItem>();
        }
        public List<OrderBuffetItem> MyOrder { get; set; }
        public List<OrderBuffetItem> TableOrder { get; set; }
    }
}