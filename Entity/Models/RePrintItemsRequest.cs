using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.Models
{
    public class RePrintItemsRequest
    {
        //public RePrintItemsRequest()
        //{
        //    RePrintItems = new List<OrderBuffetItem>();
        //}
        public Guid OrderGUID { get; set; }
        public string TableNumber { get; set; }
        public int CustCount { get; set; }
        public string OrderTime { get; set; }
        public List<OrderBuffetItem> BuffetItems { get; set; }
    }
}