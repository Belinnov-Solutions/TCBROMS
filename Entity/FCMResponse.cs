using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class FCMResponse
    {
        public long multicast_id { get; set; }
        public bool success { get; set; }
        public bool failure { get; set; }
        public int canonical_ids { get; set; }
        public List<FCMMessage> results { get; set; }
    }
}