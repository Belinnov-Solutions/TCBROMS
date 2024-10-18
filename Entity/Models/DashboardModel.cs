using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.Models
{
    public class DashboardModel
    {
        public DashboardModel()
        {
            AppOptions = new List<AppOption>();
            HeadCount = new HeadCounts();
        }
        public HeadCounts HeadCount { get; set; }
        public List<AppOption> AppOptions { get; set; }
        public int UndeliveredTicketCount { get; set; }
        public int UndeliveredItemCount { get; set; }
        public int UnprintedItemCount { get; set; }
        public int StarterItemCount { get; set; }
        public int DesertItemCount { get; set; }

        public int MainCourseItemCount { get; set; }

    }
}