using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class KitchenOrder
    {
        public KitchenOrder()
        {
            Boxes = new List<BuffetBox>();
            OrderedItems = new List<OrderPart>();
            
        }
        public Guid OrderGUID { get; set; }
        public string OrderNumber { get; set; }
        public string TableNumber { get; set; }
        public DateTime DateCreated { get; set; }
        public List<BuffetBox> Boxes { get; set; }
        public List<OrderPart> OrderedItems { get; set; }
        
        public int CollectionTime { get; set; }
        public int TimeRemaining { get; set; }
        public int Printed { get; set; }

    }
}