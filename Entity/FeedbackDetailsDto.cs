using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class FeedbackDetailsDto
    {
        public string restaurant_name {  get; set; }

        public string table_no { get; set; }

        public string customer_id { get; set; }

        public string customer_no { get; set; }

        public string order_guid { get; set; }

        public string restaurant_id { get; set;}

        public string api_url {  get; set; } 
    }
}