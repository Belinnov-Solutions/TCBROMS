using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class RefillProduct
    {
        public int ProductRefillID { get; set; }
        public int ProductId { get; set; }

        public int? ProductCode { get; set; }
        public string ProductName { get; set; }

        public string ChineseName { get; set; }

        public string ProductSize { get; set; }

        public int ProductQty { get; set; }
    }
}