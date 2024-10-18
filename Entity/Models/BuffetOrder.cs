using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.Models
{
    public class BuffetOrder
    {
        public BuffetOrder()
        {
            BuffetItems = new List<BuffetItem>();
        }
        public int TableId { get; set; }
        public string TableNumber { get; set; }
        public List<BuffetItem> BuffetItems { get; set; }

        public string ItemReceipt { get; set; }
        public int TotalItemsPerMenu { get; set; }

        public int TotalItemQty { get; set; }
        public int TotalItems { get; set; }
        public int SortOrder { get; set; }
        public bool Printed { get; set; }
        public Guid OrderGUID { get; set; }
        public int CustomerCount { get; set; }

        public int TotalPrintableItems { get; set; }

        public string OrderedBy { get; set; }

        public int BatchNo { get; set; }

        public string MenuType { get; set; }
        public string ToPrinter { get; set; }
        public bool? DirectPrint { get; set; }

        public long? UserId { get; set; }
        public string Username { get; set; }
        public int? MenuId { get; set; }
        public DateTime OrderTime { get; set; }
    }
}