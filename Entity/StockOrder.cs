using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class StockOrder
    {
        public int TemplateId { get; set; }
        public string TemplateName { get; set; }
        public string Notes { get; set; }
        public int UserId { get; set; }
        public DateTime RequiredDate { get; set; }
        public List<StockOrderProduct> OrderProducts { get; set; }
    }
}