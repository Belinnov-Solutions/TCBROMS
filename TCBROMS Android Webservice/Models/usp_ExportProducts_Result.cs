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
    
    public partial class usp_ExportProducts_Result
    {
        public int ProductID { get; set; }
        public Nullable<int> ProductGroupID { get; set; }
        public Nullable<decimal> Price { get; set; }
        public string Description { get; set; }
        public Nullable<System.DateTime> DateCreated { get; set; }
        public bool DelInd { get; set; }
        public bool CustomisePrice { get; set; }
        public string Forecolour { get; set; }
        public string Backcolour { get; set; }
        public bool FoodRefil { get; set; }
        public int SortOrder { get; set; }
        public bool IsTakeaway { get; set; }
        public System.DateTime LastModified { get; set; }
        public Nullable<int> FridgeQty { get; set; }
        public Nullable<int> ProductTypeID { get; set; }
        public bool IsFridgeItem { get; set; }
    }
}
