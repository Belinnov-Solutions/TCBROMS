using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.Models
{
    public class ReservationSlotModel
    {
        public ReservationSlotModel()
        {
            ReservationSlots = new List<ReservationSlot>();
        }
        public bool SlotAvailable { get; set; }
        public List<ReservationSlot> ReservationSlots { get; set; }
        public string Message { get; set; }
    }
}