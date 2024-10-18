using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class TablesList
    {
        public TablesList()
        {
            tablesList = new List<Table>();
            headCounts = new HeadCounts();
        }
        public HeadCounts headCounts { get; set; }
        public List<Table> tablesList { get; set; }
        //public List<NTable> tablesList { get; set; }
    }
}