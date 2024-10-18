using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    
    public class SupplierOrder
    {
        public int SupplierOrderId { get; set; }
        public int PurchaseOrderNumber { get; set; }
        public string Supplier { get; set; }
        public DateTime OrderDate { get; set; }
        public bool Completed { get; set; }
        public DateTime DateCreated { get; set; }
        public string Notes { get; set; }
        public List<SupplierOrderItem> SupplierOrderItems { get; set; }
        public string ChillTemperature { get; set; }
        public string FrozenTemperature { get; set; }
        public string Condition { get; set; }
        public string Quantity { get; set; }
        public string AuthorizerId { get; set; }
        public string SupplierSign { get; set; }
        public string AuthorizerSign { get; set; }
        public string UserId { get; set; }
        public bool SupportRequired { get; set; }
        public List<string> Images { get; set; }
        public string Authorizer { get; set; }
        public bool Saved { get; set; }
        public int StaffUserCode { get; set; }
        public int ManagerUserCode { get; set; }

        public decimal SupplierTotalAmount { get; set; }

    }
}