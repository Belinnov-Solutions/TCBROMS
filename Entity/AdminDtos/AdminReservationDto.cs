using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.AdminDtos
{
    public class AdminReservationDto
    {
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; }
        public string IpAddress { get; set; }
        public string PortAddress { get; set; }
        public string SlotSelected { get; set; }
        public int NoOfguest { get; set; }
        public int NoOfHighChairs { get; set; }
        public int NoOfWheelChairs { get; set; }
        public int NoOfParamSeats { get; set; }
        public DateTime? ReservationDate { get; set; }
        public string RDate { get; set; }

        //tblTakeAwayOrders and Items
        public string CustomerName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string PostalCode { get; set; }
        public DateTime BookingTime { get; set; }
        public string MobileNumber { get; set; }
        public string Email { get; set; }
        public decimal Booking { get; set; }
        public string DateOfBirth { get; set; }
        public string City { get; set; }
        //tblreservation and name
        public int ReservationId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string AdditionalNotes { get; set; }
        public int Seats { get; set; }
        public int Deposit { get; set; }
        public string VendorTxCode { get; set; }
        public string ContactNumber { get; set; }
        //tblPayment
        public int PaymentOrderId { get; set; }
        public string VPSTxId { get; set; }
        public float price { get; set; }
        public bool isSuccess { get; set; }
        public string faultReason { get; set; }
        public int PaymentType { get; set; }
        public string SecurityKey { get; set; }

        //static value variables
        public decimal PerguestDeposit { get; set; }
        public int CodeLength { get; set; }
        //return Only
        public decimal BookingDeposit { get; set; }
        public string UniqueCode { get; set; }
        public int OrderId { get; set; }

        public string PaymentUrl { get; set; }

        public bool isWebsite { get; set; }

        public int CustomerId { get; set; }

        public string Message { get; set; }
    }
}