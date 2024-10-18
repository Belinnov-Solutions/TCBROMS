using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class Table
    {
       
        public int TableID { get; set; }
        public int CurrentStatus { get; set; }
        public string TableNumber { get; set; }

        public string OccupiedTime { get; set; }

        public float CurrentTotal { get; set; }

        public int PaxCount { get; set; }
        public int AdCount { get; set; }
        public int KdCount { get; set; }
        public int JnCount { get; set; }

        public string ParentTableNumber { get; set; }

        public Guid OrderGUID { get; set; }

        public int UniqueCode { get; set; }
        public string Message { get; set; }

        public bool ServiceRequired { get; set; }
        public bool PayAsYouGo { get; set; }


        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; }
        public int SectionId { get; set; }
        public bool HideDrinkMenu { get; set; }

    }
}