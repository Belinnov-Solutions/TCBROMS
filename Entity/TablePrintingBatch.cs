using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class TablePrintingBatch
    {
        public TablePrintingBatch()
        {
            TableBatches = new List<PrintingBatch>();
        }
        public int TableId { get; set; }
        public string TableNumber { get; set; }
        public List<PrintingBatch> TableBatches { get; set; }
    }
}