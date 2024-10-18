using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class StockTemplate
    {
        public int TemplateID { get; set; }
        public string TemplateName { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; }

        public string DateCounted { get; set; }
    }
}