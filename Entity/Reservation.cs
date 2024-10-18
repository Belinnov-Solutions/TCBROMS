using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class Reservation
    {
        public int UserID { get; set; }
        public int ReservationID { get; set; }
        public String IncomingTime { get; set; }
        public String Mobile { get; set; }
        public String Name { get; set; }
        public String EmailID { get; set; }
        public String DOB { get; set; }
        public int WaitingTime { get; set; }
        public String Type { get; set; }
        public int AdCnt { get; set; }
        public int KdCnt { get; set; }
        public int JnCnt { get; set; }
        public int TotalSeats { get; set; }
        public int NoOfGuests { get; set; }
        public int HighChair { get; set; }
        public int WheelChair { get; set; }
        public int Prams { get; set; }
        public Boolean PrevCust { get; set; }
        public bool Processed { get; set; }
        public string AllocTable { get; set; }
        public decimal Amount { get; set; }
        public int ProductID { get; set; }
        public string ProductDescription { get; set; }
        public string ReservationType { get; set; }
        public string DateCreated { get; set; }

        public long CustWaitingTime { get; set; }
        public long ResTimePassed { get; set; }
        public long ResTimeLeft { get; set; }
        public string ReservedTables { get; set; }
        public string UniqueCode { get; set; }
        public string MobileNumber { get; set; }
        public DateTime? ReservationDate { get; set; }
        public string AdditionalNotes { get; set; }

        public bool InLounge { get; set; }

    }
}