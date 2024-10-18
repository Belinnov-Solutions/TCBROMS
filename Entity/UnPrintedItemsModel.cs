using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class UnPrintedItemsModel
    {

        public UnPrintedItemsModel()
        {
            this.UnPrintedItems = new List<OrderBuffetItem>();
        }
        public List<OrderBuffetItem> UnPrintedItems { get; set; }
    }
}