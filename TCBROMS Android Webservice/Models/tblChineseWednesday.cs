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
    
    public partial class tblChineseWednesday
    {
        public int ChineseWednesdayID { get; set; }
        public string MobileNo { get; set; }
        public string PromoCode { get; set; }
        public System.DateTime DateCreated { get; set; }
        public bool Redeemed { get; set; }
        public Nullable<System.DateTime> DateRedeemed { get; set; }
        public bool DelInd { get; set; }
        public Nullable<System.DateTime> DateValid { get; set; }
        public Nullable<int> RestaurantID { get; set; }
        public Nullable<int> PromoID { get; set; }
    }
}
