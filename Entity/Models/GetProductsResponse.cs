using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.Models
{
    public class GetProductsResponse
    {
        public GetProductsResponse()
        {
            BuffetMenu = new List<ProductGroupModel>();
            DrinksMenu = new List<ProductGroupModel>();
            
        }
        public List<ProductGroupModel> BuffetMenu { get; set; }
        public List<ProductGroupModel> DrinksMenu { get; set; }

        
    }
}