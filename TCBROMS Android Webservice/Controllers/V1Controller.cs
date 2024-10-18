using AutoMapper;
using Deznu.Products.Common.Utility;
using Entity;
using Entity.AdminDtos;
using Entity.Enums;
using Entity.Models;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.Caching;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using TCBROMS_Android_Webservice.Filters;
using TCBROMS_Android_Webservice.Helpers;
using TCBROMS_Android_Webservice.Models;
using ZXing;


namespace TCBROMS_Android_Webservice.Controllers
{
    //[AllowCrossSite]
    public class V1Controller : Controller
    {
        ROMSHelper rh = new ROMSHelper();
        ChineseTillEntities1 dbContext = new ChineseTillEntities1();
        CustomerService cs = new CustomerService();
        Logger logger = LogManager.GetLogger("databaseLogger");
        // GET: V1
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetDineInProducts(Guid orderId)
        {
            GetProductsResponse gpr = new GetProductsResponse();
            Models.ProductService ps = new Models.ProductService();
            gpr = ps.GetDineInProducts(orderId);
            return Json(gpr, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetOrderItems(Guid orderId)
        {
            ChineseTillEntities1 _context = new ChineseTillEntities1();

            TableOrder to = new TableOrder();
            to.BuffetItems = (from a in _context.tblOrderBuffetItems
                              join b in _context.tblProducts on a.ProductId equals b.ProductID
                              where a.OrderGUID == orderId
                              group a by new { a.ProductId, b.Description } into g

                              select new OrderBuffetItem
                              {
                                  Description = g.Key.Description,
                                  Qty = g.Sum(a => a.Qty)
                              }).OrderBy(b => b.Description).ToList();
            to.tableProducts = (from a in _context.tblOrderParts
                                join b in _context.tblProducts on a.ProductID equals b.ProductID
                                where a.OrderGUID == orderId && b.DelInd == false && a.DelInd == false
                                group a by new { a.ProductID, b.Description, a.Price, b.ProductTypeID } into g
                                select new Entity.Product
                                {
                                    ProductID = g.Key.ProductID,
                                    Description = g.Key.Description,
                                    Price = (float)g.Key.Price,
                                    ProductQty = (int)g.Sum(a => a.Qty),
                                    ProductTotal = ((float)g.Key.Price * (int)g.Sum(a => a.Qty)),
                                    ProductTypeId = g.Key.ProductTypeID

                                }).ToList();
            int? custCount = _context.tblOrders.Where(x => x.OrderGUID == orderId).Select(x => x.AdCount + x.JnCount + x.KdCount).FirstOrDefault();
            to.tableProducts.Add(rh.CalculateDiscount(to.tableProducts, custCount));
            return Json(to, JsonRequestBehavior.AllowGet);
        }

        public ActionResult MyDineInOrders(Guid orderId, long mobileNo = 0)
        {
            MyDineInOrderModel mdom = new MyDineInOrderModel();
            var order = dbContext.tblOrders.Where(x => x.OrderGUID == orderId).FirstOrDefault();
            List<OrderBuffetItem> drinks = new List<OrderBuffetItem>();

            //if (dbContext.tblOrderBuffetItems.Any(x => x.OrderGUID == orderId) && !order.PayAsYouGo)
            //{
            //    mdom.TableOrder = (from a in dbContext.tblOrderBuffetItems
            //                       join b in dbContext.tblProducts on a.ProductId equals b.ProductID
            //                       where a.OrderGUID == orderId
            //                       group a by new { a.ProductId, b.Description, a.Printed, a.UserId, a.UserName, a.DateCreated.Hour, a.DateCreated.Minute, PrintedHour = a.DatePrinted.Value.Hour, PrintedMinute = a.DatePrinted.Value.Minute,a.Delivered } into g
            //                       select new OrderBuffetItem
            //                       {
            //                           ProductId = g.Key.ProductId,
            //                           Description = g.Key.Description,
            //                           UserId = g.Key.UserId,
            //                           Qty = g.Sum(x => x.Qty),
            //                           //UserType = a.UserType,
            //                           Printed = g.Key.Printed,
            //                           UserName = g.Key.UserName,
            //                           Delivered = g.Key.Delivered,
            //                           OrderTime = g.Key.Printed == false ? (g.Key.Hour.ToString() + ":" + (g.Key.Minute >= 10 ? g.Key.Minute.ToString() : "0" + g.Key.Minute.ToString())) : (g.Key.PrintedHour.ToString() + ":" + (g.Key.PrintedMinute >= 10 ? g.Key.PrintedMinute.ToString() : "0" + g.Key.PrintedMinute.ToString()))
            //                       }).OrderBy(x => x.Printed).ThenBy(x => x.Delivered).ThenBy(x => x.OrderTime).ToList();
            //    //}).OrderByDescending(x => x.Delivered).OrderByDescending(x => x.Printed).ThenBy(x => x.OrderTime).ToList();
            //        drinks = (from a in dbContext.tblOrderParts
            //                      join b in dbContext.tblProducts on a.ProductID equals b.ProductID
            //                      where a.OrderGUID == orderId && a.Mobile != null && !b.Description.Contains("buffet") && !b.Description.Contains("Meal")
            //                      group a by new { a.ProductID, b.Description, a.Mobile, a.UserName, a.DateCreated.Value.Hour, a.DateCreated.Value.Minute } into g
            //                      select new OrderBuffetItem
            //                      {
            //                          ProductId = g.Key.ProductID,
            //                          Description = g.Key.Description,
            //                          Mobile = g.Key.Mobile,
            //                          UserName = g.Key.UserName,
            //                          Qty = (int)g.Sum(x => x.Qty),
            //                          OrderTime = (g.Key.Hour.ToString() + ":" + (g.Key.Minute >= 10 ? g.Key.Minute.ToString() : "0" + g.Key.Minute.ToString())),
            //                          //UserType = a.UserType,
            //                          Delivered = true,
            //                          Printed = true

            //                      }).ToList();

            //}
            //else if (order.PayAsYouGo)
            //{
            //    drinks = (from a in dbContext.tblOrderParts
            //              join b in dbContext.tblProducts on a.ProductID equals b.ProductID
            //              where a.OrderGUID == orderId && !b.Description.Contains("buffet") && !b.Description.Contains("Meal")
            //              group a by new { a.ProductID, b.Description, a.Mobile, a.UserName, a.DateCreated.Value.Hour, a.DateCreated.Value.Minute } into g
            //              select new OrderBuffetItem
            //              {
            //                  ProductId = g.Key.ProductID,
            //                  Description = g.Key.Description,
            //                  Mobile = g.Key.Mobile,
            //                  UserName = g.Key.UserName,
            //                  Qty = (int)g.Sum(x => x.Qty),
            //                  OrderTime = (g.Key.Hour.ToString() + ":" + (g.Key.Minute >= 10 ? g.Key.Minute.ToString() : "0" + g.Key.Minute.ToString())),
            //                  //UserType = a.UserType,
            //                  Delivered = true,
            //                  Printed = true

            //              }).ToList();
            //}



            if (dbContext.tblPrintBuffetItems.Any(x => x.OrderGUID == orderId) && !order.PayAsYouGo)
            {
                mdom.TableOrder = (from a in dbContext.tblPrintBuffetItems
                                   join b in dbContext.tblProducts on (int) a.ProductId equals b.ProductID
                                   where a.OrderGUID == orderId
                                   group a by new { a.ProductId, b.Description, a.Printed, a.UserId, a.UserName, a.DateOrdered.Hour, a.DateOrdered.Minute, PrintedHour = a.DatePrinted.Value.Hour, PrintedMinute = a.DatePrinted.Value.Minute, a.Delivered } into g
                                   select new OrderBuffetItem
                                   {
                                       ProductId = g.Key.ProductId,
                                       Description = g.Key.Description,
                                       UserId = g.Key.UserId,
                                       Qty = g.Sum(x => x.Qty),
                                       //UserType = a.UserType,
                                       Printed = g.Key.Printed,
                                       UserName = g.Key.UserName,
                                       Delivered = g.Key.Delivered,
                                       OrderTime = g.Key.Printed == false ? (g.Key.Hour.ToString() + ":" + (g.Key.Minute >= 10 ? g.Key.Minute.ToString() : "0" + g.Key.Minute.ToString())) : (g.Key.PrintedHour.ToString() + ":" + (g.Key.PrintedMinute >= 10 ? g.Key.PrintedMinute.ToString() : "0" + g.Key.PrintedMinute.ToString()))
                                   }).OrderBy(x => x.Printed).ThenBy(x => x.Delivered).ThenBy(x => x.OrderTime).ToList();
                //}).OrderByDescending(x => x.Delivered).OrderByDescending(x => x.Printed).ThenBy(x => x.OrderTime).ToList();
                drinks = (from a in dbContext.tblOrderParts
                          join b in dbContext.tblProducts on a.ProductID equals b.ProductID
                          where a.OrderGUID == orderId && a.Mobile != null && !b.Description.Contains("buffet") && !b.Description.Contains("Meal")
                          group a by new { a.ProductID, b.Description, a.Mobile, a.UserName, a.DateCreated.Value.Hour, a.DateCreated.Value.Minute } into g
                          select new OrderBuffetItem
                          {
                              ProductId = g.Key.ProductID,
                              Description = g.Key.Description,
                              Mobile = g.Key.Mobile,
                              UserName = g.Key.UserName,
                              Qty = (int)g.Sum(x => x.Qty),
                              OrderTime = (g.Key.Hour.ToString() + ":" + (g.Key.Minute >= 10 ? g.Key.Minute.ToString() : "0" + g.Key.Minute.ToString())),
                              //UserType = a.UserType,
                              Delivered = true,
                              Printed = true

                          }).ToList();

            }
            else if (order.PayAsYouGo)
            {
                drinks = (from a in dbContext.tblOrderParts
                          join b in dbContext.tblProducts on a.ProductID equals b.ProductID
                          where a.OrderGUID == orderId && !b.Description.Contains("buffet") && !b.Description.Contains("Meal")
                          group a by new { a.ProductID, b.Description, a.Mobile, a.UserName, a.DateCreated.Value.Hour, a.DateCreated.Value.Minute } into g
                          select new OrderBuffetItem
                          {
                              ProductId = g.Key.ProductID,
                              Description = g.Key.Description,
                              Mobile = g.Key.Mobile,
                              UserName = g.Key.UserName,
                              Qty = (int)g.Sum(x => x.Qty),
                              OrderTime = (g.Key.Hour.ToString() + ":" + (g.Key.Minute >= 10 ? g.Key.Minute.ToString() : "0" + g.Key.Minute.ToString())),
                              //UserType = a.UserType,
                              Delivered = true,
                              Printed = true

                          }).ToList();
            }




            if (drinks != null && drinks.Count > 0)
            {
                foreach (var item in drinks)
                {
                    if(item.Mobile != null)
                    item.UserId = long.Parse(item.Mobile);
                }
                mdom.TableOrder.AddRange(drinks);
            }
            mdom.MyOrder = mdom.TableOrder.Where(x => x.UserId == mobileNo).ToList();
            //foreach (var item in items)
            //{
            //    var m = mdom.TableOrder.Find(x => x.ProductId == item.ProductId && x.Printed == item.Printed);
            //    if (m != null)
            //        m.Qty += item.Qty;
            //    else
            //    {
            //        OrderBuffetItem opi = new OrderBuffetItem();
            //        opi.ProductId = item.ProductId;
            //        opi.Description = item.Description;
            //        opi.Qty = item.Qty;
            //        opi.Printed = item.Printed;
            //        mdom.TableOrder.Add(opi);
            //    }
            //    if (item.UserId != null && item.UserId == mobileNo)
            //    {


            //        var m1 = mdom.MyOrder.Find(x => x.ProductId == item.ProductId);
            //        if (m1 != null)
            //            m1.Qty += item.Qty;
            //        else
            //        {
            //            OrderBuffetItem opi = new OrderBuffetItem();
            //            opi.ProductId = item.ProductId;
            //            opi.Description = item.Description;
            //            opi.Qty = item.Qty;
            //            opi.Printed = item.Printed;
            //            mdom.MyOrder.Add(opi);
            //        }
            //    }
            //}
            return Json(mdom, JsonRequestBehavior.AllowGet);
        }

        public ActionResult TillCustomerRegistration(Customer cust)
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
            cust.RestaurantId = resId;
            cust = cs.CustomerRegistration(cust);
            return Json(cust, JsonRequestBehavior.AllowGet);
        }

        public ActionResult VerifyRegistrationOTP(string otp, string mobile)
        {
            Customer cust = cs.VerifyRegistrationOTP(otp, mobile);
            return Json(cust, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ServiceChargeApplicable(Guid orderId, bool serviceChargeApplicable)
        {
            string response = "";
            try
            {
                var order = dbContext.tblOrders.Where(x => x.OrderGUID == orderId).FirstOrDefault();
                order.ServiceChargeApplicable = serviceChargeApplicable;
                dbContext.Entry(order).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();
                if (serviceChargeApplicable)
                    response = "Service Charge applied to this order";
                else
                    response = "Service Charge removed for this order";
            }
            catch (Exception ex)
            {
                response = ex.Message;
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

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
                          OrderGUID = b.OrderGUID,
                          ServiceRequired = b.ServiceRequired ?? false,
                          PayAsYouGo = b.PayAsYouGo,
                          HideDrinkMenu = b.HideDrinkMenu ?? false
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
                    to.RestaurantId = Convert.ToInt32(ConfigurationManager.AppSettings["RestaurantId"]);
                    to.RestaurantName = ConfigurationManager.AppSettings["RestaurantName"].ToString();
                    if(!to.PayAsYouGo || (to.PayAsYouGo && _context.tblOrderParts.Any(x=>x.OrderGUID == to.OrderGUID)))
                    to.CurrentTotal = (float)_context.tblOrderParts.Where(x => x.OrderGUID == to.OrderGUID).Sum(x => x.Price);
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

        public ActionResult GetMenus()
        {
            List<int> stationIds = new List<int>();
            stationIds.Add(1); //main course
            stationIds.Add(11); //dessert
            stationIds.Add(3); //starter
            ChineseTillEntities1 _context = new ChineseTillEntities1();
            var menus = (from a in _context.tblMenus
                         where a.DelInd == false && stationIds.Contains(a.MenuID)
                         select new Menu
                         {
                             MenuID = a.MenuID,
                             MenuName = a.MenuName
                         }).ToList();
            menus.Add(new Menu
            {
                MenuID = 100,
                MenuName = "Priority"
            });
            menus.Add(new Menu
            {
                MenuID = 200,
                MenuName = "SupportStation"
            });
            return Json(menus, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetOrderedItemsbyMenu(string MenuId)
        {
            List<int> menuIds = MenuId.Split(',').Select(int.Parse).ToList();
            //List<int> menuIds = new List<int>();
            //menuIds.Add(3);
            //menuIds.Add(5);
            ChineseTillEntities1 _context = new ChineseTillEntities1();
            List<OrderMenuItemsResponse> omr = new List<OrderMenuItemsResponse>();
            //fetch records batch wise for kitchen display
            //var orderedItems = (from a in _context.tblMenuItems
            //                    join b in _context.tblOrderBuffetItems on a.ProductID equals b.ProductId
            //                    join c in _context.tblProducts on a.ProductID equals c.ProductID
            //                    //where  a.MenuID == menuId &&
            //                    where menuIds.Contains(a.MenuID) &&
            //                     DbFunctions.TruncateTime(b.DateCreated) == DbFunctions.TruncateTime(DateTime.Now)
            //                    group b by new { b.ProductId, b.BatchNo, c.Description, c.ChineseName, b.BatchTime, b.Processed, a.MenuID } into g
            //                    select new OrderPart
            //                    {
            //                        Name = g.Key.Description,
            //                        Qty = (short)g.Count(),
            //                        BatchNo = (int)g.Key.BatchNo,
            //                        BatchTime = g.Key.BatchTime,
            //                        Processed = g.Key.Processed,
            //                        MenuId = g.Key.MenuID,
            //                        ChineseName = g.Key.ChineseName
            //                    }).ToList();
            var orderedItems = (from a in _context.tblMenuItems
                                join b in _context.tblPrintBuffetItems on a.ProductID equals (int) b.ProductId
                                join c in _context.tblProducts on a.ProductID equals c.ProductID
                                //where  a.MenuID == menuId &&
                                where menuIds.Contains(a.MenuID) &&
                                 DbFunctions.TruncateTime(b.DateOrdered) == DbFunctions.TruncateTime(DateTime.Now)
                                group b by new { b.ProductId, b.BatchNo, c.Description, c.ChineseName, b.BatchTime, b.Processed, a.MenuID } into g
                                select new OrderPart
                                {
                                    Name = g.Key.Description,
                                    Qty = (short)g.Count(),
                                    BatchNo = (int)g.Key.BatchNo,
                                    BatchTime = g.Key.BatchTime,
                                    Processed = g.Key.Processed,
                                    MenuId = g.Key.MenuID,
                                    ChineseName = g.Key.ChineseName
                                }).ToList();
            if (orderedItems != null && orderedItems.Count > 0)
            {
                int batchNo = 1;
                var olItems = orderedItems.Where(x => x.BatchNo > 0).ToList();
                if (olItems.Count > 0)
                {
                    //olItems = olItems.OrderByDescending(x => x.BatchNo).ToList();
                    var uniqueBatchNo = olItems.OrderByDescending(x => x.BatchNo).Select(x => x.BatchNo).Distinct();
                    foreach (var bn in uniqueBatchNo)
                    {
                        var pl = olItems.Where(x => x.BatchNo == bn).ToList();
                        OrderMenuItemsResponse omir = new OrderMenuItemsResponse();
                        omir.BatchNo = (int)bn;
                        //DateTime ot = pl.OrderBy(x => x.DateCreated).Select(x => x.DateCreated).FirstOrDefault();
                        omir.OrderTime = pl.Select(x => x.BatchTime).FirstOrDefault();
                        omir.Processed = pl.Select(x => x.Processed).FirstOrDefault();
                        var uniqueMenuId = olItems.Select(x => x.MenuId).Distinct();
                        foreach (var m in uniqueMenuId)
                        {
                            omir.MenuItems.Add(pl.Where(x => x.MenuId == m).ToList());
                        }
                        //omir.MenuItems = pl;
                        omr.Add(omir);
                    }
                    batchNo = batchNo + uniqueBatchNo.FirstOrDefault();
                }

                var nwItems = orderedItems.Where(x => x.BatchNo == 0).ToList();
                if (nwItems.Count > 0)
                //get any new items inserted for this cycle

                {
                    //get any new items inserted for this cycle
                    //var ni = (from t1 in _context.tblOrderBuffetItems
                    //              //from t2 in _context.tblMenuItems.Where(o => t1.ProductId == o.ProductID && o.MenuID == menuId
                    //          from t2 in _context.tblMenuItems.Where(o => t1.ProductId == o.ProductID && menuIds.Contains(o.MenuID)
                    //          && t1.BatchNo == 0 &&
                    //                                        //from t2 in _context.tblMenuItems.Where(o => t1.ProductId == o.ProductID && t1.BatchNo == 0 &&
                    //                                        DbFunctions.TruncateTime(t1.DateCreated) == DbFunctions.TruncateTime(DateTime.Now))
                    //          select new
                    //          {
                    //              t1,
                    //              t2.MenuID
                    //          })
                    //                .ToList();


                    var ni = (from t1 in _context.tblPrintBuffetItems
                                  //from t2 in _context.tblMenuItems.Where(o => t1.ProductId == o.ProductID && o.MenuID == menuId
                              from t2 in _context.tblMenuItems.Where(o => t1.ProductId == o.ProductID && menuIds.Contains(o.MenuID)
                              && t1.BatchNo == 0 &&
                                                            //from t2 in _context.tblMenuItems.Where(o => t1.ProductId == o.ProductID && t1.BatchNo == 0 &&
                                                            DbFunctions.TruncateTime(t1.DateCreated) == DbFunctions.TruncateTime(DateTime.Now))
                              select new
                              {
                                  t1,
                                  t2.MenuID
                              })
                                  .ToList();

                    //update batch no for new items 
                    string batchTime = DateTime.Now.ToString("HH:mm");
                    if (ni != null && ni.Count > 0)
                    {
                        //ni = ni.Where(x => menuIds.Contains(x.MenuID)).ToList();
                        foreach (var item in ni)
                        {
                            item.t1.BatchNo = batchNo;
                            item.t1.BatchTime = batchTime;
                            item.t1.Processed = false;
                        }
                        _context.SaveChanges();
                    }

                    nwItems.ForEach(x => x.BatchNo = batchNo);
                    OrderMenuItemsResponse omir = new OrderMenuItemsResponse();
                    omir.BatchNo = batchNo;
                    //DateTime ot = pl.OrderBy(x => x.DateCreated).Select(x => x.DateCreated).FirstOrDefault();
                    omir.OrderTime = batchTime;
                    //omir.MenuItems = nwItems;
                    omir.Processed = false;
                    var uniqueMenuId = nwItems.Select(x => x.MenuId).Distinct();
                    foreach (var m in uniqueMenuId)
                    {
                        omir.MenuItems.Add(nwItems.Where(x => x.MenuId == m).ToList());
                    }
                    omr.Add(omir);
                }
            }
            if (omr.Count > 0)
                omr = omr.OrderByDescending(x => x.BatchNo).ToList();

            return Json(omr, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ProcessBatchOrder(int batchNo)
        {
            ChineseTillEntities1 _context = new ChineseTillEntities1();
            //var ni = (from t1 in _context.tblOrderBuffetItems
            //          where t1.BatchNo == batchNo
            //          && DbFunctions.TruncateTime(t1.DateCreated) == DbFunctions.TruncateTime(DateTime.Now)
            //          select t1).ToList();
            var ni = (from t1 in _context.tblPrintBuffetItems
                      where t1.BatchNo == batchNo
                      && DbFunctions.TruncateTime(t1.DateCreated) == DbFunctions.TruncateTime(DateTime.Now)
                      select t1).ToList();

            //update batch no for new items 
            if (ni != null && ni.Count > 0)
            {
                foreach (var item in ni)
                {
                    item.Processed = !item.Processed;
                }
                _context.SaveChanges();
            }

            return Json("success", JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveReceivedQuantity(SupplierOrder supplierOrder)
        {
            string response = "success";
            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    using (var _context = new ChineseTillEntities1())
                    {
                        foreach (var item in supplierOrder.SupplierOrderItems)
                        {
                            var i = _context.tblSupplierOrderItems.Where(x => x.SupplierOrderItemId == item.SupplierOrderItemId).FirstOrDefault();
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
                            _context.tblSupplierOrderItems.Attach(i);
                            _context.Entry(i).State = EntityState.Modified;
                            _context.SaveChanges();
                        }
                        var so = _context.tblSupplierOrders.Where(x => x.PurchaseOrderNumber == supplierOrder.PurchaseOrderNumber && x.DelInd == false).FirstOrDefault();
                        so.Notes = supplierOrder.Notes;
                        so.UserId = (int)_context.tblUsers.Where(x => x.UserCode == supplierOrder.UserId).Select(x => x.UserID).FirstOrDefault();
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
                        so.StaffUserCode = supplierOrder.StaffUserCode;
                        so.ManagerUserCode = supplierOrder.ManagerUserCode;
                        so.SupplierTotalAmount = supplierOrder.SupplierTotalAmount;
                        _context.tblSupplierOrders.Attach(so);
                        _context.Entry(so).State = EntityState.Modified;
                        _context.SaveChanges();
                        if (supplierOrder.Images != null)
                        {
                            foreach (var image in supplierOrder.Images)
                            {
                                tblSupplierOrderImage tsi = new tblSupplierOrderImage();
                                tsi.SupplierOrderid = supplierOrder.SupplierOrderId;
                                tsi.PurchaseOrderNumberFK = supplierOrder.PurchaseOrderNumber;
                                tsi.Image = image;
                                tsi.DelInd = false;
                                _context.tblSupplierOrderImages.Add(tsi);
                                _context.SaveChanges();
                            }
                        }
                    }
                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                if (ex.InnerException != null)
                    error = ex.InnerException.Message;
                response = error;
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateCustomerActivity(tblCustomerActivity tca)
        {
            tca.ActivityType = ActivityType.DineIn.ToString();
            string response = cs.UpdateCustomerActivity(tca);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateProduct(ProductDto product)
        {
            string response = "success";
            try
            {
                var config = new MapperConfiguration(cfg =>
                 {
                     cfg.CreateMap<tblProduct, ProductDto>();
                 });
                IMapper iMapper = config.CreateMapper();
                //tblProduct tp = new tblProduct();

                using (var _context = new ChineseTillEntities1())
                {
                    var tp = _context.tblProducts.Where(x => x.ProductID == product.ProductID).FirstOrDefault();

                    if (tp == null)
                    {
                        tp = iMapper.Map<tblProduct>(product);
                        tp.ProductCode = Convert.ToInt32(product.vProductCode);
                        tp.bOnsite = false;
                        tp.Available = true;
                        tp.DateCreated = DateTime.Now;
                        tp.Price = (decimal?)product.Price;
                        tp.ChineseName = product.vProductChineseName;
                        tp.EnglishName = product.vProductEnglishName;
                        tp.LastModified = DateTime.Now;
                        //tp.DelInd = product.DelInd;
                        //tp.Backcolour = product.Backcolour;
                        //tp.bGlutenFree = product.bGlutenFree;
                        //tp.bSpicy = product.bSpicy;
                        //tp.bVegetarain = product.bVegetarain;
                        //tp.Calories = product.Calories;
                        //tp.CostPrice = product.CostPrice;
                        //tp.CustomisePrice = product.CustomisePrice;
                        //tp.Description = product.Description;
                        //tp.FoodRefil = product.FoodRefil;
                        //tp.Forecolour = product.Forecolour;
                        //tp.FridgeQty = product.FridgeQty;
                        //tp.ImageName = product.ImageName;
                        //tp.IsFridgeItem = product.IsFridgeItem;
                        //tp.IsTakeaway = product.IsTakeaway;
                        //tp.ProductGroupID = product.ProductGroupID;
                        //tp.ProductTypeID = product.ProductTypeID;
                        //tp.RedeemEndDate = product.RedeemEndDate;
                        //tp.RedeemStartDate = product.RedeemStartDate;
                        //tp.RedeemValidDays = product.RedeemValidDays;
                        //tp.RedemptionPoints = product.RedemptionPoints;
                        //tp.RewardEndDate = product.RewardEndDate;
                        //tp.RewardPoints = product.RewardPoint;
                        //tp.RewardStartDate = product.RewardStartDate;
                        //tp.vAllergens = product.vAllergens;
                        //tp.VAT = product.Vat;
                        //tp.vMenuDescription = product.vMenuDescription;
                        //tp.SortOrder = product.SortOrder;
                        _context.tblProducts.Add(tp);
                        //_context.Database.ExecuteSqlCommand("SET IDENTITY_INSERT dbo.tblProduct ON");
                        _context.SaveChanges();
                        //_context.Database.ExecuteSqlCommand("SET IDENTITY_INSERT dbo.tblProduct OFF");

                    }
                    else
                    {
                        tp.Backcolour = product.Backcolour;
                        tp.bGlutenFree = product.bGlutenFree;
                        tp.bSpicy = product.bSpicy;
                        tp.bVegetarain = product.bVegetarain;
                        tp.Calories = product.Calories;
                        tp.CostPrice = product.CostPrice;
                        tp.CustomisePrice = product.CustomisePrice;
                        tp.DelInd = product.DelInd;
                        tp.Description = product.Description;
                        tp.FoodRefil = product.FoodRefil;
                        tp.Forecolour = product.Forecolour;
                        tp.ImageName = product.ImageName;
                        tp.IsTakeaway = product.IsTakeaway;
                        tp.ProductGroupID = product.ProductGroupID;
                        tp.ProductTypeID = product.ProductTypeID;
                        tp.RedeemEndDate = product.RedeemEndDate;
                        tp.RedeemStartDate = product.RedeemStartDate;
                        tp.RedeemValidDays = product.RedeemValidDays;
                        tp.RedemptionPoints = product.RedemptionPoints;
                        tp.RewardEndDate = product.RewardEndDate;
                        tp.RewardPoints = product.RewardPoints;
                        tp.RewardStartDate = product.RewardStartDate;
                        tp.SortOrder = product.SortOrder;
                        tp.vAllergens = product.vAllergens;
                        tp.VAT = product.Vat;
                        tp.vMenuDescription = product.vMenuDescription;
                        tp.ProductCode = Convert.ToInt32(product.vProductCode);
                        tp.bOnsite = false;
                        tp.Available = true;
                        tp.Price = (decimal?)product.Price;
                        tp.ChineseName = product.vProductChineseName;
                        tp.EnglishName = product.vProductEnglishName;
                        tp.LastModified = DateTime.Now;
                        _context.Entry(tp).State = EntityState.Modified;
                        _context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message;
                if (ex.InnerException != null)
                    response = ex.InnerException.Message;
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateProductGroup(ProductGroup productGroup)
        {
            string response = "success";
            try
            {
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<tblProductGroup, ProductGroup>();
                });
                IMapper iMapper = config.CreateMapper();
                tblProductGroup tpg = new tblProductGroup();
                using (var _context = new ChineseTillEntities1())
                {
                    tpg = _context.tblProductGroups.Where(x => x.ProductGroupID == productGroup.ProductGroupID).FirstOrDefault();
                    if (tpg == null)
                    {
                        tpg = iMapper.Map<tblProductGroup>(productGroup);
                        tpg.DelInd = false;
                        tpg.DateCreated = DateTime.Now;
                        tpg.bOnsite = true;
                        _context.tblProductGroups.Add(tpg);
                        _context.SaveChanges();
                    }
                    else
                    {
                        tpg.Groupname = productGroup.Groupname;
                        tpg.ImageName = productGroup.ImageName;
                        tpg.ParentGroupID = productGroup.ParentGroupID;
                        tpg.SortOrder = productGroup.SortOrder;
                        tpg.DelInd = productGroup.DelInd;
                        tpg.bOnsite = true;
                        _context.Entry(tpg).State = EntityState.Modified;
                        _context.SaveChanges();
                    }

                }
            }
            catch (Exception ex)
            {
                response = ex.Message;
                if (ex.InnerException != null)
                    response = ex.InnerException.Message;
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateMenu(MenuDto menu)
        {
            string response = "success";
            try
            {
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<tblMenu, MenuDto>();
                });
                IMapper iMapper = config.CreateMapper();
                tblMenu tm = new tblMenu();
                using (var _context = new ChineseTillEntities1())
                {
                    tm = _context.tblMenus.Where(x => x.MenuID == menu.MenuID).FirstOrDefault();
                    if (tm == null)
                    {
                        tm = iMapper.Map<tblMenu>(menu);
                        tm.DelInd = false;
                        tm.DateCreated = DateTime.Now;
                        _context.tblMenus.Add(tm);
                        _context.SaveChanges();
                    }
                    else
                    {
                        //tm = iMapper.Map<tblMenu>(menu);
                        tm.MenuName = menu.MenuName;
                        tm.MenuDescription = menu.MenuDescription;
                        tm.DelInd = menu.DelInd;
                        tm.ImageName = menu.ImageName;
                        tm.SortOrder = menu.SortOrder;
                        //tm.ParentMenuId = menu.ParentMenuId;
                        tm.FixedProducts = menu.FixedProducts;
                        _context.Entry(tm).State = EntityState.Modified;
                        _context.SaveChanges();
                    }

                }
            }
            catch (Exception ex)
            {
                response = ex.Message;
                if (ex.InnerException != null)
                    response = ex.InnerException.Message;
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult UpdateMenuItem(MenuItemDto menuItem)
        {
            string response = "success";
            try
            {
                tblMenuItem tm = new tblMenuItem();
                List<string> menuIds = new List<string>();
                if(menuItem.MultiplemenuId != null && menuItem.MultiplemenuId != "")
                    menuIds = menuItem.MultiplemenuId.Split(',').ToList();
                using (var _context = new ChineseTillEntities1())
                {
                    tm = _context.tblMenuItems.Where(x => x.MenuID == menuItem.MenuID && x.ProductID == menuItem.ProductID && x.DelInd == false).FirstOrDefault();
                    //var product = _context.tblMenuItems.Where(x => x.ProductID == menuItem.ProductID).ToList();
                    //if (product != null && product.Count > 0)
                    //{
                    //    product.ForEach(a => a.DelInd = true);
                    //    _context.SaveChanges();
                    //}
                    //if(menuIds != null && menuIds.Count > 0)
                    //{
                    //    foreach (var item in menuIds)
                    //    {
                    //        tm = new tblMenuItem();
                    //        tm.DelInd = false;
                    //        tm.LastModified = DateTime.Now;
                    //        tm.MenuID = Int32.Parse(item);
                    //        tm.ProductID = menuItem.ProductID;
                    //        tm.Active = true;
                    //        tm.bOnsite = false;
                    //        tm.DirectPrint = false;
                    //        tm.Priority = false;
                    //        _context.tblMenuItems.Add(tm);
                    //        _context.SaveChanges();
                    //    }
                    //}

                    if (tm == null)
                    {
                        tm = new tblMenuItem();
                        tm.DelInd = false;
                        tm.LastModified = DateTime.Now;
                        tm.MenuID = menuItem.MenuID;
                        tm.ProductID = menuItem.ProductID;
                        tm.Active = true;
                        tm.bOnsite = false;
                        tm.DirectPrint = false;
                        tm.Priority = false;
                        _context.tblMenuItems.Add(tm);
                        _context.SaveChanges();
                    }
                    else
                    {
                        tm.MenuID = menuItem.MenuID;
                        tm.ProductID = menuItem.ProductID;
                        tm.DelInd = menuItem.DelInd;
                        _context.Entry(tm).State = EntityState.Modified;
                        _context.SaveChanges();
                    }

                }
            }
            catch (Exception ex)
            {
                response = ex.Message;
                if (ex.InnerException != null)
                    response = ex.InnerException.Message;
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult UpdateMenuItemV1(MenuItemDto menuItem)
        {
            string response = "success";
            try
            {
                tblMenuItem tm = new tblMenuItem();
                List<string> menuIds = new List<string>();
                if (menuItem.MultiplemenuId != null && menuItem.MultiplemenuId != "")
                    menuIds = menuItem.MultiplemenuId.Split(',').ToList();
                using (var _context = new ChineseTillEntities1())
                {
                    //tm = _context.tblMenuItems.Where(x => x.MenuID == menuItem.MenuID && x.ProductID == menuItem.ProductID).FirstOrDefault();
                    var product = _context.tblMenuItems.Where(x => x.ProductID == menuItem.ProductID).ToList();
                    if (product != null && product.Count > 0)
                    {
                        product.ForEach(a => a.DelInd = true);
                        _context.SaveChanges();
                    }
                    if (menuIds != null && menuIds.Count > 0)
                    {
                        foreach (var item in menuIds)
                        {
                            tm = new tblMenuItem();
                            tm.DelInd = false;
                            tm.LastModified = DateTime.Now;
                            tm.MenuID = Int32.Parse(item);
                            tm.ProductID = menuItem.ProductID;
                            tm.Active = true;
                            tm.bOnsite = false;
                            tm.DirectPrint = false;
                            tm.Priority = false;
                            _context.tblMenuItems.Add(tm);
                            _context.SaveChanges();
                        }
                    }

                    //if (tm == null)
                    //{
                    //    tm = new tblMenuItem();
                    //    tm.DelInd = false;
                    //    tm.LastModified = DateTime.Now;
                    //    tm.MenuID = menuItem.MenuID;
                    //    tm.ProductID = menuItem.ProductID;
                    //    tm.Active = true;
                    //    tm.bOnsite = false;
                    //    tm.DirectPrint = false;
                    //    tm.Priority = false;
                    //    _context.tblMenuItems.Add(tm);
                    //    _context.SaveChanges();
                    //}
                    //else
                    //{
                    //    tm.MenuID = menuItem.MenuID;
                    //    tm.ProductID = menuItem.ProductID;
                    //    tm.DelInd = menuItem.DelInd;
                    //    _context.Entry(tm).State = EntityState.Modified;
                    //    _context.SaveChanges();
                    //}

                }
            }
            catch (Exception ex)
            {
                response = ex.Message;
                if (ex.InnerException != null)
                    response = ex.InnerException.Message;
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPrintingCounts(string fromDate, string toDate)
        {
            PrintingCountModel pcm = new PrintingCountModel();
            DateTime fDate = DateTime.Parse(fromDate);
            DateTime tDate = DateTime.Parse(toDate);
            List<tblPrintQueue> printQueues = new List<tblPrintQueue>();
            //List<OrderBuffetItem> buffetItems = new List<OrderBuffetItem>();
            using (var _context = new ChineseTillEntities1())
            {
                printQueues = _context.tblPrintQueues.Where(x => x.DatePrinted != null && (x.DatePrinted >= fDate && x.DatePrinted <= tDate)).ToList();
                //var buffetItems = (from a in _context.tblOrderBuffetItems
                //                   join b in _context.tblMenuItems on a.ProductId equals b.ProductID
                //                   where (a.DatePrinted != null && (a.DatePrinted >= fDate && a.DatePrinted <= tDate))
                //                   group a by new { b.MenuID } into g
                //                   select new
                //                   {
                //                       MenuId = g.Key.MenuID,
                //                       Count = g.Sum(a => a.Qty)
                //                   }).ToList();
                var buffetItems = (from a in _context.tblPrintBuffetItems
                                   join b in _context.tblMenuItems on (int)a.ProductId equals b.ProductID
                                   where (a.DatePrinted != null && (a.DatePrinted >= fDate && a.DatePrinted <= tDate))
                                   group a by new { b.MenuID } into g
                                   select new
                                   {
                                       MenuId = g.Key.MenuID,
                                       Count = g.Sum(a => a.Qty)
                                   }).ToList();
                pcm.KitchenCount = printQueues.Where(x => x.ToPrinter == "Kitchen").Count();
                pcm.Kitchen2Count = printQueues.Where(x => x.ToPrinter == "Kitchen2").Count();
                pcm.StartersCount = buffetItems.Where(x => x.MenuId == 3).Select(x => x.Count).FirstOrDefault();
                pcm.DesertsCount = buffetItems.Where(x => x.MenuId == 11).Select(x => x.Count).FirstOrDefault();
                pcm.MainCourse = buffetItems.Where(x => x.MenuId != 3 && x.MenuId != 11).Select(x => x.Count).FirstOrDefault();
            }
            return Json(pcm, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetMenuItems()
        {
            MenuItemsModel mim = new MenuItemsModel();
            mim = GetItems();
            return Json(mim, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateMenuItemAvailability(int menuItemId, bool available, int userId)
        {
            MenuItemsModel mim = new MenuItemsModel();
            using (var _context = new ChineseTillEntities1())
            {
                var menuItem = _context.tblMenuItems.Where(x => x.MenuItemID == menuItemId).FirstOrDefault();
                if (available)
                {
                    menuItem.Active = true;
                    menuItem.AavilabilityChangedBy = userId;
                }
                else
                {
                    menuItem.Active = false;
                    menuItem.LastAvailable = DateTime.Now;
                    menuItem.AavilabilityChangedBy = userId;
                }
                menuItem.LastModified = DateTime.Now;
                _context.Entry(menuItem).State = EntityState.Modified;
                _context.SaveChanges();
                MemoryCache cache = MemoryCache.Default;
                List<string> cacheKeys = cache.Select(kvp => kvp.Key).ToList();
                if (cacheKeys != null && cacheKeys.Count > 0)
                {
                    foreach (string cacheKey in cacheKeys)
                    {
                        if (cacheKey == "ProductsList" || cacheKey == "DineInProducts")
                        {
                            cache.Remove(cacheKey);
                        }
                    }
                }
            }
            mim = GetItems();
            return Json(mim, JsonRequestBehavior.AllowGet);
        }

        public MenuItemsModel GetItems()
        {
            MenuItemsModel mim = new MenuItemsModel();
            using (var _context = new ChineseTillEntities1())
            {
                var menuItems = (from a in _context.tblMenuItems
                                 join b in _context.tblProducts on a.ProductID equals b.ProductID
                                 where a.DelInd == false 
                                 select new MenuItem
                                 {
                                     MenuItemID = a.MenuItemID,
                                     Description = b.Description,
                                     ChineseName = b.ChineseName,
                                     Active = a.Active,
                                     bOnsite = a.bOnsite,
                                     LastAvailable = a.LastAvailable,
                                 }).ToList();
                mim.AvailableMenuItems = menuItems.Where(x => x.Active == true).OrderBy(x => x.Description).ToList();
                mim.UnAvailableMenuItems = menuItems.Where(x => x.Active == false).OrderBy(x => x.LastAvailable == null).ThenByDescending(x => x.LastAvailable).ThenBy(x => x.Description).ToList();

            }
            return mim;
        }

        public ActionResult GetTableSections(int UserId)
        {
            logger.Info("Get All Tables - " + UserId);
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
            logger.Info("Get All Tables Fetched - " + UserId);
            return Json(tsList, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetTablesBySection(int sectionId, int UserId)
        {
            logger.Info("Get All Tables By Section - " + UserId);
            Models.UserService us = new Models.UserService();
            TablesList tablesList = new TablesList();
            if (sectionId == 100)
                tablesList = us.GetTablesList();
            else
                tablesList = us.GetTablesBySection(sectionId);
            logger.Info("Get All Tables By Section Fetched - " + UserId);
            return Json(tablesList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ProductGroups(int UserId)
        {
            logger.Info("Starting GetProducts - " + UserId);
            var cache = MemoryCache.Default;
            AllProducts plist = new AllProducts();
            //if (cache.Get("ProductsList") == null)
            //{
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
                    pr.EnglishName = FieldConverter.To<string>(row["EnglishName"]);
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
                p = p.OrderBy(x => x.Description).ToList();
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
            //    cache.Add("ProductsList", plist, cachePolicy);
            //}
            //else
            //{
            //    plist = (AllProducts)cache.Get("ProductsList");
            //}
            logger.Info("Finished GetProducts - " + UserId);
            return Json(plist, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetTableOrder(int t, int UserId)
        {
            logger.Info("Getting OrderedItems - " + UserId);
            Models.OrderService os = new Models.OrderService();
            TableOrder to = os.GetTableOrder(t);
            logger.Info("OrderedItems Fetched - " + UserId);
            return Json(to, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GenerateQRCode(int code, string userType, int UserId)
        {
            //string folderPath = "C:\\TableCodeImages";
            //if (!System.IO.Directory.Exists(folderPath))
            //{
            //    System.IO.Directory.CreateDirectory(folderPath);
            //}
            logger.Info("Generate QR Code started - " + UserId);

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
                    using (var _context= new ChineseTillEntities1())
                    {
                        var tord = _context.tblTableOrders.Where(x => x.UniqueCode == code).FirstOrDefault();
                        tord.QRCodePrint = false;
                        _context.Entry(tord).State = EntityState.Modified;
                        _context.SaveChanges();
                    }
                    response = imagePath;
                    
                }
            }
            catch (Exception ex)
            {
                response = ex.Message;
                //throw;
            }
            logger.Info("Generate QR Code end - " + UserId);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        //Comment by Parth as this is creating multiple entries so next code is the updated one.

        //public ActionResult UpdateReservationOnTill(List<tblReservation> req)
        //{
        //    string response = "";
        //    try
        //    {
        //        using (TransactionScope scope = new TransactionScope())
        //        {
        //            using (var context = new ChineseTillEntities1())
        //            {
        //                foreach (var item in req)
        //                {
        //                    var res = context.tblReservations.Where(x => x.AdminReservationId == item.ReservationID).FirstOrDefault();
        //                    if (res != null)
        //                    {
        //                        res.ReservationDate = item.ReservationDate;
        //                        res.ReservationTime = Convert.ToInt32(item.ReservationDate.ToString("HHmm"));
        //                        res.ReservationNotes += item.ReservationNotes;
        //                        res.AdminReservationId = item.ReservationID;
        //                        context.Entry(res).State = EntityState.Modified;
        //                        context.SaveChanges();
        //                        response = "success";
        //                    }
        //                    else
        //                    {
        //                        context.usp_SS_AddOnlineReservationToTill_V2(item.ReservationID, "", item.ForeName, item.ReservationUnder, item.ContactNumber, item.ReservationNotes, item.NoOfGuests, item.ReservationDate, Convert.ToInt32(item.ReservationDate.ToString("HHmm")), item.Deposit, item.NoOfSeats, item.NoOfHighChairs, item.NoOfWheelChairs, item.NoOfPrams, item.UniqueCode);
        //                        //item.AdminReservationId = item.ReservationID;
        //                        //item.ReservationTime = Convert.ToInt32(item.ReservationDate.ToString("HHmm"));
        //                        //item.DateCreated = DateTime.Now;
        //                        //item.bDepositPaid = true;
        //                        //item.OnlineReservation = true;
        //                        //context.tblReservations.Add(item);
        //                        //context.SaveChanges();
        //                        response = "success";
        //                    }
        //                }
        //            }
        //            scope.Complete();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        response = ex.Message + ex.StackTrace;
        //        if (ex.InnerException != null)
        //            response = ex.InnerException.Message;
        //    }
        //    return Json(response, JsonRequestBehavior.AllowGet);
        //}


        //by parth on 29th July to delete multiple entries of reservation (if found).
        public ActionResult UpdateReservationOnTill(List<tblReservation> req)
        {
            string response = "";
            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    using (var context = new ChineseTillEntities1())
                    {
                        foreach (var item in req)
                        {
                            var reslist = context.tblReservations.Where(x => x.ReservationUnder == item.ReservationUnder
                            && x.ContactNumber == item.ContactNumber && x.ReservationDate == item.ReservationDate
                            && x.bDepositPaid == item.bDepositPaid && x.Delind == false 
                            && item.RestaurantId == Convert.ToInt32(ConfigurationManager.AppSettings["RestaurantId"])).ToList();

                            if (reslist.Count > 1)
                            {
                                for (int i = 1; i < reslist.Count; i++)
                                {
                                    reslist[i].Delind = true;
                                    context.Entry(reslist[i]).State = EntityState.Modified;
                                }
                                context.SaveChanges();
                            }
                            else
                            {
                                context.usp_SS_AddOnlineReservationToTill_V2(item.ReservationID, "", item.ForeName, item.ReservationUnder, item.ContactNumber, item.ReservationNotes, item.NoOfGuests, item.ReservationDate, Convert.ToInt32(item.ReservationDate.ToString("HHmm")), item.Deposit, item.NoOfSeats, item.NoOfHighChairs, item.NoOfWheelChairs, item.NoOfPrams, item.UniqueCode);
                                //item.AdminReservationId = item.ReservationID;
                                //item.ReservationTime = Convert.ToInt32(item.ReservationDate.ToString("HHmm"));
                                //item.DateCreated = DateTime.Now;
                                //item.bDepositPaid = true;
                                //item.OnlineReservation = true;
                                //context.tblReservations.Add(item);
                                //context.SaveChanges();
                                response = "success";
                            }
                        }
                    }
                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + ex.StackTrace;
                if (ex.InnerException != null)
                    response = ex.InnerException.Message;
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReOrderPrint(ReOrderModel req)
        {
            string response = "";
            try
            {
                using (var _context = new ChineseTillEntities1())
                {
                    foreach (var item in req.BuffetItems)
                    {
                        for (int i = 0; i < item.Qty; i++)
                        {
                            tblOrderBuffetItem toi = new tblOrderBuffetItem();
                            toi.DateCreated = DateTime.Now;
                            toi.OrderGUID = item.OrderGUID;
                            toi.Printed = false;
                            toi.ProductId = item.ProductId;
                            toi.TableId = item.TableId;
                            toi.UserId = item.UserId;
                            toi.UserName = item.UserName;
                            toi.Qty = 1;
                            toi.UserType = UserType.Staff.ToString();
                            toi.WebUpload = false;
                            toi.ReOrder = true;
                            _context.tblOrderBuffetItems.Add(toi);
                        }
                    }
                    _context.SaveChanges();
                    response = "Items reordered successfully";
                }
            }
            catch (Exception ex)
            {
                response = ex.Message;
                if (ex.InnerException != null)
                    response = ex.InnerException.Message;
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAllPrintBatches()
        {
            List<PrintingBatch> pb = new List<PrintingBatch>();
            
            pb = GetPrintBatches();
         
            return Json(pb, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAllPrintBatchesByTable()
        {
            List<TablePrintingBatch> tpbList = new List<TablePrintingBatch>();
            List<PrintingBatch> pb = new List<PrintingBatch>();
            pb = GetPrintBatches();
            var uniqTables = pb.Select(x => x.TableNumber).Distinct();
            foreach (var item in uniqTables)
            {
                TablePrintingBatch tpb = new TablePrintingBatch();
                tpb.TableNumber = item;
                tpb.TableBatches = pb.Where(x => x.TableNumber == item).ToList();
                tpbList.Add(tpb);
            }
            return Json(tpbList, JsonRequestBehavior.AllowGet);
        }

        private List<PrintingBatch> GetPrintBatches()
        {
            List<PrintingBatch> pb = new List<PrintingBatch>();
            List<string> printers = new List<string>();
            printers.Add("Kitchen");
            printers.Add("Kitchen2");
            try
            {
                using (var _context = new ChineseTillEntities1())
                {
                    //pb = Products.Select(p => new { a.id, a.modified })
                    //     .AsEnumerable()
                    //     .Select(p => new ProductVM()
                    //     {
                    //         id = p.id,
                    //         modified = p.modified.ToString()
                    //     });
                    pb = (from a in _context.tblPrintQueues
                          where DbFunctions.TruncateTime(a.DateCreated) == DbFunctions.TruncateTime(DateTime.Now)
                          && printers.Contains(a.ToPrinter)
                          select new
                          {
                              PrintQueueId = a.PrintQueueID,
                              TableNumber = a.TableNumber ?? "",
                              BatchNumber = a.BatchNo,
                              Processed = a.Processed,
                              BatchTime = a.DateCreated
                          }).AsEnumerable().Select(p => new PrintingBatch()
                          {
                              PrintQueueId = p.PrintQueueId,
                              TableNumber = p.TableNumber,
                              BatchNumber = p.BatchNumber,
                              Processed = p.Processed,
                              BatchTime = p.BatchTime.ToString("HH:mm")
                          }).OrderByDescending(a => a.BatchNumber).ToList();
                }
            }
            catch (Exception ex)
            {

                
            }
            return pb;
        }

        public ActionResult ProcessPrintBatch(int pqId, bool processed, int userId, int ticketStatus = 0,bool printerOffline = false)
        {
            List<PrintingBatch> pb = new List<PrintingBatch>();
            string response = "";
            try
            {
                using (var _context = new ChineseTillEntities1())
                {
                   
                    var batch = _context.tblPrintQueues.Where(x => x.PrintQueueID == pqId).FirstOrDefault();
                    batch.Processed = processed;
                    batch.ProcessedBy = userId;
                    batch.DateProcessed = DateTime.Now;
                    batch.TicketStatus = ticketStatus;
                    if (printerOffline == true)
                    {
                        if (ticketStatus == 2)
                            batch.DatePrinted = DateTime.Now;
                        else if (ticketStatus == 0)
                            batch.DatePrinted = null;
                    }
                    _context.Entry(batch).State = EntityState.Modified;
                    _context.SaveChanges();

                     
                   
                    //pb = (from a in _context.tblPrintQueues
                    //      where DbFunctions.TruncateTime(a.DateCreated) == DbFunctions.TruncateTime(DateTime.Now)
                    //      select new PrintingBatch
                    //      {
                    //          TableNumber = a.TableNumber ?? "",
                    //          BatchNumber = a.BatchNo,
                    //          Processed = a.Processed
                    //      }).OrderBy(a => a.BatchNumber).ToList();
                    //var items = _context.tblOrderBuffetItems.Where(x=>x.)
                    int batchNo = batch.OldBatchNo > 0 ? batch.OldBatchNo : batch.BatchNo;
                    //var buffetItems = _context.tblOrderBuffetItems.Where(x => x.BatchNo == batchNo).ToList();
                    var buffetItems = _context.tblPrintBuffetItems.Where(x => x.BatchNo == batchNo).ToList();

                    if (processed)
                    buffetItems.ForEach(x => x.Delivered = true);
                    else
                        buffetItems.ForEach(x => x.Delivered = false);
                    _context.SaveChanges();
                    response = "success";
                }
            }
            catch (Exception ex)
            {
                response = ex.Message;
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBatchOrder(int pqId)
        {
            string response = "";
            try
            {
                using (var _context = new ChineseTillEntities1())
                {
                    response = _context.tblPrintQueues.Where(x => x.PrintQueueID == pqId).Select(x => x.Receipt).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {

                response = ex.Message;
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        /***
         * Check if any product is made unavaialble
         * Also check for total ordered items. If they are more max dish per table, give a message
         ***/
        public ActionResult VerifyOrderProducts(TableOrder req)
        {
            VerifyOrderProductsResponse response = new VerifyOrderProductsResponse();
            response.IsSuccess = true;
            response.Message = "";
            response.ShowOptionButtons = false;
            string unAvailableProducts = "";
            int approxDeliveryTime = 12;
            try
            {
                if (req.BuffetItems != null && req.BuffetItems.Count > 0)
                {
                    //check for inactive products
                    using (var _context = new ChineseTillEntities1())
                    {
                        DateTime updatedTime = DateTime.Now.AddMinutes(-180);
                        var unAvailableproductIds = _context.tblMenuItems.Where(x => x.LastModified != null && x.LastModified >= updatedTime && x.Active == false).Select(x => x.ProductID).Distinct().ToList();
                        if (unAvailableproductIds != null && unAvailableproductIds.Count > 0)
                        {
                            foreach (var item in req.BuffetItems)
                            {
                                if (unAvailableproductIds.Contains(item.ProductId))
                                {
                                    response.IsSuccess = false;
                                    unAvailableProducts += item.Description + ",";
                                }
                            }
                        }
                        if (!response.IsSuccess)
                            //response.Message = " Unfortunately " + unAvailableProducts + " got out of stock. Kindly order some other dish.";
                            response.Message = unAvailableProducts + " is currenty unavailable";

                        else
                        {
                            var maxDishesAllowed = _context.tblTableOrders.Where(x => x.OrderGUID == req.OrderGUID && x.Active == true).Select(x => x.CustomerCount).FirstOrDefault();
                            maxDishesAllowed = maxDishesAllowed * 3;
                            //var totalUnPrintedItems = _context.tblOrderBuffetItems.Where(x => x.Printed == false && DbFunctions.TruncateTime(x.DateCreated) == DbFunctions.TruncateTime(DateTime.Now)).ToList();
                            var totalUnPrintedItems = _context.tblPrintBuffetItems.Where(x => x.Printed == false && DbFunctions.TruncateTime(x.DateCreated) == DbFunctions.TruncateTime(DateTime.Now)).ToList();
                            var orderUnPrintedItems = totalUnPrintedItems.Where(x => x.OrderGUID == req.OrderGUID).ToList();
                            if(totalUnPrintedItems != null && totalUnPrintedItems.Count > 0)
                            {
                                var totalUnprintedItemsCount = totalUnPrintedItems.Sum(x => x.Qty);
                                if (totalUnprintedItemsCount <= 75)
                                    approxDeliveryTime = 12;
                                else if (totalUnprintedItemsCount <= 100)
                                    approxDeliveryTime = 15;
                                else
                                    approxDeliveryTime = 18;
                            }
                           
                            if (req.BuffetItems.Sum(x => x.Qty) > maxDishesAllowed)
                            {
                                response.IsSuccess = false;
                                response.ShowOptionButtons = true;
                                response.Message = "This order exceeds the "+ maxDishesAllowed+ " dishes per order for your table. It may take twice as long to deliver. To meet the current delivery time of approx"+ approxDeliveryTime +" minutes, please reduce the number of dishes.";
                            }
                            
                            if (orderUnPrintedItems != null && orderUnPrintedItems.Count > 0)
                            {
                                var unPrintedItemsCount = orderUnPrintedItems.Sum(x => x.Qty);
                                 if (unPrintedItemsCount + req.BuffetItems.Count > maxDishesAllowed)
                                {
                                    response.IsSuccess = false;
                                    response.Message = "The accumulative order exceeds "+ maxDishesAllowed +" dishes per order for your table.  It may take twice as long to deliver. To meet the current delivery time of approx " + approxDeliveryTime +" minutes, please reduce the number of dishes.";
                                    response.ShowOptionButtons = true;
                                }
                            }
                            
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = "Oops!! Some eror occured. Please try again!";
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SubmitFeedback(FeedbackRequest req)
        {
            FeedbackResponse response = new FeedbackResponse();
            response.ReviewLink = "";
            try
            {
                using (var _context = new ChineseTillEntities1())
                {
                    tblCustomerFeedback r = new tblCustomerFeedback();
                    r.Mobile = req.Mobile;
                    r.OrderNo = req.OrderNo;
                    r.Feedback = req.Feedback;
                    r.OrderType = "DineIn";
                    r.OverallRating = req.OverallRating;
                    r.Recommendation = req.Recommendation;
                    r.DateCreated = DateTime.Now;
                    _context.tblCustomerFeedbacks.Add(r);
                    _context.SaveChanges();
                }
                response.Message = "Thanks for your valuable feedback.";
                if (req.OverallRating >= 3)
                {
                    if (req.RestaurantId == 1)
                        response.ReviewLink += "https://g.page/the-chinese-buffet-bolton/review?rc";
                    else if (req.RestaurantId == 2)
                        response.ReviewLink += "https://g.page/thechinesebuffetwigan/review?rc";
                    else if (req.RestaurantId == 3)
                        response.ReviewLink += "https://g.page/thechinesebuffetliverpool/review?rc";
                    else if (req.RestaurantId == 8)
                        response.ReviewLink += "https://g.page/r/CQ4Hf7LfsA-2EAg/review";
                    else if (req.RestaurantId == 11)
                        response.ReviewLink += "https://g.page/thechinesebuffetbury/review?rc";
                    else if (req.RestaurantId == 13)
                        response.ReviewLink += "https://g.page/r/CWzCg_OdRviXEAg/review";
                    else if (req.RestaurantId == 14)
                        response.ReviewLink += "https://g.page/r/CRgnLTTruxb4EAg/review";
                    else if (req.RestaurantId == 15)
                        response.ReviewLink += "https://g.page/thechinesebuffetsthelens/review?rc";
                    else if (req.RestaurantId == 16)
                        response.ReviewLink += "https://g.page/r/CbJxiAiYjg6JEAg/review";

                }
                string activityType = ActivityType.Feedback.ToString();
                tblCustomerActivity tca = new tblCustomerActivity();
                tca.Mobile = req.Mobile;
                tca.ActivityType = activityType;
                tca.FullName = req.FullName;
                tca.OrderGUID = new Guid(req.OrderNo);
                cs.UpdateCustomerActivity(tca);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReleasePaymentLock(Guid OrderId)
        {
            string response = "";
            try
            {
                using (var _context = new ChineseTillEntities1())
                {
                    var order = _context.tblOrders.Where(x => x.OrderGUID == OrderId).FirstOrDefault();
                    order.LockedForPayment = false;
                    _context.Entry(order).State = EntityState.Modified;
                    _context.SaveChanges();
                }
                response = "success";
            }
            catch (Exception ex)
            {

                response = ex.Message;
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult OrderLock(Guid OrderId, bool locked)
        {
            string response = "";
            try
            {
                using (var _context = new ChineseTillEntities1())
                {
                    var order = _context.tblOrders.Where(x => x.OrderGUID == OrderId).FirstOrDefault();
                    order.LockedForOrdering = locked;
                    _context.Entry(order).State = EntityState.Modified;
                    _context.SaveChanges();
                }
                response = "success";
            }
            catch (Exception ex)
            {

                response = ex.Message;
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult UpdatePaymentLock(Guid OrderId, bool locked,string txCode = "")
        {
            string response = "";
            try
            {
                using (var _context = new ChineseTillEntities1())
                {
                    var order = _context.tblOrders.Where(x => x.OrderGUID == OrderId).FirstOrDefault();
                    order.LockedForPayment = locked;
                    _context.Entry(order).State = EntityState.Modified;
                    _context.SaveChanges();
                    
                    if (txCode != null && txCode != "")
                    {
                        var oItems = _context.tblOrderParts.Where(x => x.VendorTxCode == txCode).ToList();
                        if (oItems != null && oItems.Count > 0)
                        {
                            oItems.ForEach(a =>
                            {
                                a.VendorTxCode = "";

                            });
                            _context.SaveChanges();
                        }
                    }
                    else
                    {
                        var orderParts = _context.tblOrderParts.Where(x => x.OrderGUID == OrderId && x.Paid == false && (x.VendorTxCode != null && x.VendorTxCode != "")).ToList();
                        if (orderParts != null && orderParts.Count > 0)
                        {
                            orderParts.ForEach(a =>
                            {
                                a.VendorTxCode = "";

                            });
                            _context.SaveChanges();
                        }
                    }
                }
                response = "success";
            }
            catch (Exception ex)
            {

                response = ex.Message;
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetDashboardItems(int UserLevel)
        {
            DashboardModel dm = new DashboardModel();
            try
            {
                using (var _context = new ChineseTillEntities1())
                {
                    dm.AppOptions = (from c in _context.tblAppOptions
                                     where c.DelInd == false && !_context.tblAppOptionRestrictions.Any(p => p.AppOptionID == c.ID && p.UserLevel == UserLevel)
                                     select new AppOption
                                     {
                                         OptionID = c.ID,
                                         OptionName = c.Name,
                                         OptionActivity = c.Activity,
                                         Position = (int)c.Position
                                     }).OrderBy(x => x.Position).ToList();
                    var headCounts = _context.usp_AN_GetHeadCounts();
                    foreach (var row in headCounts)
                    {
                        TimeSpan ts = DateTime.Now - row.DateCreated.Value;
                        if (ts.TotalMinutes < 20)
                            dm.HeadCount.Starters += (int)row.HeadCount;
                        else if (ts.TotalMinutes >= 20 && ts.TotalMinutes < 65)
                            dm.HeadCount.MainCourse += (int)row.HeadCount;
                        else
                            dm.HeadCount.Deserts += (int)row.HeadCount;
                    }
                    //var buffetItems = (from a in _context.tblOrderBuffetItems
                    //                   join b in _context.tblMenuItems on a.ProductId equals b.ProductID
                    //                   where DbFunctions.TruncateTime(a.DateCreated) == DbFunctions.TruncateTime(DateTime.Now) && (a.Delivered == false || a.Printed == false)
                    //                   select new
                    //                   {
                    //                       Delivered = a.Delivered,
                    //                       Printed= a.Printed,
                    //                       MenuId = b.MenuID
                    //                   }).ToList();
                    var buffetItems = (from a in _context.tblPrintBuffetItems
                                       join b in _context.tblMenuItems on (int) a.ProductId equals b.ProductID
                                       where DbFunctions.TruncateTime(a.DateOrdered) == DbFunctions.TruncateTime(DateTime.Now) && (a.Delivered == false || a.Printed == false)
                                       select new
                                       {
                                           Delivered = a.Delivered,
                                           Printed = a.Printed,
                                           MenuId = b.MenuID
                                       }).ToList();
                    dm.UndeliveredItemCount = buffetItems.Where(x => x.Delivered == false && x.Printed == true).Count();
                    dm.UnprintedItemCount = buffetItems.Where(x => x.Printed == false).Count();
                    dm.UndeliveredTicketCount = _context.tblPrintQueues.Where(x => DbFunctions.TruncateTime(x.DateCreated) == DbFunctions.TruncateTime(DateTime.Now) && (x.Processed == false) && x.ToPrinter != "Bar").Count();
                    dm.StarterItemCount = buffetItems.Where(x => x.MenuId == 3).Count();
                    dm.DesertItemCount = buffetItems.Where(x => x.MenuId == 11).Count();
                    dm.MainCourseItemCount = buffetItems.Where(x => x.MenuId != 3 && x.MenuId != 11).Count();
                }

            }
            catch (Exception ex)
            {

                throw;
            }
            return Json(dm, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReprintKitchenReceipt(int pqId, int userId)
        {
            string response = "";
            try
            {
                using (var _context= new ChineseTillEntities1())
                {
                    var receipt = _context.tblPrintQueues.Where(x => x.PrintQueueID == pqId).FirstOrDefault();
                    int latestBatchNo = _context.tblPrintQueues.Where(x=> DbFunctions.TruncateTime(x.DateCreated) == DbFunctions.TruncateTime(DateTime.Now)).OrderByDescending(x => x.BatchNo).Select(x => x.BatchNo).FirstOrDefault();
                    if (receipt != null)
                    {
                        string newreciept = "***Duplicate***" + Environment.NewLine + receipt.Receipt;
                        receipt.Receipt = newreciept;
                        tblPrintQueue tp = new tblPrintQueue();
                        receipt.DateCreated = DateTime.Now;
                        receipt.DatePrinted = null;
                        receipt.Processed = false;
                        receipt.OldBatchNo = receipt.BatchNo;
                        receipt.BatchNo = latestBatchNo + 1;
                        receipt.ProcessedBy = 0;
                        receipt.DateProcessed = null;
                        _context.tblPrintQueues.Add(receipt);
                        _context.SaveChanges();
                    }
                    response = "Ticket re-printed successfully";
                }
            }
            catch (Exception ex)
            {
                response = ex.Message;
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReprintTableQRCode(Guid OrderId)
        {
            string response = "";
            try
            {
                using (var _context = new ChineseTillEntities1())
                {
                    var tbOrd = _context.tblTableOrders.Where(x => x.OrderGUID == OrderId).FirstOrDefault();
                    if (tbOrd.Active)
                    {
                        tbOrd.QRCodePrint = false;
                        _context.Entry(tbOrd).State = EntityState.Modified;
                        _context.SaveChanges();
                        response = "Table code reprint submitted";
                    }
                    else
                        response = "Table Code Inactive";

                }
            }
            catch (Exception ex)
            {
                response = ex.Message;
            }
            return Json(response, JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetItemsForSplit(Guid OrderId, string MobileNo)
        {
            TableOrder to = new TableOrder();
            try
            {
                using (var _context = new ChineseTillEntities1())
                {
                    to.tableProducts = (from a in _context.tblOrderParts
                                        join b in _context.tblProducts on a.ProductID equals b.ProductID
                                        where a.OrderGUID == OrderId && b.DelInd == false && a.DelInd == false 
                                        select new Entity.Product
                                        {
                                            ProductID = a.ProductID,
                                            Description = b.Description,
                                            Price = (float)a.Price,
                                            ProductQty = (int) a.Qty,
                                            ProductTypeId = b.ProductTypeID,
                                            IsCheckedForSplit = MobileNo.Contains(a.Mobile) ? true : false
                                        }).ToList();
                    foreach (var item in to.tableProducts)
                    {
                        var buffet = to.tableProducts.Where(x => x.Description.Contains("Buffet Adult") || (x.EnglishName != null && x.EnglishName.Contains("Buffet Adult"))).FirstOrDefault();
                        buffet.IsCheckedForSplit = true;
                        break;
                        
                    }
                    //int? custCount = _context.tblOrders.Where(x => x.OrderGUID == OrderId).Select(x => x.AdCount + x.JnCount + x.KdCount).FirstOrDefault();
                    //to.tableProducts.Add(rh.CalculateDiscount(to.tableProducts, custCount));
                    to.IsSuccess = true;
                }
            }
            catch (Exception ex)
            {
                to.IsSuccess = false;
                to.Message = ex.Message;
            }
            return Json(to, JsonRequestBehavior.AllowGet);

        }

        //public ActionResult GetReservationSlots(string resDate)
        //{
        //    try
        //    {
        //        using (var _context = new ChineseTillEntities1())
        //        {
        //            DateTime rd = DateTime.Parse(resDate);

        //            var slots = _context.usp_MF_GetAvailableReservationTimes(rd);
        //            foreach (var item in slots)
        //            {

        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }
        //}

        //public ActionResult GetTakeAwayItems(string type)
        //{

        //}

        public ActionResult EditSCRate(Guid OrderId, decimal Rate)
        {
            string response = "";
            try
            {
                using (var _context = new ChineseTillEntities1())
                {
                    var order = _context.tblOrders.Where(x => x.OrderGUID == OrderId).FirstOrDefault();
                    order.SCRate = Rate;
                    _context.Entry(order).State = EntityState.Modified;
                    _context.SaveChanges();
                }
                response = "success";
            }
            catch (Exception ex)
            {

                response = ex.Message;
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        //public ActionResult Feedbackdetails(Guid OrderGuid)
        //{
        //    FeedbackDetailsDto response = new FeedbackDetailsDto();
        //    try
        //    {
        //        using(var _context = new ChineseTillEntities1())
        //        {
        //            var stripedetails = _context.tblStripePayments.Where(x => x.OrderGUID == OrderGuid && x.Success == true).FirstOrDefault();
        //            var orderdetails = _context.tblOrders.Where(x => x.OrderGUID == OrderGuid && x.Paid == true).FirstOrDefault();
        //            var tabledetails = _context.tblTables.Where(x=>x.TableID == orderdetails.TableID).FirstOrDefault();
        //            if (stripedetails != null)
        //            {
        //                response.restaurant_name = ConfigurationManager.AppSettings["RestaurantName"];
        //                response.table_no = tabledetails.TableNumber;
        //                response.customer_id = Convert.ToString(orderdetails.CustomerId);
        //                response.customer_no = stripedetails.MobileNo;
        //                response.order_guid = Convert.ToString(stripedetails.OrderGUID);
        //                response.restaurant_id = ConfigurationManager.AppSettings["RestaurantId"];
        //                response.api_url = ConfigurationManager.AppSettings["ROMSApiUrl"];
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //        Console.WriteLine(ex.StackTrace);

        //        throw;
        //    }
        //    return Json(response, JsonRequestBehavior.AllowGet);
        //}

        public ActionResult RecordManualPayment(tblRecordPayment PaymentRecord)
        {
            string message = "";
            try
            {
                using (var _context = new ChineseTillEntities1())
                {
                    tblStripePayment paymentrecord = _context.tblStripePayments.Where(x=>x.OrderGUID == PaymentRecord.OrderGUID && x.Success == false).FirstOrDefault();

                    // Create a new instance of the entity that will be saved in the database
                    var newRecord = new tblRecordPayment
                    {
                        // Map each property one by one
                        OrderGUID = PaymentRecord.OrderGUID,
                        MobileNumber = PaymentRecord.MobileNumber,
                        CustomerName = PaymentRecord.CustomerName,
                        ScreenshotType = PaymentRecord.ScreenshotType,
                        TotalAmt = paymentrecord.Amount,
                        VendorTXCode = paymentrecord.VendorTxCode,
                        RestaurantFK = Convert.ToInt32(ConfigurationManager.AppSettings["RestaurantId"]),
                        Base64Image = PaymentRecord.Base64Image,

                        // Set DateCreated if not provided
                        DateCreated = PaymentRecord.DateCreated ?? DateTime.Now,

                        // Set DelInd to false if null
                        DelInd = PaymentRecord.DelInd ?? false
                    };

                    // Add the new record to the context
                    _context.tblRecordPayments.Add(newRecord);

                    // Save changes to the database
                    _context.SaveChanges();
                    string imagePath = SaveBase64ImageToDirectory(newRecord.Base64Image, newRecord.OrderGUID.ToString());

                    SendPaymentConfirmationRequestToHQ(newRecord, imagePath);

                    //Payment sendrecord = new Payment
                    //{
                    //    Amount = paymentrecord.Amount,  // Assuming paymentrecord is your source object
                    //    DateCreated = DateTime.Now, // Set as required
                    //    DeviceType = "android", // Replace with actual value if needed
                    //    FailureMessage = null, // Set as required
                    //    FullName = PaymentRecord.CustomerName, // Replace with actual value
                    //    TablePaid = true, // Set as required
                    //    IsSuccess = true, // Set as required
                    //    OrderGUID = PaymentRecord.OrderGUID, // Replace with actual value
                    //    PaymentMethod = "Stripe", // Set as required
                    //    ServiceCharge = 0.0m, // Set as required
                    //    SplitProduct = new List<Product>() ,
                    //    TableId = 0, // Set as required
                    //    TipAmount = 0.0m, // Set as required
                    //    TxCode = paymentrecord.VendorTxCode, // Replace with actual value
                    //    isSplitPayment = false // Set as required
                    //};
                    //V3Controller controller = new V3Controller(); // Instantiate controller
                    //ActionResult result = controller.ConfirmTablePayment(sendrecord);
                }
                message = "success";
            }
            catch (Exception ex)
            {
                // Log the exception if necessary and return the error message
                message = $"Error while saving payment record: {ex.Message}";
            }

            return Json(message, JsonRequestBehavior.AllowGet);
        }

        // Method to save the Base64 image to the directory
        private string SaveBase64ImageToDirectory(string base64Image, string orderGuid)
        {
            // Define the directory path where you want to save the images
            string directoryPath = Path.Combine("D:\\", "PaymentScreenshots");

            // Check if the directory exists, if not, create it
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Define the image file name based on the OrderGUID
            string fileName = $"{orderGuid}.png"; // or .jpg based on the image type
            string fullPath = Path.Combine(directoryPath, fileName);

            // Convert the base64 string to a byte array
            byte[] imageBytes = Convert.FromBase64String(base64Image);

            // Save the image to the directory
            System.IO.File.WriteAllBytes(fullPath, imageBytes);

            return fullPath; // Return the saved image path for further use
        }
        private void SendPaymentConfirmationRequestToHQ(tblRecordPayment paymentRecord, string imagePath)
        {
            try
            {
                string gmailid = ConfigurationManager.AppSettings["emailID"].ToString();
                string toEmail = ConfigurationManager.AppSettings["HQEmailAddress"];

                using (SmtpClient smtp = new SmtpClient(ConfigurationManager.AppSettings["paolomailserver"], 587))
                {
                    smtp.EnableSsl = true;
                    smtp.UseDefaultCredentials = false;

                    // Retrieve credentials from config
                    smtp.Credentials = new NetworkCredential(gmailid, ConfigurationManager.AppSettings["password"].ToString());

                    // Create the MailMessage object
                    using (MailMessage mail = new MailMessage())
                    {
                        mail.From = new MailAddress(gmailid, "THE Chinese Buffet");
                        mail.To.Add(toEmail);
                        mail.Subject = "Payment Confirmation Request";

                        // Email body with detailed information
                        mail.Body = $"Hi,\n\n" +
                                    $"Please confirm whether the following payment is valid and correct:\n\n" +
                                    $"Customer Name: {paymentRecord.CustomerName}\n" +
                                    $"Mobile Number: {paymentRecord.MobileNumber}\n" +
                                    $"Total Amount: {paymentRecord.TotalAmt:C}\n" +
                                    $"Vendor Transaction Code: {paymentRecord.VendorTXCode}\n" +
                                    $"Thank you,\nThe Chinese Buffet";

                        if (System.IO.File.Exists(imagePath))
                        {
                            Attachment attachment = new Attachment(imagePath);
                            mail.Attachments.Add(attachment);
                        }

                        // Send the email
                        smtp.Send(mail);
                    }
                }
                Console.WriteLine("Email sent successfully.");
            }
            catch (Exception ex)
            {
                // Log or handle the email sending error as needed
                Console.WriteLine($"Error while sending email: {ex.Message}");
            }
        }

    }
}