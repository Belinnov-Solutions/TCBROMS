using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class DrinksTarget
    {
        public String TargetDate { get; set; }
        public int ProductID { get; set; }
        public String ProductName { get; set; }
        public int? ProductTarget { get; set; }
        public int? Count { get; set; }
        public int? Week { get; set; }
        public String Type { get; set; }
    }
}