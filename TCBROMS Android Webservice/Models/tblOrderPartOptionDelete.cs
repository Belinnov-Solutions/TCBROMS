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
    
    public partial class tblOrderPartOptionDelete
    {
        public int OrderPartOptionID { get; set; }
        public int OrderPartId { get; set; }
        public int ProductOptionID { get; set; }
        public Nullable<decimal> Price { get; set; }
        public bool DelInd { get; set; }
        public System.Guid OrderGUID { get; set; }
        public int UserID { get; set; }
        public System.DateTime LastModified { get; set; }
    }
}
