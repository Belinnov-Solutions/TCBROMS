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
    
    public partial class usp_AN_GetPendingRefillProducts_Result
    {
        public int ProductRefillID { get; set; }
        public string Product { get; set; }
        public string ProductSize { get; set; }
        public int UserFK { get; set; }
        public string PCName { get; set; }
        public System.DateTime DateCreated { get; set; }
        public Nullable<int> ProductID { get; set; }
        public Nullable<System.DateTime> DateRefill { get; set; }
        public Nullable<int> ProductQty { get; set; }
        public Nullable<bool> WebUpload { get; set; }
    }
}
