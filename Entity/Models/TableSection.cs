using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.Models
{
    public class TableSection
    {
        public int SectionId { get; set; }
        public string SectionName { get; set; }
        public bool DelInd { get; set; }
    }
}