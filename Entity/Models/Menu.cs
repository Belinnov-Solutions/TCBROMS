using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.Models
{
    public class Menu
    {
        public int MenuID { get; set; }
        public string MenuName { get; set; }
        public string MenuDescription { get; set; }
        public System.DateTime DateCreated { get; set; }
        public bool DelInd { get; set; }
        public Nullable<int> SortOrder { get; set; }
        public Nullable<int> ParentMenuId { get; set; }
        public Nullable<bool> FixedProducts { get; set; }
        public string ImageName { get; set; }
    }
}