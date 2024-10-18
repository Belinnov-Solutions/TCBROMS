using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.Models
{
    public class VoucherModel
    {
        public VoucherModel()
        {
            DiscountedProducts = new List<Product>();
        }
        public string Description { get; set; }
        public string Restaurant { get; set; }
        public decimal Amount { get; set; }
        public bool Used { get; set; }
        public List<Product> DiscountedProducts { get; set; }
        public bool IsValid { get; set; }
        public string VoucherStatus { get; set; }
        public bool VariableAmount { get; set; }
        public Guid OrderId { get; set; }
        public int NoOfGuests { get; set; }
        public string VoucherCode { get; set; }

        public string Message { get; set; }

        public string VoucherValue { get; set; }

        public string VoucherType { get; set; }
        public string ForeName { get; set; }
        public string SurName { get; set; }
        public string Mobile { get; set; }
        public decimal DiscountAmount { get; set; }
        public string PromoCode { get; set; }
        public DateTime BdayDate { get; set; }
        public int ProductId { get; set; }
        public decimal Price { get; set; }
        public string ProductName { get; set; }

        public int UserId { get; set; }


    }
}