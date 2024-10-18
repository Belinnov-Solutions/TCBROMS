using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Entity;
using TCBROMS_Android_Webservice.Models;
using System.Data;
using Deznu.Products.Common.Utility;
using System.Globalization;
using System.Transactions;
using System.Data.Entity.Core.Objects;
using System.Data.Entity;
using System.IO;
using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Cryptography;
using Entity.Models;
using Stripe;
using TCBROMS_Android_Webservice.Helpers;
using System.Web.Caching;
using Microsoft.Ajax.Utilities;
using ZXing;
using System.Drawing;
using System.Drawing.Imaging;
using Entity.Enums;
using System.Net.Http;
using System.Runtime.Caching;
using System.Diagnostics;
using NLog;

namespace TCBROMS_Android_Webservice.Controllers
{
    //[AllowCrossSite]
    public class HomeController : Controller
    {
        ChineseTillEntities1 dbContext = new ChineseTillEntities1();
        ROMSHelper rh = new ROMSHelper();
        string url = System.Configuration.ConfigurationManager.AppSettings["TCBAPIUrl"];
        Logger logger = LogManager.GetLogger("databaseLogger");
        public ActionResult Index()
        {
            return View();
        }
        
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Login(User user)

        {
            logger.Info("Login by user" + user.UserCode);
            Models.UserService us = new Models.UserService();
            User userInstance = us.UserLogin(user);
            return Json(userInstance, JsonRequestBehavior.AllowGet);
        }

        public ActionResult HealthCheck()

        {
            
            return Json("success", JsonRequestBehavior.AllowGet);
        }

        public ActionResult Printers()

        {
            ChineseTillEntities1 context = new ChineseTillEntities1();
            SqlDataManager manager = new SqlDataManager();
            List<Printer> pr = new List<Printer>();

            //Need to remove comment 
            var printer = from p in context.tblPrinters
                          select new Printer
                          {
                              PrinterID = p.PrinterID,
                              PrinterName = p.PrinterName,
                              Offline = p.Offline
                          };
            //foreach (var item in printer)
            //{
            //    pr.Add(new Printer
            //    {
            //        PrinterID = item.PrinterID,
            //        PrinterName = item.PrinterName,

            //    });

            //}
            return Json(printer, JsonRequestBehavior.AllowGet);


        }

        public ActionResult RefillProductList()

