using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class BuffetBox
    {
        public BuffetBox()
        {
            BoxItems = new List<OrderPartOption>();
        }
        public int ProductId { get; set; }
        public int OrderPartId { get; set; }
        public string Name { get; set; }
        public int Qty { get; set; }
        public Nullable<int> ProductTypeId { get; set; }
        public string ProductType { get; set; }
        public List<OrderPartOption> BoxItems {get;set;}
        public decimal Price { get; set; }

        public string ChineseName { get; set; }
    }
}