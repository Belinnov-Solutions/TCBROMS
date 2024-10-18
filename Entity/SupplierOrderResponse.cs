using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class SupplierOrderResponse
    {
        public SupplierOrderResponse()
        {
            supplierOrders = new List<SupplierOrder>();
        }
        public IEnumerable<SupplierOrder> supplierOrders { get; set; }
        public string message { get; set; }
    }
}