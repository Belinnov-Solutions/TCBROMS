using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class TableCustomer
    {
        public TableCustomer()
        {
            CustomerNames = new List<string>();
        }
        public string TableNumber { get; set; }
        public string OpenTime { get; set; }
        public string CloseTime { get; set; }
        public List<string> CustomerNames { get; set; }
        public Guid OrderId { get; set; }
    }
}