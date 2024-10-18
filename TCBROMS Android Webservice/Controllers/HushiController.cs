using Deznu.Products.Common.Utility;
using Entity;
using Entity.Hushi;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using TCBROMS_Android_Webservice.Models;

namespace TCBROMS_Android_Webservice.Controllers
{
    public class HushiController : Controller
    {
        // GET: Hushi
        public ActionResult Index()
        {

            return View();
        }
        public ActionResult GetOrderedProductsByRestaurant()
        {
            ProductsByRestaurant opr = new ProductsByRestaurant();
            List<PurchaseProduct> prodList = new List<PurchaseProduct>();
            prodList = GetProductsForPicker();
            foreach (var item in prodList)
            {
                bool found = false;
                foreach (var item1 in opr.RestWiseList)
                {
                    if (item.TableName == "Bolton" && item.ProductId == 1214)
                    {

                    }
                    if (item1.Restaurant.RestaurantName == item.TableName)
                    {
                        item1.Restaurant.Qty += item.ProductQty;
                        bool lfound = false;
                        foreach (var loc in item1.LocationList)
                        {
                            if (loc.Location.Name == item.Location.Name)
                            {
                                bool pfound = false;
                                loc.Location.Qty += item.ProductQty;
                                foreach (var prod in loc.ProductList)
                                {
                                    if (prod.ProductId == item.ProductId)
                                    {
                                        pfound = true;
                                        bool fo = false;
                                        prod.ProductQty += item.ProductQty;
                                        foreach (var x in prod.OrderList)
                                        {
                                            if (x.OrderGUID == item.OrderGUID)
                                            {
                                                fo = true;
                                                x.Qty += item.ProductQty;
                                            }
                                        }
                                        if (!fo)
                                        {
                                            ProductOrder p = new ProductOrder();
                                            p.OrderGUID = item.OrderGUID;
                                            p.Qty = item.ProductQty;
                                            prod.OrderList.Add(p);
                                        }
                                    }
                                }
                                if (!pfound)
                                {
                                    loc.ProductList.Add(AddProduct(item));
                                }
                                lfound = true;
                            }
                        }
                        if (!lfound)
                        {
                            StorageLocationProductList pl = new StorageLocationProductList();
                            pl.Location.Name = item.Location.Name;
                            pl.Location.Qty += item.ProductQty;
                            pl.ProductList.Add(AddProduct(item));
                            item1.LocationList.Add(pl);
                        }
                        found = true;
                    }
                }
                if (!found)
                {
                    RestaurantLocationList oprs = new RestaurantLocationList();
                    Restaurant res = new Restaurant();
                    res.Qty += item.ProductQty;
                    res.RestaurantName = item.TableName;
                    res.RestaurantId = item.Restaurant.RestaurantId;
                    StorageLocationProductList pl = new StorageLocationProductList();
                    pl.Location.Name = item.Location.Name;
                    pl.Location.Qty += item.ProductQty;
                    pl.ProductList.Add(AddProduct(item));
                    oprs.LocationList.Add(pl);
                    oprs.Restaurant = res;
                    opr.RestWiseList.Add(oprs);
                }
            }

            return Json(opr, JsonRequestBehavior.AllowGet);
        }

        private PurchaseProduct AddProduct(PurchaseProduct item)
        {
            PurchaseProduct p = new PurchaseProduct();
            p.ProductId = item.ProductId;
            p.Description = item.Description;
            p.SubLocationName = item.SubLocationName;
            p.ProductQty = item.ProductQty;
            //p.OrderGUID = item.OrderGUID;
            bool found = false;
            foreach (var or in p.OrderList)
            {
                if (or.OrderGUID == item.OrderGUID)
                {
                    found = true;
                    or.Qty += item.ProductQty;
                }
            }
            if (!found)
            {
                ProductOrder po = new ProductOrder();
                po.OrderGUID = item.OrderGUID;
                po.Qty = item.ProductQty;
                p.OrderList.Add(po);
            }
            return p;
        }

