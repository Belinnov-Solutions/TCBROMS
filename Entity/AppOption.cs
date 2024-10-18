using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class AppOption
    {
        public int OptionID { get; set; }
        public string OptionName { get; set; }
        public string OptionActivity { get; set; }

        public int Position { get; set; }
    }
}