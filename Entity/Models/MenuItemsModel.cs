using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.Models
{
    public class MenuItemsModel
    {
        public MenuItemsModel()
        {
            AvailableMenuItems = new List<MenuItem>();
            UnAvailableMenuItems = new List<MenuItem>();
            BuffetMenu = new List<ProductGroupModel>();
        }
        public List<MenuItem> AvailableMenuItems { get; set; }
        public List<MenuItem> UnAvailableMenuItems { get; set; }
        public List<ProductGroupModel> BuffetMenu { get; set; }
    }
}