        public ActionResult GetOrderedProductsByLocation()
        {
            ProductsByLocation opr = new ProductsByLocation();
            List<PurchaseProduct> prodList = new List<PurchaseProduct>();
            prodList = GetProductsForPicker();
            foreach (var item in prodList)
            {
                bool found = false;
                foreach (var item1 in opr.LocationProductList)
                {
                    if (item1.Location.Name == item.Location.Name)
                    {
                        item1.Location.Qty += item.ProductQty;
                        bool lfound = false;
                        foreach (var prod in item1.ProductRestaurantList)
                        {
                            if (prod.Product.ProductId == item.ProductId)
                            {
                                bool pfound = false;
                                prod.Product.ProductQty += item.ProductQty;
                                //bool fo = false;
                                //foreach (var x in prod.Product.OrderList)
                                //{
                                //    if (x.OrderGUID == item.OrderGUID)
                                //    {
                                //        fo = true;
                                //        x.Qty += item.ProductQty;
                                //    }
                                //}
                                //if (!fo)
                                //{
                                //    ProductOrder p = new ProductOrder();
                                //    p.OrderGUID = item.OrderGUID;
                                //    p.Qty = item.ProductQty;
                                //    prod.Product.OrderList.Add(p);
                                //}
                                foreach (var res in prod.RestaurantList)
                                {
                                    if (res.RestaurantName == item.TableName)
                                    {
                                        pfound = true;
                                        res.Qty += item.ProductQty;
                                        bool fo = false;
                                        foreach (var x in res.OrderList)
                                        {
                                            if (x.OrderGUID == item.OrderGUID)
                                            {
                                                fo = true;
                                                x.Qty += item.ProductQty;
                                            }
                                        }
                                        if (!fo)
                                        {
                                            ProductOrder p = new ProductOrder();
                                            p.OrderGUID = item.OrderGUID;
                                            p.Qty = item.ProductQty;
                                            res.OrderList.Add(p);
                                        }
                                    }
                                }
                                if (!pfound)
                                {
                                    Restaurant resInstance = new Restaurant();
                                    resInstance.Qty = item.ProductQty;
                                    resInstance.RestaurantName = item.TableName;
                                    resInstance.ProductId = item.ProductId;
                                    resInstance.OrderGUID = item.OrderGUID;
                                    resInstance.RestaurantId = item.Restaurant.RestaurantId;
                                    
                                    bool fo = false;
                                    foreach (var x in resInstance.OrderList)
                                    {
                                        if (x.OrderGUID == item.OrderGUID)
                                        {
                                            fo = true;
                                            x.Qty += item.ProductQty;
                                        }
                                    }
                                    if (!fo)
                                    {
                                        ProductOrder p = new ProductOrder();
                                        p.OrderGUID = item.OrderGUID;
                                        p.Qty = item.ProductQty;
                                        resInstance.OrderList.Add(p);
                                    }
                                    prod.RestaurantList.Add(resInstance);
                                }
                                lfound = true;
                            }
                        }
                        if (!lfound)
                        {
                            ProductRestaurantList pl = new ProductRestaurantList();
                            pl.Product = AddProduct(item);
                            Restaurant resInstance = new Restaurant();
                            resInstance.Qty = item.ProductQty;
                            resInstance.RestaurantName = item.TableName;
                            resInstance.ProductId = item.ProductId;
                            resInstance.OrderGUID = item.OrderGUID;
                            resInstance.RestaurantId = item.Restaurant.RestaurantId;
                            bool fo = false;
                            foreach (var x in resInstance.OrderList)
                            {
                                if (x.OrderGUID == item.OrderGUID)
                                {
                                    fo = true;
                                    x.Qty += item.ProductQty;
                                }
                            }
                            if (!fo)
                            {
                                ProductOrder p = new ProductOrder();
                                p.OrderGUID = item.OrderGUID;
                                p.Qty = item.ProductQty;
                                resInstance.OrderList.Add(p);
                            }
                            pl.RestaurantList.Add(resInstance);
                            item1.ProductRestaurantList.Add(pl);
                            
                        }
                        found = true;
                    }
                }
                if (!found)
                {
                    //ProductsByLocation oprs = new ProductsByLocation();
                    Restaurant resInstance = new Restaurant();
                    resInstance.Qty += item.ProductQty;
                    resInstance.RestaurantName = item.TableName;
                    resInstance.ProductId = item.ProductId;
                    resInstance.OrderGUID = item.OrderGUID;
                    resInstance.RestaurantId = item.Restaurant.RestaurantId;
                    LocationProductList lpl = new LocationProductList();
                    lpl.Location.Name = item.Location.Name;
                    lpl.Location.Qty += item.ProductQty;
                    ProductRestaurantList prl = new ProductRestaurantList();
                    prl.Product = AddProduct(item);
                    bool fo = false;
                    foreach (var x in resInstance.OrderList)
                    {
                        if (x.OrderGUID == item.OrderGUID)
                        {
                            fo = true;
                            x.Qty += item.ProductQty;
                        }
                    }
                    if (!fo)
                    {
                        ProductOrder p = new ProductOrder();
                        p.OrderGUID = item.OrderGUID;
                        p.Qty = item.ProductQty;
                        resInstance.OrderList.Add(p);
                    }
                    prl.RestaurantList.Add(resInstance);
                    lpl.ProductRestaurantList.Add(prl);
                    opr.LocationProductList.Add(lpl);
                    
                }
            }

            return Json(opr, JsonRequestBehavior.AllowGet);
        }

