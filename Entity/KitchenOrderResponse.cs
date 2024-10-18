using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class KitchenOrderResponse
    {
      public KitchenOrderResponse()
        {
            KitchenOrders = new List<KitchenOrder>();
            ProductSummary = new List<OrderPart>();
        }
        public int OrderCount { get; set; }
        public List<KitchenOrder> KitchenOrders { get; set; }
        public List<OrderPart> ProductSummary { get; set; }

    }
}