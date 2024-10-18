using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.AdminDtos
{
    public class ProductGroupDto
    {
        public int ProductGroupID { get; set; }
        public string Groupname { get; set; }
        public Nullable<int> ParentGroupID { get; set; }
        public Nullable<System.DateTime> DateCreated { get; set; }
        public bool DelInd { get; set; }
        public Nullable<bool> WebsiteDisplay { get; set; }
        public string ImageName { get; set; }
        public string Description { get; set; }
        public Nullable<int> SortOrder { get; set; }
    }
}