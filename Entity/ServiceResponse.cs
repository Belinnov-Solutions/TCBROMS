using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class ServiceResponse
    {
        public string Message { get; set; }
        public bool Logout { get; set; } = false;
    }
}