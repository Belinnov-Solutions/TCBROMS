using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class VerifyOrderProductsResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }

        public bool ShowOptionButtons { get; set; }

    }
}