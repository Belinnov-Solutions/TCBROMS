using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class AppParameters
    {
        public string DisclaimerUrl { get; set; }
        public string DisclaimerString { get; set; }

        public string SKey { get; set; }
        public ServiceCharge ServiceCharge { get; set; }
        public decimal ThresholdPayableAmount { get; set; }
        public string ThresholdAmtMessage { get; set; } 
        public int ReOrderThresholdTime { get; set; }
        public string ReOrderThresholdTimeMessage { get; set; }
        public string InsufficientPointsMessage { get; set; }

        public string SageURL { get; set; }
        public string PaymentGateway { get; set; }
        public bool ShowEditSCTab { get; set; }
        public string StripeCheckOut { get; set; }


    }
}