using Entity;
using Entity.AdminDtos;
using Entity.Enums;
using Entity.Models;
using Newtonsoft.Json;
using NLog;
using Stripe;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using TCBROMS_Android_Webservice.Helpers;
using TCBROMS_Android_Webservice.Models;
using CustomerService = TCBROMS_Android_Webservice.Models.CustomerService;

namespace TCBROMS_Android_Webservice.Controllers
{
    //[AllowCrossSite]
    public class V3Controller : Controller
    {
        string url = ConfigurationManager.AppSettings["TCBAPIUrl"];
        ROMSHelper rh = new ROMSHelper();
        CustomerService cs = new Models.CustomerService();
        Logger logger = LogManager.GetLogger("databaseLogger");
        // GET: V3
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult GetDineInProducts(Guid orderId)
        {
            GetProductsResponse gpr = new GetProductsResponse();
            Models.ProductService ps = new Models.ProductService();
            logger.Info("Starting GetDineInProducts - ");
            var cache = MemoryCache.Default;
            //if (cache.Get("DineInProducts") == null)
            //{
            //    var cachePolicy = new CacheItemPolicy();
            //    cachePolicy.AbsoluteExpiration = DateTime.Now.AddMinutes(120);

            //    cache.Add("DineInProducts", gpr, cachePolicy);
            //}
            //else
            //{
            //gpr = (GetProductsResponse)cache.Get("DineInProducts");

            //}
            gpr = ps.GetDineInProductsV2(orderId);
            return Json(gpr, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCustomerByMobile(string mobileNo)
        {
            Entity.Customer cust = new Entity.Customer();
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(url);
                    var responseTask = client.GetAsync("v2/GetCustomerByMobile?mobileNo=" + mobileNo);
                    responseTask.Wait();
                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsAsync<Entity.Customer>();
                        readTask.Wait();
                        cust = readTask.Result;
                    }
                    else //web api sent error response 
                    {
                        //log response status here..
                    }
                }
            }
            catch (Exception ex)
            {
                cust.CustomerID = -1;
            }
            return Json(cust, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SubmitRedeemedProducts(TableOrder to)
        {
            string response = "";
            List<int> orderParts = new List<int>();
            int redeemdPoints = 0;
            //update order part price = 0
            try
            {
                using (var _context = new ChineseTillEntities1())
                {
                    foreach (var item in to.tableProducts)
                    {
                        redeemdPoints += item.RedemptionPoints;
                        orderParts.Add(item.ProductID);
                        for (int i = 0; i < item.ProductQty; i++)
                        {
                            tblRedeemedProduct tr = new tblRedeemedProduct();
                            tr.ProductId = item.ProductID;
                            tr.OrderGUID = to.OrderGUID;
                            tr.OrderType = OrderType.DineIn.ToString();
                            tr.DateCreated = DateTime.Now;
                            tr.DelInd = false;
                            tr.Points = item.RedemptionPoints;
                            tr.Qty = 1;
                            tr.Price = (decimal)item.Price;
                            tr.WebUpload = false;
                            _context.tblRedeemedProducts.Add(tr);
                        }
                        _context.SaveChanges();
                    }
                    //update customer points

                    //int updatedPoints = cs.UpdateCustomerPoints(to.tableCustomer.CustomerID, redeemdPoints, 0, to.OrderGUID.ToString());
                    tblCustomerActivity tca = new tblCustomerActivity();
                    tca.FullName = to.UserName;
                    tca.Mobile = to.MobileNumber.ToString();
                    tca.OrderGUID = to.OrderGUID;
                    tca.ActivityType = ActivityType.RedeemPoints.ToString();
                    tca.RedeemPoints = redeemdPoints;
                    tca.RewardPoints = 0;
                    cs.UpdateCustomerActivity(tca);
                    //if (updatedPoints > 0)
                    if (redeemdPoints > 0)
                    {
                        foreach (var op in orderParts)
                        {
                            var pr = _context.tblOrderParts.Where(x => x.ProductID == op && x.OrderGUID == to.OrderGUID && x.Price > 0).FirstOrDefault();
                            pr.Price = 0;
                            pr.Total = 0;
                            _context.Entry(pr).State = System.Data.Entity.EntityState.Modified;
                            _context.SaveChanges();
                        }
                        response = "Products redeemed succesfully";
                    }
                }
            }
            catch (Exception ex)
            {
                response = " We could not redeem your points. please try again later";

            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetRewardPointsForOrder(Guid orderId, decimal orderAmount, bool custDetailsRequireed = false)
        {
            var cust = cs.GetRewardPointsForOrder(orderId, orderAmount, custDetailsRequireed);
            return Json(cust, JsonRequestBehavior.AllowGet);
        }

        //old code
        //public ActionResult GetOrderedItemsbyMenu(string MenuId)
        //{
        //    List<int> menuIds = MenuId.Split(',').Select(int.Parse).ToList();
        //    int maxItems = 6;
        //    ChineseTillEntities1 _context = new ChineseTillEntities1();
        //    List<OrderMenuItemsResponse> omr = new List<OrderMenuItemsResponse>();
        //    //changes for new station Priority & Deserts 27/05/2021
        //    //MenuId for Priority =100, Dessert = 11

        //    //if menuid = 11 fetch items table wise and not cumulative
        //    List<OrderPart> orderedItems = new List<OrderPart>();
        //    if (menuIds.Count == 1 && menuIds[0] == 11)
        //    {
        //        orderedItems = (from a in _context.tblMenuItems
        //                        join b in _context.tblOrderBuffetItems on a.ProductID equals b.ProductId
        //                        join c in _context.tblProducts on a.ProductID equals c.ProductID
        //                        //where  a.MenuID == menuId &&
        //                        where menuIds.Contains(a.MenuID) &&
        //                         DbFunctions.TruncateTime(b.DateCreated) == DbFunctions.TruncateTime(DateTime.Now)
        //                        group b by new { b.ProductId, b.BatchNo, c.Description, c.ChineseName, b.BatchTime, b.Processed, a.MenuID, b.OrderGUID } into g
        //                        select new OrderPart
        //                        {
        //                            Name = g.Key.Description,
        //                            Qty = (short)g.Count(),
        //                            BatchNo = (int)g.Key.BatchNo,
        //                            BatchTime = g.Key.BatchTime,
        //                            Processed = g.Key.Processed,
        //                            MenuId = g.Key.MenuID,
        //                            ChineseName = g.Key.ChineseName,
        //                            ProductID = g.Key.ProductId,
        //                            OrderGUID = g.Key.OrderGUID
        //                        }).ToList();

        //    }
        //    else if (menuIds.Count == 1 && menuIds[0] == 100)
        //    {
        //        orderedItems = (from a in _context.tblMenuItems
        //                        join b in _context.tblOrderBuffetItems on a.ProductID equals b.ProductId
        //                        join c in _context.tblProducts on a.ProductID equals c.ProductID
        //                        //where  a.MenuID == menuId &&
        //                        where a.MenuID != 11 && a.Priority == true &&
        //                         DbFunctions.TruncateTime(b.DateCreated) == DbFunctions.TruncateTime(DateTime.Now)
        //                        group b by new { b.ProductId, b.BatchNo, c.Description, c.ChineseName, b.BatchTime, b.Processed, a.MenuID } into g
        //                        select new OrderPart
        //                        {
        //                            Name = g.Key.Description,
        //                            Qty = (short)g.Count(),
        //                            BatchNo = (int)g.Key.BatchNo,
        //                            BatchTime = g.Key.BatchTime,
        //                            Processed = g.Key.Processed,
        //                            MenuId = g.Key.MenuID,
        //                            ChineseName = g.Key.ChineseName,
        //                            ProductID = g.Key.ProductId

        //                        }).ToList();

        //    }
        //    else
        //    {
        //        orderedItems = (from a in _context.tblMenuItems
        //                        join b in _context.tblOrderBuffetItems on a.ProductID equals b.ProductId
        //                        join c in _context.tblProducts on a.ProductID equals c.ProductID
        //                        //where  a.MenuID == menuId &&
        //                        where menuIds.Contains(a.MenuID) && a.Priority == false &&
        //                         DbFunctions.TruncateTime(b.DateCreated) == DbFunctions.TruncateTime(DateTime.Now)
        //                        group b by new { b.ProductId, b.BatchNo, c.Description, c.ChineseName, b.BatchTime, b.Processed, a.MenuID } into g
        //                        select new OrderPart
        //                        {
        //                            Name = g.Key.Description,
        //                            Qty = (short)g.Count(),
        //                            BatchNo = (int)g.Key.BatchNo,
        //                            BatchTime = g.Key.BatchTime,
        //                            Processed = g.Key.Processed,
        //                            MenuId = g.Key.MenuID,
        //                            ChineseName = g.Key.ChineseName,
        //                            ProductID = g.Key.ProductId,

        //                        }).ToList();
        //    }

        //    if (orderedItems != null && orderedItems.Count > 0)
        //    {
        //        int batchNo = 1;
        //        var olItems = orderedItems.Where(x => x.BatchNo > 0).ToList();
        //        if (olItems.Count > 0)
        //        {
        //            //olItems = olItems.OrderByDescending(x => x.BatchNo).ToList();

        //            if (Convert.ToInt32(MenuId) == 11)
        //            {
        //                var uniqueBatchNo = olItems.OrderByDescending(x => x.BatchNo).Select(x => x.BatchNo).Distinct();
        //                foreach (var bn in uniqueBatchNo)
        //                {
        //                    var pl = olItems.Where(x => x.BatchNo == bn).ToList();
        //                    OrderMenuItemsResponse omir = new OrderMenuItemsResponse();
        //                    omir.BatchNo = (int)bn;
        //                    //DateTime ot = pl.OrderBy(x => x.DateCreated).Select(x => x.DateCreated).FirstOrDefault();
        //                    omir.OrderTime = pl.Select(x => x.BatchTime).FirstOrDefault();
        //                    omir.Processed = pl.Select(x => x.Processed).FirstOrDefault();
        //                    var uniqueMenuId = olItems.Select(x => x.MenuId).Distinct();
        //                    foreach (var m in uniqueMenuId)
        //                    {
        //                        omir.MenuItems.Add(pl.Where(x => x.MenuId == m).ToList());
        //                    }
        //                    //omir.MenuItems = pl;
        //                    omr.Add(omir);
        //                }
        //                batchNo = batchNo + uniqueBatchNo.FirstOrDefault();
        //            }
        //            else
        //            {
        //                var uniqueBatchNo = olItems.OrderByDescending(x => x.BatchNo).Select(x => x.BatchNo).Distinct();
        //                foreach (var bn in uniqueBatchNo)
        //                {
        //                    var pl = olItems.Where(x => x.BatchNo == bn).ToList();
        //                    OrderMenuItemsResponse omir = new OrderMenuItemsResponse();
        //                    omir.BatchNo = (int)bn;
        //                    //DateTime ot = pl.OrderBy(x => x.DateCreated).Select(x => x.DateCreated).FirstOrDefault();
        //                    omir.OrderTime = pl.Select(x => x.BatchTime).FirstOrDefault();
        //                    omir.Processed = pl.Select(x => x.Processed).FirstOrDefault();
        //                    var uniqueMenuId = olItems.Select(x => x.MenuId).Distinct();
        //                    foreach (var m in uniqueMenuId)
        //                    {
        //                        omir.MenuItems.Add(pl.Where(x => x.MenuId == m).ToList());
        //                    }
        //                    //omir.MenuItems = pl;
        //                    omr.Add(omir);
        //                }
        //                batchNo = batchNo + uniqueBatchNo.FirstOrDefault();
        //            }
        //        }

        //        var nwItems = orderedItems.Where(x => x.BatchNo == 0).ToList();
        //        if (nwItems.Count > 0)
        //        //get any new items inserted for this cycle

        //        {
        //            if (Convert.ToInt32(MenuId) == 11)
        //            {
        //                //update batch no for new items 
        //                string batchTime = DateTime.Now.ToString("HH:mm");
        //                nwItems.ForEach(x => x.BatchNo = batchNo);
        //                var orderIds = nwItems.Select(x => x.OrderGUID).Distinct();
        //                foreach (var m in orderIds)
        //                {
        //                    var mi = nwItems.Where(x => x.OrderGUID == m).ToList();
        //                    //var mis = rh.SplitList(mi, maxItems);
        //                        OrderMenuItemsResponse omir = new OrderMenuItemsResponse();
        //                        omir.OrderTime = batchTime;
        //                        omir.BatchNo = batchNo;
        //                        omir.MenuItems.Add(mi);
        //                        omir.Processed = false;
        //                        omr.Add(omir);
        //                        foreach (var misit in mi)
        //                        {
        //                            foreach (var some in _context.tblOrderBuffetItems.Where(x => x.ProductId == misit.ProductID && x.BatchNo == 0 && x.OrderGUID == m).ToList())
        //                            {
        //                                some.BatchNo = batchNo;
        //                                some.BatchTime = batchTime;
        //                            }
        //                            _context.SaveChanges();
        //                        }
        //                        batchNo++;
        //                }
        //            }
        //            else
        //            {


        //                //update batch no for new items 
        //                string batchTime = DateTime.Now.ToString("HH:mm");
        //                nwItems.ForEach(x => x.BatchNo = batchNo);
        //                var uniqueMenuId = nwItems.Select(x => x.MenuId).Distinct();
        //                foreach (var m in uniqueMenuId)
        //                {
        //                    var mi = nwItems.Where(x => x.MenuId == m).ToList();
        //                    var mis = rh.SplitList(mi, maxItems);
        //                    foreach (var misi in mis)
        //                    {
        //                        OrderMenuItemsResponse omir = new OrderMenuItemsResponse();
        //                        omir.OrderTime = batchTime;
        //                        omir.BatchNo = batchNo;
        //                        omir.MenuItems.Add(misi);
        //                        omir.Processed = false;
        //                        omr.Add(omir);
        //                        foreach (var misit in misi)
        //                        {
        //                            foreach (var some in _context.tblOrderBuffetItems.Where(x => x.ProductId == misit.ProductID && x.BatchNo == 0).ToList())
        //                            {
        //                                some.BatchNo = batchNo;
        //                                some.BatchTime = batchTime;
        //                            }
        //                            _context.SaveChanges();
        //                        }
        //                        batchNo++;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    if (omr.Count > 0)
        //        omr = omr.OrderByDescending(x => x.OrderTime).ThenBy(x => x.BatchNo).ToList();

        //    return Json(omr, JsonRequestBehavior.AllowGet);
        //}

        //new code to not generated batch numbers. They are generated at printing time
        //Kitchen station match the printers
        public ActionResult GetOrderedItemsbyMenu(string MenuId)
        {
            List<int> menuIds = MenuId.Split(',').Select(int.Parse).ToList();
            List<OrderMenuItemsResponse> omr = new List<OrderMenuItemsResponse>();
            List<OrderPart> allOrderedItems = new List<OrderPart>();
            //int mId = Convert.ToInt32(MenuId);
            using (var _context = new ChineseTillEntities1())
            {
                foreach (var mId in menuIds)
                {
                    List<OrderPart> orderedItems = new List<OrderPart>();
                    if (mId == 11 || mId == 3) //Starters or Desserts
                    {
                        //orderedItems = (from a in _context.tblMenuItems
                        //                join b in _context.tblOrderBuffetItems on a.ProductID equals b.ProductId
                        //                join c in _context.tblProducts on a.ProductID equals c.ProductID
                        //                join d in _context.tblTableOrders on b.OrderGUID equals d.OrderGUID
                        //                join e in _context.tblTables on d.TableId equals e.TableID
                        //                where a.MenuID == mId && a.Priority == false && b.Printed == true &&
                        //                DbFunctions.TruncateTime(b.DateCreated) == DbFunctions.TruncateTime(DateTime.Now)
                        //                group b by new { b.ProductId, b.BatchNo, c.Description, c.ChineseName, b.BatchTime, b.Processed, a.MenuID, e.TableNumber } into g
                        //                select new OrderPart
                        //                {
                        //                    Name = g.Key.Description,
                        //                    Qty = (short)g.Count(),
                        //                    BatchNo = (int)g.Key.BatchNo,
                        //                    BatchTime = g.Key.BatchTime,
                        //                    Processed = g.Key.Processed,
                        //                    MenuId = g.Key.MenuID,
                        //                    ChineseName = g.Key.ChineseName,
                        //                    ProductID = g.Key.ProductId,
                        //                    TableNumber = g.Key.TableNumber
                        //                }).ToList();

                        orderedItems = (from a in _context.tblMenuItems
                                        join b in _context.tblPrintBuffetItems on a.ProductID equals (int)b.ProductId
                                        join c in _context.tblProducts on a.ProductID equals c.ProductID
                                        join d in _context.tblTableOrders on b.OrderGUID equals d.OrderGUID
                                        join e in _context.tblTables on d.TableId equals e.TableID
                                        where a.MenuID == mId && a.Priority == false && b.Printed == true &&
                                        DbFunctions.TruncateTime(b.DateOrdered) == DbFunctions.TruncateTime(DateTime.Now)
                                        group b by new { b.ProductId, b.BatchNo, c.Description, c.ChineseName, b.BatchTime, b.Processed, a.MenuID, e.TableNumber } into g
                                        select new OrderPart
                                        {
                                            Name = g.Key.Description,
                                            Qty = (short)g.Count(),
                                            BatchNo = (int)g.Key.BatchNo,
                                            BatchTime = g.Key.BatchTime,
                                            Processed = g.Key.Processed,
                                            MenuId = g.Key.MenuID,
                                            ChineseName = g.Key.ChineseName,
                                            ProductID = g.Key.ProductId,
                                            TableNumber = g.Key.TableNumber
                                        }).ToList();

                    }
                    else if (mId == 1) //MC
                    {

                        //orderedItems = (from a in _context.tblMenuItems
                        //                join b in _context.tblOrderBuffetItems on a.ProductID equals b.ProductId
                        //                join c in _context.tblProducts on a.ProductID equals c.ProductID
                        //                join d in _context.tblTableOrders on b.OrderGUID equals d.OrderGUID
                        //                join e in _context.tblTables on d.TableId equals e.TableID
                        //                where a.MenuID != 3 && a.MenuID != 11 && a.Priority == false && b.Printed == true &&
                        //                 //where a.Priority == false &&
                        //                 DbFunctions.TruncateTime(b.DateCreated) == DbFunctions.TruncateTime(DateTime.Now)
                        //                group b by new { b.ProductId, b.BatchNo, c.Description, c.ChineseName, b.BatchTime, b.Processed, a.MenuID, e.TableNumber } into g
                        //                select new OrderPart
                        //                {
                        //                    Name = g.Key.Description,
                        //                    Qty = (short)g.Count(),
                        //                    BatchNo = (int)g.Key.BatchNo,
                        //                    BatchTime = g.Key.BatchTime,
                        //                    Processed = g.Key.Processed,
                        //                    MenuId = g.Key.MenuID,
                        //                    ChineseName = g.Key.ChineseName,
                        //                    ProductID = g.Key.ProductId,
                        //                    TableNumber = g.Key.TableNumber
                        //                }).ToList();
                        orderedItems = (from a in _context.tblMenuItems
                                        join b in _context.tblPrintBuffetItems on a.ProductID equals (int)b.ProductId
                                        join c in _context.tblProducts on a.ProductID equals c.ProductID
                                        join d in _context.tblTableOrders on b.OrderGUID equals d.OrderGUID
                                        join e in _context.tblTables on d.TableId equals e.TableID
                                        where a.MenuID != 3 && a.MenuID != 11 && a.Priority == false && b.Printed == true &&
                                         //where a.Priority == false &&
                                         DbFunctions.TruncateTime(b.DateOrdered) == DbFunctions.TruncateTime(DateTime.Now)
                                        group b by new { b.ProductId, b.BatchNo, c.Description, c.ChineseName, b.BatchTime, b.Processed, a.MenuID, e.TableNumber } into g
                                        select new OrderPart
                                        {
                                            Name = g.Key.Description,
                                            Qty = (short)g.Count(),
                                            BatchNo = (int)g.Key.BatchNo,
                                            BatchTime = g.Key.BatchTime,
                                            Processed = g.Key.Processed,
                                            MenuId = g.Key.MenuID,
                                            ChineseName = g.Key.ChineseName,
                                            ProductID = g.Key.ProductId,
                                            TableNumber = g.Key.TableNumber
                                        }).ToList();

                    }
                    else if (mId == 100) //Priority
                    {
                        //orderedItems = (from a in _context.tblMenuItems
                        //                join b in _context.tblOrderBuffetItems on a.ProductID equals b.ProductId
                        //                join c in _context.tblProducts on a.ProductID equals c.ProductID
                        //                join d in _context.tblTableOrders on b.OrderGUID equals d.OrderGUID
                        //                join e in _context.tblTables on d.TableId equals e.TableID
                        //                where a.Priority == true && b.Printed == true &&
                        //                DbFunctions.TruncateTime(b.DateCreated) == DbFunctions.TruncateTime(DateTime.Now)
                        //                group b by new { b.ProductId, b.BatchNo, c.Description, c.ChineseName, b.BatchTime, b.Processed, a.MenuID, e.TableNumber } into g
                        //                select new OrderPart
                        //                {
                        //                    Name = g.Key.Description,
                        //                    Qty = (short)g.Count(),
                        //                    BatchNo = (int)g.Key.BatchNo,
                        //                    BatchTime = g.Key.BatchTime,
                        //                    Processed = g.Key.Processed,
                        //                    MenuId = g.Key.MenuID,
                        //                    ChineseName = g.Key.ChineseName,
                        //                    ProductID = g.Key.ProductId,
                        //                    TableNumber = g.Key.TableNumber
                        //                }).ToList();
                        orderedItems = (from a in _context.tblMenuItems
                                        join b in _context.tblPrintBuffetItems on a.ProductID equals (int)b.ProductId
                                        join c in _context.tblProducts on a.ProductID equals c.ProductID
                                        join d in _context.tblTableOrders on b.OrderGUID equals d.OrderGUID
                                        join e in _context.tblTables on d.TableId equals e.TableID
                                        where a.Priority == true && b.Printed == true &&
                                        DbFunctions.TruncateTime(b.DateOrdered) == DbFunctions.TruncateTime(DateTime.Now)
                                        group b by new { b.ProductId, b.BatchNo, c.Description, c.ChineseName, b.BatchTime, b.Processed, a.MenuID, e.TableNumber } into g
                                        select new OrderPart
                                        {
                                            Name = g.Key.Description,
                                            Qty = (short)g.Count(),
                                            BatchNo = (int)g.Key.BatchNo,
                                            BatchTime = g.Key.BatchTime,
                                            Processed = g.Key.Processed,
                                            MenuId = g.Key.MenuID,
                                            ChineseName = g.Key.ChineseName,
                                            ProductID = g.Key.ProductId,
                                            TableNumber = g.Key.TableNumber
                                        }).ToList();
                    }
                    else if (mId == 200) //Support Station
                    {
                        //orderedItems = (from a in _context.tblMenuItems
                        //                join b in _context.tblOrderBuffetItems on a.ProductID equals b.ProductId
                        //                join c in _context.tblProducts on a.ProductID equals c.ProductID
                        //                join d in _context.tblTableOrders on b.OrderGUID equals d.OrderGUID
                        //                join e in _context.tblTables on d.TableId equals e.TableID
                        //                where a.SupportItem == true && b.Printed == true &&
                        //                DbFunctions.TruncateTime(b.DateCreated) == DbFunctions.TruncateTime(DateTime.Now)
                        //                group b by new { b.ProductId, b.BatchNo, c.Description, c.ChineseName, b.BatchTime, b.Processed, a.MenuID, e.TableNumber } into g
                        //                select new OrderPart
                        //                {
                        //                    Name = g.Key.Description,
                        //                    Qty = (short)g.Count(),
                        //                    BatchNo = (int)g.Key.BatchNo,
                        //                    BatchTime = g.Key.BatchTime,
                        //                    Processed = g.Key.Processed,
                        //                    MenuId = g.Key.MenuID,
                        //                    ChineseName = g.Key.ChineseName,
                        //                    ProductID = g.Key.ProductId,
                        //                    TableNumber = g.Key.TableNumber
                        //                }).ToList();
                        orderedItems = (from a in _context.tblMenuItems
                                        join b in _context.tblPrintBuffetItems on a.ProductID equals (int)b.ProductId
                                        join c in _context.tblProducts on a.ProductID equals c.ProductID
                                        join d in _context.tblTableOrders on b.OrderGUID equals d.OrderGUID
                                        join e in _context.tblTables on d.TableId equals e.TableID
                                        where a.SupportItem == true && b.Printed == true &&
                                        DbFunctions.TruncateTime(b.DateOrdered) == DbFunctions.TruncateTime(DateTime.Now)
                                        group b by new { b.ProductId, b.BatchNo, c.Description, c.ChineseName, b.BatchTime, b.Processed, a.MenuID, e.TableNumber } into g
                                        select new OrderPart
                                        {
                                            Name = g.Key.Description,
                                            Qty = (short)g.Count(),
                                            BatchNo = (int)g.Key.BatchNo,
                                            BatchTime = g.Key.BatchTime,
                                            Processed = g.Key.Processed,
                                            MenuId = g.Key.MenuID,
                                            ChineseName = g.Key.ChineseName,
                                            ProductID = g.Key.ProductId,
                                            TableNumber = g.Key.TableNumber
                                        }).ToList();
                    }
                    //allOrderedItems.AddRange(orderedItems);
                    var uniqueBatchNo = orderedItems.Select(x => x.BatchNo).Distinct();

                    foreach (var b in uniqueBatchNo)
                    {
                        //var oi = orderedItems.Where(x => x.BatchNo == b).ToList();
                        var oi = orderedItems.Where(x => x.BatchNo == b).ToList();
                        OrderMenuItemsResponse omir = new OrderMenuItemsResponse();
                        omir.BatchNo = oi.Select(x => x.BatchNo).FirstOrDefault(); ;
                        omir.OrderTime = oi.Select(x => x.BatchTime).FirstOrDefault();
                        omir.Processed = oi.Select(x => x.Processed).FirstOrDefault();
                        omir.TableNumber = oi.Select(x => x.TableNumber).FirstOrDefault();
                        omir.MenuItems.Add(oi);
                        omr.Add(omir);
                    }
                }



            }
            //var uniqueBatchNo = orderedItems.Select(x => x.BatchNo).Distinct();
            //var uniqueBatchNo = allOrderedItems.Select(x => x.BatchNo).Distinct();

            //foreach (var b in uniqueBatchNo)
            //{
            //    //var oi = orderedItems.Where(x => x.BatchNo == b).ToList();
            //    var oi = allOrderedItems.Where(x => x.BatchNo == b).ToList();
            //    OrderMenuItemsResponse omir = new OrderMenuItemsResponse();
            //    omir.BatchNo = oi.Select(x => x.BatchNo).FirstOrDefault(); ;
            //    omir.OrderTime = oi.Select(x => x.BatchTime).FirstOrDefault();
            //    omir.Processed = oi.Select(x => x.Processed).FirstOrDefault();
            //    omir.TableNumber = oi.Select(x => x.TableNumber).FirstOrDefault();
            //    omir.MenuItems.Add(oi);
            //    omr.Add(omir);
            //}
            if (omr.Count > 0)
                omr = omr.OrderByDescending(x => x.OrderTime).ThenBy(x => x.BatchNo).ToList();
            return Json(omr, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SubmitOrder(TableOrder t)
        {
            Models.OrderService os = new Models.OrderService();
            OrderSubmitResponse r = os.SubmitOrderV3(t);
            return Json(r, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateReservation(Reservation req)
        {
            string response = "";
            try
            {
                using (var context = new ChineseTillEntities1())
                {
                    var reservation = context.tblReservations.Where(x => x.UniqueCode == req.UniqueCode && x.ContactNumber.Contains(req.MobileNumber)).FirstOrDefault();
                    if (reservation != null)
                    {
                        reservation.ReservationDate = (DateTime)req.ReservationDate;
                        reservation.ReservationTime = Convert.ToInt32(req.ReservationDate.Value.ToString("HHmm"));
                        reservation.ReservationNotes += req.AdditionalNotes;
                        context.Entry(reservation).State = EntityState.Modified;
                        context.SaveChanges();
                        response = "success";

                        //Print updated reservation slip if within 24 hours
                        if ((reservation.ReservationDate.Date - DateTime.Now).TotalHours <= 24)
                        {
                            string resTicket = "";
                            resTicket += "Online Reservation - Update" + Environment.NewLine;
                            resTicket += "Surname: " + reservation.ReservationUnder + Environment.NewLine;
                            resTicket += "Date: " + reservation.ReservationDate.ToString("dd/MM/yyyy HH:mm") + Environment.NewLine;
                            resTicket += "Guests: " + reservation.NoOfGuests;

                            tblPrintQueue tpq = new tblPrintQueue();
                            tpq.ToPrinter = "Bar";
                            tpq.Receipt = resTicket;
                            tpq.UserFK = 0;
                            tpq.PCName = "WEB SITE";
                            tpq.DateCreated = DateTime.Now;
                            context.tblPrintQueues.Add(tpq);
                            context.SaveChanges();
                        }
                    }
                    else
                        response = "Code Not found";
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

        //public ActionResult ConfirmTablePayment(Payment req)
        //{
        //    //string response = "";
        //    Guid orderId = new Guid();
        //    orderId = req.OrderGUID;
        //    PaymentResponse pr = new PaymentResponse();
        //    pr.IsSuccess = false;
        //    bool giftVouchersBought = false;
        //    try
        //    {
        //        using (TransactionScope scope = new TransactionScope())

        //        {
        //            using (var dbContext = new ChineseTillEntities1())
        //            {
        //                var to = dbContext.tblOrders.Where(x => x.OrderGUID == req.OrderGUID).FirstOrDefault();
        //                var totalCount = 0;

        //                foreach (var item in req.SplitProduct)
        //                {
        //                    totalCount += item.ProductQty;
        //                }
        //                var orderPartCount = dbContext.tblOrderParts.Where(x => x.OrderGUID == req.OrderGUID && x.DelInd == false).Sum(x => x.Qty);
        //                //if (req.SplitProduct != null && req.SplitProduct.Count > 0)
        //                if (orderPartCount > totalCount)
        //                {
        //                    //Create new order for Split Products. Move selected items to new orderid
        //                    tblOrder tor = new tblOrder();
        //                    tor.OrderGUID = Guid.NewGuid();
        //                    tor.DateCreated = DateTime.Now;
        //                    tor.TabID = 99;
        //                    tor.TableID = to.TableID;
        //                    tor.TakeAway = false;
        //                    tor.UserID = -10;
        //                    tor.Paid = true;
        //                    tor.DatePaid = DateTime.Now;
        //                    tor.PaymentMethod = "MP";
        //                    tor.TotalPaid = (decimal)req.Amount;
        //                    tor.GrandTotal = (decimal)req.Amount;
        //                    tor.TipAmount = (decimal)req.TipAmount;
        //                    tor.DateCreated = DateTime.Now;
        //                    tor.LastModified = DateTime.Now;
        //                    tor.CustomerId = req.CustomerId;
        //                    tor.ServiceCharge = req.ServiceCharge;
        //                    dbContext.tblOrders.Add(tor);
        //                    dbContext.SaveChanges();

        //                    orderId = tor.OrderGUID;
        //                    //Move selected products to this new order
        //                    foreach (var item in req.SplitProduct)
        //                    {
        //                        for (int i = 0; i < item.ProductQty; i++)
        //                        {
        //                            var op = dbContext.tblOrderParts.Where(x => x.OrderGUID == req.OrderGUID && x.ProductID == item.ProductID && x.DelInd == false).FirstOrDefault();
        //                            op.OrderGUID = orderId;
        //                            op.Paid = true;
        //                            dbContext.Entry(op).State = EntityState.Modified;
        //                            dbContext.SaveChanges();
        //                        }
        //                    }
        //                    to.SplitBill = true;
        //                    to.LockedForPayment = false;
        //                    dbContext.Entry(to).State = EntityState.Modified;
        //                    dbContext.SaveChanges();
        //                }
        //                else
        //                {
        //                    //var to = dbContext.tblOrders.Where(x => x.OrderGUID == req.OrderGUID).FirstOrDefault();
        //                    pr.Message = "Updating Order";

        //                    //rh.WriteToFile("testing Elmah");
        //                    to.Paid = true;
        //                    to.DatePaid = DateTime.Now;
        //                    to.PaymentMethod = "MP";
        //                    to.TotalPaid = (decimal)req.Amount;
        //                    to.GrandTotal = (decimal)req.Amount;
        //                    to.TipAmount = (decimal)req.TipAmount;
        //                    to.ServiceCharge = (decimal)req.ServiceCharge;
        //                    to.LockedForPayment = false;
        //                    dbContext.Entry(to).State = EntityState.Modified;
        //                    dbContext.SaveChanges();
        //                    var tblOrd = dbContext.tblTableOrders.Where(x => x.OrderGUID == req.OrderGUID).FirstOrDefault();
        //                    tblOrd.Active = false;
        //                    dbContext.Entry(tblOrd).State = EntityState.Modified;
        //                    dbContext.SaveChanges();
        //                    dbContext.usp_AN_SetTableCleaning(req.OrderGUID, 11);

        //                }
        //                //get the PCName from web config. All MP will be assigned to primary TILL. 
        //                //If primary TILL value is empty use last paid TILL
        //                string primaryTILL = "";
        //                primaryTILL = Convert.ToString(ConfigurationManager.AppSettings["PrimaryTILL"]);
        //                var top1 = dbContext.tblOrderPayments.Where(x => x.PCName != "" || x.PCName != null).OrderByDescending(x => x.DateCreated).FirstOrDefault();
        //                if (primaryTILL == "")
        //                    primaryTILL = top1.PCName;
        //                tblOrderPayment topy = new tblOrderPayment();
        //                topy.OrderGUID = orderId;
        //                topy.PaymentGUID = Guid.NewGuid();
        //                topy.DateCreated = DateTime.Now;
        //                topy.LastModified = DateTime.Now;
        //                topy.PaymentValue = (decimal)req.Amount;
        //                topy.TipAmount = (decimal)req.TipAmount;
        //                topy.ServiceCharge = (decimal)req.ServiceCharge;
        //                topy.PaymentType = "MP";
        //                topy.PCName = primaryTILL;
        //                dbContext.tblOrderPayments.Add(topy);
        //                dbContext.SaveChanges();


        //                //Update Activity
        //                string activityType = ActivityType.DineIn.ToString();

        //                //Update record in tblStripePayment
        //                var tsp = dbContext.tblStripePayments.Where(x => x.VendorTxCode == req.TxCode).FirstOrDefault();
        //                tsp.Success = true;
        //                tsp.LastModified = DateTime.Now;
        //                tsp.PaymentId = req.PaymentId;
        //                dbContext.Entry(tsp).State = EntityState.Modified;
        //                dbContext.SaveChanges();

        //                Entity.Customer cust = cs.GetRewardPointsForOrder(orderId, req.Amount, false);
        //                //tblCustomerActivity tca = new tblCustomerActivity();
        //                //tca.RewardPoints = cust.OrderPoints;
        //                //tca.Mobile = req.Mobile;
        //                //tca.ActivityType = activityType;
        //                //tca.FullName = req.FullName;
        //                //tca.OrderGUID = orderId;
        //                //cs.UpdateCustomerActivity(tca);
        //                //Print Payment Confirmation slip for the Order
        //                string confirmationStr = "";
        //                confirmationStr += "       " + DateTime.Now.ToString("dd-MM-yyyy") + Environment.NewLine;
        //                confirmationStr += "Confirmation payment of " + Environment.NewLine;
        //                confirmationStr += "£" + String.Format("{0:0.00}", req.Amount) + " for Table-" + req.OrderNo + Environment.NewLine;
        //                confirmationStr += Environment.NewLine;
        //                confirmationStr += "Thank you for your payment." + Environment.NewLine;
        //                confirmationStr += "Please give this confirmation" + Environment.NewLine;
        //                confirmationStr += "slip to the cashier " + Environment.NewLine;
        //                confirmationStr += "on your way out." + Environment.NewLine;
        //                //confirmationStr += "        Table - " + req.OrderNo + Environment.NewLine;
        //                //confirmationStr += "        Received with Thanks £" + Convert.ToString(req.Amount) + Environment.NewLine;
        //                confirmationStr += "-------------------------------------------------" + Environment.NewLine;
        //                confirmationStr += "We hope you enjoyed your " + Environment.NewLine;
        //                confirmationStr += "visit with us";
        //                tblPrintQueue tp = new tblPrintQueue();
        //                tp.Receipt = confirmationStr;
        //                tp.PCName = "App";
        //                tp.ToPrinter = "Bar";
        //                tp.UserFK = -10;
        //                tp.DateCreated = DateTime.Now;
        //                dbContext.tblPrintQueues.Add(tp);
        //                dbContext.SaveChanges();



        //                //Check if any vouchers bought with this order
        //                var giftVoucherProducts = (from a in dbContext.tblOrderParts
        //                                           join b in dbContext.tblProducts on a.ProductID equals b.ProductID
        //                                           where a.OrderGUID == req.OrderGUID && b.ProductGroupID == 17
        //                                           select new Entity.Product
        //                                           {
        //                                               ProductID = a.ProductID,
        //                                               Price = (float)b.Price,
        //                                               Description = b.Description
        //                                           }).ToList();
        //                if (giftVoucherProducts != null && giftVoucherProducts.Count > 0)
        //                {
        //                    giftVouchersBought = true;
        //                    List<PromotionDto> promList = new List<PromotionDto>();
        //                    // create unique code and send to customer
        //                    foreach (var item in giftVoucherProducts)
        //                    {
        //                        PromotionDto pd = new PromotionDto();
        //                        pd.MobileNo = req.Mobile;
        //                        pd.PromoCode = item.Price.ToString() + "xphau";
        //                        promList.Add(pd);
        //                    }
        //                    var myContent = JsonConvert.SerializeObject(promList);
        //                    var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
        //                    var byteContent = new ByteArrayContent(buffer);
        //                    byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        //                    using (var client = new HttpClient())
        //                    {
        //                        client.BaseAddress = new Uri(url);
        //                        var responseTask = client.PostAsync("v2/AvailPromotions", byteContent).Result;
        //                        //var responseTask = client.GetAsync("v1/CustomerRegistration" + cusId + "&redPoints=" + redPoints + "&earnPoints=" + earnPoints + "&resId=" + resId + "&orderId=" + orderid.ToString() + "&activityType=DineIn");
        //                        //responseTask.Wait();
        //                        //var result = responseTask.Result;
        //                        if (responseTask.IsSuccessStatusCode)
        //                        {
        //                            var readTask = responseTask.Content.ReadAsAsync<string>();
        //                            readTask.Wait();
        //                            //giftVoucherresponse = readTask.Result;
        //                        }
        //                        else //web api sent error response 
        //                        {
        //                            //giftVoucherresponse = "failure";
        //                            //log response status here..
        //                        }
        //                    }
        //                }
        //                scope.Complete();
        //                pr.IsSuccess = true;
        //                pr.Mobile = req.Mobile;
        //                pr.OrderGUID = orderId;
        //                pr.Points = cust.OrderPoints;
        //                pr.Message = "<h2 style=\"color:green;text-align:center;\">Payment Confirmation</h2> <p>Thank you for your payment. Our staff will bring you your confirmation slip.</p>";
        //                if (cust.OrderPoints > 0)
        //                    pr.Message += "<br/><h2 style=\"color:darkred\";\"text-align:center\";>Congratulations</h2> <p>you have just earned " + cust.OrderPoints + " reward points for this order and will be linked to below mobile number. You may redeem them in your next order with us.</p>";

        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //response = ex.Message;
        //        //if(ex.InnerException != null)
        //        //response = ex.InnerException.StackTrace;
        //        pr.Message = "Thank you for the payment. Your table payment is not updated on our TILL. Kindly show this message to manager. We apologise the inconvenience caused. Thanks.";
        //        //throw;
        //    }
        //    if (giftVouchersBought)
        //        pr.Message += "<br/><p>Your voucher codes will be sent to your mobile shortly.</p>";
        //    return Json(pr, JsonRequestBehavior.AllowGet);
        //}
        public ActionResult ConfirmTablePayment(Payment req)
        {
            //string response = "";
            Guid orderId = new Guid();
            orderId = req.OrderGUID;
            PaymentResponse pr = new PaymentResponse();
            pr.IsSuccess = false;
            bool giftVouchersBought = false;
            try
            {
                using (var dbContext = new ChineseTillEntities1())
                {
                    var to = dbContext.tblOrders.Where(x => x.OrderGUID == req.OrderGUID).FirstOrDefault();
                    var totalCount = 0;

                    foreach (var item in req.SplitProduct)
                    {
                        totalCount += item.ProductQty;
                    }
                    var orderPartCount = dbContext.tblOrderParts.Where(x => x.OrderGUID == req.OrderGUID && x.DelInd == false).Sum(x => x.Qty);
                    //if (req.SplitProduct != null && req.SplitProduct.Count > 0)
                    if (orderPartCount > totalCount)
                        req.isSplitPayment = true;

                    pr = rh.CompletePayment(req);

                    //Update Activity
                    string activityType = ActivityType.DineIn.ToString();
                }
            }
            catch (Exception ex)
            {
                //response = ex.Message;
                //if(ex.InnerException != null)
                //response = ex.InnerException.StackTrace;
                pr.Message = "Thank you for the payment. Your table payment is not updated on our TILL. Kindly show this message to manager. We apologise the inconvenience caused. Thanks.";
                //throw;
            }

            return Json(pr, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ConfirmSagePayment(Payment req)
        {
            //string response = "";
            Guid orderId = new Guid();
            orderId = req.OrderGUID;
            PaymentResponse pr = new PaymentResponse();
            pr.IsSuccess = req.IsSuccess;
            bool giftVouchersBought = false;
            try
            {
                using (var dbContext = new ChineseTillEntities1())
                {
                    //var to = dbContext.tblOrders.Where(x => x.OrderGUID == req.OrderGUID).FirstOrDefault();
                    // var totalCount = 0;

                    //foreach (var item in req.SplitProduct)
                    //{
                    //    totalCount += item.ProductQty;
                    //}
                    //var orderPartCount = dbContext.tblOrderParts.Where(x => x.OrderGUID == req.OrderGUID && x.DelInd == false).Sum(x => x.Qty);
                    //if (orderPartCount > totalCount)
                    //    req.isSplitPayment = true;
                    if (req.IsSuccess)
                    {
                        pr = rh.CompletePayment1(req);
                        //Update Activity
                        string activityType = ActivityType.DineIn.ToString();
                    }
                    else
                    {
                        using (TransactionScope scope = new TransactionScope())
                        {
                            //Print Payment Confirmation slip for the Order
                            var tsp = dbContext.tblSagePayments.Where(x => x.VendorTXCode == req.TxCode && x.OrderID != null && x.OrderID != Guid.Empty).FirstOrDefault();
                            if (tsp != null)
                            {
                                var tableNumber = (from a in dbContext.tblOrders
                                                   join b in dbContext.tblTables on a.TableID equals b.TableID
                                                   where a.OrderGUID == tsp.OrderID
                                                   select new
                                                   {
                                                       b.TableNumber
                                                   }).FirstOrDefault();
                                try
                                {
                                    if (req.FailureMessage != "ABORT")
                                    {
                                        string confirmationStr = "";
                                        confirmationStr += "       " + DateTime.Now.ToString("dd-MM-yyyy") + Environment.NewLine;
                                        confirmationStr += "Failed payment of " + Environment.NewLine;
                                        confirmationStr += "£" + String.Format("{0:0.00}", tsp.PaymentPrice) + " for Table-" + tableNumber.TableNumber + Environment.NewLine;
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
                                    }
                                    tsp.IsSuccess = false;
                                    tsp.StatusUpdated = DateTime.Now;
                                    tsp.SecurityKey = req.PaymentId;
                                    tsp.TransactionID = req.TransactionID;
                                    //tsp.OrderID = orderId;
                                    tsp.FaultReason = req.FailureMessage;
                                    dbContext.Entry(tsp).State = EntityState.Modified;
                                    dbContext.SaveChanges();
                                    scope.Complete();
                                    pr.Message = "Payment Failed for amount " + "£" + String.Format("{0:0.00}", tsp.PaymentPrice) + Environment.NewLine + req.FailureMessage;

                                }
                                catch (Exception ex)
                                {
                                    pr.Message = ex.Message;
                                    //response = ex.Message;
                                }
                            }
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                //response = ex.Message;
                //if(ex.InnerException != null)
                //response = ex.InnerException.StackTrace;
                pr.Message = "Thank you for the payment. Your table payment is not updated on our TILL. Kindly show this message to manager. We apologise the inconvenience caused. Thanks.";
                //throw;
            }

            return Json(pr, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ConfirmStripePayment(Payment req)
        {
            //string response = "";
            Guid orderId = new Guid();
            PaymentResponse pr = new PaymentResponse();
            pr.IsSuccess = req.IsSuccess;
            bool giftVouchersBought = false;
            try
            {
                using (var dbContext = new ChineseTillEntities1())
                {
                    //var to = dbContext.tblOrders.Where(x => x.OrderGUID == req.OrderGUID).FirstOrDefault();
                    // var totalCount = 0;

                    //foreach (var item in req.SplitProduct)
                    //{
                    //    totalCount += item.ProductQty;
                    //}
                    //var orderPartCount = dbContext.tblOrderParts.Where(x => x.OrderGUID == req.OrderGUID && x.DelInd == false).Sum(x => x.Qty);
                    //if (orderPartCount > totalCount)
                    //    req.isSplitPayment = true;
                    if (req.IsSuccess)
                    {
                        pr = rh.CompletePaymentStripe(req);
                        //Update Activity
                        string activityType = ActivityType.DineIn.ToString();
                    }
                    else
                    {
                        using (TransactionScope scope = new TransactionScope())
                        {
                            //Print Payment Confirmation slip for the Order
                            var tsp = dbContext.tblStripePayments.Where(x => x.VendorTxCode == req.TxCode && x.OrderGUID != null && x.OrderGUID != Guid.Empty).FirstOrDefault();
                            if (tsp != null)
                            {
                                var tableNumber = (from a in dbContext.tblOrders
                                                   join b in dbContext.tblTables on a.TableID equals b.TableID
                                                   where a.OrderGUID == tsp.OrderGUID
                                                   select new
                                                   {
                                                       b.TableNumber
                                                   }).FirstOrDefault();
                                try
                                {
                                    if (req.FailureMessage != "ABORT")
                                    {
                                        string confirmationStr = "";
                                        confirmationStr += "       " + DateTime.Now.ToString("dd-MM-yyyy") + Environment.NewLine;
                                        confirmationStr += "Failed payment of " + Environment.NewLine;
                                        confirmationStr += "£" + String.Format("{0:0.00}", tsp.Amount) + " for Table-" + tableNumber.TableNumber + Environment.NewLine;
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
                                    }
                                    tsp.Success = false;
                                    tsp.LastModified = DateTime.Now;
                                    tsp.PaymentId = req.PaymentId;
                                    tsp.VendorTxCode = req.TransactionID;
                                    //tsp.OrderID = orderId;
                                    tsp.FailureMessage = req.FailureMessage;
                                    dbContext.Entry(tsp).State = EntityState.Modified;
                                    dbContext.SaveChanges();
                                    scope.Complete();
                                    pr.Message = "Payment Failed for amount " + "£" + String.Format("{0:0.00}", tsp.Amount) + Environment.NewLine + req.FailureMessage;

                                }
                                catch (Exception ex)
                                {
                                    pr.Message = ex.Message;
                                    //response = ex.Message;
                                }
                            }
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                //response = ex.Message;
                //if(ex.InnerException != null)
                //response = ex.InnerException.StackTrace;
                pr.Message = "Thank you for the payment. Your table payment is not updated on our TILL. Kindly show this message to manager. We apologise the inconvenience caused. Thanks.";
                //throw;
            }

            return Json(pr, JsonRequestBehavior.AllowGet);
        }



        public ActionResult GetStripePaymentIntent(Payment req)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["TCBROMSConnection"].ConnectionString;
            string res = "";
            bool error = false;
            req.TablePaid = false;
            PaymentResponse prs = new PaymentResponse();
            decimal payableAmount = 0;
            using (var dbContext = new ChineseTillEntities1())
            {
                var order = dbContext.tblOrders.Where(x => x.OrderGUID == req.OrderGUID).FirstOrDefault();
                Models.OrderService os = new Models.OrderService();
                TableOrder to = os.GetOrderItems(req.OrderGUID);
                if(order.Paid == true)
                {
                    req.TablePaid = true;
                    req.FailureMessage = "Complete payment done for this order.";
                    error = true;
                }
                if (order.LockedForPayment)
                {
                    req.FailureMessage = "Another payment in progress. Please try again later";
                    error = true;
                }

                //if(req.DeviceType == "ios")
                //{
                //    req.FailureMessage = "We are unable to take payments on the APP at the moment. Please make your payment at the cashier. Thank you.";
                //    error = true;
                //}
                List<Entity.Product> cartItems = new List<Entity.Product>();
                if (!error)
                {
                    if (req.SplitProduct != null && req.SplitProduct.Count > 0)
                        cartItems = req.SplitProduct;
                    else
                        cartItems = req.OrderedProducts;
                    var uniqueProducts = (from a in cartItems
                                          group a by new { a.ProductID, a.Price } into g
                                          select new
                                          {
                                              ProductId = g.Key.ProductID,
                                              Price = g.Key.Price,
                                              Qty = g.Sum(a => a.ProductQty)
                                          }).ToList();
                    payableAmount = (decimal)cartItems.Sum(x => x.ProductQty * x.Price);
                    //foreach (var item in uniqueProducts)
                    //{
                    //    //var splitProduct = cartItems.Where(x => x.ProductID == item.Pr).ToList();
                    //    var pr = to.tableProducts.Where(x => x.ProductID == item.ProductId && x.Price == item.Price).FirstOrDefault();
                    //    if (pr == null || (pr != null && pr.ProductQty < item.Qty))
                    //    {
                    //        req.OrderedProducts = to.tableProducts;
                    //        error = true;
                    //        req.FailureMessage = "Some items have changed in your order. Kindly refresh";
                    //        break;
                    //    }
                    //    var orderedProducts = dbContext.tblOrderParts.Where(x => x.OrderGUID == req.OrderGUID && x.ProductID == item.ProductId && x.Price == (decimal)item.Price && x.Paid == false && x.DelInd == false).ToList();
                    //    if (orderedProducts != null && orderedProducts.Sum(x => x.Qty) < item.Qty)
                    //    {
                    //        req.FailureMessage = "Some items have been paid for this order. Kindly refresh";
                    //        error = true;
                    //        break;
                    //    }
                    //}
                    //var uniqueProductId = cartItems.Select(x => x.ProductID).Distinct().ToList();
                    //foreach (var item in uniqueProductId)
                    //{
                    //    var splitProduct = cartItems.Where(x => x.ProductID == item).ToList();
                    //    var pr = to.tableProducts.Where(x => x.ProductID == item).FirstOrDefault();
                    //    if (pr == null || (pr != null && pr.ProductQty < splitProduct.Sum(x => x.ProductQty)))
                    //    {
                    //        req.OrderedProducts = to.tableProducts;
                    //        error = true;
                    //        req.FailureMessage = "Some items have changed in your order. Kindly refresh";
                    //        break;
                    //    }
                    //    var orderedProducts = dbContext.tblOrderParts.Where(x => x.OrderGUID == req.OrderGUID && x.ProductID == item && x.Paid == false && x.DelInd == false).ToList();
                    //    if (orderedProducts != null && orderedProducts.Sum(x => x.Qty) < splitProduct.Sum(x => x.ProductQty))
                    //    {
                    //        req.FailureMessage = "Some items have been paid for this order. Kindly refresh";
                    //        error = true;
                    //        break;
                    //    }
                    //}
                }
                if (!error && req.SplitProduct != null && req.SplitProduct.Count > 0)
                {
                    
                    var uniqueProductId = req.SplitProduct.Select(x => x.ProductID).Distinct().ToList();
                    foreach (var item in uniqueProductId)
                    {
                        var splitProduct = req.SplitProduct.Where(x => x.ProductID == item).ToList();
                        var pr = to.tableProducts.Where(x => x.ProductID == item).FirstOrDefault();
                        if (pr == null || (pr != null && pr.ProductQty < splitProduct.Sum(x => x.ProductQty)))
                        {
                            req.OrderedProducts = to.tableProducts;
                            error = true;
                            req.FailureMessage = "Some items have changed in your order. Kindly refresh";
                            break;
                        }
                        var orderedProducts = dbContext.tblOrderParts.Where(x => x.OrderGUID == req.OrderGUID && x.ProductID == item && x.Paid == false && x.DelInd == false).ToList();
                        if (orderedProducts != null && orderedProducts.Sum(x => x.Qty) < splitProduct.Sum(x => x.ProductQty))
                        {
                            req.FailureMessage = "Some items have been paid for this order. Kindly refresh";
                            error = true;
                            break;
                        }
                    }
                }

                if (!error)
                {
                    //check if amount is all paid through voucher or promotions
                    //calculate total payable amount
                    string errorResponse = "";
                    if (req.Amount != (payableAmount * 100))
                        req.Amount = (payableAmount + req.TipAmount + req.ServiceCharge) * 100;
                    if (req.Amount <= 0)
                    {
                        req.Amount = 0;
                        prs = rh.CompletePayment(req);
                        req.ClientSecret = "";
                        req.FailureMessage = prs.Message;
                    }
                    else
                    {
                        order.LockedForPayment = true;
                        dbContext.Entry(order).State = EntityState.Modified;
                        dbContext.SaveChanges();
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
                            
                            //ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
                            req.FailureMessage = "";
                            var paymentIntent = service.Create(options);
                            req.ClientSecret = paymentIntent.ClientSecret;
                            req.TxCode = "1-" + Guid.NewGuid().ToString();
                            //Insert a record in tblStripPayment for recording purpose
                            tblStripePayment tsp = new tblStripePayment();
                            tsp.Amount = (decimal)req.Amount;
                            tsp.ClientSecret = req.ClientSecret;
                            tsp.DateCreated = DateTime.Now;
                            tsp.LastModified = DateTime.Now;
                            tsp.OrderGUID = req.OrderGUID;
                            tsp.Success = false;
                            tsp.VendorTxCode = req.TxCode;
                            tsp.DeviceType = req.DeviceType;
                            tsp.MobileNo = req.Mobile;
                            dbContext.tblStripePayments.Add(tsp);
                            dbContext.SaveChanges();
                        }

                        catch (Exception ex)
                        {
                            //req.FailureMessage = "We could not complete your payment request. Kindly pay at the TILL. Thanks";
                            req.FailureMessage = ex.StackTrace;
                            if (ex.InnerException != null)
                            {
                                req.FailureMessage += ex.InnerException.StackTrace;
                            }

                            req.ClientSecret = "";
                            //return Json(ex.Message, JsonRequestBehavior.AllowGet);
                            //}
                        }

                    }
                }
            }
            if (error)
                req.ClientSecret = "";
            return Json(req, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetSagePayTxCode(Payment req)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["TCBROMSConnection"].ConnectionString;
            string res = "";
            string vxcode = "1-" + Guid.NewGuid().ToString();
            bool error = false;
            bool splitPayment = false;
            req.TablePaid = false;
            PaymentResponse prs = new PaymentResponse();
            decimal payableAmount = 0;
            List<tblOrderPart> orderParts = new List<tblOrderPart>();
            List<int> unqueProductIds = new List<int>();
            using (var dbContext = new ChineseTillEntities1())
            {
                var order = dbContext.tblOrders.Where(x => x.OrderGUID == req.OrderGUID).FirstOrDefault();
                Models.OrderService os = new Models.OrderService();
                TableOrder to = os.GetOrderItems(req.OrderGUID);
                if (order.Paid == true)
                {
                    req.TablePaid = true;
                    req.FailureMessage = "Complete payment done for this order.";
                    error = true;
                }
                if (order.LockedForPayment)
                {
                    req.FailureMessage = "Another payment in progress. Please try again later";
                    error = true;
                }

                //if(req.DeviceType == "ios")
                //{
                //    req.FailureMessage = "We are unable to take payments on the APP at the moment. Please make your payment at the cashier. Thank you.";
                //    error = true;
                //}
                List<Entity.Product> cartItems = new List<Entity.Product>();
                if (!error)
                {
                    if (req.SplitProduct != null && req.SplitProduct.Count > 0)
                        cartItems = req.SplitProduct;
                    else
                        cartItems = req.OrderedProducts;
                    var uniqueProducts = (from a in cartItems
                                          group a by new { a.ProductID, a.Price } into g
                                          select new
                                          {
                                              ProductId = g.Key.ProductID,
                                              Price = g.Key.Price,
                                              Qty = g.Sum(a => a.ProductQty)
                                          }).ToList();
                    payableAmount = (decimal)cartItems.Sum(x => x.ProductQty * x.Price);
                    
                }
                unqueProductIds = cartItems.Select(x => x.ProductID).Distinct().ToList();
                orderParts = dbContext.tblOrderParts.Where(x => x.OrderGUID == req.OrderGUID && x.Paid == false && x.DelInd == false).ToList();
                if (!error && req.SplitProduct != null && req.SplitProduct.Count > 0)
                {
                    splitPayment = true;
                    var uniqueProductId = req.SplitProduct.Select(x => x.ProductID).Distinct().ToList();
                    foreach (var item in uniqueProductId)
                    {
                        var splitProduct = req.SplitProduct.Where(x => x.ProductID == item).ToList();
                        var pr = to.tableProducts.Where(x => x.ProductID == item).FirstOrDefault();
                        if (pr == null || (pr != null && pr.ProductQty < splitProduct.Sum(x => x.ProductQty)))
                        {
                            req.OrderedProducts = to.tableProducts;
                            error = true;
                            req.FailureMessage = "Some items have changed in your order. Kindly refresh";
                            break;
                        }

                        if (orderParts != null && orderParts.Sum(x => x.Qty) < splitProduct.Sum(x => x.ProductQty))
                        {
                            req.FailureMessage = "Some items have been paid for this order. Kindly refresh";
                            error = true;
                            break;
                        }
                        
                    }
                    
                }

                if (!error)
                {
                    //check if amount is all paid through voucher or promotions
                    //calculate total payable amount
                    string errorResponse = "";
                    if (req.Amount != (payableAmount))
                        req.Amount = (payableAmount + req.TipAmount + req.ServiceCharge);
                    if (req.Amount <= 0)
                    {
                        req.Amount = 0;
                        prs = rh.CompletePayment1(req);
                        req.ClientSecret = "";
                        req.FailureMessage = prs.Message;
                    }
                    else
                    {
                        //foreach (var item in cartItems)
                        //{

                        //}
                        //orderParts.ForEach(a => a.VendorTxCode = vxcode);
                        //dbContext.SaveChanges();
                        
                            foreach (var item in cartItems)
                            {
                                var oItem = dbContext.tblOrderParts.Where(x => x.OrderGUID == req.OrderGUID && x.ProductID == item.ProductID && (x.VendorTxCode == null || x.VendorTxCode == "")).FirstOrDefault();
                                oItem.VendorTxCode = vxcode;
                                dbContext.Entry(oItem).State = EntityState.Modified;
                                dbContext.SaveChanges();
                            }
                        order.LockedForPayment = true;
                  
                        dbContext.Entry(order).State = EntityState.Modified;
                        dbContext.SaveChanges();
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
                            //var service = new PaymentIntentService();

                            //ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
                            req.FailureMessage = "";
                            //var paymentIntent = service.Create(options);
                            //req.ClientSecret = paymentIntent.ClientSecret;
                            req.TxCode = vxcode;
                            req.ClientSecret = req.TxCode;
                            //Insert a record in tblStripPayment for recording purpose
                            tblSagePayment tsp = new tblSagePayment();
                            tsp.PaymentPrice = (double)req.Amount;
                            tsp.PaymentDate = DateTime.Now;
                            tsp.StatusUpdated = DateTime.Now;
                            tsp.OrderID = req.OrderGUID;
                            tsp.IsSuccess = false;
                            tsp.VendorTXCode = vxcode;
                            tsp.DeviceType = req.DeviceType;
                            tsp.MobileNo = req.Mobile;
                            tsp.SplitPayment = splitPayment;
                            tsp.TipAmount = (int) req.TipAmount;
                            tsp.SCAmount = (double)req.ServiceCharge;
                            dbContext.tblSagePayments.Add(tsp);
                            dbContext.SaveChanges();
                        }

                        catch (Exception ex)
                        {
                            //req.FailureMessage = "We could not complete your payment request. Kindly pay at the TILL. Thanks";
                            req.FailureMessage = ex.StackTrace;
                            if (ex.InnerException != null)
                            {
                                req.FailureMessage += ex.InnerException.StackTrace;
                            }

                            req.ClientSecret = "";
                            //return Json(ex.Message, JsonRequestBehavior.AllowGet);
                            //}
                        }

                    }
                }
            }
            if (error)
                req.ClientSecret = "";
            return Json(req, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetTableOrder(Guid orderid, int UserId)
        {
            logger.Info("Getting OrderedItems - " + UserId);
            Models.OrderService os = new Models.OrderService();
            TableOrder to = os.GetTableOrderV3(orderid);
            logger.Info("OrderedItems Fetched - " + UserId);
            return Json(to, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PrintBuffetItems()
        {
            string response = "";
            int printingIntervalTime = Convert.ToInt32(ConfigurationManager.AppSettings["PrintingInterval"]);
            int itemsPerCycle = Convert.ToInt32(ConfigurationManager.AppSettings["ItemCount"]);
            int maxItemsPerPerson = Convert.ToInt32(ConfigurationManager.AppSettings["MaxItemsPerPerson"]);
            int totalAllowedItemsPerCycle = 0;
            int overOrderItemsCount = 0;
            int totalAllowedItemsPerOrder = 0;
            tblPrintQueue tp = new tblPrintQueue();
            int itemsOnThisTicket = 0;
            int thisTicketCount = 0;
            int totalTicketsForTable = 0;
            string stepresponse = "";
            try
            {
                //Get all unique orders for which either we have direct print items or next order time < current print time
                List<BuffetOrder> printableOrders = new List<BuffetOrder>();
                using (var _context1 = new ChineseTillEntities1())
                {
                    printableOrders = (from a in _context1.tblPrintBuffetItems
                                       join b in _context1.tblTableOrders on a.OrderGUID equals b.OrderGUID
                                       join c in _context1.tblTables on b.TableId equals c.TableID
                                       where a.Printed == false && (b.NextOrderTime <= DateTime.Now || a.DirectPrint == true)
                                       && DbFunctions.TruncateTime(a.DateOrdered) == DbFunctions.TruncateTime(DateTime.Now)
                                       group a by new { a.OrderGUID, c.TableNumber, b.CustomerCount, a.DateCreated } into g
                                       select new BuffetOrder
                                       {
                                           OrderGUID = g.Key.OrderGUID,
                                           TableNumber = g.Key.TableNumber,
                                           CustomerCount = g.Key.CustomerCount,
                                           OrderTime = g.Key.DateCreated
                                       }).ToList();
                }

                //for all fetched orders process items
                if (printableOrders != null && printableOrders.Count > 0)
                {
                    //using (var scope = new TransactionScope())
                    //{
                    foreach (var printOrder in printableOrders)
                    {
                        overOrderItemsCount = 0;
                        totalAllowedItemsPerCycle = printOrder.CustomerCount * itemsPerCycle;
                        List<BuffetItem> printItems = new List<BuffetItem>();
                        using (var _context2 = new ChineseTillEntities1())
                        {
                            printItems = (from a in _context2.tblPrintBuffetItems
                                          join b in _context2.tblProducts on a.ProductId equals b.ProductID
                                          where a.OrderGUID == printOrder.OrderGUID && a.Printed == false
                                          select new BuffetItem
                                          {
                                              Id = a.Id,
                                              ProductID = a.ProductId,
                                              MenuID = (int)a.MenuId,
                                              DateCreated = a.DateCreated,
                                              OrderedBy = a.UserName ?? " ",
                                              Qty = a.Qty,
                                              EnglishName = b.EnglishName,
                                              ChineseName = b.ChineseName,
                                              DirectPrint = a.DirectPrint ?? false
                                          }).OrderBy(x => x.DateCreated).Take(totalAllowedItemsPerCycle).ToList();

                            if (printItems.Count > 0)
                            {
                                var desertItems = printItems.Where(x => x.MenuID == 11).ToList();
                                var starterItems = printItems.Where(x => x.MenuID == 3).ToList();
                                var mcItems = printItems.Where(x => x.MenuID != 11 && x.MenuID != 3 && x.DirectPrint == false).ToList();
                                var mcDirectItems = printItems.Where(x => x.MenuID != 11 && x.MenuID != 3 && x.DirectPrint == true).ToList();

                                //if (printItems[0].ItemsPrintedTillNow + printItems.Count > totalAllowedItemsPerOrder)
                                //{
                                //    overOrderItemsCount = printItems[0].ItemsPrintedTillNow + printItems.Count - totalAllowedItemsPerOrder;
                                //}
                                if (desertItems != null && desertItems.Count > 0)
                                {


                                    var desUniqueUsers = desertItems.Select(x => x.OrderedBy).Distinct();
                                    foreach (var user in desUniqueUsers)
                                    {
                                        var userItems = desertItems.Where(x => x.OrderedBy == user).ToList();
                                        string itemStr = "";
                                        List<int> printedItemIds = new List<int>();
                                        itemsOnThisTicket = 0;
                                        userItems = userItems.OrderBy(x => x.EnglishName).ToList();
                                        var orderProdIds = userItems.Select(x => x.ProductID).Distinct();
                                        foreach (var pId in orderProdIds)
                                        {
                                            int qty = 0;
                                            qty = userItems.Where(x => x.ProductID == pId).Sum(x => x.Qty);
                                            var item = userItems.Where(x => x.ProductID == pId).FirstOrDefault();
                                            string it = qty + " - " + item.ChineseName + " - " + item.EnglishName;
                                            it = rh.SpliceText(it, 25);
                                            itemStr += it + Environment.NewLine;
                                            itemsOnThisTicket += qty;
                                            List<int> ui = new List<int>();
                                            ui = userItems.Where(x => x.ProductID == pId).Select(x => x.Id).ToList();
                                            printedItemIds.AddRange(ui);  //check this***
                                        }
                                        if (itemStr != "")
                                        {
                                            string receipt = "";
                                            thisTicketCount = thisTicketCount + 1;
                                            totalTicketsForTable = totalTicketsForTable + 1;
                                            //WriteToFile("Starting Printing " + printOrder.OrderGUID.ToString());
                                            int batchNo = 1;
                                            var latestBatchNo = _context2.tblPrintQueues.Where(x => DbFunctions.TruncateTime(x.DateCreated) == DbFunctions.TruncateTime(DateTime.Now) && x.ToPrinter.Contains("Kitchen"))
                                                                       .OrderByDescending(x => x.PrintQueueID)
                                                                       .Select(x => x.BatchNo).FirstOrDefault();
                                            if (latestBatchNo > 0)
                                                batchNo = latestBatchNo + 1;
                                            receipt += printOrder.CustomerCount + "   " + printOrder.OrderTime.ToString("HH:mm") + "              " + batchNo + Environment.NewLine + Environment.NewLine;
                                            receipt += "DESSERT" + Environment.NewLine;
                                            receipt += itemStr + Environment.NewLine;
                                            if (user.Length > 15)
                                                receipt += user.Substring(0, 15) + "   " + DateTime.Now.ToString("HH:mm") + Environment.NewLine;
                                            else
                                                receipt += user + "   " + DateTime.Now.ToString("HH:mm") + Environment.NewLine;

                                            receipt += itemsOnThisTicket + "   " + 1 + "/" + 1 + "              " + printOrder.TableNumber + Environment.NewLine;

                                            //Update picked items as printed

                                            var prd = _context2.tblPrintBuffetItems.Where(x => printedItemIds.Contains(x.Id) && x.OrderGUID == printOrder.OrderGUID && x.Printed == false && x.BatchNo == 0).ToList();
                                            prd.ForEach(a =>
                                            {
                                                a.Printed = true;
                                                a.DatePrinted = DateTime.Now;
                                                a.BatchNo = batchNo;
                                                a.BatchTime = DateTime.Now.ToString("HH:mm");
                                            });
                                            _context2.SaveChanges();
                                            tblPrintQueue tpd = new tblPrintQueue();
                                            tpd.Receipt = receipt;
                                            tpd.PCName = "DineIn";
                                            tpd.ToPrinter = "Kitchen2";
                                            tpd.UserFK = -10;
                                            tpd.DateCreated = DateTime.Now;
                                            tpd.BatchNo = batchNo;
                                            tpd.TicketNo = batchNo.ToString();
                                            tpd.TableNumber = printOrder.TableNumber;
                                            tpd.OrderGUID = printOrder.OrderGUID;
                                            _context2.tblPrintQueues.Add(tpd);
                                            _context2.SaveChanges();
                                            //WriteToFile("Printing Complete Deserts" + printOrder.OrderGUID.ToString().ToString());
                                        }

                                    }


                                }
                                if (starterItems != null && starterItems.Count > 0)
                                {

                                    var stUniqueUsers = starterItems.Select(x => x.OrderedBy).Distinct();
                                    foreach (var user in stUniqueUsers)
                                    {
                                        var userItems = starterItems.Where(x => x.OrderedBy == user).ToList();
                                        string itemStr = "";
                                        List<int> printedItemIds = new List<int>();
                                        itemsOnThisTicket = 0;
                                        userItems = userItems.OrderBy(x => x.EnglishName).ToList();
                                        var orderProdIds = userItems.Select(x => x.ProductID).Distinct();
                                        foreach (var pId in orderProdIds)
                                        {
                                            int qty = 0;
                                            qty = userItems.Where(x => x.ProductID == pId).Sum(x => x.Qty);
                                            var item = userItems.Where(x => x.ProductID == pId).FirstOrDefault();
                                            string it = qty + " - " + item.ChineseName + " - " + item.EnglishName;
                                            it = rh.SpliceText(it, 25);
                                            itemStr += it + Environment.NewLine;
                                            itemsOnThisTicket += qty;
                                            List<int> ui = new List<int>();
                                            ui = userItems.Where(x => x.ProductID == pId).Select(x => x.Id).ToList();
                                            printedItemIds.AddRange(ui);  //check this***
                                        }
                                        if (itemStr != "")
                                        {
                                            string receipt = "";
                                            thisTicketCount = thisTicketCount + 1;
                                            totalTicketsForTable = totalTicketsForTable + 1;
                                            //WriteToFile("Starting Printing Starters" + printOrder.OrderGUID.ToString());
                                            int batchNo = 1;
                                            var latestBatchNo = _context2.tblPrintQueues.Where(x => DbFunctions.TruncateTime(x.DateCreated) == DbFunctions.TruncateTime(DateTime.Now) && x.ToPrinter.Contains("Kitchen"))
                                                                       .OrderByDescending(x => x.PrintQueueID)
                                                                       .Select(x => x.BatchNo).FirstOrDefault();
                                            if (latestBatchNo > 0)
                                                batchNo = latestBatchNo + 1;
                                            receipt += printOrder.CustomerCount + "   " + printOrder.OrderTime.ToString("HH:mm") + "              " + batchNo + Environment.NewLine + Environment.NewLine;
                                            receipt += "STARTERS" + Environment.NewLine;
                                            receipt += itemStr + Environment.NewLine;
                                            if (user.Length > 15)
                                                receipt += user.Substring(0, 15) + "   " + DateTime.Now.ToString("HH:mm") + Environment.NewLine;
                                            else
                                                receipt += user + "   " + DateTime.Now.ToString("HH:mm") + Environment.NewLine;

                                            receipt += itemsOnThisTicket + "   " + 1 + "/" + 1 + "              " + printOrder.TableNumber + Environment.NewLine;

                                            //Update picked items as printed

                                            var prd = _context2.tblPrintBuffetItems.Where(x => printedItemIds.Contains(x.Id) && x.OrderGUID == printOrder.OrderGUID && x.Printed == false && x.BatchNo == 0).ToList();
                                            prd.ForEach(a =>
                                            {
                                                a.Printed = true;
                                                a.DatePrinted = DateTime.Now;
                                                a.BatchNo = batchNo;
                                                a.BatchTime = DateTime.Now.ToString("HH:mm");
                                            });
                                            _context2.SaveChanges();
                                            //WriteToFile("Printing Update Complete Starters" + printOrder.OrderGUID.ToString().ToString());



                                            tblPrintQueue tps = new tblPrintQueue();
                                            tps.Receipt = receipt;
                                            tps.PCName = "DineIn";
                                            tps.ToPrinter = "Kitchen2";
                                            tps.UserFK = -10;
                                            tps.DateCreated = DateTime.Now;
                                            tps.BatchNo = batchNo;
                                            tps.TicketNo = batchNo.ToString();
                                            tps.TableNumber = printOrder.TableNumber;
                                            tps.OrderGUID = printOrder.OrderGUID;
                                            _context2.tblPrintQueues.Add(tps);
                                            _context2.SaveChanges();
                                            //WriteToFile("Printing Complete Starters" + printOrder.OrderGUID.ToString().ToString());
                                        }

                                    }
                                }
                                if (mcItems != null && mcItems.Count > 0)
                                {

                                    var mcUniqueUsers = mcItems.Select(x => x.OrderedBy).Distinct();
                                    foreach (var user in mcUniqueUsers)
                                    {
                                        var userItems = mcItems.Where(x => x.OrderedBy == user).ToList();
                                        var uniqueMenuIds = userItems.Select(x => x.MenuID).Distinct().ToList();
                                        int menuIdCount = 1;
                                        string itemStr = "";
                                        List<int> printedItemIds = new List<int>();
                                        itemsOnThisTicket = 0;
                                        foreach (var menuId in uniqueMenuIds)
                                        {
                                            var menuItems = userItems.Where(x => x.MenuID == menuId).ToList();
                                            menuItems = menuItems.OrderBy(x => x.EnglishName).ToList();
                                            var orderProdIds = menuItems.Select(x => x.ProductID).Distinct();
                                            if (menuIdCount > 1)
                                                itemStr += "*********************" + Environment.NewLine;
                                            foreach (var pId in orderProdIds)
                                            {
                                                int qty = 0;
                                                qty = menuItems.Where(x => x.ProductID == pId).Sum(x => x.Qty);
                                                var item = menuItems.Where(x => x.ProductID == pId).FirstOrDefault();
                                                string it = qty + " - " + item.ChineseName + " - " + item.EnglishName;
                                                it = rh.SpliceText(it, 25);
                                                itemStr += it + Environment.NewLine;
                                                itemsOnThisTicket += qty;
                                                List<int> ui = new List<int>();
                                                ui = userItems.Where(x => x.ProductID == pId).Select(x => x.Id).ToList();
                                                printedItemIds.AddRange(ui);  //check this***
                                            }
                                            menuIdCount++;
                                        }
                                        if (itemStr != "")
                                        {
                                            string receipt = "";
                                            thisTicketCount = thisTicketCount + 1;
                                            totalTicketsForTable = totalTicketsForTable + 1;
                                            //WriteToFile("Starting Printing MC " + printOrder.OrderGUID.ToString());
                                            int batchNo = 1;
                                            var latestBatchNo = _context2.tblPrintQueues.Where(x => DbFunctions.TruncateTime(x.DateCreated) == DbFunctions.TruncateTime(DateTime.Now) && x.ToPrinter.Contains("Kitchen"))
                                                                           .OrderByDescending(x => x.PrintQueueID)
                                                                           .Select(x => x.BatchNo).FirstOrDefault();
                                            if (latestBatchNo > 0)
                                                batchNo = latestBatchNo + 1;
                                            receipt += printOrder.CustomerCount + "   " + printOrder.OrderTime.ToString("HH:mm") + "              " + batchNo + Environment.NewLine + Environment.NewLine;

                                            receipt += itemStr + Environment.NewLine;
                                            if (user.Length > 15)
                                                receipt += user.Substring(0, 15) + "   " + DateTime.Now.ToString("HH:mm") + Environment.NewLine;
                                            else
                                                receipt += user + "   " + DateTime.Now.ToString("HH:mm") + Environment.NewLine;
                                            receipt += itemsOnThisTicket + "   " + 1 + "/" + 1 + "              " + printOrder.TableNumber + Environment.NewLine;

                                            //Update picked items as printed

                                            var prd = _context2.tblPrintBuffetItems.Where(x => printedItemIds.Contains(x.Id) && x.OrderGUID == printOrder.OrderGUID && x.Printed == false && x.BatchNo == 0).ToList();
                                            prd.ForEach(a =>
                                            {
                                                a.Printed = true;
                                                a.DatePrinted = DateTime.Now;
                                                a.BatchNo = batchNo;
                                                a.BatchTime = DateTime.Now.ToString("HH:mm");
                                            });
                                            _context2.SaveChanges();

                                            //WriteToFile("Printing Update Complete MC" + printOrder.OrderGUID.ToString().ToString());

                                            tblPrintQueue tpm = new tblPrintQueue();
                                            tpm.Receipt = receipt;
                                            tpm.PCName = "DineIn";
                                            tpm.ToPrinter = "Kitchen";
                                            tpm.UserFK = -10;
                                            tpm.DateCreated = DateTime.Now;
                                            tpm.BatchNo = batchNo;
                                            tpm.TicketNo = batchNo.ToString();
                                            tpm.TableNumber = printOrder.TableNumber;
                                            tpm.OrderGUID = printOrder.OrderGUID;
                                            _context2.tblPrintQueues.Add(tpm);
                                            _context2.SaveChanges();
                                            //WriteToFile("Printing Complete MC" + printOrder.OrderGUID.ToString());
                                            itemsOnThisTicket = 0;
                                        }

                                    }
                                }
                                if (mcDirectItems != null && mcDirectItems.Count > 0)
                                {

                                    var mcdUniqueUsers = mcDirectItems.Select(x => x.OrderedBy).Distinct();
                                    foreach (var user in mcdUniqueUsers)
                                    {
                                        var userItems = mcDirectItems.Where(x => x.OrderedBy == user).ToList();
                                        string itemStr = "";
                                        List<int> printedItemIds = new List<int>();
                                        itemsOnThisTicket = 0;
                                        userItems = userItems.OrderBy(x => x.EnglishName).ToList();
                                        var orderProdIds = userItems.Select(x => x.ProductID).Distinct();
                                        foreach (var pId in orderProdIds)
                                        {
                                            int qty = 0;
                                            qty = userItems.Where(x => x.ProductID == pId).Sum(x => x.Qty);
                                            var item = userItems.Where(x => x.ProductID == pId).FirstOrDefault();
                                            string it = qty + " - " + item.ChineseName + " - " + item.EnglishName;
                                            it = rh.SpliceText(it, 25);
                                            itemStr += it + Environment.NewLine;
                                            itemsOnThisTicket += qty;
                                            List<int> ui = new List<int>();
                                            ui = userItems.Where(x => x.ProductID == pId).Select(x => x.Id).ToList();
                                            printedItemIds.AddRange(ui);  //check this***
                                        }
                                        if (itemStr != "")
                                        {
                                            string receipt = "";
                                            thisTicketCount = thisTicketCount + 1;
                                            totalTicketsForTable = totalTicketsForTable + 1;
                                            //WriteToFile("Starting MCD Starters" + printOrder.OrderGUID.ToString());
                                            int batchNo = 1;
                                            var latestBatchNo = _context2.tblPrintQueues.Where(x => DbFunctions.TruncateTime(x.DateCreated) == DbFunctions.TruncateTime(DateTime.Now) && x.ToPrinter.Contains("Kitchen"))
                                                                       .OrderByDescending(x => x.PrintQueueID)
                                                                       .Select(x => x.BatchNo).FirstOrDefault();
                                            if (latestBatchNo > 0)
                                                batchNo = latestBatchNo + 1;
                                            receipt += printOrder.CustomerCount + "   " + printOrder.OrderTime.ToString("HH:mm") + "              " + batchNo + Environment.NewLine + Environment.NewLine;
                                            receipt += itemStr + Environment.NewLine;
                                            if (user.Length > 15)
                                                receipt += user.Substring(0, 15) + "   " + DateTime.Now.ToString("HH:mm") + Environment.NewLine;
                                            else
                                                receipt += user + "   " + DateTime.Now.ToString("HH:mm") + Environment.NewLine;

                                            receipt += itemsOnThisTicket + "   " + 1 + "/" + 1 + "              " + printOrder.TableNumber + Environment.NewLine;

                                            //Update picked items as printed

                                            var prd = _context2.tblPrintBuffetItems.Where(x => printedItemIds.Contains(x.Id) && x.OrderGUID == printOrder.OrderGUID && x.Printed == false && x.BatchNo == 0).ToList();
                                            prd.ForEach(a =>
                                            {
                                                a.Printed = true;
                                                a.DatePrinted = DateTime.Now;
                                                a.BatchNo = batchNo;
                                                a.BatchTime = DateTime.Now.ToString("HH:mm");
                                            });
                                            _context2.SaveChanges();
                                            //WriteToFile("Printing Update Complete MCD" + printOrder.OrderGUID.ToString().ToString());



                                            tblPrintQueue tps = new tblPrintQueue();
                                            tps.Receipt = receipt;
                                            tps.PCName = "DineIn";
                                            tps.ToPrinter = "Kitchen2";
                                            tps.UserFK = -10;
                                            tps.DateCreated = DateTime.Now;
                                            tps.BatchNo = batchNo;
                                            tps.TicketNo = batchNo.ToString();
                                            tps.TableNumber = printOrder.TableNumber;
                                            tps.OrderGUID = printOrder.OrderGUID;
                                            _context2.tblPrintQueues.Add(tps);
                                            _context2.SaveChanges();
                                            //WriteToFile("Printing Complete MCD" + printOrder.OrderGUID.ToString().ToString());
                                        }

                                    }
                                }
                            }

                        }

                    }
                    //    scope.Complete();
                    //}

                }


            }
            catch (SqlException sqlex)
            {
                //logger.Error(sqlex);
                string error = sqlex.Message;
                if (sqlex.InnerException != null)
                    error += sqlex.InnerException.StackTrace;
                response = error;
                //WriteToFile("SQL Exception at PrintBuffetItemstoKitchen " + error);
            }
            catch (Exception ex)
            {
                //logger.Error(ex);
                string error = ex.Message;
                if (ex.InnerException != null)
                    error = ex.InnerException.StackTrace;
                response = error;
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTickets(string printer)
        {
            List<PrintingBatch> pb = new List<PrintingBatch>();
            int printThresholdTime = Convert.ToInt32(ConfigurationManager.AppSettings["PrintTicketThesholdTime"]);
            try
            {

                Models.OrderService os = new Models.OrderService();
                List<string> printers = new List<string>();
                printers.Add(printer);
                pb = os.GetPrintBatches(printers, printThresholdTime);
                pb = pb.Where(x => x.PrinterName == printer).ToList();
               
            }
            catch (Exception ex)
            {
                pb = new List<PrintingBatch>();
                //throw;
            }
            return Json(pb, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReprintItems(RePrintItemsRequest reprintItems)
        {
            List<PrintingBatch> pb = new List<PrintingBatch>();
            try
            {

                ROMSHelper rh = new ROMSHelper();
                List<OrderBuffetItem> starterItems = new List<OrderBuffetItem>();
                List<OrderBuffetItem> desertItems = new List<OrderBuffetItem>();
                List<OrderBuffetItem> mcItems = new List<OrderBuffetItem>();
                List<int> allProductIds = reprintItems.BuffetItems.Select(x => x.ProductId).ToList();
                List<int> strProdIds = new List<int>();
                List<int> desProdIds = new List<int>();
                List<int> mcProdIds = new List<int>();



                using (var _context = new ChineseTillEntities1())
                {
                    var menuItems = (from a in _context.tblMenuItems
                                     where allProductIds.Contains(a.ProductID)
                                     select new
                                     {
                                         MenuId = a.MenuID,
                                         ProductId = a.ProductID
                                     }).ToList();
                    strProdIds = menuItems.Where(x => x.MenuId == 3).Select(x => x.ProductId).ToList();
                    desProdIds = menuItems.Where(x => x.MenuId == 11).Select(x => x.ProductId).ToList();
                    mcProdIds = menuItems.Where(x => x.MenuId != 3 && x.MenuId != 11).Select(x => x.ProductId).ToList();

                }
                
                if (strProdIds.Count > 0)
                {
                    starterItems = reprintItems.BuffetItems.Where(x=> strProdIds.Contains(x.ProductId)).ToList();
                    rh.PrintTicket(reprintItems.TableNumber, reprintItems.CustCount, reprintItems.OrderTime, "STARTERS", starterItems);

                }
                if (desProdIds.Count > 0)
                {
                    desertItems = reprintItems.BuffetItems.Where(x => desProdIds.Contains(x.ProductId)).ToList();
                    rh.PrintTicket(reprintItems.TableNumber, reprintItems.CustCount, reprintItems.OrderTime, "DESSERTS", desertItems);

                }
                if (mcProdIds.Count > 0)
                {
                    mcItems = reprintItems.BuffetItems.Where(x => mcProdIds.Contains(x.ProductId)).ToList();
                    rh.PrintTicket(reprintItems.TableNumber, reprintItems.CustCount, reprintItems.OrderTime, "", mcItems);

                }
                //rh.PrintTicket(reprintItems.TableNumber,reprintItems.CustCount,reprintItems.OrderTime,)

            }
            catch (Exception ex)
            {
                pb = new List<PrintingBatch>();
                //throw;
            }
            return Json(pb, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdatePrinterStatus(string printer,bool offline)
        {
            string response = "Printer updated successfully";
            try
            {
                using (var _context = new ChineseTillEntities1())
                {
                    var pter = _context.tblPrinters.Where(x => x.PrinterName == printer).FirstOrDefault();
                    pter.Offline = offline;
                    _context.Entry(pter).State = EntityState.Modified;
                    _context.SaveChanges();
                }
                //pb = pb.Where(x => x.PrinterName == printer).ToList();

            }
            catch (Exception ex)
            {
                response = "Please try again";
                //throw;
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetUnPrintedItems(string printer)
        {
            List<OrderBuffetItem> unPrintedItems = new List<OrderBuffetItem>();
            try
            {
                using (var _context = new ChineseTillEntities1())
                {
                    unPrintedItems = (from a in _context.tblPrintBuffetItems
                                      join b in _context.tblProducts on a.ProductId equals b.ProductID
                                      join c in _context.tblMenuItems on a.ProductId equals c.ProductID
                                      join d in _context.tblTables on a.TableId equals d.TableID
                                      where a.Printed == false && DbFunctions.TruncateTime(a.DateCreated)
                                      == DbFunctions.TruncateTime(DateTime.Now)
                                      select new OrderBuffetItem
                                      {
                                          Id=a.Id,
                                          Qty = a.Qty,
                                          EnglishName = b.EnglishName,
                                          ChineseName = b.ChineseName,
                                          TableNumber = d.TableNumber,
                                          MenuId = c.MenuID,
                                          DateCreated = a.DateCreated,
                                          
                                      }).OrderByDescending(x=>x.DateCreated).ToList();
                    if (printer == ConfigurationManager.AppSettings["MCPrinter"])
                        unPrintedItems = unPrintedItems.Where(x => x.MenuId != 3 && x.MenuId != 11).ToList();
                    else if (printer == ConfigurationManager.AppSettings["StarterPrinter"])
                        unPrintedItems = unPrintedItems.Where(x => x.MenuId == 3).ToList();
                    else if (printer == ConfigurationManager.AppSettings["DessertPrinter"])
                        unPrintedItems = unPrintedItems.Where(x => x.MenuId == 11).ToList();
                    else
                        unPrintedItems = new List<OrderBuffetItem>();
                }
                //pb = pb.Where(x => x.PrinterName == printer).ToList();

            }
            catch (Exception ex)
            {
                //response = "Please try again";
                //throw;
            }
            return Json(unPrintedItems, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PrintUnPrintedItems(List<int> unPrintedItems)
        {
            string response = "Success";
            try
            {
                using (var _context = new ChineseTillEntities1())
                {
                    var items = _context.tblPrintBuffetItems.Where(x => unPrintedItems.Contains(x.Id)).ToList();
                    items.ForEach(a =>
                    {
                        a.Printed = true;
                        a.DatePrinted = DateTime.Now;
                    });
                    _context.SaveChanges();
                }
                //pb = pb.Where(x => x.PrinterName == printer).ToList();

            }
            catch (Exception ex)
            {
                response = "Please try again";
                //throw;
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetUnPrintedItemsByTable(string printer)
        {
            List<TableOrder> tableOrders = new List<TableOrder>();
            List<OrderBuffetItem> unPrintedItems = new List<OrderBuffetItem>();
            List<OrderBuffetItem> unPrintedItems1 = new List<OrderBuffetItem>();
            List<OrderBuffetItem> mcUnPrintedItems1 = new List<OrderBuffetItem>();
            List<OrderBuffetItem> stUnPrintedItems1 = new List<OrderBuffetItem>();
            List<OrderBuffetItem> dsUnPrintedItems1 = new List<OrderBuffetItem>();


            try
            {
                using (var _context = new ChineseTillEntities1())
                {
                    unPrintedItems = (from a in _context.tblPrintBuffetItems
                                      join b in _context.tblProducts on a.ProductId equals b.ProductID
                                      join c in _context.tblMenuItems on a.ProductId equals c.ProductID
                                      join d in _context.tblTables on a.TableId equals d.TableID
                                      where a.Printed == false && DbFunctions.TruncateTime(a.DateCreated)
                                      == DbFunctions.TruncateTime(DateTime.Now) && c.DelInd == false
                                      select new OrderBuffetItem
                                      {
                                          Id = a.Id,
                                          Qty = a.Qty,
                                          EnglishName = b.EnglishName,
                                          ChineseName = b.ChineseName,
                                          TableNumber = d.TableNumber,
                                          MenuId = c.MenuID,
                                          DateCreated = a.DateCreated,
                                          Status = a.InProcess == true ? 1 : 0
                                      }).OrderBy(x => x.DateCreated).ToList();
                    if (printer == ConfigurationManager.AppSettings["MCPrinter"])
                        mcUnPrintedItems1 = unPrintedItems.Where(x => x.MenuId != 3 && x.MenuId != 11).ToList();
                    if (printer == ConfigurationManager.AppSettings["StarterPrinter"])
                        stUnPrintedItems1 = unPrintedItems.Where(x => x.MenuId == 3).ToList();
                    if (printer == ConfigurationManager.AppSettings["DessertPrinter"])
                        dsUnPrintedItems1 = unPrintedItems.Where(x => x.MenuId == 11).ToList();

                    unPrintedItems1.AddRange(mcUnPrintedItems1);
                    unPrintedItems1.AddRange(stUnPrintedItems1);
                    unPrintedItems1.AddRange(dsUnPrintedItems1);



                    if (unPrintedItems1.Count > 0)
                    {
                        var uniqueTables = unPrintedItems1.Select(x => x.TableNumber).Distinct().ToList();
                        foreach (var item in uniqueTables)
                        {
                            TableOrder to = new TableOrder();
                            to.BuffetItems = unPrintedItems1.Where(x => x.TableNumber == item).ToList();
                            to.tableDetails.TableNumber = item;
                            tableOrders.Add(to);
                        }
                        tableOrders = tableOrders.OrderBy(x =>
                        {
                            x.BuffetItems = x.BuffetItems.OrderBy(y => y.DateCreated).ToList();
                            return x.BuffetItems;
                        }).ToList();
                    }

                }
                //pb = pb.Where(x => x.PrinterName == printer).ToList();

            }
            catch (Exception ex)
            {
                //response = "Please try again";
                //throw;
            }
            return Json(tableOrders, JsonRequestBehavior.AllowGet);
        }

   
        public ActionResult UpdateUnPrintedItemStatus(List<OrderBuffetItem> UnPrintedItems)
        {
            string response = "Success";
            try
            {
                using (var _context = new ChineseTillEntities1())
                {
                    foreach (var item in UnPrintedItems)
                    {
                        var unItem = _context.tblPrintBuffetItems.Where(x => x.Id == item.Id).FirstOrDefault();
                        if (item.Status == 1)
                            unItem.InProcess = true;
                        else if(item.Status == 2)
                        {
                            unItem.Printed = true;
                            unItem.DatePrinted = DateTime.Now;
                        }
                        else if(item.Status == 0)
                        {
                            unItem.Printed = false;
                            unItem.InProcess = false;
                            unItem.DatePrinted = null;
                        }
                        _context.Entry(unItem).State = EntityState.Modified;
                        _context.SaveChanges();
                    }
                    //items.ForEach(a =>
                    //{
                    //    a.Printed = true;
                    //    a.DatePrinted = DateTime.Now;
                    //});
                    //_context.SaveChanges();
                }
                //pb = pb.Where(x => x.PrinterName == printer).ToList();

            }
            catch (Exception ex)
            {
                response = "Please try again";
                //throw;
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTableCustomers()
        {
            List<TableCustomer> TablesList = new List<TableCustomer>();
            var now = DateTime.Now;
            DateTime currentDate = new DateTime(now.Year, now.Month, now.Day, 01, 00, 00);
            try
            {
                using (var _context = new ChineseTillEntities1())
                {
                    var tList = (from a in _context.tblOrders
                                  //join b in _context.tblOrderBuffetItems on a.OrderGUID equals b.OrderGUID
                                  join c in _context.tblTables on a.TableID equals c.TableID 
                                  where a.DelInd == false && c.DelInd == false && a.TableID > 0 && a.DateCreated > currentDate
                                  //&& b.UserType == "Customer"
                                  select  new 
                                  {
                                      TableNumber = c.TableNumber,
                                      OpenTime = a.DateCreated,
                                      CloseTime = a.DatePaid ,
                                      OrderId = a.OrderGUID,
                                      CustomerNames =  _context.tblOrderBuffetItems.
                                                       Where(x=> x.UserType == "Customer" && x.OrderGUID == a.OrderGUID).
                                                       Select(x=>x.UserName + "-" + x.UserId).Distinct().ToList()
                                      
                                  }).ToList();
                    foreach (var item in tList)
                    {
                        TableCustomer tc = new TableCustomer();
                        tc.TableNumber = item.TableNumber;
                        tc.OpenTime = item.OpenTime.ToString("hh:mm");
                        tc.CloseTime = item.CloseTime == null ? "" : item.CloseTime.Value.ToString("hh:mm");
                        tc.OrderId = item.OrderId;
                        tc.CustomerNames = item.CustomerNames;
                        TablesList.Add(tc);
                    }
                }
                
            }
            catch (Exception)
            {

                throw;
            }
            return Json(TablesList, JsonRequestBehavior.AllowGet); ;
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
                                    group a by new { a.ProductID, b.Description, a.Price, b.RewardPoints, b.RedemptionPoints } into g
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
                int custThreshold = Convert.ToInt32(ConfigurationManager.AppSettings["SCCustThreshold"]);
                decimal scRate = Convert.ToDecimal(ConfigurationManager.AppSettings["ServiceChargeRate"]);

                if (scab && order.CustCount > custThreshold )
                {
                    if (order.ServiceChargeApplicable == null)
                    {
                        order.ServiceChargeApplicable = true;
                        to.ServiceChargeApplicable = true;
                        if (order.SCRate == null || (order.SCRate != null && order.SCRate == 0))
                        {
                            order.SCRate = scRate;
                            to.SCRate = scRate;
                        }
                        else if (order.SCRate != null && order.SCRate > 0)
                            to.SCRate = (int)order.SCRate;
                    }
                    else if (order.ServiceChargeApplicable != null && order.ServiceChargeApplicable == true)
                    {
                        to.ServiceChargeApplicable = true;
                        if (order.SCRate == null || (order.SCRate != null && order.SCRate == 0))
                        {
                            order.SCRate = scRate;
                            to.SCRate = scRate;
                        }
                        else if (order.SCRate != null && order.SCRate > 0)
                            to.SCRate = (decimal)order.SCRate;
                    }
                    else
                    {
                        to.ServiceChargeApplicable = false;
                    }
                   
                    //order.SCRate = scRate;
                    
                    _context.Entry(order).State = EntityState.Modified;
                    _context.SaveChanges();
                }
                //if ((sca != null && sca == false) || !scab)
                //    to.ServiceChargeApplicable = false;
                //else
                //    to.ServiceChargeApplicable = true;



                //to.ServiceChargeApplicable = (bool)_context.tblOrders.Where(x => x.OrderGUID == orderId).Select(x => x.ServiceChargeApplicable).FirstOrDefault();
                to.CustCount = (int)order.AdCount + (int)order.KdCount + (int)order.JnCount;
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

        public ActionResult SCAmountUpdate(Guid orderId,int userId, string oldAmount, string newAmount)
        {
            string response = "success";
            try
            {
                using (var dbContext = new ChineseTillEntities1())
                {
                    tblSCAmountUpdate tsc = new tblSCAmountUpdate();
                    tsc.OrderId = orderId;
                    tsc.UserId = userId;
                    tsc.OldAmount = Convert.ToDecimal(oldAmount);
                    tsc.NewAmount = Convert.ToDecimal(newAmount);
                    tsc.DateCreated = DateTime.Now;
                    dbContext.tblSCAmountUpdates.Add(tsc);
                    dbContext.SaveChanges();

                }

            }
            catch (Exception ex)
            {

                throw;
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }
    }


}