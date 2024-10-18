using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity
{
    public class Product
    {
        public Product()
        {
            productsLinker = new List<LinkedProduct>();
    }
        public int OrderPartID { get; set; }
        public int ProductID { get; set; }
        public string ChineseName { get; set; }
        public string EnglishName { get; set; }

        public int ProductCode { get; set; }
        public string Description { get; set; }
        public string GroupName { get; set; }
        public float Price { get; set; }

        public int? ParentGroupID { get; set; }
        public int? ProductGroupID { get; set; }

        public Boolean ProductAvailable { get; set; }

        public Boolean HasLinkedProducts { get; set; }

        public string Options { get; set; }

        public int OptionID { get; set; }

        public int ProductQty { get; set; }

        public string OrderedTime { get; set; }

        public string ServedTime { get; set; }

        public string Type { get; set; }

        public Boolean SentToKitchen { get; set; }
        public Boolean Served { get; set; }

        public int OrderNo { get; set; }

        public float WastageQty { get; set; }

        public Guid OrderGUID { get; set; }
        public string TableName { get; set; }
        public string Location { get; set; }
        public string SubLocation { get; set; }

        public int PickedBy { get; set; }
        public int ReceivedQty { get; set; }

        public string ImageName { get; set; }
        public bool bVegetarain { get; set; }

        public Nullable<int> bSpicy { get; set; }
        public bool bGlutenFree { get; set; }
        public string vAllergens { get; set; }
        public string menuDescripton { get; set; }
        public float ProductTotal { get; set; }

        public bool Priority { get; set; }

        public int MenuID { get; set; }
        public List<LinkedProduct> productsLinker { get; set; }

        public bool? bOnsite { get; set; }

        public int SortOrder { get; set; }

        public string color { get; set; }
        public decimal VAT { get; set; }

        public int? ProductTypeId { get; set; }

        public bool FoodRefil { get; set; }
        public int RewardPoints { get; set; }
        public int RedemptionPoints { get; set; }
        public bool IsRedemptionProduct { get; set; }

        public bool IsFixedProducts { get; set; }

        public int Calories { get; set; }

        public bool IsCheckedForSplit { get; set; }

        public bool IsGiftVoucher { get; set; }
        public string DeviceType { get; set; }
        public Nullable<DateTime> DateCreated { get; set; }
        public Nullable<DateTime> LastModified { get; set; }
        public Nullable<DateTime> ServeTime { get; set; }



    }
}