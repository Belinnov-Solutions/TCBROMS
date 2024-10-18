using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.AdminDtos
{
    public class MenuItemDto
    {
        public int MenuItemID { get; set; }
        public int MenuID { get; set; }
        public int ProductID { get; set; }
        public bool DelInd { get; set; }
        public string MultiplemenuId { get; set; }
    }
}