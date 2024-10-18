using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class TemplateProduct
    {
        public int ProductID { get; set; }

        public int ProductCode { get; set; }
        public string Description { get; set; }
        public string ChineseName { get; set; }
        public Boolean SmallPortion { get; set; }
        public Boolean RegularPortion { get; set; }
        public Boolean LargePortion { get; set; }
        public Boolean Served { get; set; }

    }
}