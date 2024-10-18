using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.AdminDtos
{
    public class MenuDto
    {
        public int MenuID { get; set; }
        public string MenuName { get; set; }
        public string MenuDescription { get; set; }
        public DateTime DateCreated { get; set; }
        public bool DelInd { get; set; }
        public bool bOnsite { get; set; }
        public int SortOrder { get; set; }
        public string ImageName { get; set; }
        public int ParentMenuId { get; set; }
        public bool FixedProducts { get; set; }
    }
}