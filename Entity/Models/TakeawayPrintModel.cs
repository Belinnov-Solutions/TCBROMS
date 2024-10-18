using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.Models
{
    public class TakeawayPrintModel
    {
        public TakeawayPrintModel()
        {
            Orders = new List<Guid>();
        }
        public int GroupCount { get; set; }
        public List<Guid> Orders { get; set; }
        public int GroupId { get; set; }
    }
}