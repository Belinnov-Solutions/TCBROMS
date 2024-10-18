using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class OrderFeedback
    {
        public int FeedbackId { get; set; }
        public System.Guid OrderGUID { get; set; }
        public int UserId { get; set; }
        public System.DateTime DateCreated { get; set; }
        public string Feedback { get; set; }
        public System.DateTime LastModified { get; set; }

       public int TableId { get; set; }
    }
}