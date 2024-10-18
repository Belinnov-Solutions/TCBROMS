using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class KitchenOrderItems
    {
        public KitchenOrderItems()
        {
            Boxes = new List<BuffetBox>();
            OrderedItems = new List<OrderPart>();
            ProductSummary = new List<OrderPart>();
        }
        public List<BuffetBox> Boxes { get; set; }
        public List<OrderPart> OrderedItems { get; set; }
        public List<OrderPart> ProductSummary { get; set; }
    }
}