using Entity.Hushi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class LocationProduct
    {
        public LocationProduct()
        {
            Locations = new List<StorageLocation>();
        }
        public Product Product { get; set; }
        public List<StorageLocation> Locations { get; set; }
    }
}