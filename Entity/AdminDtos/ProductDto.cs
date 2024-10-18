using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.AdminDtos
{
    public class ProductDto
    {
        public int ProductID { get; set; }
        public int? ProductGroupID { get; set; }
        public string vProductCode { get; set; }
        public string vProductEnglishName { get; set; }
        public string vProductChineseName { get; set; }
        public double? Price { get; set; }
        public bool CustomisePrice { get; set; }
        public string Description { get; set; }
        public string vSpicy { get; set; }
        public string vVegetarian { get; set; }
        public string vOrigin { get; set; }
        public string vCourse { get; set; }
        public string vMenuDescription { get; set; }
        public DateTime? DateCreated { get; set; }
        public bool DelInd { get; set; }
        public string Forecolour { get; set; }
        public string Backcolour { get; set; }
        public bool FoodRefil { get; set; }
        public int SortOrder { get; set; }
        public bool IsTakeaway { get; set; }
        public DateTime LastModified { get; set; }
        public int? FridgeQty { get; set; }
        public int? ProductTypeID { get; set; }
        public string ProductType { get; set; }
        public bool IsFridgeItem { get; set; }
        public DateTime? ReplenishedOn { get; set; }
        public bool? IncludeInOutofDateAlert { get; set; }
        public double? ShelfLifeInHours { get; set; }
        public bool ProductLinker { get; set; }
        public int? Menuid { get; set; }
        public int? MenuItemId { get; set; }
        public string MenuName { get; set; }
        public string vAllergens { get; set; }
        public string ImageName { get; set; }
        public bool bVegetarain { get; set; }
        public int bSpicy { get; set; }
        public bool bGlutenFree { get; set; }
        public string ImageBase64 { get; set; }
        public string RestaurantId { get; set; }
        public bool DeleteImage { get; set; }
        public decimal Vat { get; set; }
        public int? ItemCount { get; set; }
        public int BoxGroupCount { get; set; }
        public decimal CostPrice { get; set; }
        public int RewardPoints { get; set; }
        public int RedemptionPoints { get; set; }
        public Nullable<System.DateTime> RewardStartDate { get; set; }
        public Nullable<System.DateTime> RewardEndDate { get; set; }
        public Nullable<System.DateTime> RedeemStartDate { get; set; }
        public Nullable<System.DateTime> RedeemEndDate { get; set; }
        public Nullable<int> RedeemValidDays { get; set; }
        public int Calories { get; set; }

        public bool Available { get; set; }
        public int ProductCode { get; set; }
        public string ChineseName { get; set; }
       public bool bOnsite { get; set; }
        public string EnglishName { get; set; }
    }
}