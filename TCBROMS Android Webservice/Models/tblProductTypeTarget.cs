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
    
    public partial class tblProductTypeTarget
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public System.DateTime DateCreated { get; set; }
        public string Type { get; set; }
        public int ProductTypeID { get; set; }
        public string TargetDate { get; set; }
        public int Target { get; set; }
        public bool ProductsLinked { get; set; }
        public bool WebUpload { get; set; }
    }
}
