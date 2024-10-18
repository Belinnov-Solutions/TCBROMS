using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace TCBROMS_Android_Webservice
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("elmah.axd");
            routes.MapRoute(
              name: "Login",
              url: "Login",
              defaults: new { controller = "Home", action = "Login" }
          );

            routes.MapRoute(
             name: "Printers",
             url: "Printers",
             defaults: new { controller = "Home", action = "Printers" }
         );

            routes.MapRoute(
            name: "RefillProductList",
            url: "RefillProductList",
            defaults: new { controller = "Home", action = "RefillProductList" }
        );

            routes.MapRoute(
          name: "RefillProducts",
          url: "RefillProducts",
          defaults: new { controller = "Home", action = "RefillProducts" }
      );

            routes.MapRoute(
         name: "UpdateRefillProduct",
         url: "UpdateRefillProduct",
         defaults: new { controller = "Home", action = "UpdateRefillProduct" }
     );
            routes.MapRoute(
             name: "GetTables",
             url: "GetTables",
             defaults: new { controller = "Home", action = "Tables" }
         );
            routes.MapRoute(
             name: "GetTableSections",
             url: "GetTableSections",
             defaults: new { controller = "Home", action = "GetTableSections" }
         );
            routes.MapRoute(
             name: "GetTablesBySection",
             url: "GetTablesBySection",
             defaults: new { controller = "Home", action = "GetTablesBySection" }
         );
            routes.MapRoute(
            name: "GetContactNumbers",
            url: "GetContactNumbers",
            defaults: new { controller = "Home", action = "ContactNumbers" }
        );

            routes.MapRoute(
           name: "GetCustomerDetails",
           url: "GetCustomerDetails",
           defaults: new { controller = "Home", action = "CustomerDetails" }
       );

            routes.MapRoute(
             name: "GetProducts",
             url: "GetProducts",
             defaults: new { controller = "Home", action = "ProductGroups" }
         );

            routes.MapRoute(
           name: "GetLinkedProducts",
           url: "GetLinkedProducts",
           defaults: new { controller = "Home", action = "ProductsLinker" }
       );

            routes.MapRoute(
         name: "SubmitOrder",
         url: "SubmitOrder",
         defaults: new { controller = "Home", action = "SubmitOrder" }
     );
            routes.MapRoute(
       name: "v1/SubmitOrder",
       url: "v1/SubmitOrder",
       defaults: new { controller = "MobileAPI", action = "SubmitOrder" }
   );



            routes.MapRoute(
        name: "GetUserOrders",
        url: "GetUserOrders",
        defaults: new { controller = "Home", action = "GetUserOrders" }
    );

            routes.MapRoute(
      name: "GetTableOrder",
      url: "GetTableOrder",
      defaults: new { controller = "Home", action = "GetTableOrder" }
  );


            routes.MapRoute(
        name: "UserLogout",
        url: "UserLogout",
        defaults: new { controller = "Home", action = "UserLogout" }
    );

            routes.MapRoute(
       name: "GetRefillTemplates",
       url: "GetRefillTemplates",
       defaults: new { controller = "Home", action = "GetTemplates" }
   );

            routes.MapRoute(
      name: "GetTemplateProducts",
      url: "GetTemplateProducts",
      defaults: new { controller = "Home", action = "TemplateProducts" }
  );
            routes.MapRoute(
             name: "ProductGroups",
             url: "ProductGroups",
             defaults: new { controller = "Home", action = "ProductGroups" }
         );
            routes.MapRoute(
             name: "ItemServeTime",
             url: "UpdateItemServeTime",
             defaults: new { controller = "Home", action = "UpdateServeTime" }
         );
            routes.MapRoute(
           name: "UpdateAllProductServed",
           url: "UpdateAllProductServed",
           defaults: new { controller = "Home", action = "UpdateAllProductServed" }
       );
            routes.MapRoute(
            name: "ProductsWastage",
            url: "ProductsWastage",
            defaults: new { controller = "Home", action = "ProductsWastage" }
        );

            routes.MapRoute(
            name: "RequestBill",
            url: "RequestBill",
            defaults: new { controller = "Home", action = "RequestTableBill" }
        );


            routes.MapRoute(
            name: "DrinksTarget",
            url: "DrinksTarget",
            defaults: new { controller = "Home", action = "DrinksTarget" }
        );

            routes.MapRoute(
           name: "UpdateWashroomCheck",
           url: "UpdateWashroomCheck",
           defaults: new { controller = "Home", action = "UpdateWashroomCheck" }
       );

            routes.MapRoute(
        name: "SaveToken",
        url: "SaveToken",
        defaults: new { controller = "Home", action = "SaveToken" }
    );

            routes.MapRoute(
   name: "CoffeeConfirmed",
   url: "CoffeeConfirmed",
   defaults: new { controller = "Home", action = "CoffeeConfirmed" }
);

            routes.MapRoute(
   name: "RepeatDrinksConfirmed",
   url: "RepeatDrinksConfirmed",
   defaults: new { controller = "Home", action = "RepeatDrinksConfirmed" }
);

            routes.MapRoute(
name: "News",
url: "News",
defaults: new { controller = "Home", action = "GetLatestNews" }
);
            routes.MapRoute(
name: "DashboardItems",
url: "DashboardItems",
defaults: new { controller = "Home", action = "GetDashboardItems" }
);
            routes.MapRoute(
name: "Waitlist",
url: "Waitlist",
defaults: new { controller = "Home", action = "GetWaitlistDetails" }
);

            routes.MapRoute(
name: "AvailableTables",
url: "AvailableTables",
defaults: new { controller = "Home", action = "GetAvailableTables" }
);

            routes.MapRoute(
name: "HealthCheck",
url: "HealthCheck",
defaults: new { controller = "Home", action = "HealthCheck" }
);

            routes.MapRoute(
name: "AssignTable",
url: "AssignTable",
defaults: new { controller = "Home", action = "AssignTable" }
);


            routes.MapRoute(
name: "SaveCustomerDetails",
url: "SaveCustomerDetails",
defaults: new { controller = "Home", action = "SaveCustomerDetails" }
);


            routes.MapRoute(
name: "FreeUpTable",
url: "FreeUpTable",
defaults: new { controller = "Home", action = "FreeUpTable" }
);
            routes.MapRoute(
     name: "RevertAllocation",
     url: "RevertAllocation",
     defaults: new { controller = "Home", action = "RevertAllocation" }
     );
            routes.MapRoute(
    name: "ReservationInLounge",
    url: "ReservationInLounge",
    defaults: new { controller = "Home", action = "ReservationInLounge" }
    );

            routes.MapRoute(
name: "GetPendingRefillProducts",
url: "GetPendingRefillProducts",
defaults: new { controller = "Home", action = "GetPendingRefillProducts" }
);
            routes.MapRoute(
    name: "RevertUnAllocation",
    url: "RevertUnAllocation",
    defaults: new { controller = "Home", action = "RevertUnAllocation" }
    );

            routes.MapRoute(
   name: "Reservation",
   url: "Reservation",
   defaults: new { controller = "Home", action = "Reservation" }
   );

            routes.MapRoute(
   name: "SendTableConfirmSMS",
   url: "SendTableConfirmSMS",
   defaults: new { controller = "Home", action = "SendTableConfirmSMS" }
   );

            routes.MapRoute(
            name: "EditWLCustomerDetails",
        url: "EditWLCustomerDetails",
            defaults: new { controller = "Home", action = "EditWLCustomerDetails" }
);
            routes.MapRoute(
          name: "SetWLUnAllocated",
      url: "SetWLUnAllocated",
          defaults: new { controller = "Home", action = "SetWLUnAllocated" }
);

            routes.MapRoute(
name: "ReleaseTable",
url: "ReleaseTable",
defaults: new { controller = "Home", action = "ReleaseTable" }
);
            routes.MapRoute(
name: "LinkTable",
url: "LinkTable",
defaults: new { controller = "Home", action = "LinkTable" }
);
            routes.MapRoute(
name: "GetStockTemplates",
url: "GetStockTemplates",
defaults: new { controller = "Home", action = "GetStockTemplates" }
);

            routes.MapRoute(
name: "GetStockTemplateItems",
url: "GetStockTemplateItems",
defaults: new { controller = "Home", action = "GetStockTemplateItems" }
);
            routes.MapRoute(
name: "InsertStockCount",
url: "InsertStockCount",
defaults: new { controller = "Home", action = "InsertStockCount" }
);
            routes.MapRoute(
            name: "GetCountedTemplates",
            url: "GetCountedTemplates",
            defaults: new { controller = "Home", action = "GetCountedTemplates" }
            );

            routes.MapRoute(
           name: "GetCountedTemplateProducts",
           url: "GetCountedTemplateProducts",
           defaults: new { controller = "Home", action = "GetCountedTemplateProducts" }
           );

            routes.MapRoute(
          name: "SubmitStockOrder",
          url: "SubmitStockOrder",
          defaults: new { controller = "Home", action = "SubmitStockOrder" }
          );
            routes.MapRoute(
          name: "GetHeadCounts",
          url: "GetHeadCounts",
          defaults: new { controller = "Home", action = "GetHeadCounts" }
          );

            routes.MapRoute(
        name: "GetSupplierOrders",
        url: "GetSupplierOrders",
defaults: new { controller = "Home", action = "GetSupplierOrders" }
);
            routes.MapRoute(
          name: "GetSupplierOrderItems",
          url: "GetSupplierOrderItems",
          defaults: new { controller = "Home", action = "GetSupplierOrderItems" }
          );
            routes.MapRoute(
         name: "SaveReceivedQuantity",
         url: "SaveReceivedQuantity",
         defaults: new { controller = "Home", action = "SaveReceivedQuantity" }
         );


            routes.MapRoute(
        name: "ValidateUserCode",
        url: "ValidateUserCode",
        defaults: new { controller = "Home", action = "ValidateUserCode" }
        );
            routes.MapRoute(
       name: "SubmitWastageQuantity",
       url: "SubmitWastageQuantity",
       defaults: new { controller = "Home", action = "SubmitWastageQuantity" }
       );

            // For Hushi
            routes.MapRoute(
           name: "GetOrderedProductsByRestaurant",
           url: "GetOrderedProductsByRestaurant",
           defaults: new { controller = "Hushi", action = "GetOrderedProductsByRestaurant" }
           );

            routes.MapRoute(
         name: "SavePickedProducts",
         url: "SavePickedProducts",
         defaults: new { controller = "Hushi", action = "SavePickedProducts" }
         );

            routes.MapRoute(
     name: "GetDeliveryList",
     url: "GetDeliveryList",
     defaults: new { controller = "Hushi", action = "GetDeliveryList" }
     );
            routes.MapRoute(
        name: "GetItemsForDelivery",
        url: "GetItemsForDelivery",
        defaults: new { controller = "Hushi", action = "GetItemsForDelivery" }
        );

            routes.MapRoute(
       name: "SaveItemsForDelivery",
       url: "SaveItemsForDelivery",
       defaults: new { controller = "Hushi", action = "SaveItemsForDelivery" }
       );
            routes.MapRoute(
      name: "SaveDeliveryList",
      url: "SaveDeliveryList",
      defaults: new { controller = "Hushi", action = "SaveDeliveryList" }
      );
            routes.MapRoute(
     name: "SyncDeliveryList",
     url: "SyncDeliveryList",
     defaults: new { controller = "Hushi", action = "SyncDeliveryList" }
     );

            routes.MapRoute(
         name: "GetOrderedProductsByLocation",
         url: "GetOrderedProductsByLocation",
         defaults: new { controller = "Hushi", action = "GetOrderedProductsByLocation" }
         );
            routes.MapRoute(
       name: "GetKitchenOrders",
       url: "GetKitchenOrders",
       defaults: new { controller = "Home", action = "GetKitchenOrders" }
       );

            routes.MapRoute(
 name: "KitchenOrderDetails",
 url: "KitchenOrderDetails",
 defaults: new { controller = "Home", action = "KitchenOrderDetails" }
 );
            routes.MapRoute(
    name: "CompleteKitchenOrder",
    url: "CompleteKitchenOrder",
    defaults: new { controller = "Home", action = "CompleteKitchenOrder" }
    );
            routes.MapRoute(
    name: "GetDineInProducts",
    url: "GetDineInProducts",
    defaults: new { controller = "Home", action = "GetDineInProducts" }
    );
            routes.MapRoute(
   name: "ValidateTableCode",
   url: "ValidateTableCode",
   defaults: new { controller = "Home", action = "ValidateTableCode" }
   );



            routes.MapRoute(
               name: "GetCartItems",
               url: "GetCartItems",
               defaults: new { controller = "Home", action = "GetCartItems" }
               );
            routes.MapRoute(
              name: "GetBuffetOrderItems",
              url: "GetBuffetOrderItems",
              defaults: new { controller = "Home", action = "GetBuffetOrderItems" }
              );
            routes.MapRoute(
               name: "ConfirmOrderPayment",
               url: "ConfirmOrderPayment",
               defaults: new { controller = "Home", action = "ConfirmOrderPayment" }
               );
            routes.MapRoute(
              name: "PaymentFailed",
              url: "PaymentFailed",
              defaults: new { controller = "Home", action = "PaymentFailed" }
              );

            routes.MapRoute(
              name: "GetPrinterReceipts",
              url: "GetPrinterReceipts",
              defaults: new { controller = "Menu", action = "GetPrinterReceipts" }
              );

            routes.MapRoute(
             name: "GetOrderItems",
             url: "GetOrderItems",
             defaults: new { controller = "Home", action = "GetOrderItems" }
             );
            routes.MapRoute(
name: "GetReceiptsURL",
url: "GetReceiptsURL",
defaults: new { controller = "Home", action = "GetReceiptsURL" }
);
            routes.MapRoute(
name: "GetStripePaymentIntent",
url: "GetStripePaymentIntent",
defaults: new { controller = "Home", action = "GetStripePaymentIntent" }
);


            routes.MapRoute(
name: "GetAppParameters",
url: "GetAppParameters",
defaults: new { controller = "Home", action = "GetAppParameters" }
);

            routes.MapRoute(
name: "WaiterService",
url: "WaiterService",
defaults: new { controller = "Home", action = "WaiterService" }
);

            routes.MapRoute(
name: "MyDineInOrders",
url: "MyDineInOrders",
defaults: new { controller = "Home", action = "MyDineInOrders" }
);


            routes.MapRoute(
name: "ServiceRequired",
url: "ServiceRequired",
defaults: new { controller = "Home", action = "ServiceRequired" }
);
            routes.MapRoute(
name: "DrinksOptions",
url: "DrinksOptions",
defaults: new { controller = "Home", action = "DrinksOptions" }
);

            routes.MapRoute(
               name: "v2/GetOrderItems",
               url: "v2/GetOrderItems",
               defaults: new { controller = "V2", action = "GetOrderItems" }
               );


            routes.MapRoute(
              name: "ReprintKitchenReceipts",
              url: "ReprintKitchenReceipts",
              defaults: new { controller = "Home", action = "ReprintKitchenReceipts" }
              );
            routes.MapRoute(
              name: "CancelOrderedBuffetItems",
              url: "CancelOrderedBuffetItems",
              defaults: new { controller = "Home", action = "CancelOrderedBuffetItems" }
              );
            routes.MapRoute(
              name: "GetPrintersList",
              url: "GetPrintersList",
              defaults: new { controller = "Home", action = "GetPrintersList" }
              );

            routes.MapRoute(
             name: "GenerateQRCode",
             url: "GenerateQRCode",
             defaults: new { controller = "Home", action = "GenerateQRCode" }
             );

            routes.MapRoute(
             name: "GetVoucherDetails",
             url: "GetVoucherDetails",
             defaults: new { controller = "Home", action = "GetVoucherDetails" }
             );

            routes.MapRoute(
            name: "GetVoucherDetailsV1",
            url: "GetVoucherDetailsV1",
            defaults: new { controller = "Home", action = "GetVoucherDetailsV1" }
            );

            routes.MapRoute(
            name: "RedeemVoucher",
            url: "RedeemVoucher",
            defaults: new { controller = "Home", action = "RedeemVoucher" }
            );
            routes.MapRoute(
           name: "RedeemVoucherV1",
           url: "RedeemVoucherV1",
           defaults: new { controller = "Home", action = "RedeemVoucherV1" }
           );
            routes.MapRoute(
           name: "UpdatePercentageDiscount",
           url: "UpdatePercentageDiscount",
           defaults: new { controller = "Home", action = "UpdatePercentageDiscount" }
           );
            routes.MapRoute(
name: "SubmitOrderFeedback",
url: "SubmitOrderFeedback",
defaults: new { controller = "Home", action = "SubmitOrderFeedback" }
);





            routes.MapRoute(
         name: "ReserveTable",
     url: "ReserveTable",
         defaults: new { controller = "Home", action = "ReserveTable" }
);
            routes.MapRoute(
     name: "GetDeliveryStaff",
 url: "GetDeliveryStaff",
     defaults: new { controller = "Home", action = "GetDeliveryStaff" }
);
            routes.MapRoute(
      name: "SaveAvailableDeliveryStaff",
  url: "SaveAvailableDeliveryStaff",
      defaults: new { controller = "Home", action = "SaveAvailableDeliveryStaff" }
);

            routes.MapRoute(
    name: "GetAvailableTakeAwaySlots",
url: "GetAvailableTakeAwaySlots",
    defaults: new { controller = "Home", action = "GetAvailableTakeAwaySlots" }
);
            routes.MapRoute(
  name: "GetDeliveryStaffOrders",
url: "GetDeliveryStaffOrders",
  defaults: new { controller = "Home", action = "GetDeliveryStaffOrders" }
);
            routes.MapRoute(
  name: "SaveAmountCollected",
url: "SaveAmountCollected",
  defaults: new { controller = "Home", action = "SaveAmountCollected" }
);
            routes.MapRoute(
name: "SetDriverUnavailable",
url: "SetDriverUnavailable",
defaults: new { controller = "Home", action = "SetDriverUnavailable" }
);
            routes.MapRoute(
name: "PrintTakeAwayOrder",
url: "PrintTakeAwayOrder",
defaults: new { controller = "Home", action = "PrintTakeAwayOrder" }
);

            routes.MapRoute(
name: "PrintTakeAwayOrders",
url: "PrintTakeAwayOrders",
defaults: new { controller = "Home", action = "PrintTakeAwayOrders" }
);
            routes.MapRoute(
name: "UpdateTakeAwayOrderDelivery",
url: "UpdateTakeAwayOrderDelivery",
defaults: new { controller = "Home", action = "UpdateTakeAwayOrderDelivery" }
);
            routes.MapRoute(
name: "ConfirmDriverAvailability",
url: "ConfirmDriverAvailability",
defaults: new { controller = "Home", action = "ConfirmDriverAvailability" }
);









            routes.MapRoute(
name: "GetMenuAgainstTime",
url: "GetMenuAgainstTime",
defaults: new { controller = "Menu", action = "GetMenuAgainstTime" }
);

            routes.MapRoute(
     name: "GetAllTableOrders",
     url: "GetAllTableOrders",
     defaults: new { controller = "Home", action = "GetAllTableOrders" }
 );

            routes.MapRoute(
     name: "MenuAutomate",
     url: "MenuAutomate",
     defaults: new { controller = "Menu", action = "MenuAutomate" }
 );

            #region V1 
            routes.MapRoute(
        name: "v1/SaveReceivedQuantity",
        url: "v1/SaveReceivedQuantity",
        defaults: new { controller = "V1", action = "SaveReceivedQuantity" }
        );
            routes.MapRoute(
name: "v1/ValidateTableCode",
url: "v1/ValidateTableCode",
defaults: new { controller = "V1", action = "ValidateTableCode" }
);
            routes.MapRoute(
name: "v1/GetStripePaymentIntent",
url: "v1/GetStripePaymentIntent",
defaults: new { controller = "MobileAPI", action = "GetStripePaymentIntent" }
);
            routes.MapRoute(
name: "v1/GetAppParameters",
url: "v1/GetAppParameters",
defaults: new { controller = "Home", action = "GetAppParametersV1" }
);
            routes.MapRoute(
               name: "v1/GetOrderItems",
               url: "v1/GetOrderItems",
               defaults: new { controller = "V1", action = "GetOrderItems" }
               );
            routes.MapRoute(
              name: "v1/CalculatePromotionDiscount",
              url: "v1/CalculatePromotionDiscount",
              defaults: new { controller = "MobileAPI", action = "CalculatePromotionDiscount" }
              );
            routes.MapRoute(
name: "v1/MyDineInOrders",
url: "v1/MyDineInOrders",
defaults: new { controller = "V1", action = "MyDineInOrders" }
);


            routes.MapRoute(
name: "v1/WaiterService",
url: "v1/WaiterService",
defaults: new { controller = "MobileAPI", action = "WaiterService" }
);
            routes.MapRoute(
name: "v1/RequestBill",
url: "v1/RequestBill",
defaults: new { controller = "MobileAPI", action = "RequestBill" }
);
            routes.MapRoute(
               name: "v1/ConfirmOrderPayment",
               url: "v1/ConfirmOrderPayment",
               defaults: new { controller = "MobileAPI", action = "ConfirmOrderPayment" }
               );
            routes.MapRoute(
   name: "v1/GetDineInProducts",
   url: "v1/GetDineInProducts",
   defaults: new { controller = "V1", action = "GetDineInProducts" }
   );
            routes.MapRoute(
name: "v1/TillCustomerRegistration",
url: "v1/TillCustomerRegistration",
defaults: new { controller = "V1", action = "TillCustomerRegistration" }
);
            routes.MapRoute(
name: "v1/VerifyRegistrationOTP",
url: "v1/VerifyRegistrationOTP",
defaults: new { controller = "V1", action = "VerifyRegistrationOTP" }
);

            routes.MapRoute(
name: "v1/ServiceChargeApplicable",
url: "v1/ServiceChargeApplicable",
defaults: new { controller = "V1", action = "ServiceChargeApplicable" }
);
            routes.MapRoute(
name: "v1/GetOrderedItemsbyMenu",
url: "v1/GetOrderedItemsbyMenu",
defaults: new { controller = "V1", action = "GetOrderedItemsbyMenu" }
);
            routes.MapRoute(
name: "v1/GetMenus",
url: "v1/GetMenus",
defaults: new { controller = "V1", action = "GetMenus" }
);
            routes.MapRoute(
name: "v1/ProcessBatchOrder",
url: "v1/ProcessBatchOrder",
defaults: new { controller = "V1", action = "ProcessBatchOrder" }
);
            routes.MapRoute(
         name: "v1/UpdateCustomerActivity",
         url: "v1/UpdateCustomerActivity",
         defaults: new { controller = "V1", action = "UpdateCustomerActivity" }
         );

            routes.MapRoute(
         name: "v1/UpdateProduct",
         url: "v1/UpdateProduct",
         defaults: new { controller = "V1", action = "UpdateProduct" }
         );

            routes.MapRoute(
         name: "v1/UpdateProductGroup",
         url: "v1/UpdateProductGroup",
         defaults: new { controller = "V1", action = "UpdateProductGroup" }
         );
            routes.MapRoute(
       name: "v1/UpdateMenu",
       url: "v1/UpdateMenu",
       defaults: new { controller = "V1", action = "UpdateMenu" }
       );
            routes.MapRoute(
       name: "v1/UpdateMenuItem",
       url: "v1/UpdateMenuItem",
       defaults: new { controller = "V1", action = "UpdateMenuItem" }
       );
            routes.MapRoute(
       name: "v1/UpdateStockOrders",
       url: "v1/UpdateStockOrders",
       defaults: new { controller = "V1", action = "UpdateStockOrders" }
       );
            routes.MapRoute(
       name: "v1/GetReceivedOrders",
       url: "v1/GetReceivedOrders",
       defaults: new { controller = "V1", action = "GetReceivedOrders" }
       );
            routes.MapRoute(
       name: "v1/UpdateReceivedOrders",
       url: "v1/UpdateReceivedOrders",
       defaults: new { controller = "V1", action = "UpdateReceivedOrders" }
       );

            routes.MapRoute(
       name: "v1/GetPrintingCounts",
       url: "v1/GetPrintingCounts",
       defaults: new { controller = "V1", action = "GetPrintingCounts" }
       );

            routes.MapRoute(
      name: "v1/GetAverageCounts",
      url: "v1/GetPrintingCounts",
      defaults: new { controller = "V1", action = "GetPrintingCounts" }
      );
            routes.MapRoute(
      name: "v1/GetMenuItems",
      url: "v1/GetMenuItems",
      defaults: new { controller = "V1", action = "GetMenuItems" }
      );
            routes.MapRoute(
       name: "v1/UpdateMenuItemAvailability",
       url: "v1/UpdateMenuItemAvailability",
       defaults: new { controller = "V1", action = "UpdateMenuItemAvailability" }
       );
            routes.MapRoute(
             name: "v1/GetTableSections",
             url: "v1/GetTableSections",
             defaults: new { controller = "V1", action = "GetTableSections" }
         );
            routes.MapRoute(
             name: "v1/GetTablesBySection",
             url: "v1/GetTablesBySection",
             defaults: new { controller = "V1", action = "GetTablesBySection" }
         );
            routes.MapRoute(
             name: "v1/GetProducts",
             url: "v1/GetProducts",
             defaults: new { controller = "V1", action = "ProductGroups" }
         );
            routes.MapRoute(
      name: "v1/GetTableOrder",
      url: "v1/GetTableOrder",
      defaults: new { controller = "V1", action = "GetTableOrder" }
  );
            routes.MapRoute(
     name: "v2/GetTableOrder",
     url: "v2/GetTableOrder",
     defaults: new { controller = "V2", action = "GetTableOrder" }
 );
            routes.MapRoute(
     name: "v3/GetTableOrder",
     url: "v3/GetTableOrder",
     defaults: new { controller = "V3", action = "GetTableOrder" }
 );

            routes.MapRoute(
            name: "v1/GenerateQRCode",
            url: "v1/GenerateQRCode",
            defaults: new { controller = "V1", action = "GenerateQRCode" }
            );

            routes.MapRoute(
          name: "v2/GenerateQRCode",
          url: "v2/GenerateQRCode",
          defaults: new { controller = "V2", action = "GenerateQRCode" }
          );
            routes.MapRoute(
      name: "v1/UpdateReservationOnTill",
      url: "v1/UpdateReservationOnTill",
      defaults: new { controller = "V1", action = "UpdateReservationOnTill" }
      );
            routes.MapRoute(
      name: "v1/ReOrderPrint",
      url: "v1/ReOrderPrint",
      defaults: new { controller = "V1", action = "ReOrderPrint" }
      );

            routes.MapRoute(
     name: "v1/GetAllPrintBatches",
     url: "v1/GetAllPrintBatches",
     defaults: new { controller = "V1", action = "GetAllPrintBatches" }
     );
            routes.MapRoute(
    name: "v1/GetAllPrintBatchesByTable",
    url: "v1/GetAllPrintBatchesByTable",
    defaults: new { controller = "V1", action = "GetAllPrintBatchesByTable" }
    );
            routes.MapRoute(
     name: "v1/ProcessPrintBatch",
     url: "v1/ProcessPrintBatch",
     defaults: new { controller = "V1", action = "ProcessPrintBatch" }
     );
            routes.MapRoute(
    name: "v1/GetBatchOrder",
    url: "v1/GetBatchOrder",
    defaults: new { controller = "V1", action = "GetBatchOrder" }
    );

            routes.MapRoute(
     name: "v1/VerifyOrderProducts",
     url: "v1/VerifyOrderProducts",
     defaults: new { controller = "V1", action = "VerifyOrderProducts" }
     );
            routes.MapRoute(
    name: "v2/VerifyOrderProducts",
    url: "v2/VerifyOrderProducts",
    defaults: new { controller = "V2", action = "VerifyOrderProducts" }
    );

            routes.MapRoute(
    name: "v1/SubmitFeedback",
    url: "v1/SubmitFeedback",
    defaults: new { controller = "V1", action = "SubmitFeedback" }
    );
            routes.MapRoute(
    name: "v1/ReleasePaymentLock",
    url: "v1/ReleasePaymentLock",
    defaults: new { controller = "V1", action = "ReleasePaymentLock" }
    );
            routes.MapRoute(
name: "v1/OrderLock",
url: "v1/OrderLock",
defaults: new { controller = "V1", action = "OrderLock" }
);
            routes.MapRoute(
    name: "v1/UpdatePaymentLock",
    url: "v1/UpdatePaymentLock",
    defaults: new { controller = "V1", action = "UpdatePaymentLock" }
    );

            routes.MapRoute(
    name: "v2/UpdatePaymentLock",
    url: "v2/UpdatePaymentLock",
    defaults: new { controller = "V2", action = "UpdatePaymentLock" }
    );
            routes.MapRoute(
    name: "v1/GetDashBoardItems",
    url: "v1/GetDashBoardItems",
    defaults: new { controller = "V1", action = "GetDashBoardItems" }
    );
            routes.MapRoute(
    name: "v1/ReprintKitchenReceipt",
    url: "v1/ReprintKitchenReceipt",
    defaults: new { controller = "V1", action = "ReprintKitchenReceipt" }
    );
            routes.MapRoute(
    name: "v1/ReprintTableQRCode",
    url: "v1/ReprintTableQRCode",
    defaults: new { controller = "V1", action = "ReprintTableQRCode" }
    );
            routes.MapRoute(
    name: "v1/GetItemsForSplit",
    url: "v1/GetItemsForSplit",
    defaults: new { controller = "V1", action = "GetItemsForSplit" }
    );

            routes.MapRoute(
    name: "v1/GetReservationSlots",
    url: "v1/GetReservationSlots",
    defaults: new { controller = "V1", action = "GetReservationSlots" }
    );

            routes.MapRoute(
   name: "v1/EditSCRate",
   url: "v1/EditSCRate",
   defaults: new { controller = "V1", action = "EditSCRate" }
   );
            routes.MapRoute(
name: "v1/Feedbackdetails",
url: "v1/Feedbackdetails",
defaults: new { controller = "V1", action = "Feedbackdetails" }
);

            #endregion
            #region V2
            routes.MapRoute(
   name: "v2/GetDineInProducts",
   url: "v2/GetDineInProducts",
   defaults: new { controller = "V2", action = "GetDineInProducts" }
   );
            routes.MapRoute(
              name: "v2/ConfirmTablePayment",
              url: "v2/ConfirmTablePayment",
              defaults: new { controller = "V2", action = "ConfirmTablePayment" }
              );
            routes.MapRoute(
             name: "v2/FailedTablePayment",
             url: "v2/FailedTablePayment",
             defaults: new { controller = "V2", action = "FailedTablePayment" }
             );
            routes.MapRoute(
name: "v2/GetStripePaymentIntent",
url: "v2/GetStripePaymentIntent",
defaults: new { controller = "V2", action = "GetStripePaymentIntent" }
);
            routes.MapRoute(
name: "v2/GetProductsForRedemption",
url: "v2/GetProductsForRedemption",
defaults: new { controller = "V2", action = "GetProductsForRedemption" }
);
            routes.MapRoute(
name: "v2/SubmitProductsForRedemption",
url: "v2/SubmitProductsForRedemption",
defaults: new { controller = "V2", action = "SubmitProductsForRedemption" }
);
            routes.MapRoute(
  name: "v2/SubmitOrder",
  url: "v2/SubmitOrder",
  defaults: new { controller = "V2", action = "SubmitOrderV2" }
);
            routes.MapRoute(
name: "v2/UpdatePointsByOrder",
url: "v2/UpdatePointsByOrder",
defaults: new { controller = "V2", action = "UpdatePointsByOrder" }
);

            routes.MapRoute(
name: "v2/AddRewardPointsByCustomerId",
url: "v2/AddRewardPointsByCustomerId",
defaults: new { controller = "V2", action = "AddRewardPointsByCustomerId" }
);
            routes.MapRoute(
            name: "v2/RequestBill",
            url: "v2/RequestBill",
            defaults: new { controller = "V2", action = "RequestBill" }
            );
            routes.MapRoute(
         name: "v2/GetVoucherDetails",
         url: "v2/GetVoucherDetails",
         defaults: new { controller = "V2", action = "GetVoucherDetails" }
         );

            routes.MapRoute(
           name: "v2/RedeemVoucher",
           url: "v2/RedeemVoucher",
           defaults: new { controller = "V2", action = "RedeemVoucher" }
           );
            routes.MapRoute(
             name: "v2/GetTablesBySection",
             url: "v2/GetTablesBySection",
             defaults: new { controller = "V2", action = "GetTablesBySection" }
         );
            routes.MapRoute(
    name: "v2/GetAllPrintBatches",
    url: "v2/GetAllPrintBatches",
    defaults: new { controller = "V2", action = "GetAllPrintBatches" }
    );
            routes.MapRoute(
    name: "v2/GetAllPrintBatchesByTable",
    url: "v2/GetAllPrintBatchesByTable",
    defaults: new { controller = "V2", action = "GetAllPrintBatchesByTable" }
    );
            routes.MapRoute(
    name: "v2/ReprintKitchenReceipt",
    url: "v2/ReprintKitchenReceipt",
    defaults: new { controller = "V2", action = "ReprintKitchenReceipt" }
    );

            routes.MapRoute(
name: "v2/Waitlist",
url: "v2/Waitlist",
defaults: new { controller = "V2", action = "GetWaitlistDetails" }
);
            #endregion
            #region V3
            routes.MapRoute(
  name: "v3/GetDineInProducts",
  url: "v3/GetDineInProducts",
  defaults: new { controller = "V3", action = "GetDineInProducts" }
  );
            routes.MapRoute(
 name: "v3/SubmitOrder",
 url: "v3/SubmitOrder",
 defaults: new { controller = "V3", action = "SubmitOrder" }
);
            routes.MapRoute(
 name: "v4/SubmitOrder",
 url: "v4/SubmitOrder",
 defaults: new { controller = "V4", action = "SubmitOrder" }
);
            routes.MapRoute(
name: "v4/DataCleanup",
url: "v4/DataCleanup",
defaults: new { controller = "V4", action = "DataCleanup" }
);
            routes.MapRoute(
name: "v1/RecordManualPayment",
url: "v1/RecordManualPayment",
defaults: new { controller = "V1", action = "RecordManualPayment" }
);
            routes.MapRoute(
name: "v4/GetTableOrder",
url: "v4/GetTableOrder",
defaults: new { controller = "V4", action = "GetTableOrder" }
);
            routes.MapRoute(
       name: "v3/UpdateReservation",
       url: "v3/UpdateReservation",
       defaults: new { controller = "V3", action = "UpdateReservation" }
       );
            routes.MapRoute(
name: "v3/GetCustomerByMobile",
url: "v3/GetCustomerByMobile",
defaults: new { controller = "V3", action = "GetCustomerByMobile" }
);

            routes.MapRoute(
name: "v3/SubmitRedeemedProducts",
url: "v3/SubmitRedeemedProducts",
defaults: new { controller = "V3", action = "SubmitRedeemedProducts" }
);

            routes.MapRoute(
name: "v3/GetRewardPointsForOrder",
url: "v3/GetRewardPointsForOrder",
defaults: new { controller = "V3", action = "GetRewardPointsForOrder" }
);

            routes.MapRoute(
              name: "v3/ConfirmTablePayment",
              url: "v3/ConfirmTablePayment",
              defaults: new { controller = "V3", action = "ConfirmTablePayment" }
              );

            routes.MapRoute(
              name: "v3/ConfirmSagePayment",
              url: "v3/ConfirmSagePayment",
              defaults: new { controller = "V3", action = "ConfirmSagePayment" }
              );

            routes.MapRoute(
name: "v3/GetStripePaymentIntent",
url: "v3/GetStripePaymentIntent",
defaults: new { controller = "V3", action = "GetStripePaymentIntent" }
);
            routes.MapRoute(
name: "v3/GetSagePayTxCode",
url: "v3/GetSagePayTxCode",
defaults: new { controller = "V3", action = "GetSagePayTxCode" }
);

            routes.MapRoute(
name: "v3/PrintBuffetItems",
url: "v3/PrintBuffetItems",
defaults: new { controller = "V3", action = "PrintBuffetItems" }
);

            routes.MapRoute(
name: "v3/GetTickets",
url: "v3/GetTickets",
defaults: new { controller = "V3", action = "GetTickets" }
);
            routes.MapRoute(
name: "v3/ReprintItems",
url: "v3/ReprintItems",
defaults: new { controller = "V3", action = "ReprintItems" }
);
            routes.MapRoute(
name: "v3/UpdatePrinterStatus",
url: "v3/UpdatePrinterStatus",
defaults: new { controller = "V3", action = "UpdatePrinterStatus" }
);
            routes.MapRoute(
name: "v3/GetUnPrintedItems",
url: "v3/GetUnPrintedItems",
defaults: new { controller = "V3", action = "GetUnPrintedItems" }
);
            routes.MapRoute(
name: "v3/PrintUnPrintedItems",
url: "v3/PrintUnPrintedItems",
defaults: new { controller = "V3", action = "PrintUnPrintedItems" }
);
            routes.MapRoute(
name: "v3/GetUnPrintedItemsByTable",
url: "v3/GetUnPrintedItemsByTable",
defaults: new { controller = "V3", action = "GetUnPrintedItemsByTable" }
);
            routes.MapRoute(
name: "v3/UpdateUnPrintedItemStatus",
url: "v3/UpdateUnPrintedItemStatus",
defaults: new { controller = "V3", action = "UpdateUnPrintedItemStatus" }
);
            routes.MapRoute(
name: "v3/ParameterTest",
url: "v3/ParameterTest",
defaults: new { controller = "V3", action = "ParameterTest" }
);

            routes.MapRoute(
            name: "v3/GetOrderItems",
            url: "v3/GetOrderItems",
            defaults: new { controller = "V3", action = "GetOrderItems" }
            );

            routes.MapRoute(
name: "v3/SCAmountUpdate",
url: "v3/SCAmountUpdate",
defaults: new { controller = "V3", action = "SCAmountUpdate" }
);
            routes.MapRoute(
name: "v4/GetStripePaymentIntent",
url: "v4/GetStripePaymentIntent",
defaults: new { controller = "V4", action = "GetStripePaymentIntent" }
);

            routes.MapRoute(
name: "v4/ConfirmStripePayment",
url: "v4/ConfirmStripePayment",
defaults: new { controller = "V4", action = "ConfirmStripePayment" }
);

         

            #endregion
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Kitchen", action = "Index", id = UrlParameter.Optional }
            );



        }
    }
}
