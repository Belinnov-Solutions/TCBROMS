using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class OrderSubmitResponse
    {
        public Guid OrderGUID { get; set; }
        public string message { get; set; }

        public int UniqueCode { get; set; }
        public string TableNumber { get; set; }

        public bool Logout { get; set; } = false;
        public int CustomerPoints { get; set; }

        public bool IsRefreshRequired { get; set; }
        public float CurrentTotal { get; set; }
    }

}