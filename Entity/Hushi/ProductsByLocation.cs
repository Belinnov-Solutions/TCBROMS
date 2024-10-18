using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.Hushi
{
    public class ProductsByLocation
    {
        public ProductsByLocation()
        {
            LocationProductList = new List<LocationProductList>();
        }
        public List<LocationProductList> LocationProductList { get; set; }
    }
}