        {
            ChineseTillEntities1 context = new ChineseTillEntities1();
            List<RefillProduct> pr = new List<RefillProduct>();
            var products = context.tblProducts.OrderBy(x => x.ProductID).
                                               Where(x => x.DelInd == false && x.FoodRefil == true).ToList();
            foreach (var item in products)
            {
                pr.Add(new RefillProduct
                {
                    ProductId = item.ProductID,
                    ProductName = item.Description,
                    ProductCode = item.ProductCode,
                    ChineseName = item.ChineseName
                });
            }
            return Json(pr, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RefillProducts(RefillProductRequest request)

        {

            string nl = System.Environment.NewLine;
            String strReceipt = "";
            int UserID = 0;
            string PCName = "";
            string PrinterName = "";
            UserID = request.UserId;
            PCName = request.PCName;
            PrinterName = request.PrinterName;
            int qty = 0;

            foreach (var row in request.rfProducts)
            {
                if (strReceipt == "")
                {
                    strReceipt = "----------------------------" + nl;
                    strReceipt = strReceipt + "        " + DateTime.Now.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture) + nl;
                    strReceipt = strReceipt + "Ordered by " + request.UserName + nl + nl;
                }
                strReceipt = strReceipt + row.ProductCode + "  " + row.ProductName + nl;
                strReceipt = strReceipt + row.ChineseName + nl;
                string size = "";
                if (row.ProductQty == 0)
                    qty = 1;
                else
                    qty = row.ProductQty;
                //if (row.ProductSize == "R")
                //{
                //    strReceipt = strReceipt + ", " + qty + "  中," + nl;
                //    size = "REGULAR 中";
                //}
                //else if (row.ProductSize == "S")
                //{
                //    strReceipt = strReceipt + ", " + qty + " 小," + nl;
                //    size = "SMALL 小";
                //}
                //else if (row.ProductSize == "XS")
                //{
                //    strReceipt = strReceipt + ", " + qty + " 超小," + nl;
                //    size = "EXTRA Small 超小";
                //}

                strReceipt = strReceipt + ", " + qty;
                size = "REGULAR 中";
                strReceipt = strReceipt + "----------------------------" + nl + nl;
                SqlDataManager manager = new SqlDataManager();
                manager.AddParameter("@PID", row.ProductId);
                manager.AddParameter("@ProductName", row.ProductName);
                manager.AddParameter("@Size", size);
                manager.AddParameter("@ProductQty", row.ProductQty);
                manager.AddParameter("@UserFk", UserID);
                manager.AddParameter("@PCName", PCName);
                manager.ExecuteNonQuery("usp_AN_InsertRefillProduct");
            }
            SqlDataManager manager1 = new SqlDataManager();
            manager1.AddParameter("@User", UserID);
            manager1.AddParameter("@PC", PCName);
            manager1.AddParameter("@Printer", PrinterName);
            manager1.AddParameter("@Receipt", strReceipt);
            manager1.ExecuteNonQuery("usp_AN_InsertPrintQueue");
            return Json("message = success", JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateRefillProduct(List<RefillProduct> productList)

        {
            foreach (var item in productList)
            {
                SqlDataManager manager = new SqlDataManager();
                manager.AddParameter("@PID", item.ProductRefillID);
                manager.ExecuteNonQuery("usp_AN_UpdateRefillProduct");
            }
            return Json("message = success", JsonRequestBehavior.AllowGet);
        }
        public ActionResult Tables()

        {
            Models.UserService us = new Models.UserService();
            TablesList tablesList = us.GetTablesList();
            //string s = JsonConvert.SerializeObject(tablesList);
            //string t = CompressData(s);
            //return Content(s);

            return Json(tablesList, JsonRequestBehavior.AllowGet);
        }


        public String CompressData(string INPUTDATA)
        {
            String compressedDATA = "";
            try
            {
                byte[] uncompressedbytes = new byte[INPUTDATA.Length];
                int index = 0;
                foreach (char item in INPUTDATA.ToCharArray())
                {
                    uncompressedbytes[index] = (byte)item;
                    index++;
                }

                using (MemoryStream memorystream = new MemoryStream())
                {
                    using (GZipStream gzipstream = new GZipStream(memorystream, CompressionMode.Compress, true))
                    {
                        gzipstream.Write(uncompressedbytes, 0, uncompressedbytes.Length);
                    }
                    byte[] compressedbytes = memorystream.ToArray();
                    StringBuilder SB = new StringBuilder(compressedbytes.Length);
                    for (int i = 0; i < compressedbytes.Length; i++)
                    {
                        SB.Append((char)compressedbytes[i]);
                    }
                    compressedDATA = SB.ToString();
                }
            }
            catch (Exception ex)
            {
                compressedDATA = "";
            }
            return compressedDATA;
        }


        public ActionResult ContactNumbers()

        {
            ChineseTillEntities1 context = new ChineseTillEntities1();
            var c = context.tblCustomers;
            List<String> contacts = new List<string>();
            foreach (var item in c)
            {
                contacts.Add(item.Mobile);
            }
            return Json(contacts, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CustomerDetails(String mobile)

        {
            SqlDataManager manager = new SqlDataManager();
            manager.AddParameter("@Mobile", mobile);
            DataTable results = manager.ExecuteDataTable("usp_AN_GetCustomerDetails");
            Entity.Customer c = new Entity.Customer();
            List<String> r = new List<String>();
            foreach (DataRow row in results.Rows)
            {
                String p = "";
                c.CustomerID = FieldConverter.To<int>(row["CustomerID"]);
                c.Mobile = FieldConverter.To<String>(row["Mobile"]);
                c.Name = FieldConverter.To<String>(row["Name"]);
                c.EmailID = FieldConverter.To<String>(row["EmailID"]);
                c.DOB = FieldConverter.To<String>(row["DOB"]);
                //p = FieldConverter.To<DateTime>(row["DOA"]).ToString("dd/MM", CultureInfo.InvariantCulture);
                p = FieldConverter.To<String>(row["DOA"]);
                c.PrevOrdersDate.Add(p);
            }
            //ChineseTillEntities1 context = new ChineseTillEntities1();
            //var c = from a in context.tblCustomers
            //        join b in context.tblCustomerOrders on a.CustomerID equals b.CustomerID
            //        join o in context.tblOrders on b.OrderGUID equals o.OrderGUID
            //        where a.Mobile == mobile
            //        select new { a.CustomerID,a.Name, a.Mobile, a.DOA, a.DOB, a.EmailID, o.DateCreated };
            //Customer q = new Customer();

            //if (c.Count() != 0)
            //{
            //    foreach (var item in c)
            //    {

            //        q.CustomerID = item.CustomerID;
            //        q.Name = item.Name;
            //        q.Mobile = item.Mobile;
            //        q.EmailID = item.EmailID;
            //        q.DOB = item.DOB;
            //        q.DOA = item.DOA;
            //        p = item.DateCreated.ToString("dd/MM", CultureInfo.InvariantCulture);
            //        r.Add(p);
            //    }
            //}
            return Json(c, JsonRequestBehavior.AllowGet);

        }

        public ActionResult Products()

        {
            Models.UserService us = new Models.UserService();
            AllProducts productsList = us.GetProductsList();

            return Json(productsList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ProductsLinker()

        {
            ChineseTillEntities1 context = new ChineseTillEntities1();
            var a = context.tblProductLinkers;
            List<ProductLinker> pl = new List<ProductLinker>();

            foreach (var item in a)
            {
                ProductLinker p = new ProductLinker();
                p.PrimaryProductID = item.PrimaryProductID;
                p.SecondaryProductID = item.SecondaryProductID;
                pl.Add(p);
            }
            return Json(pl, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SubmitOrder(TableOrder t)

        {
            Models.OrderService os = new Models.OrderService();
            OrderSubmitResponse r = os.SubmitOrder(t);
            return Json(r, JsonRequestBehavior.AllowGet);
        }



        public ActionResult GetTemplates()
        {
            ChineseTillEntities1 context = new ChineseTillEntities1();
            var RefillTemplate = from rt in context.tblProductRefillTemplates
                                 where rt.DelInd == false
                                 select new
                                 {
                                     TemplateID = rt.iRefillTemplateID,
                                     TemplateName = rt.TemplateName
                                 };

            List<RefillTemplate> rft = new List<RefillTemplate>();
            foreach (var item in RefillTemplate)
            {
                rft.Add(new RefillTemplate
                {
                    TemplateID = item.TemplateID,
                    TemplateName = item.TemplateName,

                });

            }
            return Json(rft, JsonRequestBehavior.AllowGet);
        }

        public ActionResult TemplateProducts(int TemplateId)
        {
            SqlDataManager manager1 = new SqlDataManager();
            manager1.AddParameter("@TemplateFK", TemplateId);
            DataTable results = manager1.ExecuteDataTable("usp_SS_LoadRefillTemplateItems");
            List<TemplateProduct> productslist = new List<TemplateProduct>();
            foreach (DataRow row in results.Rows)
            {
                TemplateProduct productInsatance = new TemplateProduct();
                productInsatance.ProductID = FieldConverter.To<int>(row["ProductFK"]);
                productInsatance.ProductCode = FieldConverter.To<int>(row["ProductCode"]);
                productInsatance.Description = FieldConverter.To<string>(row["Description"]);
                productInsatance.ChineseName = FieldConverter.To<string>(row["ChineseName"]);
                productInsatance.SmallPortion = FieldConverter.To<Boolean>(row["SmallPortion"]);
                productInsatance.RegularPortion = FieldConverter.To<Boolean>(row["RegularPortion"]);
                productInsatance.LargePortion = FieldConverter.To<Boolean>(row["LargePortion"]);
                productslist.Add(productInsatance);
            }
            return Json(productslist, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetUserOrders(int t)

        {
            Models.OrderService os = new Models.OrderService();
            UserOrders a = os.GetUserTables(t);
            return Json(a, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTableOrder(int t)

        {
            Models.OrderService os = new Models.OrderService();
            TableOrder to = os.GetTableOrder(t);
            return Json(to, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UserLogout(int UserId)

        {
            ChineseTillEntities1 context = new ChineseTillEntities1();
            context.usp_AN_LogoutUser(UserId);
            return Json("success", JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateServeTime(int OrderPartID, int ProductId, int OptionId, Guid OrderId, int OrderNo)

        {
            //ChineseTillEntities1 context = new ChineseTillEntities1();
            //(from p in context.tblOrderParts
            // where p.ProductID == ProductId & p.OrderGUID == OrderId & p.OrderNo == OrderNo
            // select p).ToList().ForEach(x => x.DateServed = DateTime.Now);
            //string date = DateTime.Now.ToString();
            Models.OrderService os = new Models.OrderService();
            string date = os.UpdateServeTime(OrderPartID, ProductId, OptionId, OrderId, OrderNo);
            return Json(date, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ProductGroups()
        {
            logger.Info("Starting GetProducts");
            var cache = MemoryCache.Default;
            AllProducts plist = new AllProducts();
            if (cache.Get("ProductsList") == null)
            {
                var cachePolicy = new CacheItemPolicy();
                cachePolicy.AbsoluteExpiration = DateTime.Now.AddMinutes(60);
                int currTime = Convert.ToInt32(DateTime.Now.ToString("HHMM"));
                int currDay = (int)DateTime.Today.DayOfWeek;
                if (currDay == 0)
                    currDay = 7;
                using (TransactionScope scope = new TransactionScope())
                {

                    List<ProductGroup> pg = new List<ProductGroup>();
                    List<Entity.Product> p = new List<Entity.Product>();
                    SqlDataManager manager = new SqlDataManager();
                    DataTable results = manager.ExecuteDataTable("usp_AN_GetProducts");
                    foreach (DataRow row in results.Rows)
                    {
                        Entity.Product pr = new Entity.Product();
                        pr.ProductID = FieldConverter.To<int>(row["ProductID"]);
                        pr.Description = FieldConverter.To<string>(row["Description"]);
                        pr.Price = FieldConverter.To<float>(row["Price"]);
                        pr.ProductGroupID = FieldConverter.To<int>(row["ProductGroupID"]);
                        //pr.ParentGroupID = FieldConverter.To<int>(row["ParentGroupID"]);
                        //pr.GroupName = FieldConverter.To<string>(row["GroupName"]);
                        pr.HasLinkedProducts = FieldConverter.To<Boolean>(row["HasLinkedProducts"]);
                        pr.ProductAvailable = FieldConverter.To<Boolean>(row["ProductAvailable"]);
                        if (pr.ProductAvailable)
                        {
                            p.Add(pr);
                        }
                    }
                    SqlDataManager manager1 = new SqlDataManager();
                    DataTable results1 = manager1.ExecuteDataTable("usp_AN_GetProductGroup");
                    foreach (DataRow row in results1.Rows)
                    {
                        ProductGroup pr = new ProductGroup();
                        pr.ProductGroupID = FieldConverter.To<int>(row["ProductGroupID"]);
                        pr.Groupname = FieldConverter.To<string>(row["Groupname"]);
                        pr.ParentGroupID = FieldConverter.To<int>(row["ParentGroupID"]);
                        pg.Add(pr);

                    }

                    SqlDataManager manager2 = new SqlDataManager();
                    DataTable results2 = manager2.ExecuteDataTable("usp_AN_GetProductLinker");
                    List<ProductLinker> pl = new List<ProductLinker>();
                    foreach (DataRow row in results2.Rows)
                    {
                        ProductLinker pr = new ProductLinker();
                        pr.PrimaryProductID = FieldConverter.To<int>(row["PrimaryProductID"]);
                        pr.SecondaryProductID = FieldConverter.To<int>(row["SecondaryProductID"]);
                        pl.Add(pr);
                    }

                    plist.productsLinker = pl;
                    plist.pGroupList = pg;
                    plist.productsList = p;
                    plist.BuffetMenu = rh.GetBuffetMenu();
                    scope.Complete();
                }
                cache.Add("ProductsList", plist, cachePolicy);
            }
            else
            {
               plist =(AllProducts)cache.Get("ProductsList");
            }
            logger.Info("Finished GetProducts");
            return Json(plist, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ProductsWastage(ProductWastage productList)

        {
            string response = "success";
            ProductWastage pw = productList;
            try
            {
                foreach (var item in pw.ProductList)
                {
                    SqlDataManager manager = new SqlDataManager();
                    manager.AddParameter("@UserID", pw.UserID);
                    manager.AddParameter("@ProductID", item.ProductID);
                    manager.AddParameter("@Qty", item.WastageQty);
                    manager.ExecuteNonQuery("usp_AN_InsertProductWastage");
                }
            }
            catch (Exception ex)
            {

                response = ex.InnerException.Message;
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            try
            {
                //foreach (var item in pw.Images)
                //{
                //    ChineseTillEntities1 context = new ChineseTillEntities1();
                //    tblWastageImage twi = new tblWastageImage();
                //    twi.UserId = pw.UserID;
                //    twi.Image = item;
                //    twi.DateCreated = DateTime.Now;
                //    context.tblWastageImages.Add(twi);
                //    context.SaveChanges();
                //}
            }
            catch (Exception ex)
            {

                response = ex.InnerException.Message;
                return Json(response, JsonRequestBehavior.AllowGet);
            }


            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RequestTableBill(int TableId, Guid OrderId)

        {
            int row = 0;
            if (dbContext.tblOrders.Any(x => x.OrderGUID == OrderId && x.Paid == true && x.DelInd == false))
            {
                return Json("success", JsonRequestBehavior.AllowGet);
            }
            else
            {
                SqlDataManager manager = new SqlDataManager();
                manager.AddParameter("@TableID", TableId);
                manager.AddParameter("@OrderID", OrderId);
                manager.AddOutputParameter("@UpdatedRow", DbType.Int32, row);
                manager.ExecuteNonQuery("usp_AN_RequestBill");
                row = FieldConverter.To<Int32>(manager.GetParameterValue("@UpdatedRow"));
                if (row > 0)
                    return Json("success", JsonRequestBehavior.AllowGet);
                else
                    return Json("failure", JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult DrinksTarget()
        {


            Dictionary<int, Targets> targets = new Dictionary<int, Targets>();
            //ChineseTillEntities1 context = new ChineseTillEntities1();
            List<DrinksTarget> pr = new List<DrinksTarget>();
            //var items = from p in context.tblDrinksTargets
            //            join q in context.tblProducts on p.ProductID equals q.ProductID
            //            select new
            //            {
            //                ProductID = p.ProductID,
            //                ProductName = q.Description,
            //                //Type = p.Type,
            //                Target = p.ProductTarget,
            //                Count = p.Count,
            //                TargetDate = p.TargetDate
            //                //Week = p.Week
            //            };
            SqlDataManager manager1 = new SqlDataManager();
            //manager1.AddParameter("@CurrentDate", DateTime.Now.ToString("yyyy-MM-dd"));
            DataTable results = manager1.ExecuteDataTable("usp_AN_GetProductsTarget");
            string currDate = DateTime.Now.ToString("yyyy-MM-dd");
            foreach (DataRow row in results.Rows)
            {
                int target = 0;
                int days = 0;
                TimeSpan t = FieldConverter.To<DateTime>(row["ToDate"]).Subtract(FieldConverter.To<DateTime>(row["FromDate"]));
                days = (int)t.TotalDays;
                if (days == 0)
                    target = FieldConverter.To<int>(row["Target"]);
                else if (days > 0)
                    target = FieldConverter.To<int>(row["Target"]) / days;
                Targets tr = new Targets();
                if (FieldConverter.To<int>(row["ProductID"]) == 0)
                {
                    if (targets.ContainsKey(FieldConverter.To<int>(row["ProductTypeID"])))
                    {
                        tr = targets[FieldConverter.To<int>(row["ProductTypeID"])];
                        tr.Target += target;
                    }
                    else
                    {
                        tr.Description = FieldConverter.To<string>(row["TypeDescription"]);
                        tr.Type = "Type";
                        tr.Target = target;
                        targets.Add(FieldConverter.To<int>(row["ProductTypeID"]), tr);
                    }
                }
                else
                {
                    if (targets.ContainsKey(FieldConverter.To<int>(row["ProductID"])))
                    {
                        tr = targets[FieldConverter.To<int>(row["ProductID"])];
                        tr.Target += target;
                    }
                    else
                    {
                        tr.Description = FieldConverter.To<string>(row["Description"]);
                        tr.Type = "Product";
                        tr.Target = target;
                        targets.Add(FieldConverter.To<int>(row["ProductID"]), tr);
                    }
                }
            }
            ChineseTillEntities1 context = new ChineseTillEntities1();
            var categories = from p in context.tblDrinksSolds
                             where DbFunctions.TruncateTime(p.DateCreated) == DbFunctions.TruncateTime(DateTime.Now)
                             group p by new { p.ProductID, p.ProductTypeID } into g
                             select new { ProductID = g.Key.ProductID, ProductTypeID = g.Key.ProductTypeID, Sold = g.Sum(p => p.Qty) };
            foreach (var item in categories)
            {
                int ptid = 0;
                if (item.ProductTypeID != null)
                {
                    ptid = (int)item.ProductTypeID;
                }
                else
                {
                    var tid = (from ps in context.tblProducts
                               where (ps.ProductID == item.ProductID)
                               select new { ProductTypeID = ps.ProductTypeID }).FirstOrDefault();
                    ptid = (int)tid.ProductTypeID;
                }
                Targets t1 = new Targets();
                if (targets.ContainsKey(item.ProductID))
                    t1 = targets[item.ProductID];
                //else if (targets.ContainsKey((int)item.ProductTypeID))
                //t1 = targets[(int)item.ProductTypeID];
                else if (targets.ContainsKey(ptid))
                    t1 = targets[ptid];
                t1.Sold += (int)item.Sold;
            }
            foreach (KeyValuePair<int, Targets> item in targets)
            {
                pr.Add(new DrinksTarget
                {
                    ProductID = item.Key,
                    ProductName = item.Value.Description,
                    TargetDate = currDate,
                    ProductTarget = item.Value.Target,
                    Count = item.Value.Sold,
                    Type = item.Value.Type
                });
            }

            return Json(pr, JsonRequestBehavior.AllowGet);
        }
        public ActionResult UpdateAllProductServed(Guid OrderId, string MenuType)

        {
            //ChineseTillEntities1 context = new ChineseTillEntities1();
            //(from p in context.tblOrderParts
            // where p.ProductID == ProductId & p.OrderGUID == OrderId & p.OrderNo == OrderNo
            // select p).ToList().ForEach(x => x.DateServed = DateTime.Now);
            //string date = DateTime.Now.ToString();
            Models.OrderService os = new Models.OrderService();
            string date = os.UpdateAllProductServeTime(OrderId, MenuType);
            return Json(date, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateWashroomCheck(int UserId, string Name)

        {
            int row = 0;
            SqlDataManager manager = new SqlDataManager();
            manager.AddParameter("@UserID", UserId);
            manager.AddParameter("@Name", Name);
            manager.AddOutputParameter("@UpdatedRow", DbType.Int32, row);
            manager.ExecuteNonQuery("usp_AN_UpdateWashroomCheck");
            row = FieldConverter.To<Int32>(manager.GetParameterValue("@UpdatedRow"));
            if (row > 0)
                return Json("success", JsonRequestBehavior.AllowGet);
            else
                return Json("failure", JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveToken(string DeviceID, string Token)

        {

            int row = 0;
            SqlDataManager manager = new SqlDataManager();
            manager.AddParameter("@DeviceID", DeviceID);
            manager.AddParameter("@Token", Token);
            manager.AddOutputParameter("@UpdatedRow", DbType.Int32, row);
            manager.ExecuteNonQuery("usp_AN_SaveToken");
            row = FieldConverter.To<Int32>(manager.GetParameterValue("@UpdatedRow"));
            if (row > 0)
                return Json("success", JsonRequestBehavior.AllowGet);
            else
                return Json("failure", JsonRequestBehavior.AllowGet);
        }

        public ActionResult CoffeeConfirmed(Guid OrderID, int TableID)

        {
            ChineseTillEntities1 context = new ChineseTillEntities1();
            var result = context.tblOrders.SingleOrDefault(b => b.OrderGUID == OrderID);
            if (result != null)
            {
                result.CoffeeConfirm = true;
                context.SaveChanges();
            }
            var result1 = context.tblTables.SingleOrDefault(b => b.TableID == TableID);
            if (result1 != null)
            {
                result1.CurrentStatus = 1;
                context.SaveChanges();
            }
            //int row = 0;
            //SqlDataManager manager = new SqlDataManager();
            //manager.AddParameter("@DeviceID", DeviceID);
            //manager.AddParameter("@Token", Token);
            //manager.AddOutputParameter("@UpdatedRow", DbType.Int32, row);
            //manager.ExecuteNonQuery("usp_AN_SaveToken");
            //row = FieldConverter.To<Int32>(manager.GetParameterValue("@UpdatedRow"));
            //if (row > 0)
            //    return Json("success", JsonRequestBehavior.AllowGet);
            //else
            //    return Json("failure", JsonRequestBehavior.AllowGet);
            return Json("success", JsonRequestBehavior.AllowGet);
        }

        public ActionResult RepeatDrinksConfirmed(Guid OrderID, int TableID)

        {
            ChineseTillEntities1 context = new ChineseTillEntities1();
            var result = context.tblOrders.SingleOrDefault(b => b.OrderGUID == OrderID);
            if (result != null)
            {
                result.DrinksRep = true;
                context.SaveChanges();
            }
            var result1 = context.tblTables.SingleOrDefault(b => b.TableID == TableID);
            if (result1 != null)
            {
                result1.CurrentStatus = 1;
                context.SaveChanges();
            }
            return Json("success", JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetLatestNews()

        {
            List<String> news = new List<string>();
            //ChineseTillEntities1 context = new ChineseTillEntities1();
            //var n = context.tblNotes.Where(x => x.NoteLiveDate <= DateTime.Now && x.NoteEndDate >= DateTime.Now);




            SqlDataManager manager = new SqlDataManager();
            DataTable results = manager.ExecuteDataTable("usp_AN_GetLatestNews");
            foreach (DataRow row in results.Rows)
            {
                String nw = FieldConverter.To<string>(row["NoteText"]);
                news.Add(nw);
            }


            //foreach (var item in n)
            //{
            //    if(!item.DelInd)
            //    {
            //        news.Add(item.NoteText);
            //    }
            //}
            return Json(news, JsonRequestBehavior.AllowGet);
        }



        public ActionResult GetDashboardItems(int UserLevel)
        {
            ChineseTillEntities1 context = new ChineseTillEntities1();

            var a = from c in context.tblAppOptions
                    where c.DelInd == false && !context.tblAppOptionRestrictions.Any(p => p.AppOptionID == c.ID && p.UserLevel == UserLevel)
                    orderby c.Position ascending
                    select c;
            //var a = from c in context.tblAppOptions
            //        orderby c.Position ascending
            //        select c;
            List<AppOption> r = new List<AppOption>();


            if (a.Count() != 0)
            {
                foreach (var item in a)
                {
                    AppOption q = new AppOption();
                    q.OptionID = item.ID;
                    q.OptionName = item.Name;
                    q.OptionActivity = item.Activity;
                    r.Add(q);
                }
            }

            return Json(r, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetWaitlistDetails()
        {
            SqlDataManager manager = new SqlDataManager();
            DataTable results = manager.ExecuteDataTable("usp_AN_GetWaitListDetails");
            List<Reservation> r = new List<Reservation>();
            foreach (DataRow row in results.Rows)
            {
                Reservation q = new Reservation();
                q.ReservationID = FieldConverter.To<int>(row["ReservationID"]);
                q.Name = FieldConverter.To<string>(row["ReservationUnder"]);
                q.Mobile = FieldConverter.To<string>(row["ContactNumber"]);
                q.AdCnt = FieldConverter.To<int>(row["AdCount"]);
                q.KdCnt = FieldConverter.To<int>(row["KdCount"]);
                q.JnCnt = FieldConverter.To<int>(row["JnCount"]);
                q.TotalSeats = FieldConverter.To<int>(row["NoOfSeats"]);
                q.NoOfGuests = FieldConverter.To<int>(row["NoOfGuests"]);
                q.HighChair = FieldConverter.To<int>(row["NoOfHighChairs"]);
                q.WheelChair = FieldConverter.To<int>(row["NoOfWheelChairs"]);
                q.Prams = FieldConverter.To<int>(row["NoOfPrams"]);
                q.Amount = FieldConverter.To<decimal>(row["Total"]);
                q.ProductID = FieldConverter.To<int>(row["ProductID"]);
                q.ProductDescription = FieldConverter.To<string>(row["Description"]);
                q.Processed = FieldConverter.To<bool>(row["Processed"]);
                q.InLounge = FieldConverter.To<bool>(row["InLounge"]);
                q.UniqueCode = FieldConverter.To<string>(row["UniqueCode"]);
                q.AdditionalNotes = FieldConverter.To<string>(row["ReservationNotes"]);
                string[] tables = FieldConverter.To<string>(row["Tables"]).Split(',');
                q.AllocTable = "";
                foreach (var item in tables)
                {
                    int tid = 0;
                    if (Int32.TryParse(item, out tid))
                    {
                        q.AllocTable += dbContext.tblTables.Where(x => x.TableID == tid).Select(x => x.TableNumber).FirstOrDefault() + ",";
                    }
                    else
                        FieldConverter.To<string>(row["Tables"]);
                   

                }
                //q.AllocTable = FieldConverter.To<string>(row["Tables"]);
                DateTime dt = FieldConverter.To<DateTime>(row["DateCreated"]);
                DateTime currdt = DateTime.Now;
                double minDiff = currdt.Subtract(dt).TotalMinutes;
                q.CustWaitingTime = (long)minDiff;
                q.DateCreated = dt.ToString("dd/MM/yyyy HH:mm:ss");
                q.IncomingTime = dt.ToShortTimeString();
                q.ReservationType = FieldConverter.To<string>(row["Type"]);
                q.WaitingTime = FieldConverter.To<int>(row["ReservationTime"]);
                string hr = q.WaitingTime.ToString().Substring(0, 2);
                string mn = q.WaitingTime.ToString().Substring(2);
                DateTime s = DateTime.Now;
                TimeSpan ts = new TimeSpan(Convert.ToInt32(hr), Convert.ToInt32(mn), 0);
                s = s.Date + ts;
                q.ResTimeLeft = (long)s.Subtract(currdt).TotalMinutes;
                q.ResTimePassed = (long)currdt.Subtract(s).TotalMinutes;
                r.Add(q);
            }



            //ChineseTillEntities1 context = new ChineseTillEntities1();
            //DateTime today = Convert.ToDateTime(DateTime.Now).Date;
            ////var today = DateTime.Today;
            ////var a = from c in context.tblCustomerWaitLists
            ////        join b in context.tblCustomers on c.CustomerID equals b.CustomerID
            ////        where DbFunctions.TruncateTime(c.DateCreated) == today && c.TableID == 0
            ////        orderby c.DateCreated ascending
            ////        select new { c.ID, b.Name, b.Mobile,b.CustomerID,c.AdCount,c.KdCount,c.JnCount,c.DateCreated};

            //var a = from c in context.tblReservations
            //        where DbFunctions.TruncateTime(c.DateCreated) == today
            //        orderby c.DateCreated ascending
            //        select c;
            //List<Customer> r = new List<Customer>();
            //if (a.Count() != 0)
            //{
            //    foreach (var item in a)
            //    {
            //        Customer q = new Customer();
            //        q.ReservationID = item.ReservationID;
            //        q.Name = item.ReservationUnder;
            //        q.Mobile = item.ContactNumber;
            //        q.AdCnt = item.AdCount;
            //        q.KdCnt = item.KdCount;
            //        q.JnCnt = item.JnCount;
            //        q.HighChair = item.NoOfHighChairs;
            //        q.WheelChair = item.NoOfWheelChairs;
            //        q.Prams = item.NoOfPrams;
            //        q.Processed = item.Processed;
            //        q.AllocTable = item.Tables;
            //        q.IncomingTime = item.DateCreated.ToShortTimeString();
            //        r.Add(q);
            //    }
            //}
            return Json(r, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAvailableTables()
        {
            ChineseTillEntities1 context = new ChineseTillEntities1();
            var a = from c in context.tblTables
                    where c.CurrentStatus == 0 && c.DelInd == false
                    orderby c.TableNumber ascending
                    select c;
            List<Table> r = new List<Table>();
            if (a.Count() != 0)
            {
                foreach (var item in a)
                {
                    Table q = new Table();
                    q.TableID = item.TableID;
                    q.TableNumber = item.TableNumber;
                    r.Add(q);
                }
            }
            return Json(r, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AssignTable(int ReservationID, int TableID, int UserID)
        {
            int row = 0;
            SqlDataManager manager = new SqlDataManager();
            manager.AddParameter("@ReservationID", ReservationID);
            manager.AddParameter("@TableID", TableID);
            manager.AddParameter("@UserID", UserID);
            manager.AddOutputParameter("@RID", DbType.Int32, row);
            manager.ExecuteNonQuery("usp_AN_AssignTable");
            row = FieldConverter.To<Int32>(manager.GetParameterValue("@RID"));
            if (row > 0)
                return Json("success", JsonRequestBehavior.AllowGet);
            else
                return Json("failure", JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveCustomerDetails(Entity.Customer CustDetails)
        {
            try
            {
                using (var _dbContext = new ChineseTillEntities1())
                {
                    tblReservation tr = new tblReservation();
                    tr.Tables = "-2";
                    tr.ReservationUnder = CustDetails.Name;
                    tr.EmailID =  CustDetails.EmailID;
                    tr.ContactNumber =  CustDetails.Mobile;
                    tr.DOB =  CustDetails.DOB;
                    tr.ReservationTime =  CustDetails.WaitingTime;
                    tr.AdCount =  CustDetails.AdCnt;
                    tr.KdCount =  CustDetails.KdCnt;
                    tr.JnCount =  CustDetails.JnCnt;
                    tr.UserID =  CustDetails.UserID;
                    tr.ReservationType =  "WL";
                    tr.NoOfGuests = (short) (CustDetails.AdCnt + CustDetails.JnCnt + CustDetails.KdCnt);
                    tr.NoOfSeats = (CustDetails.AdCnt + CustDetails.JnCnt + CustDetails.KdCnt);
                    tr.NoOfHighChairs = CustDetails.HighChair;
                    tr.NoOfWheelChairs = CustDetails.WheelChair;
                    tr.NoOfPrams = CustDetails.Prams;
                    tr.ReservationNotes = CustDetails.Message;
                    tr.DateCreated = DateTime.Now;
                    tr.ReservationDate = DateTime.Now;
                    _dbContext.tblReservations.Add(tr);
                    _dbContext.SaveChanges();
                    return Json("success", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception  ex)
            {

                return Json("failure", JsonRequestBehavior.AllowGet);
            }
            //int row = 0;
            //SqlDataManager manager = new SqlDataManager();
            //manager.AddParameter("@Name", CustDetails.Name);
            //manager.AddParameter("@EmailID", CustDetails.EmailID);
            //manager.AddParameter("@Mobile", CustDetails.Mobile);
            //manager.AddParameter("@DateOfBirth", CustDetails.DOB);
            //manager.AddParameter("@WaitingTime", CustDetails.WaitingTime);
            //manager.AddParameter("@AdCount", CustDetails.AdCnt);
            //manager.AddParameter("@KdCount", CustDetails.KdCnt);
            //manager.AddParameter("@JnCount", CustDetails.JnCnt);
            //manager.AddParameter("@UserId", CustDetails.UserID);
            //manager.AddParameter("@Type", "WL");
            //manager.AddParameter("@TotalGuests", CustDetails.AdCnt + CustDetails.JnCnt + CustDetails.KdCnt);
            //manager.AddParameter("@TotalSeats", (CustDetails.AdCnt + CustDetails.JnCnt + CustDetails.KdCnt));
            //manager.AddParameter("@HC", CustDetails.HighChair.ToString());
            //manager.AddParameter("@WC", CustDetails.WheelChair.ToString());
            //manager.AddParameter("@Prams", CustDetails.Prams.ToString());
            //manager.AddOutputParameter("@RID", DbType.Int32, row);
            //manager.ExecuteNonQuery("usp_AN_InsertCustomer");
            //row = FieldConverter.To<Int32>(manager.GetParameterValue("@RID"));
            //if (row > 0)
            //    return Json("success", JsonRequestBehavior.AllowGet);
            //else
            //    return Json("failure", JsonRequestBehavior.AllowGet);
        }

        public ActionResult EditWLCustomerDetails(Entity.Customer CustDetails)
        {

            int row = 0;
            SqlDataManager manager = new SqlDataManager();
            manager.AddParameter("@RID", CustDetails.ReservationID);
            manager.AddParameter("@Name", CustDetails.Name);
            manager.AddParameter("@Mobile", CustDetails.Mobile);
            manager.AddParameter("@AdCount", CustDetails.AdCnt);
            manager.AddParameter("@KdCount", CustDetails.KdCnt);
            manager.AddParameter("@JnCount", CustDetails.JnCnt);
            manager.AddParameter("@TotalGuests", CustDetails.AdCnt + CustDetails.JnCnt + CustDetails.KdCnt);
            manager.AddParameter("@TotalSeats", (CustDetails.AdCnt + CustDetails.JnCnt + CustDetails.KdCnt));
            manager.AddParameter("@HC", CustDetails.HighChair.ToString());
            manager.AddParameter("@WC", CustDetails.WheelChair.ToString());
            manager.AddParameter("@Prams", CustDetails.Prams.ToString());
            manager.AddOutputParameter("@ROW", DbType.Int32, row);
            manager.ExecuteNonQuery("usp_AN_EditWLCustomer");
            row = FieldConverter.To<Int32>(manager.GetParameterValue("@ROW"));
            if (row > 0)
                return Json("success", JsonRequestBehavior.AllowGet);
            else
                return Json("failure", JsonRequestBehavior.AllowGet);
        }

        public ActionResult SetWLUnAllocated(int ReservationId)
        {

            int row = 0;
            SqlDataManager manager = new SqlDataManager();
            manager.AddParameter("@RID", ReservationId);
            manager.AddOutputParameter("@ROW", DbType.Int32, row);
            manager.ExecuteNonQuery("usp_AN_SetWLUnAllocated");
            row = FieldConverter.To<Int32>(manager.GetParameterValue("@ROW"));
            return Json("success", JsonRequestBehavior.AllowGet);
            //if (row > 0)
            //    return Json("success", JsonRequestBehavior.AllowGet);
            //else
            //    return Json("failure", JsonRequestBehavior.AllowGet);
        }
        public ActionResult FreeUpTable(int TableID, int UserID)
        {
            int row = 0;
            SqlDataManager manager = new SqlDataManager();
            manager.AddParameter("@TableId", TableID);
            manager.AddParameter("@UserId", UserID);
            manager.AddOutputParameter("@RID", DbType.Int32, row);
            manager.ExecuteNonQuery("usp_AN_FreeUpTable");
            row = FieldConverter.To<Int32>(manager.GetParameterValue("@RID"));
            if (row > 0)
                return Json("success", JsonRequestBehavior.AllowGet);
            else
                return Json("failure", JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReleaseTable(int TableID, int UserID)
        {
            int row = 0;
            SqlDataManager manager = new SqlDataManager();
            manager.AddParameter("@TableId", TableID);
            manager.AddParameter("@UserId", UserID);
            manager.AddOutputParameter("@RID", DbType.Int32, row);
            manager.ExecuteNonQuery("usp_AN_ReleaseTable");
            row = FieldConverter.To<Int32>(manager.GetParameterValue("@RID"));
            if (row > 0)
                return Json("success", JsonRequestBehavior.AllowGet);
            else
                return Json("failure", JsonRequestBehavior.AllowGet);
        }

        public ActionResult LinkTable(int TableID, int MasterTableID)
        {
            int row = 0;
            SqlDataManager manager = new SqlDataManager();
            manager.AddParameter("@TableId", TableID);
            manager.AddParameter("@MasterTableId", MasterTableID);
            manager.AddOutputParameter("@RID", DbType.Int32, row);
            manager.ExecuteNonQuery("usp_AN_LinkTable");
            row = FieldConverter.To<Int32>(manager.GetParameterValue("@RID"));
            if (row > 0)
                return Json("success", JsonRequestBehavior.AllowGet);
            else
                return Json("failure", JsonRequestBehavior.AllowGet);
        }

        public ActionResult RevertAllocation(int ResID, int UserID)
        {
            int row = 0;
            SqlDataManager manager = new SqlDataManager();
            manager.AddParameter("@ReservationId", ResID);
            manager.AddParameter("@UserId", UserID);
            manager.AddOutputParameter("@RID", DbType.Int32, row);
            manager.ExecuteNonQuery("usp_AN_RevertAllocation");
            row = FieldConverter.To<Int32>(manager.GetParameterValue("@RID"));
            if (row > 0)
                return Json("success", JsonRequestBehavior.AllowGet);
            else
                return Json("failure", JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReservationInLounge(int ResID, int UserID)
        {
            string response = "";
            try
            {
                using (var _context = new ChineseTillEntities1())
                {
                    var reservation = _context.tblReservations.Where(x => x.ReservationID == ResID).FirstOrDefault();
                    reservation.InLounge = !reservation.InLounge;
                    _context.Entry(reservation).State = EntityState.Modified;
                    _context.SaveChanges();
                    response = "Reservation updated successfully";
                }
            }
            catch (Exception ex)
            {
                response = ex.Message;
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult RevertUnAllocation(int ResID)
        {
            int row = 0;
            string time = DateTime.Now.ToString("HH:mm");
            int rtime = Convert.ToInt32((time.Substring(0, 2)) + (time.Substring(3)));
            SqlDataManager manager = new SqlDataManager();
            manager.AddParameter("@ReservationId", ResID);
            manager.AddParameter("@ResTime", rtime);
            manager.AddOutputParameter("@RID", DbType.Int32, row);
            manager.ExecuteNonQuery("usp_AN_RevertUnAllocation");
            row = FieldConverter.To<Int32>(manager.GetParameterValue("@RID"));
            if (row > 0)
                return Json("success", JsonRequestBehavior.AllowGet);
            else
                return Json("failure", JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetPendingRefillProducts()
        {
            SqlDataManager manager = new SqlDataManager();
            DataTable results = manager.ExecuteDataTable("usp_AN_GetPendingRefillProducts");
            List<RefillProduct> rpList = new List<RefillProduct>();
            if (results.Rows.Count > 0)
            {
                foreach (DataRow row in results.Rows)
                {
                    RefillProduct rp = new RefillProduct();
                    rp.ProductRefillID = FieldConverter.To<int>(row["ProductRefillID"]);
                    rp.ProductId = FieldConverter.To<int>(row["ProductID"]);
                    rp.ProductName = FieldConverter.To<string>(row["Product"]);
                    rpList.Add(rp);
                }
            }
            return Json(rpList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Reservation(string ResturantFK, string ForeName, string Surname, string Mobile)
        {
            SqlDataManager manager = new SqlDataManager();
            DataTable results = manager.ExecuteDataTable("usp_AN_GetPendingRefillProducts");
            List<RefillProduct> rpList = new List<RefillProduct>();
            if (results.Rows.Count > 0)
            {
                foreach (DataRow row in results.Rows)
                {
                    RefillProduct rp = new RefillProduct();
                    rp.ProductRefillID = FieldConverter.To<int>(row["ProductRefillID"]);
                    rp.ProductId = FieldConverter.To<int>(row["ProductID"]);
                    rp.ProductName = FieldConverter.To<string>(row["Product"]);
                    rpList.Add(rp);
                }
            }
            return Json(rpList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SendTableConfirmSMS(string MobileNo)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["AdminServerConnection"].ConnectionString;
            string Message = "TCB UNLIMITED DINING. Thank you for waiting. We are now preparing your table. Please see the host to be seated.";
            int row = 0;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("usp_TCB_SS_SendTableConfirmSMS", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@MobileNumber", MobileNo);
                cmd.Parameters.AddWithValue("@Message", Message);
                cmd.Parameters.Add("@RowId", SqlDbType.Int).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                row = Convert.ToInt32(cmd.Parameters["@RowId"].Value);
                con.Close();

            }
            if (row > 0)
            {
                return Json("success", JsonRequestBehavior.AllowGet);
            }
            else
                return Json("failure", JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetStockTemplates()
        {
            ChineseTillEntities1 context = new ChineseTillEntities1();
            var a = (from c in context.tblStockTemplates
                     where c.DelInd == false
                     select new StockTemplate
                     {
                         TemplateID = c.StockTemplateID,
                         TemplateName = c.TemplateName,
                         DateCounted = " ",
                     }).ToList();
            return Json(a, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetStockTemplateItems(int TemplateId)
        {
            ChineseTillEntities1 context = new ChineseTillEntities1();
            var a = (from c in context.tblStockTemplateProducts
                     where c.TemplateFK == TemplateId && c.MaxQuantity > 0
                     select new StockOrderProduct
                     {
                         ProductID = c.ProductID,
                         ProductName = c.ProductName,
                         //StockQuantity = 0,
                         MaxQuantity = c.MaxQuantity,
                         Type = c.Type,
                         OrderedQuantity = c.MaxQuantity - 0
                     }).ToList();
            foreach (var item in a)
            {
                if (context.tblStockProductsCounts.Any(x => x.ProductID == item.ProductID && x.Ordered == false))
                {
                    item.StockQuantity = context.tblStockProductsCounts.Where(x => x.ProductID == item.ProductID && x.Ordered == false).Select(z => z.Quantity).FirstOrDefault();
                    item.Counted = true;
                    item.OrderedQuantity = item.MaxQuantity - item.StockQuantity;
                }
            }
            return Json(a, JsonRequestBehavior.AllowGet);
        }

        public ActionResult InsertStockCount(List<StockProductCount> ItemsCount)
        {
            ChineseTillEntities1 context = new ChineseTillEntities1();
            string response = "success";
            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    foreach (var item in ItemsCount)
                    {
                        if (context.tblStockProductsCounts.Any(x => x.ProductID == item.ProductID && x.Ordered == false))
                        {
                            var s = context.tblStockProductsCounts.Where(x => x.ProductID == item.ProductID && x.Ordered == false).FirstOrDefault();
                            s.Quantity = item.StockQuantity;
                            context.tblStockProductsCounts.Attach(s);
                            context.Entry(s).Property(x => x.Quantity).IsModified = true;
                            context.SaveChanges();
                        }
                        else
                        {
                            tblStockProductsCount sc = new tblStockProductsCount();
                            sc.DateCreated = DateTime.Now;
                            sc.ProductID = item.ProductID;
                            sc.Quantity = item.StockQuantity;
                            sc.TemplateID = item.TemplateID;
                            sc.UserID = item.UserID;
                            sc.LastModified = DateTime.Now;
                            sc.Ordered = false;
                            context.tblStockProductsCounts.Add(sc);
                            context.SaveChanges();
                        }
                    }
                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                response = ex.Message;
                throw;
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCountedTemplates(int UserID)
        {
            ChineseTillEntities1 context = new ChineseTillEntities1();
            var a = (from c in context.tblStockProductsCounts
                     join d in context.tblStockTemplates on c.TemplateID equals d.StockTemplateID
                     join u in context.tblUsers on c.UserID equals u.UserID
                     where c.Ordered == false
                     select new StockTemplate
                     {
                         TemplateID = d.StockTemplateID,
                         TemplateName = d.TemplateName,
                         UserId = c.UserID,
                         UserName = u.UserName
                     }).Distinct().ToList();
            foreach (var item in a)
            {
                DateTime dt = context.tblStockProductsCounts.Where(x => x.TemplateID == item.TemplateID).OrderByDescending(x => x.DateCreated).Select(x => x.DateCreated).FirstOrDefault();
                item.DateCounted = dt.ToString("dd-MMM HH:mm");
            }
            int UserLevelId = context.tblUsers.Where(x => x.UserID == UserID).Select(x => x.UserLevelID).FirstOrDefault();
            List<StockTemplate> st = new List<StockTemplate>();
            if (UserLevelId < 3)
                st = a.Where(x => x.UserId == UserID).ToList();
            else
                st = a;
            return Json(st, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCountedTemplateProducts(int TemplateId)
        {

            ChineseTillEntities1 context = new ChineseTillEntities1();
            var a = (from c in context.tblStockTemplateProducts
                     where c.TemplateFK == TemplateId && c.MaxQuantity > 0
                     select new StockOrderProduct
                     {
                         ProductID = c.ProductID,
                         ProductName = c.ProductName,
                         StockQuantity = 0,
                         MaxQuantity = c.MaxQuantity,
                         OrderedQuantity = 0,
                         Counted = false
                     }).ToList();
            foreach (var item in a)
            {
                if (context.tblStockProductsCounts.Any(x => x.ProductID == item.ProductID && x.Ordered == false))
                {
                    item.StockQuantity = context.tblStockProductsCounts.Where(x => x.ProductID == item.ProductID && x.Ordered == false).Select(z => z.Quantity).FirstOrDefault();
                    //int lastOrderQty = 0;
                    //lastOrderQty = (int) context.tblStockOrderProducts.Where(x => x.ProductFK == item.ProductID).OrderByDescending(x => x.StockOrderProductID).Select(x => x.Quantity).FirstOrDefault();
                    //item.OrderedQuantity = item.MaxQuantity - (item.StockQuantity + lastOrderQty);
                    item.OrderedQuantity = item.MaxQuantity - item.StockQuantity;
                    item.Counted = true;
                }
            }
            return Json(a, JsonRequestBehavior.AllowGet);



            //ChineseTillEntities1 context = new ChineseTillEntities1();
            //var a = (from c in context.tblStockProductsCounts
            //         join d in context.tblStockTemplateProducts on c.ProductID equals d.ProductID
            //         where c.TemplateID == TemplateId && c.Ordered == false
            //         select new StockOrderProduct
            //         {
            //             ProductID = d.ProductID,
            //             ProductName = d.ProductName,
            //             StockQuantity = c.Quantity,
            //             MaxQuantity = d.MaxQuantity,
            //             OrderedQuantity = d.MaxQuantity - c.Quantity
            //         }).ToList();
            //return Json(a, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SubmitStockOrder(StockOrder StockOrder)
        {
            ChineseTillEntities1 context = new ChineseTillEntities1();
            tblStockOrder so = new tblStockOrder();
            so.DateCreated = DateTime.Now;
            so.Completed = false;
            so.DateOrdered = DateTime.Now;
            so.DelInd = false;
            so.iTemplateID = StockOrder.TemplateId;
            so.TemplateName = StockOrder.TemplateName;
            so.Processed = false;
            so.WebUpload = false;
            so.RequiredBy = StockOrder.RequiredDate;
            so.Notes = StockOrder.Notes;
            so.UserId = StockOrder.UserId;
            try
            {
                context.tblStockOrders.Add(so);
                context.SaveChanges();
            }
            catch (Exception ex)
            {

                throw;
            }

            int i = so.PurchaseOrderNumber;
            foreach (var item in StockOrder.OrderProducts)
            {
                tblStockOrderProduct sop = new tblStockOrderProduct();
                sop.PurchaseOrderNumberFK = i;
                sop.ProductFK = item.ProductID;
                sop.Quantity = item.OrderedQuantity;
                sop.delInd = false;
                sop.Cancelled = false;
                sop.WebUpload = false;
                sop.UnitPrice = item.Price;
                sop.ItemTotal = item.OrderedQuantity * item.Price;
                sop.Received = 0;
                sop.MaxQuantity = item.MaxQuantity;
                sop.Counted = item.Counted;
                sop.StockQuantity = item.StockQuantity;
                try
                {
                    context.tblStockOrderProducts.Add(sop);
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw;
                }

            }
            SqlDataManager manager = new SqlDataManager();
            DataTable results = manager.ExecuteDataTable("usp_AN_GetStockOrderData");

            string consString = ConfigurationManager.ConnectionStrings["AdminServerConnection"].ConnectionString;

            foreach (DataRow item in results.Rows)
            {
                int templateId = 0;
                using (SqlConnection con = new SqlConnection(consString))
                {
                    using (SqlCommand cmd = new SqlCommand("usp_TCB_SS_InsertStockTemplate", con))
                    {
                        con.Open();
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@TemplateName", FieldConverter.To<string>(item["TemplateName"]));
                        cmd.Parameters.AddWithValue("@UserName", FieldConverter.To<string>(item["UserName"]));
                        cmd.Parameters.AddWithValue("@DelInd", 0);
                        cmd.Parameters.AddWithValue("@RestaurantFK", FieldConverter.To<int>(item["RestaurantID"]));
                        cmd.Parameters.AddWithValue("@Notes", FieldConverter.To<string>(item["Notes"]));
                        cmd.Parameters.AddWithValue("@DateRequired", FieldConverter.To<DateTime>(item["RequiredBy"]));
                        cmd.Parameters.Add("@TemplateId", SqlDbType.Int);
                        cmd.Parameters["@TemplateId"].Direction = ParameterDirection.Output;
                        cmd.ExecuteNonQuery();
                        templateId = (int)cmd.Parameters["@TemplateId"].Value;
                        //Set the database table name
                        SqlDataManager manager1 = new SqlDataManager();
                        manager1.AddParameter("@PONumber", FieldConverter.To<int>(item["PurchaseOrderNumber"]));
                        DataTable results1 = manager1.ExecuteDataTable("usp_AN_GetStockOrderItemsData");
                        foreach (DataRow row in results1.Rows)
                        {
                            row["TemplateFK"] = templateId;
                        }
                        using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(con))
                        {
                            sqlBulkCopy.DestinationTableName = "dbo.tblSubmittedTemplateItem";
                            //[OPTIONAL]: Map the DataTable columns with that of the database table
                            sqlBulkCopy.ColumnMappings.Add("TemplateFK", "TemplateFK");
                            sqlBulkCopy.ColumnMappings.Add("ProductFK", "ProductFK");
                            sqlBulkCopy.ColumnMappings.Add("delInd", "delInd");
                            sqlBulkCopy.ColumnMappings.Add("Quantity", "Quantity");
                            sqlBulkCopy.ColumnMappings.Add("UnitPrice", "UnitPrice");
                            sqlBulkCopy.ColumnMappings.Add("ItemTotal", "ItemTotal");
                            sqlBulkCopy.ColumnMappings.Add("Counted", "Counted");
                            sqlBulkCopy.ColumnMappings.Add("MaxQuantity", "MaxQuantity");
                            sqlBulkCopy.ColumnMappings.Add("StockQuantity", "StockQuantity");
                            sqlBulkCopy.WriteToServer(results1);
                            con.Close();
                        }
                    }
                }

                SqlDataManager mgr = new SqlDataManager();
                mgr.AddParameter("@PONumber", FieldConverter.To<int>(item["PurchaseOrderNumber"]));
                mgr.ExecuteNonQuery("usp_AN_UpdateStockOrderSend");
            }
            return Json("success", JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetHeadCounts()
        {
            HeadCounts hc = new HeadCounts();
            SqlDataManager manager = new SqlDataManager();
            DataTable results = manager.ExecuteDataTable("usp_AN_GetHeadCounts");
            foreach (DataRow row in results.Rows)
            {
                TimeSpan ts = DateTime.Now - FieldConverter.To<DateTime>(row["DateCreated"]);
                if (ts.TotalMinutes < 20)
                    hc.Starters += FieldConverter.To<int>(row["HeadCount"]);
                else if (ts.TotalMinutes >= 20 && ts.TotalMinutes < 65)
                    hc.MainCourse += FieldConverter.To<int>(row["HeadCount"]);
                else
                    hc.Deserts += FieldConverter.To<int>(row["HeadCount"]);
            }
            return Json(hc, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSupplierOrders()
        {
            ChineseTillEntities1 context = new ChineseTillEntities1();
            var so = (from a in context.tblSupplierOrders
                      where a.DelInd == false && (a.OrderDate >= DbFunctions.AddDays(DateTime.Now, -14)
                      || a.Completed == false)
                      //where a.Completed == false
                      select new SupplierOrder
                      {
                          SupplierOrderId = a.SupplierOrderId,
                          Supplier = a.Supplier,
                          PurchaseOrderNumber = a.PurchaseOrderNumber,
                          OrderDate = a.OrderDate,
                          Notes = a.Notes,
                          Completed = a.Completed,
                          Quantity = a.Quantity ?? "Correct",
                          Condition = a.Condition ?? "Good",
                          ChillTemperature = a.Chill ?? "-4",
                          FrozenTemperature = a.Frozen ?? "-18",
                          Saved = a.Saved ?? false,
                          SupplierSign = a.SupplierSign ?? "",
                          SupportRequired = a.UrgentReport,
                          StaffUserCode = a.StaffUserCode ?? 0,
                          ManagerUserCode = a.ManagerUserCode ?? 0
                      }).ToList().OrderByDescending(x => x.OrderDate);

            return Json(so, JsonRequestBehavior.AllowGet);
            //try
            //{
            //    var so = (from a in context.tblSupplierOrders
            //              where a.DelInd == false && (a.OrderDate >= DbFunctions.AddDays(DateTime.Now, -14)
            //              || a.Completed == false)
            //              //where a.Completed == false
            //              select new SupplierOrder
            //              {
            //                  SupplierOrderId = a.SupplierOrderId,
            //                  Supplier = a.Supplier,
            //                  PurchaseOrderNumber = a.PurchaseOrderNumber,
            //                  OrderDate = a.OrderDate,
            //                  Notes = a.Notes,
            //                  Completed = a.Completed,
            //                  Quantity = a.Quantity ?? "Correct",
            //                  Condition = a.Condition ?? "Good",
            //                  ChillTemperature = a.Chill ?? "-4",
            //                  FrozenTemperature = a.Frozen ?? "-18",
            //                  Saved = a.Saved ?? false,
            //                  SupplierSign = a.SupplierSign ?? "",
            //                  SupportRequired = a.UrgentReport
            //              }).ToList().OrderByDescending(x => x.OrderDate);
            //    sor.supplierOrders = so;
            //    sor.message = "success";
            //    return Json(sor, JsonRequestBehavior.AllowGet);
            //}
            //catch (Exception ex)
            //{
            //    sor.message = "failure";
            //    return Json(sor, JsonRequestBehavior.AllowGet);
            //}

        }

        public ActionResult GetSupplierOrderItems(int poNumber)
        {
            ChineseTillEntities1 context = new ChineseTillEntities1();
            var items = (from a in context.tblSupplierOrderItems
                         where a.PurchaseOrderNumberFK == poNumber
                         select new SupplierOrderItem
                         {
                             SupplierOrderItemId = a.SupplierOrderItemId,
                             ProductFK = a.ProductFK,
                             ProductName = a.ProductName,
                             Quantity = a.Quantity,
                             Received = (bool)a.Checked == true ? a.Received : a.Quantity,
                             QuantityOver = a.QuantityOver,
                             QuantityUnder = a.QuantityUnder,
                             ExpirationIssue = a.ExpirationIssue,
                             PoorQuality = a.PoorQuality,
                             Return = a.Return,
                             TempertaureIssue = a.TemperatureIssue,
                             CurrentReceive = (int)(a.Quantity - a.Received),
                             Cancelled = (bool)a.Cancelled,
                             Checked = a.Checked ?? false,
                             SupplierAmount = a.SupplierAmount
                         }).ToList();
            return Json(items, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveReceivedQuantity(SupplierOrder supplierOrder)
        {
            ChineseTillEntities1 context = new ChineseTillEntities1();
            bool completed = true;
            string response = "success";
            foreach (var item in supplierOrder.SupplierOrderItems)
            {
                try
                {
                    var i = context.tblSupplierOrderItems.Where(x => x.SupplierOrderItemId == item.SupplierOrderItemId).FirstOrDefault();
                    i.Received = item.CurrentReceive;
                    i.PoorQuality = item.PoorQuality;
                    i.QuantityOver = item.QuantityOver;
                    i.QuantityUnder = item.QuantityUnder;
                    i.Return = item.Return;
                    i.TemperatureIssue = item.TempertaureIssue;
                    i.ExpirationIssue = item.ExpirationIssue;
                    i.LastModified = DateTime.Now;
                    i.Checked = item.Checked;
                    i.SupplierAmount = item.SupplierAmount;
                    context.tblSupplierOrderItems.Attach(i);
                    context.Entry(i).State = EntityState.Modified;
                    context.SaveChanges();
                }
                catch (Exception ex)
                {

                    response = ex.InnerException.Message;
                }

            }
            try
            {
                var so = context.tblSupplierOrders.Where(x => x.PurchaseOrderNumber == supplierOrder.PurchaseOrderNumber && x.DelInd == false).FirstOrDefault();
                so.Notes = supplierOrder.Notes;
                so.UserId = (int)context.tblUsers.Where(x => x.UserCode == supplierOrder.UserId).Select(x => x.UserID).FirstOrDefault();
                //so.Authorizer = context.tblUsers.Where(x => x.UserCode == supplierOrder.AuthorizerId).Select(x => x.UserName).FirstOrDefault();
                so.Authorizer = supplierOrder.Authorizer;
                so.Chill = supplierOrder.ChillTemperature;
                so.Frozen = supplierOrder.FrozenTemperature;
                so.SupplierSign = supplierOrder.SupplierSign;
                so.AuthorizerSign = supplierOrder.AuthorizerSign;
                so.Quantity = supplierOrder.Quantity;
                so.Condition = supplierOrder.Condition;
                so.UrgentReport = supplierOrder.SupportRequired;
                so.Completed = supplierOrder.Completed;
                so.Saved = supplierOrder.Saved;
                so.LastModified = DateTime.Now;
                so.CompletedDate = DateTime.Now;
                context.tblSupplierOrders.Attach(so);
                context.Entry(so).State = EntityState.Modified;
                context.SaveChanges();
            }
            catch (Exception ex)
            {

                response = ex.InnerException.Message;
            }
            if (supplierOrder.Images != null)
            {
                foreach (var image in supplierOrder.Images)
                {
                    try
                    {
                        tblSupplierOrderImage tsi = new tblSupplierOrderImage();
                        tsi.SupplierOrderid = supplierOrder.SupplierOrderId;
                        tsi.PurchaseOrderNumberFK = supplierOrder.PurchaseOrderNumber;
                        tsi.Image = image;
                        tsi.DelInd = false;
                        context.tblSupplierOrderImages.Add(tsi);
                        context.SaveChanges();
                    }
                    catch (Exception ex)
                    {

                        response = ex.InnerException.Message;
                    }

                }
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ValidateUserCode(string UserCode)
        {
            ChineseTillEntities1 context = new ChineseTillEntities1();
            tblUser up = new tblUser();
            try
            {
                
                up = context.tblUsers.Where(x => x.UserCode == UserCode && x.DelInd == false).FirstOrDefault();
                if (up == null)
                    up = new tblUser();
                //if (up.UserID > 0)
                //{
                //    return Json(uc, JsonRequestBehavior.AllowGet);

                //    //if (uc.UserID > 0)
                //    //else
                //    //    uc.UserID = 0;
                //    //    return Json("failure", JsonRequestBehavior.AllowGet);
                //}
                //else
                //{
                    
                //    uc.UserID = 0;
                //    return Json(up, JsonRequestBehavior.AllowGet);
                //}
            }
            catch (Exception ex)
            {
                up = new tblUser();
                //string error = ex.Message;
                //if (ex.InnerException != null)
                //    error = ex.InnerException.StackTrace;
                //return Json(error, JsonRequestBehavior.AllowGet);
            }
            return Json(up, JsonRequestBehavior.AllowGet);


        }
        public ActionResult SubmitWastageQuantity(ProductWastage productWastage)
        {
            string response = "success";
            ChineseTillEntities1 context = new ChineseTillEntities1();
            try
            {
                foreach (var item in productWastage.ProductList)
                {
                    tblProductWastage tbw = new tblProductWastage();
                    tbw.UserID = productWastage.UserID;
                    tbw.ProductID = item.ProductID;
                    tbw.Qty = item.WastageQty;
                    tbw.Date = DateTime.Now;
                    context.tblProductWastages.Add(tbw);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                response = ex.InnerException.Message;
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            try
            {
                foreach (var item in productWastage.Images)
                {

                    //tblWastageImage twi = new tblWastageImage();
                    //twi.UserId = productWastage.UserID;
                    //twi.Image = item;
                    //twi.DateCreated = DateTime.Now;
                    //context.tblWastageImages.Add(twi);
                    //context.SaveChanges();
                }
            }
            catch (Exception ex)
            {

                response = ex.InnerException.Message;
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        //public ActionResult GetKitchenOrders()
        //{
        //    string response = "success";
        //    ChineseTillEntities1 _context = new ChineseTillEntities1();
        //    KitchenOrderResponse kor = new KitchenOrderResponse();

        //    int currentTime = Convert.ToInt32(DateTime.Now.ToString("HHmm"));
        //    //currentTime = 1530;
        //    int pt = Convert.ToInt32(ConfigurationManager.AppSettings["PreparationTime"]);
        //    int collectionTime = currentTime + pt;

        //    try
        //    {
        //        var orderItems = (from t in _context.tblTakeAwayOrders
        //                          join a in _context.tblOrders on t.OrderGUID equals a.OrderGUID
        //                          join b in _context.tblOrderParts on a.OrderGUID equals b.OrderGUID
        //                          join c in _context.tblProducts on b.ProductID equals c.ProductID
        //                          where t.CollectionTime <= collectionTime && t.delInd == 0 && t.HasBeenCollected == 0 && (t.HasBeenPrepared == 0 || t.HasBeenPrepared == null)
        //                          && a.DelInd == false && b.DelInd == false
        //                          select new
        //                          {
        //                              a.OrderGUID,
        //                              a.TakeAwayName,
        //                              a.TableID,
        //                              b.OrderPartID,
        //                              c.Description,
        //                              b.ProductID,
        //                              b.Qty,
        //                              t.CollectionTime,
        //                              t.HasPrinted,

        //                          }).ToList();


        //        foreach (var item in orderItems)
        //        {
        //            OrderPart op = new OrderPart();
        //            op.OrderPartID = item.OrderPartID;
        //            op.Name = item.Description;
        //            op.ProductID = item.ProductID;
        //            op.Qty = item.Qty;
        //            var k = kor.KitchenOrders.Find(x => x.OrderGUID == item.OrderGUID);
        //            if (k == null)
        //            {
        //                KitchenOrder ko = new KitchenOrder();
        //                ko.OrderGUID = item.OrderGUID;
        //                ko.OrderNumber = item.TakeAwayName;
        //                ko.CollectionTime = item.CollectionTime;
        //                ko.TimeRemaining = item.CollectionTime - currentTime;
        //                ko.Printed = item.HasPrinted;
        //                if (item.TableID > 0)
        //                {
        //                    int tableId = orderItems[0].TableID;
        //                    ko.TableNumber = _context.tblTables.Where(x => x.TableID == tableId).Select(x => x.TableNumber).FirstOrDefault().ToString();
        //                }


        //                if (_context.tblOrderPartOptions.Any(x => x.OrderPartId == item.OrderPartID))
        //                {
        //                    BuffetBox bb = new BuffetBox();
        //                    bb.Name = op.Name;
        //                    bb.ProductId = op.ProductID;
        //                    bb.Qty = (int)op.Qty;
        //                    bb.BoxItems = (from a1 in _context.tblOrderPartOptions
        //                                   join b1 in _context.tblProducts on a1.ProductOptionID equals b1.ProductID
        //                                   where a1.OrderPartId == item.OrderPartID
        //                                   select new OrderPartOption
        //                                   {
        //                                       OrderPartId = a1.OrderPartId,
        //                                       ProductOptionID = b1.ProductID,
        //                                       Name = b1.Description
        //                                   }).ToList();
        //                    foreach (var bb1 in bb.BoxItems)
        //                    {
        //                        var p = kor.ProductSummary.Find(x => x.ProductID == bb1.ProductOptionID);
        //                        if (p != null)
        //                        {
        //                            p.Qty += 1;
        //                        }
        //                        else
        //                        {
        //                            OrderPart op1 = new OrderPart();
        //                            op1.Name = bb1.Name;
        //                            op1.Qty = 1;
        //                            op1.ProductID = bb1.ProductOptionID;
        //                            kor.ProductSummary.Add(op1);
        //                        }

        //                    }
        //                    ko.Boxes.Add(bb);

        //                }
        //                else
        //                {
        //                    ko.OrderedItems.Add(op);
        //                    var p = kor.ProductSummary.Find(x => x.ProductID == op.ProductID);
        //                    if (p != null)
        //                    {
        //                        p.AlacarteQty += (int)op.Qty;
        //                    }
        //                    else
        //                    {
        //                        OrderPart op1 = new OrderPart();
        //                        op1.Name = op.Name;
        //                        op1.Qty = 1;
        //                        op1.ProductID = op.ProductID;
        //                        kor.ProductSummary.Add(op1);
        //                    }

        //                }
        //                kor.KitchenOrders.Add(ko);
        //                kor.OrderCount = kor.OrderCount + 1;
        //            }
        //            else
        //            {

        //                if (_context.tblOrderPartOptions.Any(x => x.OrderPartId == item.OrderPartID))
        //                {
        //                    BuffetBox bb = new BuffetBox();
        //                    bb.Name = op.Name;
        //                    bb.ProductId = op.ProductID;
        //                    bb.Qty = (int)op.Qty;
        //                    bb.BoxItems = (from a1 in _context.tblOrderPartOptions
        //                                   join b1 in _context.tblProducts on a1.ProductOptionID equals b1.ProductID
        //                                   where a1.OrderPartId == item.OrderPartID
        //                                   select new OrderPartOption
        //                                   {
        //                                       OrderPartId = a1.OrderPartId,
        //                                       ProductOptionID = b1.ProductID,
        //                                       Name = b1.Description
        //                                   }).ToList();
        //                    k.Boxes.Add(bb);
        //                    foreach (var bb1 in bb.BoxItems)
        //                    {
        //                        var p = kor.ProductSummary.Find(x => x.ProductID == bb1.ProductOptionID);
        //                        if (p != null)
        //                        {
        //                            p.Qty += 1;
        //                        }
        //                        else
        //                        {
        //                            OrderPart op1 = new OrderPart();
        //                            op1.Name = bb1.Name;
        //                            op1.Qty = 1;
        //                            op1.ProductID = bb1.ProductOptionID;
        //                            kor.ProductSummary.Add(op1);
        //                        }

        //                    }

        //                }
        //                else
        //                {
        //                    var oi = k.OrderedItems.Find(x => x.OrderGUID == item.OrderGUID);
        //                    if (oi != null)
        //                        oi.Qty += item.Qty;
        //                    else
        //                        k.OrderedItems.Add(op);
        //                    var p = kor.ProductSummary.Find(x => x.ProductID == op.ProductID);
        //                    if (p != null)
        //                    {
        //                        p.AlacarteQty += (int)op.Qty;
        //                    }
        //                    else
        //                    {
        //                        OrderPart op1 = new OrderPart();
        //                        op1.Name = op.Name;
        //                        op1.Qty = 1;
        //                        op1.ProductID = op.ProductID;
        //                        kor.ProductSummary.Add(op1);
        //                    }
        //                }
        //            }
        //        }
        //        foreach (var item in kor.KitchenOrders)
        //        {
        //            if (item.Printed == 0)
        //            {
        //                string orderDetailStr = "";
        //                string kitchenReceipt = "";
        //                string barReceipt = "";
        //                string ct = Convert.ToString(item.CollectionTime);
        //                if (ct.Length == 3)
        //                {
        //                    ct = "0" + ct;
        //                }
        //                string itemStr = "";
        //                int itemCount = 0;

        //                foreach (var box in item.Boxes)
        //                {
        //                    itemStr += box.Name + "           - " + box.Qty + Environment.NewLine;
        //                    itemCount += box.Qty;
        //                    foreach (var b12 in box.BoxItems)
        //                    {
        //                        itemStr += "     " + b12.Name + Environment.NewLine;

        //                    }
        //                }
        //                foreach (var i1 in item.OrderedItems)
        //                {
        //                    itemCount += (int)i1.Qty;
        //                    itemStr += i1.Name + "           - " + i1.Qty + Environment.NewLine;
        //                }
        //                orderDetailStr = "";
        //                orderDetailStr += "        TakeAway               " + Environment.NewLine;
        //                orderDetailStr += "        " + item.OrderNumber + "               " + Environment.NewLine;
        //                //str += "    Total Items - " + Convert.ToString(item.Boxes.Count + item.OrderedItems.Count) + Environment.NewLine;
        //                orderDetailStr += "        Total Items - " + Convert.ToString(itemCount) + Environment.NewLine;
        //                orderDetailStr += "        Collection Time - " + ct.Substring(0, 2) + ":" + ct.Substring(2) + Environment.NewLine;
        //                orderDetailStr += "-------------------------------------------------" + Environment.NewLine;
        //                kitchenReceipt = orderDetailStr + itemStr;
        //                tblPrintQueue tp = new tblPrintQueue();
        //                tp.Receipt = kitchenReceipt;
        //                tp.PCName = "Website";
        //                tp.ToPrinter = "Kitchen";
        //                tp.UserFK = -10;
        //                tp.DateCreated = DateTime.Now;
        //                _context.tblPrintQueues.Add(tp);
        //                _context.SaveChanges();


        //                //Print receipt at Bar printer
        //                var custDetails = (from a in _context.tblTakeAwayOrders
        //                                   join b in _context.tblAddresses on a.AddressID equals b.AddressID
        //                                   where a.OrderGUID == item.OrderGUID
        //                                   select new
        //                                   {
        //                                       a.NAME,
        //                                       a.Phone,
        //                                       b.AddressFull,
        //                                       b.PostCode
        //                                   }).FirstOrDefault();
        //                string custStr = "";
        //                custStr += "        " + custDetails.NAME + Environment.NewLine;
        //                custStr += "   " + custDetails.AddressFull + Environment.NewLine;
        //                custStr += "        " + custDetails.Phone + Environment.NewLine;
        //                barReceipt = custStr + orderDetailStr + itemStr;
        //                tblPrintQueue tp1 = new tblPrintQueue();
        //                tp1.Receipt = barReceipt;
        //                tp1.PCName = "Website";
        //                tp1.ToPrinter = "Bar";
        //                tp1.UserFK = -10;
        //                tp1.DateCreated = DateTime.Now;
        //                _context.tblPrintQueues.Add(tp1);
        //                _context.SaveChanges();
        //                var ta = _context.tblTakeAwayOrders.Where(x => x.OrderGUID == item.OrderGUID).First();
        //                ta.HasPrinted = 1;
        //                _context.Entry(ta).State = EntityState.Modified;
        //                _context.SaveChanges();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        response = ex.InnerException.Message;
        //        //return Json(response, JsonRequestBehavior.AllowGet);
        //    }
        //    //kor.koList = koList;
        //    return Json(kor, JsonRequestBehavior.AllowGet);
        //}

        public ActionResult CompleteKitchenOrder(Guid OrderGUID)
        {
            ChineseTillEntities1 _context = new ChineseTillEntities1();
            var twOrder = _context.tblTakeAwayOrders.Where(x => x.OrderGUID == OrderGUID).FirstOrDefault();
            twOrder.HasBeenPrepared = 1;
            _context.Entry(twOrder).State = EntityState.Modified;
            _context.SaveChanges();
            return Json("success", JsonRequestBehavior.AllowGet);
        }

        //public ActionResult GetDineInProducts()
        //{
        //    ChineseTillEntities1 _context = new ChineseTillEntities1();
        //    GetProductsResponse gpr = new GetProductsResponse();
        //    int currTime = Convert.ToInt32(DateTime.Now.ToString("HHMM"));
        //    int currDay = (int)DateTime.Today.DayOfWeek;
        //    if (currDay == 0)
        //        currDay = 7;
        //    gpr.BuffetMenu = (from a in _context.tblMenus
        //                      where a.DelInd == false
        //                      select new ProductGroupModel
        //                      {
        //                          Group = new ProductGroup
        //                          {
        //                              Groupname = a.MenuName,
        //                              ProductGroupID = a.ID,
        //                          },
        //                          GroupProducts = (from mi in _context.tblMenuItems
        //                                           join p in _context.tblProducts on mi.ProductID equals p.ProductID
        //                                            join pt in _context.tblProductTimeRestrictions on mi.ProductID equals pt.ProductID
        //                                           where mi.MenuID == a.ID && mi.DelInd == false
        //                                           && pt.DayID == currDay && currTime > pt.StartTime && currTime < pt.EndTime
        //                                           select new Entity.Product
        //                                           {
        //                                               ProductID = p.ProductID,
        //                                               Description = p.Description,
        //                                               ProductGroupID = mi.MenuID

        //                                           }).ToList()

        //                      }).ToList();
        //    gpr.DrinksMenu = (from a in _context.tblProductGroups
        //                      where a.DelInd == false && a.Groupname.Contains("Drink")
        //                      select new ProductGroupModel
        //                      {
        //                          Group = new ProductGroup
        //                          {
        //                              Groupname = a.Groupname,
        //                              ProductGroupID = a.ProductGroupID,
        //                          },
        //                          GroupProducts = (from p in _context.tblProducts
        //                                           where p.DelInd == false && p.FoodRefil == false
        //                                           select new Entity.Product
        //                                           {
        //                                               ProductID = p.ProductID,
        //                                               Description = p.Description,
        //                                               ProductGroupID = p.ProductGroupID

        //                                           }).ToList()

        //                      }).ToList();
        //    return Json(gpr, JsonRequestBehavior.AllowGet);
        //}

        public ActionResult ValidateTableCode(int code)
        {
            ChineseTillEntities1 _context = new ChineseTillEntities1();
            string response = "";
            Table to = new Table();
            try
            {


                int printInterval = Convert.ToInt32(ConfigurationManager.AppSettings["PrintingInerval"]);
                to = (from a in _context.tblTableOrders
                      join b in _context.tblOrders on a.OrderGUID equals b.OrderGUID
                      join c in _context.tblTables on a.TableId equals c.TableID
                      where a.UniqueCode == code && a.Active == true
                      select new Table
                      {
                          TableNumber = c.TableNumber,
                          TableID = c.TableID,
                          OrderGUID = b.OrderGUID
                      }).FirstOrDefault();
                if (to != null)
                {
                    var tord = _context.tblTableOrders.Where(x => x.UniqueCode == code).FirstOrDefault();
                    tord.DateValidated = DateTime.Now;
                    tord.SignInCount += 1;
                    //tord.NextOrderTime = DateTime.Now.AddMinutes(printInterval);
                    _context.Entry(tord).State = EntityState.Modified;
                    _context.SaveChanges();
                    to.Message = "success";
                }
                else
                {
                    to = new Table();
                    to.Message = "Invalid/Expired code. Please contact our staff for unique code";
                }
            }
            catch (Exception ex)
            {

                to.Message = ex.Message + ex.InnerException.Message;
            }
            return Json(to, JsonRequestBehavior.AllowGet);
        }






        public ActionResult GetCartItems(Guid orderId)
        {
            ChineseTillEntities1 _context = new ChineseTillEntities1();
            var oi = (from a in _context.tblOrderParts
                      join b in _context.tblProducts on a.ProductID equals b.ProductID
                      where a.OrderGUID == orderId && a.DelInd == false
                      group a by new { a.ProductID, b.Description, a.Qty, b.Price } into g
                      select new Entity.Product
                      {
                          ProductID = g.Key.ProductID,
                          Description = g.Key.Description,
                          Price = (float)g.Key.Price,
                          ProductQty = (int)g.Sum(a => a.Qty),
                          ProductTotal = ((float)g.Key.Price * (int)g.Sum(a => a.Qty))
                      }).ToList();
            return Json(oi, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ConfirmOrderPayment(Payment req)
        {
            string response = "";
            Guid orderId = new Guid();
            orderId = req.OrderGUID;
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    //StripeConfiguration.ApiKey = ConfigurationManager.AppSettings["StripeKey"];
                    //var token = req.Token;
                    //var options = new ChargeCreateOptions
                    //{
                    //    Amount = (long)req.Amount * 100,
                    //    Currency = "GBP",
                    //    Description = req.OrderNo.ToString(),
                    //    Source = req.Token,
                    //};

                    //var service = new ChargeService();
                    //Charge charge = service.Create(options);
                    var to = dbContext.tblOrders.Where(x => x.OrderGUID == req.OrderGUID).FirstOrDefault();
                    if (req.SplitProduct != null && req.SplitProduct.Count > 0)
                    {
                        //Create new order for Split Products. Move selected items to new orderid
                        tblOrder tor = new tblOrder();
                        try
                        {

                            tor.OrderGUID = Guid.NewGuid();
                            tor.DateCreated = DateTime.Now;
                            tor.TabID = 99;
                            tor.TableID = to.TableID;
                            tor.TakeAway = false;
                            tor.UserID = -10;
                            tor.Paid = true;
                            tor.DatePaid = DateTime.Now;
                            tor.PaymentMethod = "MP";
                            tor.TotalPaid = (decimal)req.Amount;
                            tor.GrandTotal = (decimal)req.Amount;
                            tor.TipAmount = (decimal)req.TipAmount;
                            tor.DateCreated = DateTime.Now;
                            tor.LastModified = DateTime.Now;

                            dbContext.tblOrders.Add(tor);
                            dbContext.SaveChanges();
                        }
                        catch (Exception ex)
                        {

                            response = ex.Message;
                            return Json(response, JsonRequestBehavior.AllowGet);
                        }

                        orderId = tor.OrderGUID;
                        //Move selected products to this new order
                        foreach (var item in req.SplitProduct)
                        {
                            try
                            {
                                var op = dbContext.tblOrderParts.Where(x => x.OrderGUID == req.OrderGUID && x.ProductID == item.ProductID).FirstOrDefault();
                                op.OrderGUID = orderId;
                                dbContext.Entry(op).State = EntityState.Modified;
                                dbContext.SaveChanges();
                            }
                            catch (Exception ex)
                            {

                                response = ex.Message + ex.InnerException.Message;
                                return Json(response, JsonRequestBehavior.AllowGet);
                            }

                        }
                        to.SplitBill = true;
                        dbContext.Entry(to).State = EntityState.Modified;
                        dbContext.SaveChanges();

                    }
                    else
                    {
                        //var to = dbContext.tblOrders.Where(x => x.OrderGUID == req.OrderGUID).FirstOrDefault();
                        response = "Updating Order";

                        try
                        {
                            //rh.WriteToFile("testing Elmah");
                            to.Paid = true;
                            to.DatePaid = DateTime.Now;
                            to.PaymentMethod = "MP";
                            to.TotalPaid = (decimal)req.Amount;
                            to.GrandTotal = (decimal)req.Amount;
                            to.TipAmount = (decimal)req.TipAmount;
                            dbContext.Entry(to).State = EntityState.Modified;
                            dbContext.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            response = ex.Message + ex.InnerException.Message;
                            return Json(response, JsonRequestBehavior.AllowGet);
                        }
                        try
                        {
                            var tblOrd = dbContext.tblTableOrders.Where(x => x.OrderGUID == req.OrderGUID).FirstOrDefault();
                            tblOrd.Active = false;
                            dbContext.Entry(tblOrd).State = EntityState.Modified;
                            dbContext.SaveChanges();
                            dbContext.usp_AN_SetTableCleaning(req.OrderGUID, 11);
                        }
                        catch (Exception ex)
                        {
                            response = ex.Message;
                            return Json(response, JsonRequestBehavior.AllowGet);
                        }


                    }
                    try
                    {
                        string primaryTILL = "";
                        primaryTILL = Convert.ToString(ConfigurationManager.AppSettings["PrimaryTILL"]);
                        var top1 = dbContext.tblOrderPayments.Where(x => x.PCName != "" || x.PCName != null).OrderByDescending(x => x.DateCreated).FirstOrDefault();
                        if (primaryTILL == "")
                            primaryTILL = top1.PCName;
                        tblOrderPayment top = new tblOrderPayment();
                        top.OrderGUID = orderId;
                        top.PaymentGUID = Guid.NewGuid();
                        top.DateCreated = DateTime.Now;
                        top.LastModified = DateTime.Now;
                        top.PaymentValue = (decimal)req.Amount;
                        top.TipAmount = (decimal)req.TipAmount;
                        top.PaymentType = "MP";
                        top.PCName = primaryTILL;
                        dbContext.tblOrderPayments.Add(top);
                        dbContext.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        response = ex.Message;
                        return Json(response, JsonRequestBehavior.AllowGet);
                    }

                    //Print Payment Confirmation slip for the Order
                    try
                    {
                        string confirmationStr = "";
                        confirmationStr += "       " + DateTime.Now.ToString("dd-MM-yyyy") + Environment.NewLine;
                        confirmationStr += "Confirmation payment of " + Environment.NewLine;
                        confirmationStr += "£" + String.Format("{0:0.00}", req.Amount) + " for Table-" + req.OrderNo + Environment.NewLine;
                        confirmationStr += Environment.NewLine;
                        confirmationStr += "Thank you for your payment." + Environment.NewLine;
                        confirmationStr += "Please give this confirmation" + Environment.NewLine;
                        confirmationStr += "slip to the cashier " + Environment.NewLine;
                        confirmationStr += "on your way out." + Environment.NewLine;
                        //confirmationStr += "        Table - " + req.OrderNo + Environment.NewLine;
                        //confirmationStr += "        Received with Thanks £" + Convert.ToString(req.Amount) + Environment.NewLine;
                        confirmationStr += "-------------------------------------------------" + Environment.NewLine;
                        confirmationStr += "We hope you enjoyed your " + Environment.NewLine;
                        confirmationStr += "visit with us";
                        tblPrintQueue tp = new tblPrintQueue();
                        tp.Receipt = confirmationStr;
                        tp.PCName = "App";
                        tp.ToPrinter = "Bar";
                        tp.UserFK = -10;
                        tp.DateCreated = DateTime.Now;
                        dbContext.tblPrintQueues.Add(tp);
                        dbContext.SaveChanges();
                        scope.Complete();
                    }
                    catch (Exception ex)
                    {
                        response = ex.Message;
                        return Json(response, JsonRequestBehavior.AllowGet);
                    }
                    response = "success";
                }
                catch (Exception ex)
                {
                    response = ex.Message + ex.InnerException.Message;
                    //response = "failure";
                    throw;
                }
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult PaymentFailed(Payment req)
        {
            string response = "";
            Guid orderId = new Guid();
            orderId = req.OrderGUID;
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {

                    var to = dbContext.tblOrders.Where(x => x.OrderGUID == req.OrderGUID).FirstOrDefault();

                    //Print Payment Confirmation slip for the Order
                    try
                    {
                        string confirmationStr = "";
                        confirmationStr += "       " + DateTime.Now.ToString("dd-MM-yyyy") + Environment.NewLine;
                        confirmationStr += "Failed payment of " + Environment.NewLine;
                        confirmationStr += "£" + String.Format("{0:0.00}", req.Amount) + " for Table-" + req.OrderNo + Environment.NewLine;
                        confirmationStr += Environment.NewLine;
                        confirmationStr += "We regret the payment failed." + Environment.NewLine;
                        confirmationStr += req.FailureMessage + Environment.NewLine;
                        confirmationStr += "-------------------------------------------------" + Environment.NewLine;
                        tblPrintQueue tp = new tblPrintQueue();
                        tp.Receipt = confirmationStr;
                        tp.PCName = "App";
                        tp.ToPrinter = "Bar";
                        tp.UserFK = -10;
                        tp.DateCreated = DateTime.Now;
                        dbContext.tblPrintQueues.Add(tp);
                        dbContext.SaveChanges();
                        scope.Complete();
                    }
                    catch (Exception ex)
                    {
                        response = ex.Message;
                        return Json(response, JsonRequestBehavior.AllowGet);
                    }
                    response = "Payment Failed for amount " + "£" + String.Format("{0:0.00}", req.Amount) + Environment.NewLine + req.FailureMessage;
                }
                catch (Exception ex)
                {
                    response = ex.Message + ex.InnerException.Message;
                    //response = "failure";
                    throw;
                }
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetDineInProducts()
        {
            GetProductsResponse gpr = new GetProductsResponse();
            Models.ProductService ps = new Models.ProductService();
            gpr = ps.GetDineInProducts();
            return Json(gpr, JsonRequestBehavior.AllowGet);
        }

        //public ActionResult GetPrinterReceipts()
        //{

        //var receipts = dbContext.tblPrintQueues.Where(x => x.DatePrinted != null &&
        //DbFunctions.TruncateTime(x.DateCreated) == DbFunctions.TruncateTime(DateTime.Now))
        //    .Select(x=>x.Receipt).ToList();
        //var receipts = (from a in dbContext.tblPrintQueues
        //                 select new KitchenReceipt
        //                 {
        //                    Receipt = a.Receipt
        //                 }).ToList();
        //return Json(receipts, JsonRequestBehavior.AllowGet);
        //}
        public ActionResult GetBuffetOrderItems()
        {
            BuffetItemsSummary bis = new BuffetItemsSummary();
            Models.OrderService os = new Models.OrderService();
            bis = os.GetBuffetOrderItems();
            return Json(bis, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetKitchenOrders()
        {
            KitchenOrderResponse kor = new KitchenOrderResponse();
            Models.OrderService os = new Models.OrderService();
            kor = os.GetKitchenOrders();
            return Json(kor, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetOrderItems(Guid orderId)
        {
            ChineseTillEntities1 _context = new ChineseTillEntities1();

            TableOrder to = new TableOrder();
            try
            {

                to.BuffetItems = (from a in _context.tblOrderBuffetItems
                                  join b in _context.tblProducts on a.ProductId equals b.ProductID
                                  where a.OrderGUID == orderId
                                  group a by new { a.ProductId, b.Description, a.Printed } into g

                                  select new OrderBuffetItem
                                  {
                                      Description = g.Key.Description,
                                      Printed = g.Key.Printed,
                                      Qty = g.Sum(a => a.Qty)
                                  }).OrderByDescending(b => b.Printed).ThenBy(b => b.Description).ToList();
                to.tableProducts = (from a in _context.tblOrderParts
                                    join b in _context.tblProducts on a.ProductID equals b.ProductID
                                    where a.OrderGUID == orderId && b.DelInd == false && a.DelInd == false
                                    group a by new { a.ProductID, b.Description, a.Price,b.RewardPoints,b.RedemptionPoints } into g
                                    select new Entity.Product
                                    {
                                        ProductID = g.Key.ProductID,
                                        Description = g.Key.Description,
                                        Price = (float)g.Key.Price,
                                        ProductQty = (int)g.Sum(a => a.Qty),
                                        RewardPoints = (int)g.Key.RewardPoints,
                                        RedemptionPoints = (int)g.Key.RedemptionPoints,
                                        ProductTotal = ((float)g.Key.Price * (int)g.Sum(a => a.Qty)),
                                        LastModified = DateTime.Now
                                    }).ToList();
                var order = _context.tblOrders.Where(x => x.OrderGUID == orderId).FirstOrDefault();
                bool sca = order.ServiceChargeApplicable ?? false;

                bool scab = Convert.ToBoolean(ConfigurationManager.AppSettings["ServiceChargeApplicable"]);
                if ((sca != null && sca == false) || !scab)
                    to.ServiceChargeApplicable = false;
                else
                    to.ServiceChargeApplicable = true;
                //to.ServiceChargeApplicable = (bool)_context.tblOrders.Where(x => x.OrderGUID == orderId).Select(x => x.ServiceChargeApplicable).FirstOrDefault();
                to.CustCount = (int) order.AdCount  + (int) order.KdCount + (int) order.JnCount;
                to.IsSuccess = true;
            }
            catch (Exception ex)
            {
                to.IsSuccess = false;
                to.Message = ex.Message;
                if (ex.InnerException != null)
                    to.Message += ex.InnerException.Message;
            }
            return Json(to, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetReceiptsURL()
        {
            string url = ConfigurationManager.AppSettings["PrintingURL"];
            return Json(url, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetStripePaymentIntent(Payment req)
        {
            //StripeConfiguration.ApiKey = "sk_test_51H199hFKr3czhxcVyaUlqkAFiOhHWJM73fg6sXHjH4KltaC7o5KTFSWepVDMFiLHuoD4cVAW5cDkE4ldq9pRgS2s00nDuOcLN2";
            string connectionString = ConfigurationManager.ConnectionStrings["TCBROMSConnection"].ConnectionString;
            string res = "";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT Address2 FROM tblRestaurant";
                res = Convert.ToString(cmd.ExecuteScalar());
            }
            StripeConfiguration.ApiKey = ConfigurationManager.AppSettings["StripeKey"];
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)req.Amount,
                Description = req.OrderNo + "- " + res,
                Currency = "gbp",
            };
            try
            {
                var service = new PaymentIntentService();
                var paymentIntent = service.Create(options);
                return Json(paymentIntent.ClientSecret, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

        }


        public ActionResult WaiterService(int TableId)
        {
            int freeState = (int)TableState.Free;

            try
            {
                var table = dbContext.tblTables.Where(x => x.TableID == TableId && x.DelInd == false).FirstOrDefault();
                if (table.CurrentStatus == (int)TableState.Free || table.CurrentStatus == (int)TableState.TableCleaning)
                {
                    return Json("Table Paid/Code expired. Kindly contact our staff at bar counter.", JsonRequestBehavior.AllowGet);
                }
                if (table.CurrentStatus != (int)TableState.WaiterService)
                {
                    table.PastStatus = table.CurrentStatus;
                    table.CurrentStatus = (int)TableState.WaiterService;
                    dbContext.Entry(table).State = EntityState.Modified;
                    dbContext.SaveChanges();
                    var devices = dbContext.tblAppUsers.Where(x => x.DeviceType == "App" && x.LogoutDate == null && x.Token != null).Select(x => x.Token).ToList();
                    if (devices != null && devices.Count > 0)
                    {
                        foreach (var item in devices)
                        {
                            rh.SendNotificationToCUs(item, "Waiter Service requested at table :" + table.TableNumber);
                        }
                    }
                }
                return Json("Our staff will serve you soon", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

        }



        public ActionResult MyDineInOrders(Guid orderId, long mobileNo)
        {
            MyDineInOrderModel mdom = new MyDineInOrderModel();
            var items = (from a in dbContext.tblOrderBuffetItems
                         join b in dbContext.tblProducts on a.ProductId equals b.ProductID
                         where a.OrderGUID == orderId
                         select new OrderBuffetItem
                         {
                             ProductId = a.ProductId,
                             Description = b.Description,
                             UserId = a.UserId,
                             Qty = a.Qty,
                             UserType = a.UserType,
                             Printed = a.Printed
                         }).OrderByDescending(x => x.Printed).ToList();
            foreach (var item in items)
            {
                var m = mdom.TableOrder.Find(x => x.ProductId == item.ProductId && x.Printed == item.Printed);
                if (m != null)
                    m.Qty += item.Qty;
                else
                {
                    OrderBuffetItem opi = new OrderBuffetItem();
                    opi.ProductId = item.ProductId;
                    opi.Description = item.Description;
                    opi.Qty = item.Qty;
                    opi.Printed = item.Printed;
                    mdom.TableOrder.Add(opi);
                }
                if (item.UserId != null && item.UserId == mobileNo)
                {


                    var m1 = mdom.MyOrder.Find(x => x.ProductId == item.ProductId);
                    if (m1 != null)
                        m1.Qty += item.Qty;
                    else
                    {
                        OrderBuffetItem opi = new OrderBuffetItem();
                        opi.ProductId = item.ProductId;
                        opi.Description = item.Description;
                        opi.Qty = item.Qty;
                        opi.Printed = item.Printed;
                        mdom.MyOrder.Add(opi);
                    }
                }
            }
            return Json(mdom, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAppParameters()
        {
            try
            {
                AppParameters au = new AppParameters();
                au.DisclaimerString = ConfigurationManager.AppSettings["DisclaimerString"].ToString();
                au.DisclaimerUrl = ConfigurationManager.AppSettings["DisclaimerUrl"].ToString();
                au.SKey = ConfigurationManager.AppSettings["PublishabelKey"].ToString();
                return Json(au, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public ActionResult GetAppParametersV1()
        {
            try
            {
                AppParameters au = new AppParameters();
                au.DisclaimerString = ConfigurationManager.AppSettings["DisclaimerString"].ToString();
                au.DisclaimerUrl = ConfigurationManager.AppSettings["NewDisclaimerUrl"].ToString();
                au.SKey = ConfigurationManager.AppSettings["PublishabelKey"].ToString();
                ServiceCharge sc = new ServiceCharge();
                sc.IsApplicable = Convert.ToBoolean(ConfigurationManager.AppSettings["ServiceChargeApplicable"]);
                sc.Rate = Convert.ToDecimal(ConfigurationManager.AppSettings["ServiceChargeRate"]);
                au.ServiceCharge = sc;
                au.ThresholdPayableAmount = Convert.ToDecimal(ConfigurationManager.AppSettings["ThresholdPayableAmount"]);
                au.ThresholdAmtMessage = (ConfigurationManager.AppSettings["ThresholdAmtMessage"]).ToString();
                au.ReOrderThresholdTime = Convert.ToInt32(ConfigurationManager.AppSettings["ReOrderThresholdTime"]);
                au.ReOrderThresholdTimeMessage = (ConfigurationManager.AppSettings["ReOrderThresholdTimeMessage"]).ToString();
                au.InsufficientPointsMessage = (ConfigurationManager.AppSettings["InsufficientPointsMessage"]).ToString();
                au.SageURL = (ConfigurationManager.AppSettings["SageURL"]).ToString();
                au.PaymentGateway = (ConfigurationManager.AppSettings["PaymentGateway"]).ToString();
                au.ShowEditSCTab = Convert.ToBoolean(ConfigurationManager.AppSettings["ShowEditSCTab"]);
                au.StripeCheckOut = (ConfigurationManager.AppSettings["StripeCheckOut"]).ToString();
                return Json(au, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public ActionResult ServiceRequired(Guid OrderId)
        {
            var order = dbContext.tblOrders.Where(x => x.OrderGUID == OrderId).FirstOrDefault();
            order.ServiceRequired = true;
            dbContext.Entry(order).State = EntityState.Modified;
            dbContext.SaveChanges();
            return Json("success", JsonRequestBehavior.AllowGet);
        }

        public ActionResult DrinksOptions(Guid OrderId, bool HideAlcoholicDrinks, bool HideDrinkMenu)
        {
            var order = dbContext.tblOrders.Where(x => x.OrderGUID == OrderId).FirstOrDefault();
            order.ServiceRequired = HideAlcoholicDrinks;
            order.HideDrinkMenu = HideDrinkMenu;
            dbContext.Entry(order).State = EntityState.Modified;
            dbContext.SaveChanges();
            return Json("success", JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReprintKitchenReceipts(string fTime, string tTime, string fprinter, string tprinter)
        {
            var dateNow = DateTime.Now;
            DateTime fromDate = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, Convert.ToInt32(fTime.Substring(0, 2)), Convert.ToInt32(fTime.Substring(2)), 0);
            DateTime toDate = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, Convert.ToInt32(tTime.Substring(0, 2)), Convert.ToInt32(tTime.Substring(2)), 0);
            //DateTime fromDate = DateTime.Parse(from);
            //DateTime toDate = DateTime.Parse(to);
            //var receipts = dbContext.tblPrintQueues.Where(x => x.DateCreated >= fromDate && x.DateCreated <= toDate && (x.ToPrinter == "Kitchen" || x.ToPrinter == "Kitchen2") ).OrderBy(x => x.DateCreated).ToList();
            var receipts = dbContext.tblPrintQueues.Where(x => x.DateCreated >= fromDate && x.DateCreated <= toDate && x.ToPrinter == fprinter).OrderBy(x => x.DateCreated).ToList();
            int receiptCount = 0;
            //if (printer == "")
            //    printer = "Kitchen";

            foreach (var item in receipts)
            {
                if (!item.Receipt.Contains("Duplicate"))
                {
                    string receipt = "***Duplicate***" + Environment.NewLine + item.Receipt;
                    tblPrintQueue tp = new tblPrintQueue();
                    tp.PCName = item.PCName;
                    tp.Receipt = receipt;
                    tp.DateCreated = DateTime.Now;
                    tp.ToPrinter = tprinter;
                    tp.UserFK = item.UserFK;
                    dbContext.tblPrintQueues.Add(tp);
                    dbContext.SaveChanges();
                    receiptCount += 1;
                }
            }
            string message = "Total " + receiptCount + " re-printed";
            return Json(message, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CancelOrderedBuffetItems(List<OrderBuffetItem> cancelledItems)
        {
            int items = 0;
            foreach (var item in cancelledItems)
            {
                for (int i = 0; i < item.Qty; i++)
                {
                    var it = dbContext.tblOrderBuffetItems.Where(x => x.ProductId == item.ProductId && x.Printed == false).FirstOrDefault();
                    it.Cancelled = true;
                    it.DateCancelled = DateTime.Now;
                    dbContext.Entry(it).State = EntityState.Modified;
                    dbContext.SaveChanges();
                    items += 1;
                }
            }
            string message = "Total " + items + " cancelled";
            return Json(message, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPrintersList()
        {
            var printers = dbContext.tblPrinters.Select(x => x.PrinterName).ToList();
            return Json(printers, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GenerateQRCode(int code, string userType)
        {
            //string folderPath = "C:\\TableCodeImages";
            //if (!System.IO.Directory.Exists(folderPath))
            //{
            //    System.IO.Directory.CreateDirectory(folderPath);
            //}
            string response = "";
            try
            {
                string folderPath = "~/Content/Images/QRCode/";
                string imagePath = "~/Content/Images/QRCode/" + code.ToString() + ".jpg";
                // If the directory doesn't exist then create it.
                if (!Directory.Exists(Server.MapPath(folderPath)))
                {
                    Directory.CreateDirectory(Server.MapPath(folderPath));
                }
                if (response == "")
                {



                    if (System.IO.File.Exists(imagePath))
                        System.IO.File.Delete(imagePath);

                    string appURL = ConfigurationManager.AppSettings["AppUrl"];
                    string qrcodeText = Convert.ToString(code) + ";" + appURL;
                    var barcodeWriter = new BarcodeWriter();
                    barcodeWriter.Format = BarcodeFormat.QR_CODE;
                    var result = barcodeWriter.Write(qrcodeText);
                    string barcodePath = Server.MapPath(imagePath);
                    var barcodeBitmap = new Bitmap(result);
                    using (MemoryStream memory = new MemoryStream())
                    {
                        using (FileStream fs = new FileStream(barcodePath, FileMode.Create, FileAccess.ReadWrite))
                        {
                            barcodeBitmap.Save(memory, ImageFormat.Jpeg);
                            byte[] bytes = memory.ToArray();
                            fs.Write(bytes, 0, bytes.Length);
                        }
                    }
                    string QRImageUrl = ConfigurationManager.AppSettings["QRImageUrl"];
                    imagePath = imagePath.Replace("~", appURL);
                    if (userType == "Staff")
                    {
                        imagePath = "~/Content/Images/QRCode/" + code.ToString() + ".jpg";
                        imagePath = imagePath.Replace("~", QRImageUrl);
                    }

                    response = imagePath;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message;
                //throw;
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetVoucherDetails(string voucherCode)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["TCBROMSConnection"].ConnectionString;
            int resId = 0;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT RestaurantID FROM tblRestaurant";
                resId = Convert.ToInt32(cmd.ExecuteScalar());
            }

            //Call API to TCBAPI to get voucherdetails
            VoucherModel vm = new VoucherModel();
            try
            {

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(url);
                    var responseTask = client.GetAsync("GetVoucherDetails?resId=" + resId + "&voucherCode=" + voucherCode);
                    responseTask.Wait();

                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsAsync<VoucherModel>();
                        readTask.Wait();
                        vm = readTask.Result;
                        if (voucherCode.Substring(0, 2) == "RS" && vm.VoucherStatus == "VALID")
                        {
                            //check if reservation code already used
                            var res = dbContext.tblReservations.Where(x => x.UniqueCode == vm.VoucherCode && x.UniqueCodeUsed == true && x.ContactNumber == vm.Mobile).FirstOrDefault();
                            if (res != null)
                            {
                                vm.IsValid = false;
                                vm.Message = "Deposit already applied on table";
                                vm.VoucherStatus = "REDEEMED";
                            }
                        }
                        else if (voucherCode.Substring(0, 2) == "CB" && vm.VoucherStatus == "VALID")
                        {
                            string desc = "";
                            int day = (int)DateTime.Now.DayOfWeek;
                            if (day == 0)
                                day = 7;
                            int time = Convert.ToInt32(DateTime.Now.ToString("HHmm"));
                            int age = DateTime.Now.Year - vm.BdayDate.Year;
                            if (age > 10)
                            {
                                vm.VoucherType = VoucherTypes.Adult.ToString();
                                desc = "Buffet Adult";
                            }
                            else if (age > 6)
                            {
                                vm.VoucherType = VoucherTypes.Juniors.ToString();
                                desc = "Buffet Junior";
                            }
                            else if (age > 3)
                            {
                                vm.VoucherType = VoucherTypes.Kids.ToString();
                                desc = "Buffet Kids";
                            }
                            var product = (from a in dbContext.tblProducts
                                           join b in dbContext.tblProductTimeRestrictions on a.ProductID equals b.ProductID
                                           where (a.Description.Contains(desc) || (a.EnglishName != null &&  a.EnglishName.Contains(desc))) && a.DelInd == false
                                           && b.DayID == day && b.StartTime <= time && b.EndTime >= time
                                           select new
                                           {
                                               ProductId = a.ProductID,
                                               Price = a.Price,
                                               Description = a.Description
                                           }).FirstOrDefault();
                            vm.ProductId = product.ProductId;
                            vm.Price = (decimal)product.Price;
                            //vm.VoucherValue = product.Price.ToString();
                            vm.Amount = (decimal)product.Price;
                            vm.DiscountAmount = (decimal)product.Price;
                            vm.VoucherValue = desc;

                            //vm.VoucherValue = "BDY";
                        }
                    }
                    else //web api sent error response 
                    {
                        //log response status here..

                        var vm1 = Enumerable.Empty<VoucherModel>();

                        ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                    }
                }
                vm.VoucherCode = voucherCode;
            }
            catch (Exception ex)
            {
                vm.IsValid = false;
                vm.Description = "Unable to fetch voucher details. Please try again later";
            }
            return Json(vm, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RedeemVoucher(VoucherModel vm)
        {
            //Call API to mark voucher redeemed in DB
            int prDscountId = Convert.ToInt32(ConfigurationManager.AppSettings["PromotionDiscount"]);
            int onlineDepositId = Convert.ToInt32(ConfigurationManager.AppSettings["OnlineDepositId"]);
            if (vm.VoucherCode.Substring(0, 2) == "PR")
            {
                vm.VoucherType = VoucherTypes.Promotion.ToString();
                var appliedVouchers = dbContext.tblOrderVouchers.Where(x => x.OrderGUID == vm.OrderId && x.VoucherType == vm.VoucherType).ToList();
                var orderItems = (from a in dbContext.tblOrderParts
                                  join b in dbContext.tblProducts on a.ProductID equals b.ProductID
                                  join c in dbContext.tblProductTypes on b.ProductTypeID equals c.ProductTypeID
                                  where a.OrderGUID == vm.OrderId && a.DelInd == false
                                  select new
                                  {
                                      ProductId = a.ProductID,
                                      Description = b.Description,
                                      Price = a.Price,
                                      ProductType = c.TypeDescription
                                  }).ToList();

                var buffetItems = orderItems.Where(x => x.Description.Contains("Buffet") || x.Description.Contains("Meal")).ToList();
                var drinkItems = orderItems.Where(x => x.ProductType.Contains("Wet")).ToList();
                if (vm.VoucherValue.Contains("PBE"))
                {
                    if (buffetItems.Count == 0)
                    {
                        vm.IsValid = false;
                        vm.Message = "Add buffet to apply this code";
                    }
                    else
                    {
                        var dp = orderItems.Find(x => x.ProductId == prDscountId);
                        if (dp == null)
                        {
                            vm.IsValid = true;
                            //Calculate discount value
                            var buffetValue = buffetItems.Sum(x => x.Price);
                            decimal d = Convert.ToInt32(vm.VoucherValue.Substring(3)) * (decimal)0.01;
                            vm.Amount = Math.Round((decimal)buffetValue * d, 2);
                            //Call API to mark voucher redeemed in TCB DB
                            using (var client = new HttpClient())
                            {
                                client.BaseAddress = new Uri(url);
                                var responseTask = client.GetAsync("RedeemVoucher?voucherCode=" + vm.VoucherCode + "&amount=" + vm.Amount.ToString());
                                responseTask.Wait();

                                var result = responseTask.Result;
                                if (result.IsSuccessStatusCode)
                                {
                                    var readTask = result.Content.ReadAsAsync<string>();
                                    readTask.Wait();
                                    vm.Message = readTask.Result;

                                }
                                else //web api sent error response 
                                {
                                    //log response status here..

                                    var vm1 = Enumerable.Empty<VoucherModel>();

                                    ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                                }
                            }
                            //Add product in ordered products

                            vm.Message = rh.AddProductToOrder(prDscountId, vm.Amount * -1, vm.OrderId);
                            //Insert record in tblVouchers
                            if (vm.Message == "")
                                vm.Message = rh.AddVoucherToOrder(vm);
                        }
                        else
                            vm.Message += "Only 1 voucher per table is valid. Similar voucher already applied to table";
                        //return message
                        if (vm.Message == "")
                            vm.Message = "Voucher applied successfully";
                        //else
                        //    vm.Message += "We could not apply voucher. Please try again later";
                    }
                }

            }
            else if (vm.VoucherCode.Substring(0, 2) == "RS")
            {
                vm.VoucherType = VoucherTypes.Reservation.ToString();
                //Calculate discount value
                int depPrice = Convert.ToInt32(vm.VoucherValue) / vm.NoOfGuests;
                //Call API to mark voucher redeemed in TCB DB
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(url);
                    var responseTask = client.GetAsync("RedeemVoucher?voucherCode=" + vm.VoucherCode + "&amount=" + vm.Amount.ToString());
                    responseTask.Wait();

                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsAsync<string>();
                        readTask.Wait();
                        vm.Message = readTask.Result;

                    }
                    else //web api sent error response 
                    {
                        //log response status here..

                        var vm1 = Enumerable.Empty<VoucherModel>();

                        ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                    }
                }
                //Add product in ordered products
                vm.Message = rh.AddProductToOrder(onlineDepositId, depPrice * -1, vm.OrderId);
                //for (int i = 0; i < vm.NoOfGuests; i++)
                //{
                //    vm.Message = rh.AddProductToOrder(onlineDepositId, depPrice * -1, vm.OrderId);
                //}
                //Update Reservation record
                var res = dbContext.tblReservations.Where(x => x.UniqueCode == vm.VoucherCode).FirstOrDefault();
                if (res != null)
                {
                    res.Processed = true;
                    res.ProcessedDate = DateTime.Now;
                    res.ProcessedOrderGUID = vm.OrderId;
                    dbContext.Entry(res).State = EntityState.Modified;
                    dbContext.SaveChanges();
                }
                else
                {
                    ObjectParameter resId = new ObjectParameter("ReservationID", typeof(int));
                    int rId = dbContext.usp_SS_AllocateOnlineReservation(vm.ForeName, vm.SurName, vm.Mobile, vm.OrderId, vm.VoucherCode, resId);
                }
                //Insert record in tblVouchers
                vm.VoucherValue = "RES";
                vm.Amount = depPrice;
                if (vm.Message == "")
                    vm.Message = rh.AddVoucherToOrder(vm);
                //return message
                if (vm.Message == "")
                    vm.Message = "Voucher applied successfully";
                else
                    vm.Message += "We could not apply voucher. Please try again later";

            }
            else if (vm.VoucherCode.Substring(0, 2) == "CB")
            {
                vm.VoucherType = VoucherTypes.Adult.ToString();
                vm.VoucherValue = "BDY";

                //Call API to mark voucher redeemed in TCB DB
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(url);
                    var responseTask = client.GetAsync("RedeemVoucher?voucherCode=" + vm.VoucherCode + "&amount=" + vm.Amount.ToString());
                    responseTask.Wait();

                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsAsync<string>();
                        readTask.Wait();
                        vm.Message = readTask.Result;

                    }
                    else //web api sent error response 
                    {
                        //log response status here..

                        var vm1 = Enumerable.Empty<VoucherModel>();

                        ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                    }
                }
                //Add product in ordered products
                vm.Message = rh.AddProductToOrder(vm.ProductId, vm.Amount * -1, vm.OrderId);

                //Insert record in tblVouchers
                if (vm.Message == "")
                    vm.Message = rh.AddVoucherToOrder(vm);
                //return message
                if (vm.Message == "")
                    vm.Message = "Voucher applied successfully";
                else
                    vm.Message += "We could not apply voucher. Please try again later";

            }
            return Json(vm.Message, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SubmitOrderFeedback(OrderFeedback req)
        {
            try
            {


                tblOrderFeedback tof = new tblOrderFeedback();
                tof.DateCreated = DateTime.Now;
                tof.Feedback = req.Feedback;
                tof.LastModified = DateTime.Now;
                tof.OrderGUID = req.OrderGUID;
                tof.UserId = req.UserId;
                dbContext.tblOrderFeedbacks.Add(tof);
                dbContext.SaveChanges();
                if (req.Feedback == "Bad")
                {
                    var table = dbContext.tblTables.Where(x => x.TableID == req.TableId).FirstOrDefault();
                    table.PastStatus = table.CurrentStatus;
                    table.CurrentStatus = (int)TableState.BadFeedback;
                    dbContext.Entry(table).State = EntityState.Modified;
                    dbContext.SaveChanges();
                }
                return Json("Feedback submitted.", JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                return Json("Some error occured. Please try again.", JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetTableSections()

        {
            List<TableSection> tsList = new List<TableSection>();
            var sectionList = (from a in dbContext.tblTableSections
                               where a.DelInd == false
                               select new
                               {
                                   SectionId = a.SectionId,
                                   SectionName = a.SectionName
                               }).ToList();
            TableSection ts = new TableSection();
            ts.SectionId = 100;
            ts.SectionName = "All";
            tsList.Add(ts);
            foreach (var item in sectionList)
            {
                TableSection ts1 = new TableSection();
                ts1.SectionId = item.SectionId;
                ts1.SectionName = item.SectionName;
                tsList.Add(ts1);
            }

            return Json(tsList, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetTablesBySection(int sectionId)

        {
            Models.UserService us = new Models.UserService();
            TablesList tablesList = new TablesList();
            if (sectionId == 100)
                tablesList = us.GetTablesList();
            else
                tablesList = us.GetTablesBySection(sectionId);

            return Json(tablesList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReserveTable(int reservationId, string tableNumbers)

        {
            string response = "";
            string tables = "";
            try
            {
                var res = dbContext.tblReservations.Where(x => x.ReservationID == reservationId).FirstOrDefault();
                //string[] tablesList = tableNumbers.Split(",");
                //foreach (string table in tablesList)
                //{

                //}
                res.Tables = tableNumbers;
                dbContext.Entry(res).State = EntityState.Modified;
                dbContext.SaveChanges();
                response = "Tables reserved successfully";

            }
            catch (Exception ex)
            {
                response = "We could not complete your resquest. Kindly try later";
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        
        #region DeliveryStaff
        public ActionResult GetDeliveryStaff()
        {
            //DateTime d = DateTime.Parse(date);
            //var ds = (from a in dbContext.tblDeliveryStaffs
            //         join b in dbContext.tblTakeAwayOrderDeliveries on a.DeliveryStaffId equals b.DeliveryStaffId
            //          where a.DelInd == false   
            //          group b by new { a.DeliveryStaffId, a.Name, a.Mobile, a.Available } into g
            //          select new AvailableDeliveryStaff
            //          {
            //              DeliveryStaffId = g.Key.DeliveryStaffId,
            //              Name = g.Key.Name,
            //              Mobile = g.Key.Mobile,
            //              Available = (bool)g.Key.Available,
            //              AmountDue = (decimal)(g.Sum(b=>b.Amount == null ?0 :b.Amount) - g.Sum(b=>b.AmountCollected == null?0:b.AmountCollected))
            //          }).ToList();
            var ds = (from a in dbContext.tblDeliveryStaffs
                      where a.DelInd == false
                      select new AvailableDeliveryStaff
                      {
                          DeliveryStaffId = a.DeliveryStaffId,
                          Name = a.Name,
                          Mobile = a.Mobile,
                          Available = (bool)a.Available,
                          AmountDue = 0,
                          OutForDelivery = (bool)a.OutForDelivery,
                          CollectedAt = "",
                          DeliveredAt = ""
                      }).ToList();
            foreach (var item in ds)
            {

                if (item.Available == null)
                    item.Available = false;
                if (item.OutForDelivery == null)
                    item.OutForDelivery = false;
                if (dbContext.tblTakeAwayOrderDeliveries.Any(x => x.DeliveryStaffId == item.DeliveryStaffId))
                //item.AmountDue = (decimal)dbContext.tblTakeAwayOrderDeliveries
                //    .Where(x => x.DeliveryStaffId == item.DeliveryStaffId).
                //    Sum(b => b.Amount == null ? 0 : b.Amount - b.AmountCollected == null ? 0 : b.AmountCollected);

                {
                    var amount = dbContext.tblTakeAwayOrderDeliveries
                          .Where(x => x.DeliveryStaffId == item.DeliveryStaffId)
                          .Sum(y => y.Amount == null ? 0 : y.Amount);
                    var amountCollected = dbContext.tblTakeAwayOrderDeliveries
                         .Where(x => x.DeliveryStaffId == item.DeliveryStaffId)
                          .Sum(y => y.AmountCollected == null ? 0 : y.AmountCollected);
                    item.AmountDue = (decimal)(amount - amountCollected);
                    var times = dbContext.tblTakeAwayOrderDeliveries.Where(x => x.DeliveryStaffId == item.DeliveryStaffId
                                && DbFunctions.TruncateTime(x.CollectedAt) == DbFunctions.TruncateTime(DateTime.Now)).OrderByDescending(x => x.CollectedAt).FirstOrDefault();
                    if (times != null)
                    {
                        item.CollectedAt = ((DateTime)times.CollectedAt).ToString("HH:mm");
                        item.DeliveredAt = times.DeliveredAt == null ? "" : ((DateTime)times.DeliveredAt).ToString("HH:mm");
                    }
                    else
                    {
                        item.CollectedAt = "";
                        item.DeliveredAt = "";
                    }
                }
            }
            ds = ds.OrderByDescending(x => x.OutForDelivery).ThenByDescending(x => x.Available).ToList();
            return Json(ds, JsonRequestBehavior.AllowGet);

        }

        public ActionResult SaveAvailableDeliveryStaff(DeliveryStaffModel req)
        {
            //dbContext.Database.ExecuteSqlCommand("Delete from tblAvailableDeliveryStaff");
            foreach (var item in req.DeliveryStaff)
            {
                //tblAvailableDeliveryStaff ds = new tblAvailableDeliveryStaff();
                //ds.DeliveryStaffId = item.DeliveryStaffId;
                //ds.Available = item.Available;
                //ds.MaxOrders = item.MaxOrders;
                //dbContext.tblAvailableDeliveryStaffs.Add(ds);
                //dbContext.SaveChanges();
                var ds = dbContext.tblDeliveryStaffs.Where(x => x.DeliveryStaffId == item.DeliveryStaffId).FirstOrDefault();
                ds.Available = item.Available;
                ds.OutForDelivery = item.OutForDelivery;
                dbContext.Entry(ds).State = EntityState.Modified;
                dbContext.SaveChanges();
            }
            return Json("Save successfull", JsonRequestBehavior.AllowGet);
        }

        public ActionResult SetDriverUnavailable(int dsId)
        {
            var ds = dbContext.tblDeliveryStaffs.Where(x => x.DeliveryStaffId == dsId).FirstOrDefault();
            ds.Available = false;
            ds.OutForDelivery = false;
            dbContext.Entry(ds).State = EntityState.Modified;
            dbContext.SaveChanges();
            return Json("Save successfull", JsonRequestBehavior.AllowGet);
        }
        public DateTime RoundUp(DateTime dt, TimeSpan d)
        {
            return new DateTime((dt.Ticks + d.Ticks - 1) / d.Ticks * d.Ticks, dt.Kind);
        }
        public ActionResult GetAvailableTakeAwaySlots(string type)
        {

            ReservationSlotModel rsm = new ReservationSlotModel();
            bool isTakeAwayAvailable = Convert.ToBoolean(ConfigurationManager.AppSettings["isTakeAwayAvailable"]);
            if (isTakeAwayAvailable)
            {


                var maxOrders = dbContext.tblDeliveryStaffs.Where(x => x.Available == true || x.OutForDelivery == true).Count();
                int maxOrdersPerDriver = Convert.ToInt32(ConfigurationManager.AppSettings["MaxOrdersPerDriver"]);
                maxOrders = maxOrders * maxOrdersPerDriver;
                int collectionThresholdTime = Convert.ToInt32(ConfigurationManager.AppSettings["CollectionThresholdTime"]);
                int deliveryThresholdTime = Convert.ToInt32(ConfigurationManager.AppSettings["DeliveryThresholdTime"]);
                int deliveryStartTime = Convert.ToInt32(ConfigurationManager.AppSettings["DeliveryStartTime"]);
                int collectionStartTime = Convert.ToInt32(ConfigurationManager.AppSettings["CollectionStartTime"]);
                DateTime deliverystime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, deliveryStartTime, 00, 0);
                DateTime collectionstime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, collectionStartTime, 00, 0);
                int deliveryEndTime = Convert.ToInt32(ConfigurationManager.AppSettings["DeliveryEndTime"]);
                if (DateTime.Now > deliverystime)
                {
                    deliverystime = DateTime.Now;
                }
                if (DateTime.Now > collectionstime)
                {
                    collectionstime = DateTime.Now;
                }

                int slotInterval = Convert.ToInt32(ConfigurationManager.AppSettings["SlotsIntervalPeriod"]);
                DateTime currDate = new DateTime();
                if (type == "Delivery")
                {
                    currDate = RoundUp(deliverystime, TimeSpan.FromMinutes(slotInterval));
                    currDate = currDate.AddMinutes(deliveryThresholdTime);
                }
                else
                {
                    currDate = RoundUp(collectionstime, TimeSpan.FromMinutes(slotInterval));
                    currDate = currDate.AddMinutes(collectionThresholdTime);
                }


                DateTime endTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, Convert.ToInt32(Convert.ToString(deliveryEndTime).Substring(0, 2)), Convert.ToInt32(Convert.ToString(deliveryEndTime).Substring(2)), 0);
                while (currDate <= endTime)
                {
                    ReservationSlot rsd = new ReservationSlot();
                    int currTime = Convert.ToInt32(currDate.ToString("HHmm"));
                    rsd.Time = currTime.ToString();
                    currDate = currDate.AddMinutes(slotInterval);
                    //if (type == "Delivery")
                    //{
                    //    var bookedOrdersCount = dbContext.tblTakeAwayOrders.Where(x => DbFunctions.TruncateTime(x.DateCreated) == DbFunctions.TruncateTime(DateTime.Now) && x.CollectionTime == currTime).Count();
                    //    if (maxOrders > bookedOrdersCount)
                    //        rsm.ReservationSlots.Add(rsd);
                    //}
                    //else
                    //    rsm.ReservationSlots.Add(rsd);
                    rsm.ReservationSlots.Add(rsd);
                }
                if (rsm.ReservationSlots.Count > 0)
                    rsm.SlotAvailable = true;
            }
            else
                rsm.SlotAvailable = false;

            return Json(rsm, JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetDeliveryStaffOrders(int deliveryStaffId, string fromDate, string toDate)
        {
            DateTime fDate = DateTime.Parse(fromDate);
            DateTime tDate = DateTime.Parse(toDate);
            tDate = new DateTime(tDate.Year, tDate.Month, tDate.Day, 23, 59, 59);
            var orders = (from a in dbContext.tblTakeAwayOrderDeliveries
                          join b in dbContext.tblTakeAwayOrders on a.TakeAwayId equals b.TakeAwayId
                          where a.DeliveryStaffId == deliveryStaffId && a.DateCreated >= fDate && a.DateCreated <= tDate
                          select new TakeAwayOrder
                          {
                              OrderGUID = b.OrderGUID,
                              OrderNumber = b.OrderNumber,
                              dtDateCreated = a.DateCreated,
                              vCustomerName = b.NAME,
                              Amount = (a.Amount == null ? 0 : a.Amount) - (a.AmountCollected == null ? 0 : a.AmountCollected)
                          }).OrderByDescending(x => x.dtDateCreated).ToList();

            return Json(orders, JsonRequestBehavior.AllowGet);

        }

        public ActionResult SaveAmountCollected(Guid orderId, decimal amount, int userId)
        {
            var order = dbContext.tblTakeAwayOrderDeliveries.Where(x => x.OrderGUID == orderId).FirstOrDefault();
            order.CollectedAt = DateTime.Now;
            order.CollectedBy = userId;
            order.AmountCollected = (order.AmountCollected == null ? 0 : order.AmountCollected) + amount;
            dbContext.Entry(order).State = EntityState.Modified;
            dbContext.SaveChanges();
            return Json("Amount saved successfully", JsonRequestBehavior.AllowGet);
        }

        //For printing single order from TILL
        public ActionResult PrintTakeAwayOrder(TakeAwayOrder req)
        {
            TakeawayPrintModel tpm = new TakeawayPrintModel();
            tpm.GroupCount = 1;
            tpm.GroupId = 1;
            tpm.Orders.Add((Guid)req.OrderGUID);
            string response = rh.PrintTakeAwayOrder(tpm);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        //For printing all orders from Service
        //public ActionResult PrintTakeAwayOrders(List<Guid> req)
        public ActionResult PrintTakeAwayOrders(List<TakeawayPrintModel> req)
        {
            string response = "";
            foreach (var item in req)
            {
                rh.PrintTakeAwayOrder(item);
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult UpdateTakeAwayOrderDelivery(TakeAwayOrderDelivery req)
        {
            var tord = dbContext.tblTakeAwayOrderDeliveries.Where(x => x.OrderGUID == req.OrderGUID).FirstOrDefault();
            if (tord == null)
            {
                tblTakeAwayOrderDelivery to = new tblTakeAwayOrderDelivery();
                to.DateCreated = DateTime.Now;
                to.CollectedAt = DateTime.Now;
                to.DeliveryStaffId = req.DeliveryStaffId;
                to.TakeAwayId = req.TakeAwayId;
                to.OrderGUID = req.OrderGUID;
                to.LastModified = DateTime.Now;
                to.Amount = req.Amount;
                dbContext.tblTakeAwayOrderDeliveries.Add(to);
                dbContext.SaveChanges();

            }
            else
            {
                tord.CollectedAt = DateTime.Now;
                tord.DeliveryStaffId = req.DeliveryStaffId;
                dbContext.Entry(tord).State = EntityState.Modified;
                dbContext.SaveChanges();
            }
            //Mark driver Out for Delivery
            var ds = dbContext.tblDeliveryStaffs.Where(x => x.DeliveryStaffId == req.DeliveryStaffId).FirstOrDefault();
            ds.OutForDelivery = true;
            dbContext.Entry(ds);
            dbContext.SaveChanges();
            return Json("Order printed succesfully", JsonRequestBehavior.AllowGet);
        }

        public string ConfirmDriverAvailability(int deliveryStaffId)
        {
            string response = "";
            try
            {
                var driver = dbContext.tblDeliveryStaffs.Where(x => x.DeliveryStaffId == deliveryStaffId).FirstOrDefault();
                if (driver != null)
                {
                    driver.Available = true;
                    driver.OutForDelivery = false;
                    dbContext.Entry(driver).State = EntityState.Modified;
                    dbContext.SaveChanges();
                    return "Done";
                }
                else
                    return "User not found";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        #endregion


        #region Voucher Code

        public ActionResult UpdatePercentageDiscount(Guid orderId)

        {
            string response = "";
            string tables = "";
            try
            {
                if (orderId != null && orderId != Guid.Empty)
                {
                    var vouchers = dbContext.tblOrderVouchers.Where(x => x.OrderGUID == orderId && x.VoucherValue.Contains("P")).FirstOrDefault();
                    var orderProducts = (from a in dbContext.tblOrderParts
                                         join b in dbContext.tblProducts on a.ProductID equals b.ProductID
                                         join c in dbContext.tblProductTypes on b.ProductTypeID equals c.ProductTypeID
                                         where a.DelInd == false
                                         select new
                                         {
                                             a.ProductID,
                                             a.Price,
                                             b.Description,
                                             b.EnglishName,
                                             c.TypeDescription

                                         }).ToList();
                    if (orderProducts != null && orderProducts.Count > 0)
                    {
                        //var adCount = orderProducts.Where(x => x.EnglishName.Contains("Buffet Adult")).Count();
                        //var kdCount = orderProducts.Where(x => x.EnglishName.Contains("Buffet Kid")).Count();
                        //var jnCount = orderProducts.Where(x => x.EnglishName.Contains("Buffet Junior")).Count();
                        //var drinkCount = orderProducts.Where(x => x.TypeDescription.Contains("Wet")).Count();

                        var adTotal = orderProducts.Where(x => x.EnglishName.Contains("Buffet Adult")).Sum(x => x.Price);
                        var kdTotal = orderProducts.Where(x => x.EnglishName.Contains("Buffet Kid")).Sum(x => x.Price);
                        var jnTotal = orderProducts.Where(x => x.EnglishName.Contains("Buffet Junior")).Sum(x => x.Price);
                        var drinkTotal = orderProducts.Where(x => x.TypeDescription.Contains("Wet")).Sum(x => x.Price);

                        decimal newDiscount = 0;
                        var discountPercentage = Convert.ToInt32(vouchers.VoucherValue.Substring(3)) / 100;
                        if (vouchers.VoucherValue.Substring(0, 3) == "PBK")
                            newDiscount = (decimal)(kdTotal * discountPercentage * -1);
                        else if (vouchers.VoucherValue.Substring(0, 3) == "PBJ")
                            newDiscount = (decimal)(jnTotal * discountPercentage * -1);
                        else if (vouchers.VoucherValue.Substring(0, 3) == "PBA")
                            newDiscount = (decimal)(adTotal * discountPercentage * -1);
                        else if (vouchers.VoucherValue.Substring(0, 3) == "PBE")
                            newDiscount = (decimal)((kdTotal + adTotal + jnTotal) * discountPercentage * -1);
                        else if (vouchers.VoucherValue.Substring(0, 3) == "PBD")
                            newDiscount = (decimal)(drinkTotal * discountPercentage * -1);

                        //Update discount in tblOrderPart
                        var discountProduct = dbContext.tblOrderParts.Where(x => x.ProductID == 1909 && x.DelInd == false).FirstOrDefault();
                        discountProduct.Price = newDiscount;
                        discountProduct.Total = newDiscount;

                        dbContext.Entry(discountProduct).State = EntityState.Modified;
                        dbContext.SaveChanges();
                        response = "Discount updated successfully";
                    }
                  

                }
                //get vouchers on this order with percentage discount

            }
            catch (Exception ex)
            {
                response = "We could not complete your resquest. Kindly try later";
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetVoucherDetailsV1(string voucherCode, string orderId)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["TCBROMSConnection"].ConnectionString;
            int resId = 0;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT RestaurantID FROM tblRestaurant";
                resId = Convert.ToInt32(cmd.ExecuteScalar());
            }

            //Call API to TCBAPI to get voucherdetails
            VoucherModel vm = new VoucherModel();
            try
            {
                //09-03-2024 Changes by Gaurav to check if reservation code already used
                if (voucherCode.Contains("RS"))
                {
                    vm.Message = rh.ReservationVoucherUsed(voucherCode);
                    if (vm.Message != "")
                    {
                        vm.IsValid = false;
                        vm.VoucherStatus = "INVALID";
                    }
                }
                if (!voucherCode.Contains("RS") || (voucherCode.Contains("RS") && vm.Message == ""))
                {
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(url);
                        var responseTask = client.GetAsync("GetVoucherDetails?resId=" + resId + "&voucherCode=" + voucherCode);
                        responseTask.Wait();

                        var result = responseTask.Result;
                        if (result.IsSuccessStatusCode)
                        {
                            var readTask = result.Content.ReadAsAsync<VoucherModel>();
                            readTask.Wait();
                            vm = readTask.Result;
                            //vm.PromoCode = vm.VoucherValue;
                            if (vm.VoucherStatus == "INVALID")
                            {
                                vm.IsValid = false;
                                if (vm.VoucherStatus == "TOO SOON")
                                    vm.Message = "TOO SOON";
                                else
                                    vm.Message = "Invalid Code";

                            }
                            else if (vm.VoucherStatus == "TOO SOON")
                            {
                                vm.Message = "TOO SOON";
                                vm.IsValid = false;
                            }
                            else
                            {
                                if (voucherCode.Substring(0, 2) == "RS" && vm.VoucherStatus == "VALID")
                                {
                                    //check if reservation code already used
                                    var res = dbContext.tblReservations.Where(x => x.UniqueCode == vm.VoucherCode && x.UniqueCodeUsed == true && x.ContactNumber == vm.Mobile).FirstOrDefault();
                                    vm.DiscountAmount = Convert.ToDecimal(vm.VoucherValue);
                                    if (res != null)
                                    {
                                        vm.IsValid = false;
                                        vm.Message = "Deposit already applied on table";
                                        vm.VoucherStatus = "REDEEMED";
                                    }
                                    vm.OrderId = new Guid(orderId);
                                }
                                else if (voucherCode.Substring(0, 2) == "CB" && vm.VoucherStatus == "VALID")
                                {
                                    string desc = "";
                                    int day = (int)DateTime.Now.DayOfWeek;
                                    if (day == 0)
                                        day = 7;
                                    int time = Convert.ToInt32(DateTime.Now.ToString("HHmm"));
                                    int age = DateTime.Now.Year - vm.BdayDate.Year;
                                    vm.IsValid = true;
                                    if (age > 10)
                                    {
                                        vm.VoucherType = VoucherTypes.Adult.ToString();
                                        desc = "Buffet Adult";
                                    }
                                    else if (age > 6)
                                    {
                                        vm.VoucherType = VoucherTypes.Juniors.ToString();
                                        desc = "Buffet Junior";
                                    }
                                    else if (age > 3)
                                    {
                                        vm.VoucherType = VoucherTypes.Kids.ToString();
                                        desc = "Buffet Kids";
                                    }
                                    else
                                    {
                                        vm.IsValid = false;
                                        vm.Message = "DOB under 3 years. Code not applicable";
                                    }
                                    if (vm.IsValid)
                                    {
                                        var product = (from a in dbContext.tblProducts
                                                       join b in dbContext.tblProductTimeRestrictions on a.ProductID equals b.ProductID
                                                       where (a.Description.Contains(desc) || (a.EnglishName != null && a.EnglishName.Contains(desc))) && a.DelInd == false
                                                       && b.DayID == day && b.StartTime <= time && b.EndTime >= time && b.DelInd == false
                                                       select new
                                                       {
                                                           ProductId = a.ProductID,
                                                           Price = a.Price,
                                                           Description = a.Description
                                                       }).FirstOrDefault();
                                        vm.ProductId = product.ProductId;
                                        vm.Price = (decimal)product.Price;
                                        vm.VoucherValue = desc;
                                        vm.Amount = (decimal)product.Price;
                                        vm.DiscountAmount = (decimal)product.Price;
                                        vm.ProductName = product.Description;
                                        vm.OrderId = new Guid(orderId);
                                        vm.VoucherCode = voucherCode;
                                        vm.VoucherType = VoucherTypes.Birthday.ToString();

                                        if (vm.VoucherStatus == "VALID" || vm.VoucherStatus != "REDEEMED")
                                        {
                                            vm = VoucherValidations(vm);
                                        }
                                    }

                                    //vm.VoucherValue = "BDY";
                                }
                                else if ((voucherCode.Substring(0, 2) == "PR" || voucherCode.Substring(0, 2) == "GV") && vm.VoucherStatus == "VALID")
                                {
                                    if (vm.IsValid)
                                    {
                                        double discountPercentage = 0;
                                        double discountValue = 0;
                                        string text = "";
                                        if (vm.VoucherValue.Substring(0, 1) == "P")
                                        {
                                            if (vm.VoucherValue.Substring(1, 2) != "PP")
                                                discountPercentage = Convert.ToDouble(vm.VoucherValue.Substring(3));

                                            if (vm.VoucherValue.Substring(1, 2) == "BA")
                                                text = discountPercentage.ToString() + "% On Adult Buffet";
                                            else if (vm.VoucherValue.Substring(1, 2) == "BK")
                                                text = discountPercentage.ToString() + "% On Kids Buffet";
                                            else if (vm.VoucherValue.Substring(1, 2) == "BJ")
                                                text = discountPercentage.ToString() + "% On Juniors Buffet";
                                            else if (vm.VoucherValue.Substring(1, 2) == "BE")
                                                text = discountPercentage.ToString() + "% On Buffet";
                                            else if (vm.VoucherValue.Substring(1, 2) == "DE")
                                                text = discountPercentage.ToString() + "% On Drinks";
                                            else if (vm.VoucherValue.Substring(1, 2) == "BD")
                                                text = discountPercentage.ToString() + "% On Bill";
                                            else if (vm.VoucherValue.Substring(1, 2) == "PP")
                                                text = vm.Description;

                                        }
                                        else if (vm.VoucherValue.Substring(0, 1) == "F")
                                        {
                                            if (vm.VoucherValue.Length > 6)
                                                discountValue = Convert.ToDouble(vm.VoucherValue.Substring(3, 3));
                                            else
                                                discountValue = Convert.ToDouble(vm.VoucherValue.Substring(3));
                                            if (vm.VoucherValue.Substring(1, 2) == "BA")
                                                text = "£" + discountValue.ToString() + " off on Adult Buffet";
                                            else if (vm.VoucherValue.Substring(1, 2) == "BK")
                                                text = "£" + discountValue.ToString() + " off on Kids Buffet";
                                            else if (vm.VoucherValue.Substring(1, 2) == "BJ")
                                                text = "£" + discountValue.ToString() + " off on Juniors Buffet";
                                            else if (vm.VoucherValue.Substring(1, 2) == "BD")
                                                text = "£" + discountValue.ToString() + " off on Bill";
                                            else if (vm.VoucherValue.Substring(1, 2) == "BI")
                                            {
                                                text = "£" + discountValue.ToString() + " Above" + " £" + vm.VoucherValue.Substring(6) + " Buffet";
                                            }
                                            else if (vm.VoucherValue.Substring(1, 2) == "TI")
                                            {
                                                text = "£" + discountValue.ToString() + " Above" + " £" + vm.VoucherValue.Substring(6) + " Bill";
                                            }
                                        }
                                        else if (vm.VoucherValue.Substring(0, 1) == "V")
                                        {
                                            //string strconnection = ConfigurationManager.ConnectionStrings["TCBROMSConnection"].ConnectionString;
                                            //using (SqlConnection conn = new SqlConnection(strconnection))
                                            //{
                                            //    conn.Open();

                                            //    SqlCommand cmd = new SqlCommand("SELECT ProductID,Description,Price,(SELECT dbo.[usp_MF_CheckIfProductIsAvailableAtThisTime](dbo.tblProduct.ProductID))" +
                                            //                "as ProductAvailable  FROM tblProduct where Description like('%Buffet Adult%')", conn);

                                            //    var product = cmd.ExecuteScalar();
                                            //    conn.Close();
                                            //    // text = "£" & Convert.ToString(product.)
                                            //}
                                            text = vm.Description;
                                            string desc = "";
                                            int day = (int)DateTime.Now.DayOfWeek;
                                            if (day == 0)
                                                day = 7;
                                            int time = Convert.ToInt32(DateTime.Now.ToString("HHmm"));
                                            int age = DateTime.Now.Year - vm.BdayDate.Year;
                                            vm.IsValid = true;
                                            vm.VoucherType = VoucherTypes.Adult.ToString();
                                            desc = "Buffet Adult";
                                            if (vm.IsValid)
                                            {
                                                var product = (from a in dbContext.tblProducts
                                                               join b in dbContext.tblProductTimeRestrictions on a.ProductID equals b.ProductID
                                                               where (a.Description.Contains(desc) || (a.EnglishName != null && a.EnglishName.Contains(desc))) && a.DelInd == false
                                                               && b.DayID == day && b.StartTime <= time && b.EndTime >= time
                                                               select new
                                                               {
                                                                   ProductId = a.ProductID,
                                                                   Price = a.Price,
                                                                   Description = a.Description
                                                               }).FirstOrDefault();
                                                vm.ProductId = product.ProductId;
                                                vm.Price = (decimal)product.Price;
                                                vm.VoucherValue = "VAR";
                                                vm.Amount = (decimal)product.Price;
                                                vm.DiscountAmount = (decimal)product.Price;
                                                vm.ProductName = product.Description;
                                                vm.OrderId = new Guid(orderId);
                                                vm.VoucherCode = voucherCode;
                                                vm.VoucherType = VoucherTypes.Promotion.ToString();
                                                discountValue = (double)product.Price;
                                            }
                                        }
                                        else if (vm.VoucherValue.Substring(0, 1) == "S")
                                        {
                                            discountValue = Convert.ToDouble(vm.VoucherValue.Substring(3));
                                            text = "Buffet for £" + Convert.ToString(discountValue);
                                        }
                                        else if (Convert.ToDecimal(vm.VoucherValue) > 0)
                                        {
                                            discountValue = Convert.ToDouble(vm.VoucherValue);
                                        }
                                        //vm.VoucherValue = discountValue.ToString();
                                        vm.DiscountAmount = (decimal)discountValue;
                                        vm.Description = text;
                                        vm.VoucherCode = voucherCode;
                                        if (vm.VoucherStatus == "VALID")
                                        {
                                            vm.IsValid = true;
                                            vm.OrderId = new Guid(orderId);
                                            vm.VoucherType = VoucherTypes.Promotion.ToString();
                                            vm = VoucherValidations(vm);
                                            //vm.DiscountAmount = (decimal)discountValue;
                                            vm.Price = vm.DiscountAmount;
                                        }
                                        else
                                            vm.IsValid = false;
                                    }
                                }
                                //else if (voucherCode.Substring(0, 2) == "GV" && vm.VoucherStatus == "VALID")
                                //{
                                //    Guid orderGUID = new Guid(orderId);
                                //    var orderAmount = dbContext.tblOrderParts.Where(x => x.OrderGUID == orderGUID && x.DelInd == false).Sum(x => x.Price);
                                //    if (orderAmount < Convert.ToDecimal(Math.Round(vm.DiscountAmount, 2)))
                                //        vm.DiscountAmount = (decimal)orderAmount;
                                //    vm.VoucherValue = "£" + vm.VoucherValue + " Gift Voucher";
                                //    vm.OrderId = orderGUID;
                                //    vm.Price = vm.DiscountAmount;
                                //    //vm.Price = Convert.ToDecimal(vm.VoucherValue);
                                //    //vm.DiscountAmount = vm.Price;

                                //}
                            }
                        }
                        else //web api sent error response 
                        {
                            //log response status here..

                            var vm1 = Enumerable.Empty<VoucherModel>();

                            ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                        }
                    }
                }
               

                vm.VoucherCode = voucherCode;
            }
            catch (Exception ex)
            {
                vm.IsValid = false;
                vm.Description = ex.Message;
                if(ex.InnerException != null)
                vm.Description =ex.InnerException.Message;
            }
            return Json(vm, JsonRequestBehavior.AllowGet);
        }

        private VoucherModel VoucherValidations(VoucherModel vm)
        {
            decimal promotionAmount = 0;
            var appliedVouchers = dbContext.tblOrderVouchers.Where(x => x.OrderGUID == vm.OrderId && x.VoucherType == vm.VoucherType).ToList();
            var orderItems = (from a in dbContext.tblOrderParts
                              join b in dbContext.tblProducts on a.ProductID equals b.ProductID
                              join c in dbContext.tblProductTypes on b.ProductTypeID equals c.ProductTypeID
                              where a.OrderGUID == vm.OrderId && a.DelInd == false
                              select new
                              {
                                  ProductId = a.ProductID,
                                  Description = b.Description,
                                  Price = a.Price,
                                  ProductType = c.TypeDescription,
                                  EnglishName = b.EnglishName ?? ""
                              }).ToList();
            if (vm.VoucherCode.Substring(0, 2) == "PR")
            {
                vm.VoucherType = VoucherTypes.Promotion.ToString();
                //Get Already applied Vouchers for this order
                var buffetItems = orderItems.Where(x => x.Description.Contains("Buffet") || x.Description.Contains("Meal")
                || x.EnglishName.Contains("Buffet")).ToList();
                var drinkItems = orderItems.Where(x => !x.ProductType.Contains("Buffet") && !x.Description.Contains("Meal")
                && !x.EnglishName.Contains("Buffet")).ToList();
                var appliedAdCount = 0;
                var appliedKdCount = 0;
                var appliedJnCount = 0;
                var appliedSBPCount = 0;
                var appliedPPPCount = 0;
                var singleVoucherTypes = new List<string>() { "FBI", "FTI", "FBD", "VAR", "BDY", "PBA", "PBK", "PBJ", "PBE", "PBD" };
                var multiVoucherTypes = new List<string>() { "FBA", "FBK", "FBJ", "SBP","PPP" };
                var adBufCount = buffetItems.Where(x => x.Description.Contains("Adult") || (x.EnglishName != null && x.EnglishName.Contains("Adult"))).Count();
                var kdBufCount = buffetItems.Where(x => x.Description.Contains("Kids") || (x.EnglishName != null && x.EnglishName.Contains("Kids"))).Count();
                var jnBufCount = buffetItems.Where(x => x.Description.Contains("Junior") || (x.EnglishName != null && x.EnglishName.Contains("Junior"))).Count();
                //Validations based on already applied vouchers
                if (appliedVouchers.Count > 0)
                {
                    appliedVouchers = appliedVouchers.Where(x => x.VoucherType == vm.VoucherType).ToList();
                    appliedAdCount = appliedVouchers.Where(x => x.VoucherValue.Contains("BA")).Count();
                    appliedKdCount = appliedVouchers.Where(x => x.VoucherValue.Contains("BK")).Count();
                    appliedJnCount = appliedVouchers.Where(x => x.VoucherValue.Contains("BJ")).Count();
                    appliedSBPCount = appliedVouchers.Where(x => x.VoucherValue.Contains("SBP")).Count();
                    appliedPPPCount = appliedVouchers.Where(x => x.VoucherValue.Contains("PPP")).Count();


                    //if (appliedVouchers.Any(x => singleVoucherTypes.Contains(x.VoucherValue))
                    //    || appliedVouchers.Any(x => x.VoucherValue.Substring(0, 1) == "P")
                    //    || (appliedVouchers.Any(x => multiVoucherTypes.Contains(x.VoucherValue))
                    //    && (singleVoucherTypes.Contains(vm.VoucherValue) || vm.VoucherValue.Substring(0, 1) == "P")))
                    //{
                    //    vm.IsValid = false;
                    //    vm.Message = "Only one voucher can be applied";
                    //}
                    foreach (var item in appliedVouchers)
                    {
                        var varVoucherTypes = new List<string>() { "F", "P", "S","V"};
                        if (varVoucherTypes.Contains(item.VoucherValue.Substring(0,1)))
                        {
                            string vv = "";
                            if (item.VoucherValue.Length > 1)
                            vv = item.VoucherValue.Substring(0, 3);
                            else if(item.VoucherValue == "V")
                            vv= "VAR";
                            //     if (appliedVouchers.Any(x => singleVoucherTypes.Contains(x.VoucherValue))
                            //|| appliedVouchers.Any(x => x.VoucherValue.Substring(0, 1) == "P")
                            //|| (appliedVouchers.Any(x => multiVoucherTypes.Contains(x.VoucherValue))
                            //&& (singleVoucherTypes.Contains(vm.VoucherValue) || vm.VoucherValue.Substring(0, 1) == "P")))
                            //     {
                            //         vm.IsValid = false;
                            //         vm.Message = "Only one voucher can be applied";
                            //     }
                            if (singleVoucherTypes.Contains(vv) || (vv.Substring(0, 1) == "P" && vv != "PPP")
                          || (multiVoucherTypes.Contains(vv)
                          && (singleVoucherTypes.Contains(vm.VoucherValue) || (vm.VoucherValue.Substring(0, 1) == "P" && vm.VoucherValue.Substring(0, 3) != "PPP"))))
                            {
                                vm.IsValid = false;
                                vm.Message = "Only one voucher can be applied";
                            }
                        }
                    }
                    
                }
                if (vm.VoucherValue.Contains("FBA"))
                {
                    promotionAmount = Convert.ToDecimal(vm.VoucherValue.Substring(3));
                    if (appliedAdCount >= adBufCount)
                    {
                        vm.IsValid = false;
                        vm.Message = "Add Adult buffet to apply this code";
                    }
                }
                else if (vm.VoucherValue.Contains("FBK"))
                {
                    promotionAmount = Convert.ToDecimal(vm.VoucherValue.Substring(3));
                    if (appliedKdCount >= kdBufCount)
                    {
                        vm.IsValid = false;
                        vm.Message = "Add Kids buffet to apply this code";
                    }
                }
                else if (vm.VoucherValue.Contains("FBJ"))
                {
                    promotionAmount = Convert.ToDecimal(vm.VoucherValue.Substring(3));
                    if (appliedJnCount >= jnBufCount)
                    {
                        vm.IsValid = false;
                        vm.Message = "Add Junior buffet to apply this code";
                    }

                }
                else if (vm.VoucherValue.Contains("FBD"))
                {
                    promotionAmount = Convert.ToDecimal(vm.VoucherValue.Substring(3));
                    if (adBufCount + kdBufCount + jnBufCount + drinkItems.Count == 0)
                    {
                        vm.IsValid = false;
                        vm.Message = "Promo Applicable for Buffet & Drinks only. Please add item";
                    }

                }
                else if (vm.VoucherValue.Contains("PBA"))
                {
                    var bufTotal = buffetItems.Where(x => (x.Description.Contains("Adult") || (x.EnglishName != null && x.EnglishName.Contains("Adult")))).Sum(x => x.Price);
                    promotionAmount = (decimal)(bufTotal * (Convert.ToInt32(vm.VoucherValue.Substring(3)) * (decimal)0.01));
                    if (adBufCount == 0)
                    {
                        vm.IsValid = false;
                        vm.Message = "Add Adult buffet to apply this code";
                    }

                }
                else if (vm.VoucherValue.Contains("PBK"))
                {
                    var bufTotal = buffetItems.Where(x => (x.Description.Contains("Kid") || (x.EnglishName != null && x.EnglishName.Contains("Kid")))).Sum(x => x.Price);
                    promotionAmount = (decimal)(bufTotal * (Convert.ToInt32(vm.VoucherValue.Substring(3)) * (decimal)0.01));
                    if (kdBufCount == 0)
                    {
                        vm.IsValid = false;
                        vm.Message = "Add Kids buffet to apply this code";
                    }
                    else
                    {

                    }
                }
                else if (vm.VoucherValue.Contains("PBJ"))
                {
                    var bufTotal = buffetItems.Where(x => (x.Description.Contains("Junior") || (x.EnglishName != null && x.EnglishName.Contains("Junior")))).Sum(x => x.Price);
                    promotionAmount = (decimal)(bufTotal * (Convert.ToInt32(vm.VoucherValue.Substring(3)) * (decimal)0.01));
                    if (jnBufCount == 0)
                    {
                        vm.IsValid = false;
                        vm.Message = "Add Junior buffet to apply this code";
                    }
                    else
                    {

                    }
                }
                else if (vm.VoucherValue.Contains("PBE"))
                {
                    var bufTotal = buffetItems.Sum(x => x.Price);
                    promotionAmount = (decimal)(bufTotal * (Convert.ToInt32(vm.VoucherValue.Substring(3)) * (decimal)0.01));
                    if (adBufCount + kdBufCount + jnBufCount == 0)
                    {
                        vm.IsValid = false;
                        vm.Message = "Add buffet to apply this code";
                    }
                    else
                    {

                    }
                }
                else if (vm.VoucherValue.Contains("PDE"))
                {
                    var bufTotal = drinkItems.Sum(x => x.Price);
                    promotionAmount = (decimal)(bufTotal * (Convert.ToInt32(vm.VoucherValue.Substring(3)) * (decimal)0.01));
                    if (drinkItems.Count == 0)
                    {
                        vm.IsValid = false;
                        vm.Message = "Add drinks to apply this code";
                    }
                    else
                    {

                    }
                }
                else if (vm.VoucherValue.Contains("PBD"))
                {
                    var bufTotal = buffetItems.Sum(x => x.Price) + drinkItems.Sum(x => x.Price);
                    promotionAmount = (decimal)(bufTotal * (Convert.ToInt32(vm.VoucherValue.Substring(3)) * (decimal)0.01));
                    if (adBufCount + kdBufCount + jnBufCount + drinkItems.Count == 0)
                    {
                        vm.IsValid = false;
                        vm.Message = "Add buffet/drinks to apply this code";
                    }
                    else
                    {

                    }
                }
                else if (vm.VoucherValue.Contains("SBP"))
                {
                    if (appliedSBPCount >= adBufCount || adBufCount == 0)
                    {
                        vm.IsValid = false;
                        vm.Message = "Add Adult buffet to apply this code";
                    }
                    else if(adBufCount > 0)
                    {
                        var bufTotal = buffetItems.Where(x => (x.Description.Contains("Adult") || (x.EnglishName != null && x.EnglishName.Contains("Adult")))).Sum(x => x.Price) / adBufCount;
                        promotionAmount = (decimal)(bufTotal - Convert.ToInt32(vm.VoucherValue.Substring(3)));
                        vm.DiscountAmount = promotionAmount;
                    }
                }
                else if (vm.VoucherValue.Contains("BDY"))
                {
                    promotionAmount = Convert.ToDecimal(vm.VoucherValue.Substring(3));
                    if (appliedVouchers.Where(x => x.VoucherType.Contains("Adult") && x.VoucherValue == "BDY").Count() >= adBufCount)
                    {
                        vm.IsValid = false;
                        vm.Message = "Add Junior buffet to apply this code";
                    }
                    else
                    {

                    }
                }
                else if (vm.VoucherValue.Contains("FTI"))
                {
                    promotionAmount = Convert.ToDecimal(vm.VoucherValue.Substring(3, 3));
                    var minOrderAmount = Convert.ToInt32(vm.VoucherValue.Substring(6, 3));
                    var orderAmount = orderItems.Sum(x => x.Price);
                    if (Convert.ToDecimal(orderAmount) < minOrderAmount)
                    {
                        vm.IsValid = false;
                        vm.Message = "Promotion valid for orders over £" + minOrderAmount.ToString();
                    }
                }
                else if (vm.VoucherValue.Contains("FBI"))
                {
                    promotionAmount = Convert.ToDecimal(vm.VoucherValue.Substring(3, 3));
                    var minOrderAmount = Convert.ToInt32(vm.VoucherValue.Substring(6, 3));
                    var orderAmount = buffetItems.Sum(x => x.Price);
                    if (Convert.ToDecimal(orderAmount) < minOrderAmount)
                    {
                        vm.IsValid = false;
                        vm.Message = "Promotion valid for buffet order over £" + minOrderAmount.ToString();
                    }
                }
                else if(vm.VoucherValue.Contains("VAR"))
                {
                    promotionAmount = vm.DiscountAmount;
                    if (appliedAdCount >= adBufCount)
                    {
                        vm.IsValid = false;
                        vm.Message = "Add Adult buffet to apply this code";
                    }
                }
                else if (vm.VoucherValue.Contains("PPP"))
                {
                    string product = vm.VoucherValue.Substring(3);
                    var promProducts = drinkItems.Where(x => x.Description.Contains(product)).ToList();
                    if (promProducts == null || appliedPPPCount >= promProducts.Count || promProducts.Count == 0 )
                    {
                        vm.IsValid = false;
                        vm.Message = "Add " + product + " to apply this code";
                    }
                    else if (promProducts != null && promProducts.Count > 0)
                    {
                            promotionAmount = (decimal)(promProducts[0].Price);
                    }
                }
                else if (Convert.ToDecimal(vm.VoucherValue) > 0)
                    promotionAmount = Convert.ToDecimal(vm.VoucherValue);
                vm.DiscountAmount = promotionAmount;
            }
            else if (vm.VoucherCode.Substring(0, 2) == "CB")
            {
                if (appliedVouchers.Any(x => x.VoucherType.Contains(VoucherTypes.Birthday.ToString())))
                {
                    vm.IsValid = false;
                    vm.Message = "Only 1 birthday code per table is allowed";
                }
                else if (!orderItems.Any(x => x.ProductId == vm.ProductId))
                {
                    vm.IsValid = false;
                    vm.Message = "Add " + vm.ProductName + " to apply this code";
                }
                else if (orderItems.Any(x => x.ProductId == vm.ProductId)
                    && !orderItems.Any(x => (x.Description.Contains("Adult") || (x.EnglishName != null && x.EnglishName.Contains("Adult")))))
                {
                    vm.IsValid = false;
                    vm.Message = "Add Adult Buffet to apply this code";
                }
                else if (vm.ProductName.Contains("Adult") &&
                         orderItems.Where(x => (x.Description.Contains("Adult") || (x.EnglishName != null && x.EnglishName.Contains("Adult")))).Count() < 2)
                {
                    vm.IsValid = false;
                    vm.Message = "Add Adult Buffet to apply this code";
                }

            }

            //vm.VoucherValue = promotionAmount.ToString();

            return vm;
        }

        public ActionResult RedeemVoucherV1(VoucherModel vm)
        {
            decimal promotionAmount = vm.DiscountAmount;
            //Call API to mark voucher redeemed in DB
            int prDscountId = Convert.ToInt32(ConfigurationManager.AppSettings["PromotionDiscount"]);
            int onlineDepositId = Convert.ToInt32(ConfigurationManager.AppSettings["OnlineDepositId"]);
            int giftVoucherId = Convert.ToInt32(ConfigurationManager.AppSettings["GiftVoucherId"]);
            int restId = Convert.ToInt32(ConfigurationManager.AppSettings["RestaurantId"]);
            if (vm.VoucherCode.Substring(0, 2) == "PR" || vm.VoucherCode.Substring(0, 2) == "GV")
            {
                vm.VoucherType = VoucherTypes.Promotion.ToString();
                //Get Already applied Vouchers for this order
                var orderItems = (from a in dbContext.tblOrderParts
                                  join b in dbContext.tblProducts on a.ProductID equals b.ProductID
                                  join c in dbContext.tblProductTypes on b.ProductTypeID equals c.ProductTypeID
                                  where a.OrderGUID == vm.OrderId && a.DelInd == false
                                  select new
                                  {
                                      ProductId = a.ProductID,
                                      Description = b.Description,
                                      Price = a.Price,
                                      ProductType = c.TypeDescription
                                  }).ToList();
                if (vm.VoucherCode.Substring(0,2) == "GV")
                {
                    vm.VoucherCode.Replace("GV", "PR");
                }
                //Validations based on already applied vouchers
                if (promotionAmount != 0)
                {
                    //var dp = orderItems.Find(x => x.ProductId == prDscountId);
                    //if (dp == null)
                    //{
                        vm.IsValid = true;
                        //Calculate discount value
                        //var buffetValue = buffetItems.Sum(x => x.Price);
                        //decimal d = Convert.ToInt32(vm.VoucherValue.Substring(3)) * (decimal)0.01;
                        //vm.Amount = Math.Round((decimal)buffetValue * d, 2);
                        vm.Amount = promotionAmount;
                    //Call API to mark voucher redeemed in TCB DB
                    
                        using (var client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(url);
                            var responseTask = client.GetAsync("RedeemVoucher?voucherCode=" + vm.VoucherCode + "&amount=" + vm.Amount.ToString() + "&resId=" + restId);
                            responseTask.Wait();

                            var result = responseTask.Result;
                            if (result.IsSuccessStatusCode)
                            {
                                var readTask = result.Content.ReadAsAsync<string>();
                                readTask.Wait();
                                vm.Message = readTask.Result;

                            }
                            else //web api sent error response 
                            {
                                //log response status here..

                                var vm1 = Enumerable.Empty<VoucherModel>();

                                ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                            }
                        }
                        //Add product in ordered products

                        vm.Message = rh.AddProductToOrder(prDscountId, vm.Amount * -1, vm.OrderId);
                        //Insert record in tblVouchers
                        if (vm.Message == "")
                            vm.Message = rh.AddVoucherToOrder(vm);
                    //}
                    //else
                      //  vm.Message += "Only 1 voucher per table is valid. Similar voucher already applied to table";
                    //return message
                    if (vm.Message == "")
                        vm.Message = "Voucher applied successfully";
                    //else
                    //    vm.Message += "We could not apply voucher. Please try again later";
                }

            }
            else if (vm.VoucherCode.Substring(0, 2) == "RS")
            {
                vm.VoucherType = VoucherTypes.Reservation.ToString();
                //Calculate discount value
                //int depPrice = Convert.ToInt32(vm.VoucherValue) / vm.NoOfGuests;
                int depPrice = Convert.ToInt32(vm.VoucherValue);
                //Call API to mark voucher redeemed in TCB DB
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(url);
                    var responseTask = client.GetAsync("RedeemVoucher?voucherCode=" + vm.VoucherCode + "&amount=" + vm.Amount.ToString() + "&resId=" + restId);
                    responseTask.Wait();

                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsAsync<string>();
                        readTask.Wait();
                        vm.Message = readTask.Result;

                    }
                    else //web api sent error response 
                    {
                        //log response status here..

                        var vm1 = Enumerable.Empty<VoucherModel>();

                        ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                    }
                }
                //Add product in ordered products
                vm.Message = rh.AddProductToOrder(onlineDepositId, depPrice * -1, vm.OrderId);
                //for (int i = 0; i < vm.NoOfGuests; i++)
                //{
                //    vm.Message = rh.AddProductToOrder(onlineDepositId, depPrice * -1, vm.OrderId);
                //}
                //Update Reservation record
                var res = dbContext.tblReservations.Where(x=>vm.VoucherCode.Contains(x.UniqueCode)).FirstOrDefault();
                if (res != null)
                {
                    res.Processed = true;
                    res.ProcessedDate = DateTime.Now;
                    res.ProcessedOrderGUID = vm.OrderId;
                    res.UniqueCodeUsed = true;
                    dbContext.Entry(res).State = EntityState.Modified;
                    dbContext.SaveChanges();
                }
                else
                {
                    ObjectParameter resId = new ObjectParameter("ReservationID", typeof(int));
                    int rId = dbContext.usp_SS_AllocateOnlineReservation(vm.ForeName, vm.SurName, vm.Mobile, vm.OrderId, vm.VoucherCode, resId);
                }
                //Insert record in tblVouchers
                vm.VoucherValue = "RES";
                vm.Amount = depPrice;
                if (vm.Message == "")
                    vm.Message = rh.AddVoucherToOrder(vm);
                //return message
                if (vm.Message == "")
                    vm.Message = "Voucher applied successfully";
                else
                    vm.Message += "We could not apply voucher. Please try again later";

            }
            else if (vm.VoucherCode.Substring(0, 2) == "CB")
            {
                //vm.VoucherType = VoucherTypes.Adult.ToString();
                //vm.VoucherValue = "BDY";

                //Call API to mark voucher redeemed in TCB DB
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(url);
                    var responseTask = client.GetAsync("RedeemVoucher?voucherCode=" + vm.VoucherCode + "&amount=" + vm.Amount.ToString() + "&resId=" + restId);
                    responseTask.Wait();

                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsAsync<string>();
                        readTask.Wait();
                        vm.Message = readTask.Result;

                    }
                    else //web api sent error response 
                    {
                        //log response status here..

                        var vm1 = Enumerable.Empty<VoucherModel>();

                        ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                    }
                }
                //Add product in ordered products
                //var productId = dbContext.tblProducts.Where(x => x.Price == vm.DiscountAmount && x.DelInd == false && x.Description.Contains("Buffet")).Select(x => x.ProductID).FirstOrDefault();
                vm.Message = rh.AddProductToOrder(vm.ProductId, Convert.ToDecimal(vm.DiscountAmount) * -1, vm.OrderId);

                //Insert record in tblVouchers
                if (vm.Message == "")
                    vm.Message = rh.AddVoucherToOrder(vm);
                //return message
                if (vm.Message == "")
                    vm.Message = "Voucher applied successfully";
                else
                    vm.Message += "We could not apply voucher. Please try again later";

            }
            //else if (vm.VoucherCode.Substring(0, 2) == "GV")
            //{
            //    vm.Amount = vm.Price;
            //    //Call API to mark voucher redeemed in TCB DB
            //    using (var client = new HttpClient())
            //    {
            //        client.BaseAddress = new Uri(url);
            //        var responseTask = client.GetAsync("RedeemVoucher?voucherCode=" + vm.VoucherCode + "&amount=" + vm.Amount.ToString() + "&resid=1");
            //        responseTask.Wait();

            //        var result = responseTask.Result;
            //        if (result.IsSuccessStatusCode)
            //        {
            //            var readTask = result.Content.ReadAsAsync<string>();
            //            readTask.Wait();
            //            vm.Message = readTask.Result;

            //        }
            //        else //web api sent error response 
            //        {
            //            //log response status here..

            //            var vm1 = Enumerable.Empty<VoucherModel>();

            //            ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
            //        }
            //    }

            //    vm.Message = rh.AddProductToOrder(giftVoucherId, vm.Price * -1, vm.OrderId);
            //    if (vm.Message == "")
            //        vm.Message = "Voucher applied successfully";
            //    else
            //        vm.Message += "We could not apply voucher. Please try again later";

            //}
            return Json(vm.Message, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAllTableOrders()
        {
            List<TableOrder> tableOrders = new List<TableOrder>();
            try
            {
                using (var _context = new ChineseTillEntities1())
                {
                    var openOrders = (from a in _context.tblOrders
                                      join b in _context.tblTables on a.TableID equals b.TableID
                                      where a.DelInd == false && a.Paid == false
                                      select new
                                      {
                                          OrderGuid = a.OrderGUID,
                                          TableNumber = b.TableNumber,
                                          TableId = a.TableID
                                      }).ToList();
                    if(openOrders != null && openOrders.Count > 0)
                    {
                        foreach (var item in openOrders)
                        {
                            TableOrder to = new TableOrder();
                            to.tableDetails.TableNumber = item.TableNumber;
                            to.tableDetails.TableID = item.TableId;
                            to.OrderGUID = item.OrderGuid;
                            to.BuffetItems = (from a in dbContext.tblOrderBuffetItems
                                              join b in dbContext.tblProducts on a.ProductId equals b.ProductID
                                              where a.OrderGUID == item.OrderGuid
                                              group a by new { a.ProductId, b.Description, a.Printed, a.UserId, a.UserName, a.DateCreated.Hour, a.DateCreated.Minute } into g
                                              select new OrderBuffetItem
                                              {
                                                  //Id = random.Next(2),
                                                  ProductId = g.Key.ProductId,
                                                  Description = g.Key.Description,
                                                  UserId = g.Key.UserId,
                                                  Qty = g.Sum(x => x.Qty),
                                                  //UserType = a.UserType,
                                                  Printed = g.Key.Printed,
                                                  UserName = g.Key.UserName,
                                                  OrderTime = g.Key.Hour.ToString() + ":" + (g.Key.Minute >= 10 ? g.Key.Minute.ToString() : "0" + g.Key.Minute.ToString())
                                              }).OrderByDescending(x => x.Printed).ThenBy(x=>x.OrderTime).ToList();
                           
                            if (to.BuffetItems !=  null && to.BuffetItems.Count > 0)
                            {
                                for (int i = 0 ;i < to.BuffetItems.Count;i++)
                                {
                                    to.BuffetItems[i].Id = i + 1;
                                }
                                tableOrders.Add(to);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return Json(tableOrders, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}
