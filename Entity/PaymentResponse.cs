using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class PaymentResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public int Points { get; set; }
        public Guid OrderGUID { get; set; }
        public string Mobile { get; set; }
    }
}