using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class RefillProductRequest
    {
        public List<RefillProduct> rfProducts {get; set;}
        public int UserId { get; set; }
        public string PCName { get; set; }
        public string UserName { get; set; }
        public string PrinterName { get; set; }


    }
}