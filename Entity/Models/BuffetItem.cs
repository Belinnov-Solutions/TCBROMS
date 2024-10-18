using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.Models
{
    public class BuffetItem
    {
        public int ProductID { get; set; }
        public string Description { get; set; }
        public int Qty { get; set; }
        public Nullable<int> ProductCode { get; set; }

        public string ChineseName { get; set; }
        public string EnglishName { get; set; }

        public int MenuID { get; set; }
        public int? SortOrder { get; set; }
        public int MenuItemCount { get; set; }
        public int CustomerCount { get; set; }

        public bool Printed { get; set; }

        public string OrderedBy { get; set; }

        public bool Priority { get; set; }
        public Guid OrderGUID { get; set; }
        public bool ReOrder { get; set; }
        public string TableNumber { get; set; }
        public long? UserId { get; set; }
        public string UserType { get; set; }
        public DateTime DateCreated { get; set; }
        public int Id { get; set; }
        public bool DirectPrint { get; set; }
        public DateTime? NextOrderTime { get; set; }

        public int ItemsPrintedTillNow { get; set; }
        public string InitialOrderTime { get; set; }

    }
}