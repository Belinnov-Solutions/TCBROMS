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
    
    public partial class tblProduct
    {
        public Nullable<int> ProductCode { get; set; }
        public Nullable<int> ProductGroupID { get; set; }
        public Nullable<decimal> Price { get; set; }
        public string Description { get; set; }
        public string ChineseName { get; set; }
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
        public string ImageName { get; set; }
        public Nullable<bool> bVegetarain { get; set; }
        public Nullable<int> bSpicy { get; set; }
        public Nullable<bool> bGlutenFree { get; set; }
        public string vMenuDescription { get; set; }
        public string vAllergens { get; set; }
        public Nullable<bool> Available { get; set; }
        public Nullable<bool> bOnsite { get; set; }
        public string EnglishName { get; set; }
        public Nullable<decimal> VAT { get; set; }
        public Nullable<decimal> CostPrice { get; set; }
        public Nullable<int> RewardPoints { get; set; }
        public Nullable<int> RedemptionPoints { get; set; }
        public Nullable<System.DateTime> RewardStartDate { get; set; }
        public Nullable<System.DateTime> RewardEndDate { get; set; }
        public Nullable<System.DateTime> RedeemStartDate { get; set; }
        public Nullable<System.DateTime> RedeemEndDate { get; set; }
        public Nullable<int> RedeemValidDays { get; set; }
        public int Calories { get; set; }
        public int ProductID { get; set; }
    }
}
