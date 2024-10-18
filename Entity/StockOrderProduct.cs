using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class StockOrderProduct
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public int StockQuantity { get; set; }
        public int MaxQuantity { get; set; }
        public int OrderedQuantity { get; set; }

        public decimal Price { get; set; }

        public bool Counted { get; set; }

        public string Type { get; set; }
    }
}