using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class ReservationSlot
    {
        public string Time { get; set; }
        public int NoOfSpaces { get; set; }
        public int MaxReservationsAtThisTime { get; set; }
    }
}