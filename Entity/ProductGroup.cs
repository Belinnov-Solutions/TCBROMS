using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class ProductGroup
    {
        public int ProductGroupID { get; set; }
        public string Groupname { get; set; }
        public int? ParentGroupID { get; set; }

        public bool DelInd { get; set; }
        public Nullable<int> SortOrder { get; set; }

        public string GroupImage { get; set; }
        public bool WebsiteDisplay { get; set; }
        public string ImageName { get; set; }
        public string Description { get; set; }
        public bool bOnsite { get; set; }

        public DateTime DateCreated { get; set; }

       
    }
}