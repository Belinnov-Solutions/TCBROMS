using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class AvailableDeliveryStaff
    {
        public int Id { get; set; }
        public int DeliveryStaffId { get; set; }
        public bool? Available { get; set; }
        public Nullable<System.DateTime> NextAvailableTime { get; set; }
        public int MaxOrders { get; set; }
        public string Name { get; set; }
        public string Mobile { get; set; }

        public decimal AmountDue { get; set; }
        public int TotalOrders { get; set; }
        public bool? OutForDelivery { get; set; }
        public string CollectedAt { get; set; }
        public string DeliveredAt { get; set; }

    }
}