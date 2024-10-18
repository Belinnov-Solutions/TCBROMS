using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.Models
{
    public class ReOrderModel
    {
        public ReOrderModel()
        {
            this.BuffetItems = new List<OrderBuffetItem>();
        }
        public List<OrderBuffetItem> BuffetItems { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public Guid OrderGUID { get; set; }
        public int TableId { get; set; }
    }
}