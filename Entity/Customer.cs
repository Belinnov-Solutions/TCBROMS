using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class Customer
    {
        public int CustomerID { get; set; }
       
        public int UserID { get; set; }
        public int ReservationID { get; set; }

        public String IncomingTime { get; set; }
        public String Mobile { get; set; }
        public String Name { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public String EmailID { get; set; }
        public String DOB { get; set; }
        public int WaitingTime { get; set; }
        public String Address { get; set; }

        public String Type { get; set; }
        public int AdCnt { get; set; }
        public int KdCnt { get; set; }
        public int JnCnt { get; set; }

        public int HighChair { get; set; }
        public int WheelChair { get; set; }
        public int Prams { get; set; }
        public Boolean PrevCust { get; set; }
        public int NoOfGuests { get; set; }

        public List<String> PrevOrdersDate { get; set; }

        public bool Processed { get; set; }

        public string AllocTable { get; set; }

        public bool DepositPaid { get; set; }

        public decimal DepositAmount { get; set; }

        public int ProductID { get; set; }
        public int TableID { get; set; }
        public int CustomerPoints { get; set; }

        public int OrderPoints { get; set; }
        public string Message { get; set; }
        public bool RegSuccess { get; set; }
        public int RestaurantId { get; set; }
        public Customer()
        {
            this.PrevOrdersDate = new List<string>();
        }
    }
}