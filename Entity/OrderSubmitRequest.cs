using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class OrderSubmitRequest
    {
        public Guid OrderGUID { get; set; }
        public Table TableDetails { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public string UserType { get; set; }
        public int AdCount { get; set; }
        public int KdCount { get; set; }
        public int JnCount { get; set; }
        public int CustomerCount { get; set; }
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; }
        public List<OrderBuffetItem> BuffetItems { get; set; }
    }
}