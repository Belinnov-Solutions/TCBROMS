using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class PrintingBatch
    {
        public int PrintQueueId { get; set; }
        public string TableNumber { get; set; }
        public int BatchNumber { get; set; }
        public bool Processed { get; set; }

        public string BatchTime { get; set; }
        public string TicketNo { get; set; }
        public bool RePrint { get; set; }

        public string PrinterName { get; set; }

        public string TicketItems { get; set; }

        public int TicketStatus { get; set; }
    }
}