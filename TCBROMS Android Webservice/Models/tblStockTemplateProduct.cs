//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TCBROMS_Android_Webservice.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class tblStockTemplateProduct
    {
        public int StockTemplateProductID { get; set; }
        public int TemplateFK { get; set; }
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public int MaxQuantity { get; set; }
        public System.DateTime DateCreated { get; set; }
        public string Type { get; set; }
    }
}
