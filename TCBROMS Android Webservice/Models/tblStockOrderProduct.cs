//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TCBROMS_Android_Webservice.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class tblStockOrderProduct
    {
        public int StockOrderProductID { get; set; }
        public int PurchaseOrderNumberFK { get; set; }
        public int ProductFK { get; set; }
        public decimal Quantity { get; set; }
        public bool delInd { get; set; }
        public Nullable<decimal> UnitPrice { get; set; }
        public Nullable<decimal> ItemTotal { get; set; }
        public Nullable<decimal> Received { get; set; }
        public bool Cancelled { get; set; }
        public bool WebUpload { get; set; }
        public Nullable<decimal> MaxQuantity { get; set; }
        public Nullable<bool> Counted { get; set; }
        public Nullable<int> StockQuantity { get; set; }
    
        public virtual tblStockOrder tblStockOrder { get; set; }
    }
}
