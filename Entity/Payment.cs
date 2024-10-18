using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class Payment
    {
        public Payment()
        {
            SplitProduct = new List<Product>();
            OrderedProducts = new List<Product>();
        }
        public System.Guid PaymentGUID { get; set; }
        public System.Guid OrderGUID { get; set; }
        public Nullable<decimal> Amount { get; set; }
        public System.DateTime DateCreated { get; set; }
        public string PaymentMethod { get; set; }
        public string Token { get; set; }

        public string OrderNo { get; set; }
        public int TableId { get; set; }
        public List<Product> SplitProduct { get; set; }
        public decimal TipAmount { get; set; }
       

        public string ClientSecret { get; set; }
        public string FailureMessage { get; set; }

        public List<Product> OrderedProducts { get; set; }

        public bool isSplitPayment { get; set; }

        public string TxCode { get; set; }

        public string DeviceType { get; set; }
        public string PaymentId { get; set; }
        public int CustomerPoints { get; set; }

        public int CustomerId { get; set; }

        public decimal ServiceCharge { get; set; }

        public string Mobile { get; set; }
        public bool TablePaid { get; set; }

        public string FullName { get; set; }

        public string TransactionID { get; set; }

        public bool IsSuccess { get; set; }

    }
}