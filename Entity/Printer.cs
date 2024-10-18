using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class Printer
    {
        public int PrinterID { get; set; }
        public string PrinterName { get; set; }
        public bool Offline { get; set; }
    }
}