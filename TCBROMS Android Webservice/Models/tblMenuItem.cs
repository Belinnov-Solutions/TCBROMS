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
    
    public partial class tblMenuItem
    {
        public int MenuItemID { get; set; }
        public int MenuID { get; set; }
        public int ProductID { get; set; }
        public bool DelInd { get; set; }
        public bool Active { get; set; }
        public bool Priority { get; set; }
        public bool DirectPrint { get; set; }
        public Nullable<int> MaxQuantity { get; set; }
        public Nullable<bool> bOnsite { get; set; }
        public Nullable<int> SortOrder { get; set; }
        public Nullable<System.DateTime> LastModified { get; set; }
        public bool SupportItem { get; set; }
        public Nullable<System.DateTime> LastAvailable { get; set; }
        public Nullable<int> AavilabilityChangedBy { get; set; }
    }
}
