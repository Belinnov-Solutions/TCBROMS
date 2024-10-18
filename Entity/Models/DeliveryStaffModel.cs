using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.Models
{
    public class DeliveryStaffModel
    {
        public DeliveryStaffModel()
        {
            DeliveryStaff = new List<AvailableDeliveryStaff>();
        }
        public List<AvailableDeliveryStaff> DeliveryStaff { get; set; }

    }
}