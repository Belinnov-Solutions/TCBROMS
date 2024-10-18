using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class ServiceCharge
    {
        public bool IsApplicable { get; set; }
        public string Name { get; set; }

        public decimal Rate { get; set; }
        public string Description { get; set; }
        public string InfoBtnName { get; set; }
        public string AcceptBtnName { get; set; }
        public string DenyBtnName { get; set; }
    }
}