        private List<PurchaseProduct> GetProductsForPicker()
        {
            HushiDataManager manager1 = new HushiDataManager();
            DataTable results = manager1.ExecuteDataTable("usp_AN_GetOrderedProductsByRestaurant");
            List<PurchaseProduct> prodList = new List<PurchaseProduct>();
            foreach (DataRow row in results.Rows)
            {
                PurchaseProduct productInstance = new PurchaseProduct();
                productInstance.ProductId = FieldConverter.To<int>(row["ProductID"]);
                productInstance.Description = FieldConverter.To<string>(row["Description"]);
                productInstance.TableName = FieldConverter.To<string>(row["TableNumber"]);
                productInstance.Location.Name = FieldConverter.To<string>(row["StorageLocation"]);
                productInstance.SubLocationName = FieldConverter.To<string>(row["StorageSubLocation"]);
                productInstance.ProductQty = FieldConverter.To<int>(row["Qty"]);
                productInstance.OrderGUID = FieldConverter.To<Guid>(row["OrderGUID"]);
                productInstance.Restaurant.RestaurantId = FieldConverter.To<int>(row["RestaurantId"]);
                prodList.Add(productInstance);
            }
            return prodList;
        }

        public ActionResult SavePickedProducts(ProductsSaveRequest productsSaveRequest)
        {
            string response = "";
            try
            {
                foreach (var item in productsSaveRequest.PickedProducts)
                {
                    for (int i = 0; i < item.ProductQty; i++)
                    {
                        HushiDataManager manager = new HushiDataManager();
                        manager.AddParameter("@PID", item.ProductId);
                        manager.AddParameter("@OrderGUID", item.OrderGUID);
                        manager.AddParameter("@PickedBy", item.PickedBy);
                        //manager.AddParameter("@RestaurantId", item.Restaurant.RestaurantId);
                        manager.ExecuteNonQuery("usp_AN_UpdatePickedProduct");
                    }
                }

                response = "success";
            }
            catch (Exception ex)
            {

                response = ex.InnerException.Message;
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public static Image Base64ToImage(string base64String)
        {
            string a = Regex.Replace(base64String, "[%]", "");
            byte[] imageBytes = Convert.FromBase64String(base64String);
            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
            ms.Write(imageBytes, 0, imageBytes.Length);
            Image image = Image.FromStream(ms, true);
            return image;
        }

        public ActionResult GetItemsForDelivery()
        {
            DeliveryProductsResponse opr = new DeliveryProductsResponse();

            HushiDataManager manager1 = new HushiDataManager();
            DataTable results = manager1.ExecuteDataTable("usp_AN_GetProductsForDeliveryByRestaurant");

            List<PurchaseProduct> prodList = new List<PurchaseProduct>();
            foreach (DataRow row in results.Rows)
            {
                PurchaseProduct productInstance = new PurchaseProduct();
                productInstance.ProductId = FieldConverter.To<int>(row["ProductID"]);
                productInstance.Description = FieldConverter.To<string>(row["Description"]);
                productInstance.TableName = FieldConverter.To<string>(row["TableNumber"]);
                productInstance.Location.Name = FieldConverter.To<string>(row["StorageLocation"]);
                productInstance.SubLocationName = FieldConverter.To<string>(row["StorageSubLocation"]);
                productInstance.ProductQty = FieldConverter.To<int>(row["Qty"]);
                productInstance.OrderGUID = FieldConverter.To<Guid>(row["OrderGUID"]);
                prodList.Add(productInstance);
            }

            foreach (var item in prodList)
            {
                bool found = false;
                foreach (var item1 in opr.orderProductsDeliveries)
                {
                    if (item1.Restaurant.RestaurantName == item.TableName)
                    {
                        item1.Restaurant.Qty += item.ProductQty;
                        bool pfound = false;
                        foreach (var prod in item1.ProductList)
                        {
                            if (prod.ProductId == item.ProductId)
                            {
                                pfound = true;
                                prod.ProductQty += item.ProductQty;
                                bool fo = false;
                                foreach (var x in prod.OrderList)
                                {
                                    if (x.OrderGUID == item.OrderGUID)
                                    {
                                        fo = true;
                                        x.Qty += item.ProductQty;
                                    }
                                }
                                if (!fo)
                                {
                                    ProductOrder p = new ProductOrder();
                                    p.OrderGUID = item.OrderGUID;
                                    p.Qty = item.ProductQty;
                                    prod.OrderList.Add(p);
                                }
                            }
                        }
                        if (!pfound)
                        {
                            item1.ProductList.Add(AddProduct(item));
                        }
                        found = true;
                    }
                }
                if (!found)
                {
                    OrderProductsDelivery oprs = new OrderProductsDelivery();
                    Restaurant res = new Restaurant();
                    res.Qty += item.ProductQty;
                    res.RestaurantName = item.TableName;
                    oprs.ProductList.Add(AddProduct(item));
                    oprs.Restaurant = res;
                    opr.orderProductsDeliveries.Add(oprs);
                }
            }
            return Json(opr, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDeliveryList(int UserId)
        {
            HushiDataManager manager1 = new HushiDataManager();
            manager1.AddParameter("@UserId", UserId);
            DataTable results = manager1.ExecuteDataTable("usp_AN_GetDeliveryListByUser");
            List<DeliveryListResponse> dlrList = new List<DeliveryListResponse>();
            foreach (DataRow row in results.Rows)
            {
                bool found = false;
                foreach (var item in dlrList)
                {
                    if (item.OrderDeliveryId == FieldConverter.To<int>(row["OrderDeliveryId"]))
                    {
                        PurchaseProduct p = new PurchaseProduct();
                        p.Description = FieldConverter.To<string>(row["Description"]);
                        p.ProductQty = FieldConverter.To<int>(row["Qty"]);
                        item.ProductList.Add(p);
                        item.Restaurant.Qty += FieldConverter.To<int>(row["Qty"]);
                        found = true;
                    }
                }
                if (!found)
                {
                    DeliveryListResponse dlr = new DeliveryListResponse();
                    Restaurant resInstance = new Restaurant();
                    resInstance.RestaurantName = FieldConverter.To<string>(row["Location"]);
                    resInstance.PickDate = FieldConverter.To<string>(row["PickDate"]);
                    resInstance.Qty += FieldConverter.To<int>(row["Qty"]);
                    PurchaseProduct p = new PurchaseProduct();
                    p.Description = FieldConverter.To<string>(row["Description"]);
                    p.ProductQty = FieldConverter.To<int>(row["Qty"]);
                    dlr.Restaurant = resInstance;
                    dlr.OrderDeliveryId = FieldConverter.To<int>(row["OrderDeliveryId"]);
                    dlr.ProductList.Add(p);
                    dlrList.Add(dlr);
                }
            }
            return Json(dlrList, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveItemsForDelivery(ProductsSaveRequest productsSaveRequest)
        {
            string response = "";
            try
            {
                string fileName = "";
                //if (productsSaveRequest.Signature != "")
                //{
                //    fileName = "Delivery" + Convert.ToString(productsSaveRequest.PickedProducts[0].PickedBy + Convert.ToString(DateTime.Now));
                //    Image img = Base64ToImage(productsSaveRequest.Signature);
                //    string savedFilePath = Path.Combine(Server.MapPath("~/Content/Images/Signature/"), fileName);
                //    img.Save(savedFilePath);
                //}

                foreach (var item in productsSaveRequest.PickedProducts)
                {
                    for (int i = 0; i < item.ProductQty; i++)
                    {
                        HushiDataManager manager = new HushiDataManager();
                        manager.AddParameter("@PID", item.ProductId);
                        manager.AddParameter("@OrderGUID", item.OrderGUID);
                        manager.AddParameter("@UserId", item.PickedBy);
                        manager.AddParameter("@Qty", 1);
                        manager.AddParameter("@Signature", fileName);
                        manager.ExecuteNonQuery("usp_AN_InsertOrderDelivery");
                    }
                   
                }

                response = "success";
            }
            catch (Exception ex)
            {

                response = ex.InnerException.Message;
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveDeliveryList(List<DeliveryListResponse> deliveryList)
        {
            string response = "";
            try
            {
                string fileName = "";
                foreach (var item in deliveryList)
                {
                    HushiDataManager manager = new HushiDataManager();
                    manager.AddParameter("@OrderDeliveryId", item.OrderDeliveryId);
                    manager.AddParameter("@ReceivedBy", item.ReceiverName);
                    manager.AddParameter("@ReceivedSignature", item.ReceiverName);
                    //manager.AddParameter("@UserId", item.ReceiverName);
                    manager.AddParameter("@ReceivedDate",DbType.DateTime, Convert.ToDateTime(item.ReceiveDate));
                    manager.ExecuteNonQuery("usp_AN_UpdateOrderDelivery");
                    foreach (var item1 in item.ProductList)
                    {
                        HushiDataManager manager1 = new HushiDataManager();
                        manager1.AddParameter("@OrderPartId", item1.OrderPartId);
                        manager1.AddParameter("@ReceivedQty", item1.ReceivedQty);
                        manager1.ExecuteNonQuery("usp_AN_UpdateDeliveryItem");
                    }

                }

                response = "success";
            }
            catch (Exception ex)
            {

                response = ex.InnerException.Message;
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SyncDeliveryList(SyncDeliveryResponse sdr)
        {
            SyncDeliveryResponse sd = new SyncDeliveryResponse();
            try
            {
                string fileName = "";
                foreach (var item in sdr.DeliveryList)
                {
                    if (item.ReceiveDate != null && item.ReceiverName != null)
                    {
                    HushiDataManager manager = new HushiDataManager();
                    manager.AddParameter("@OrderDeliveryId", item.OrderDeliveryId);
                    manager.AddParameter("@ReceivedBy", item.ReceiverName);
                    manager.AddParameter("@ReceivedSignature", item.ReceiverName);
                    manager.AddParameter("@ReceivedDate", DbType.DateTime, Convert.ToDateTime(item.ReceiveDate));
                    manager.ExecuteNonQuery("usp_AN_UpdateOrderDelivery");
                    foreach (var item1 in item.ProductList)
                    {
                        HushiDataManager manager1= new HushiDataManager();
                            manager1.AddParameter("@OrderDeliveryId", item.OrderDeliveryId);
                            manager1.AddParameter("@ProductId", item1.ProductId);
                            manager1.AddParameter("@ReceivedQty", 1);
                            manager1.AddParameter("@ReceivedDate", DbType.DateTime, Convert.ToDateTime(item.ReceiveDate));
                            manager1.ExecuteNonQuery("usp_AN_UpdateDeliveryItem");
                    }
                    }
                }

                HushiDataManager manager2 = new HushiDataManager();
                manager2.AddParameter("@UserId", sdr.UserId);
                DataTable results = manager2.ExecuteDataTable("usp_AN_GetDeliveryListByUser");
                foreach (DataRow row in results.Rows)
                {
                    if(FieldConverter.To<string>(row["ReceivedDate"]) != "" &&
                        FieldConverter.To<string>(row["ReceivedBy"]) != "")
                    {
                        bool found = false;
                        foreach (var item in sd.DeliveredList)
                        {
                            if (item.OrderDeliveryId == FieldConverter.To<int>(row["OrderDeliveryId"]))
                            {
                                item.ProductList = GetProductDetails(row, item.ProductList);
                                item.Restaurant.Qty += FieldConverter.To<int>(row["Qty"]);
                                found = true;
                            }
                        }
                        if (!found)
                        {
                            DeliveryListResponse dlr = new DeliveryListResponse();
                            dlr.OrderDeliveryId = FieldConverter.To<int>(row["OrderDeliveryId"]);
                            dlr.Restaurant = PopulateRestaurantList(row);
                            dlr.ProductList = GetProductDetails(row, dlr.ProductList);
                            sd.DeliveredList.Add(dlr);
                        }
                    }
                    else
                    {
                        bool found = false;
                        foreach (var item in sd.DeliveryList)
                        {
                            if (item.OrderDeliveryId == FieldConverter.To<int>(row["OrderDeliveryId"]))
                            {
                             
                                item.ProductList = GetProductDetails(row, item.ProductList);
                                item.Restaurant.Qty += FieldConverter.To<int>(row["Qty"]);
                                found = true;
                            }
                        }
                        if (!found)
                        {
                            DeliveryListResponse dlr = new DeliveryListResponse();
                            dlr.OrderDeliveryId = FieldConverter.To<int>(row["OrderDeliveryId"]);
                            dlr.Restaurant = PopulateRestaurantList(row);
                            dlr.ProductList = GetProductDetails(row,dlr.ProductList);
                            sd.DeliveryList.Add(dlr);
                        }
                    }
                }
                sd.response = "success";
            }
            catch (Exception ex)
            {

                sd.response = ex.InnerException.Message;
            }

            return Json(sd, JsonRequestBehavior.AllowGet);
        }

        private List<PurchaseProduct> GetProductDetails(DataRow row,List<PurchaseProduct> pl)
        {
            bool found = false;
            foreach (var item in pl)
            {

                if (item.ProductId == FieldConverter.To<int>(row["ProductId"]))
                {
                    found = true;
                    item.ProductQty += FieldConverter.To<int>(row["Qty"]);
                }
            }
            if (!found)
            {
                PurchaseProduct p = new PurchaseProduct();
                p.Description = FieldConverter.To<string>(row["Description"]);
                p.ProductQty = FieldConverter.To<int>(row["Qty"]);
                p.ProductId = FieldConverter.To<int>(row["ProductId"]);
                pl.Add(p);
            }
            return pl;
        }

        private Restaurant PopulateRestaurantList(DataRow row)
        {
            Restaurant resInstance = new Restaurant();
            resInstance.RestaurantName = FieldConverter.To<string>(row["Location"]);
            resInstance.PickDate = FieldConverter.To<string>(row["PickDate"]);
            resInstance.Qty += FieldConverter.To<int>(row["Qty"]);
            return resInstance;
        }
    }
}