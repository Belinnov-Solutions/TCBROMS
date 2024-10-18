using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.Models
{
    public class ProductRedemptionResponse
    {
        public bool IsRedeemed { get; set; }
        public int CustomerPoints { get; set; }
        public string Message { get; set; }
        public bool Logout { get; set; }
    }
}