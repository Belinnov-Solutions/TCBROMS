using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class OrderPart
    {
        public int OrderPartID { get; set; }
        public int ProductID { get; set; }
        public string Name { get; set; }
        public Nullable<short> Qty { get; set; }
        public Nullable<decimal> Price { get; set; }
        public Nullable<decimal> Total { get; set; }
        public bool DelInd { get; set; }
        public System.Guid OrderGUID { get; set; }
        public int UserID { get; set; }
        public System.DateTime LastModified { get; set; }
        public Nullable<int> OldOrderPartID { get; set; }
        public Nullable<System.DateTime> DateCreated { get; set; }
        public Nullable<System.DateTime> DateServed { get; set; }
        public Nullable<int> OrderNo { get; set; }
        public Nullable<bool> WebUpload { get; set; }
        public Nullable<bool> Priority { get; set; }

        public int AlacarteQty { get; set; }
        public int BatchNo { get; set; }
        public string BatchTime { get; set; }
        public bool Processed { get; set; }
        public int MenuId { get; set; }
        public string ChineseName { get; set; }
        public string TableNumber { get; set; }

      
    }
}