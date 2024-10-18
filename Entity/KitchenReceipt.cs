using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class KitchenReceipt
    {
        public string Receipt { get; set; }
        public DateTime? DatePrinted { get; set; }
        public DateTime DateCreated { get; set; }
    }
}