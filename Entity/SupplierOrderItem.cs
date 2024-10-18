using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class SupplierOrderItem
    {
        public int SupplierOrderItemId { get; set; }
        public int PurchaseOrderNumberFK { get; set; }
        public int ProductFK { get; set; }
        public string ProductName { get; set; }
        public decimal Quantity { get; set; }
        public decimal Received { get; set; }
        public bool Cancelled { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime LastModified { get; set; }
        public int CurrentReceive { get; set; }
        public bool PoorQuality { get; set; }
        public bool QuantityUnder { get; set; }
        public bool QuantityOver { get; set; }
        public bool TempertaureIssue { get; set; }
        public bool ExpirationIssue { get; set; }
        public bool Return { get; set; }
       public bool Checked { get; set; }
        public decimal SupplierAmount { get; set; }
    }
}