using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class TakeAwayOrder
    {
        public int iTakeAwayId { get; set; }
        public int iRestaurantID { get; set; }
        public string vCustomerName { get; set; }
        public string vHouseNumber { get; set; }
        public string vFullAddress { get; set; }
        public string vPostCode { get; set; }
        public Nullable<int> CollectionTime { get; set; }
        public string vCustomerPhone { get; set; }
        public string vCustomerEmail { get; set; }
        public Nullable<double> fOrderTotal { get; set; }
        public bool bHasPrinted { get; set; }
        public bool bHasBeenCollected { get; set; }
        public Nullable<bool> bReservation { get; set; }
        public System.DateTime dtDateCreated { get; set; }
        public bool delInd { get; set; }
        public Nullable<System.DateTime> dtLastModified { get; set; }
        public bool bDownLoaded { get; set; }
        public Nullable<bool> bTakeAway { get; set; }
        public Nullable<bool> bDelivery { get; set; }
        public string vLatitude { get; set; }
        public string vLongitude { get; set; }
        public Nullable<System.Guid> OrderGUID { get; set; }
        public string OrderNumber { get; set; }
        public Nullable<bool> bPaid { get; set; }
        public Nullable<bool> Confirmed { get; set; }
        public string PaymentType { get; set; }
        public decimal DeliveryCharges { get; set; }

        public string VendorTxCode { get; set; }

        public bool Delivered { get; set; }

        public string Message { get; set; }

        public string Restaurant { get; set; }

        public bool bDelivered { get; set; }

        public Nullable<decimal> Amount { get; set; }
    }
}