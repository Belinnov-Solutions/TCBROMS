using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class WaitingCustomer
    {
        public Customer WaitCustomer { get; set; }
        public string IncomingTime { get; set; }
    }
}