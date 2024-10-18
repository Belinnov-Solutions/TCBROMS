using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class User
    {
       
        public string UserCode { get; set; }
        public int UserID { get; set; }
        public int DailyPin { get; set; }

        public int UserLevel { get; set; }

        public string UserName { get; set; }

        public string UserPrinter { get; set; }

        public string DeviceID { get; set; }
        public string Token { get; set; }

    }
}