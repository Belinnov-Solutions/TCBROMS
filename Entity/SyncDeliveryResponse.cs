using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class SyncDeliveryResponse
    {
        public SyncDeliveryResponse()
        {
            DeliveredList = new List<DeliveryListResponse>();
            DeliveryList = new List<DeliveryListResponse>();
        }
        public List<DeliveryListResponse> DeliveredList { get; set; }
        public List<DeliveryListResponse> DeliveryList { get; set; }
        public int UserId { get; set; }
        public string response { get; set; }

    }
}