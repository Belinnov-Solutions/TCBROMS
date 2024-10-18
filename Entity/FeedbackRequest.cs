using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class FeedbackRequest
    {
        public Nullable<int> CustomerId { get; set; }

        public int RestaurantId { get; set; }
        public string Mobile { get; set; }
        public decimal OverallRating { get; set; }
        public string Recommendation { get; set; }
        public string Feedback { get; set; }
        public string OrderNo { get; set; }
        public string OrderType { get; set; }
        public System.DateTime DateCreated { get; set; }
        public string FullName { get; set; }
    }
}