using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class OrderBuffetItem
    {
        public int Id { get; set; }
        public int TableId { get; set; }
        public System.Guid OrderGUID { get; set; }
        public int ProductId { get; set; }
        public int Qty { get; set; }
        public System.DateTime DateCreated { get; set; }
        public bool Printed { get; set; }

        public string Description { get; set; }
        public bool Served { get; set; }
        public bool Ordered { get; set; }

        public float Price { get; set; }
        public Nullable<long> UserId { get; set; }
        public string UserType { get; set; }

        public string OrderTime { get; set; }
        public string UserName { get; set; }
        public bool Delivered { get; set; }

        public string Mobile { get; set; }
        public string DeviceType { get; set; }
        public string ChineseName { get; set; }
        public string EnglishName { get; set; }
        public string TableNumber { get; set; }
        public int MenuId { get; set; }
        public int Status { get; set; }

    }
}