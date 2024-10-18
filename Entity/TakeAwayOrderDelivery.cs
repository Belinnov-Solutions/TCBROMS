using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class TakeAwayOrderDelivery
    {
        public int Id { get; set; }
        public int RestaurantId { get; set; }
        public int TakeAwayId { get; set; }
        public System.Guid OrderGUID { get; set; }
        public Nullable<int> DeliveryStaffId { get; set; }
        public System.DateTime DateCreated { get; set; }
        public Nullable<int> UserId { get; set; }
        public System.DateTime LastModified { get; set; }
        public Nullable<decimal> Amount { get; set; }
        public Nullable<System.DateTime> CollectedAt { get; set; }
        public Nullable<System.DateTime> DeliveredAt { get; set; }
        public bool DelInd { get; set; }

        public string OrderNo { get; set; }
    }
}