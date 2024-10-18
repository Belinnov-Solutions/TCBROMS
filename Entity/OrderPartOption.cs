using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class OrderPartOption
    {
        public int OrderPartOptionID { get; set; }
        public int OrderPartId { get; set; }
        public int ProductOptionID { get; set; }
        public Nullable<decimal> Price { get; set; }
        public bool DelInd { get; set; }
        public System.Guid OrderGUID { get; set; }
        public int UserID { get; set; }
        public System.DateTime LastModified { get; set; }
        public string Name { get; set; }
    }
}