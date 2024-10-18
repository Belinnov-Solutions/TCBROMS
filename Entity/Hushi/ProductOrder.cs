using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.Hushi
{
    public class ProductOrder
    {
        public Guid OrderGUID { get; set; }
        public int Qty { get; set; }
    }
}