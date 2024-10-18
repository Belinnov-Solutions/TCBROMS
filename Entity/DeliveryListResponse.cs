using Entity.Hushi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class DeliveryListResponse
    {
        public DeliveryListResponse()
        {
            ProductList = new List<PurchaseProduct>();
        }
        public Restaurant Restaurant { get; set; }
        public List<PurchaseProduct> ProductList { get; set; }
        public int OrderDeliveryId { get; set; }
        public string ReceiverSignature { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiveDate { get; set; }
        public int UserId { get; set; }
    }
}