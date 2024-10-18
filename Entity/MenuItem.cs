using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class MenuItem
    {
        public int MenuItemID { get; set; }
        public int MenuID { get; set; }
        public int ProductID { get; set; }
        public bool DelInd { get; set; }
        public bool Active { get; set; }
        public bool Priority { get; set; }
        public bool DirectPrint { get; set; }
        public Nullable<int> MaxQuantity { get; set; }
        public Nullable<bool> bOnsite { get; set; }
        public Nullable<int> SortOrder { get; set; }
        public Nullable<System.DateTime> LastModified { get; set; }
        public Nullable<System.DateTime> LastAvailable { get; set; }
        public bool SupportItem { get; set; }
        public string LastAvailableDate {
            set { }
            get
            {
                if (LastAvailable != null && Active == false)
                    return LastAvailable.Value.ToString("dd/MM/yyyy");
                else
                    return "";
            }    
        }
        public string Description { get; set; }
        public string ChineseName { get; set; }
    }
}