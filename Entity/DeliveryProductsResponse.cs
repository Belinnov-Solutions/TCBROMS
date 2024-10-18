using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class DeliveryProductsResponse
    {
        public DeliveryProductsResponse()
        {
            orderProductsDeliveries = new List<OrderProductsDelivery>();
        }
        public List<OrderProductsDelivery> orderProductsDeliveries { get; set; }

    }
}