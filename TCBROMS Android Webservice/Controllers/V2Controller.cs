using Deznu.Products.Common.Utility;
using Entity;
using Entity.Enums;
using Entity.Models;
using NLog;
using Stripe;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using TCBROMS_Android_Webservice.Helpers;
using TCBROMS_Android_Webservice.Models;
using ZXing;

namespace TCBROMS_Android_Webservice.Controllers
{
    //[AllowCrossSite]
    // Test commit
    public class V2Controller : Controller
    {
        ChineseTillEntities1 dbContext = new ChineseTillEntities1();
        string url = ConfigurationManager.AppSettings["TCBAPIUrl"];
        ROMSHelper rh = new ROMSHelper();
        Models.CustomerService cs = new Models.CustomerService();
        Logger logger = LogManager.GetLogger("databaseLogger");

        // GET: V2
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult UpdatePaymentLock(Guid OrderId, bool locked,string txCode)
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
                    if (txCode != "")
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
                }
                response = "success";
            }
            catch (Exception ex)
            {

                response = ex.Message;
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ConfirmTablePayment(Payment req)
        {
            string response = "";
            Guid orderId = new Guid();
            orderId = req.OrderGUID;

            try
            {
                using (TransactionScope scope = new TransactionScope())
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
                    {
                        //Create new order for Split Products. Move selected items to new orderid
                        tblOrder tor = new tblOrder();

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
                        tor.CustomerId = req.CustomerId;
                        tor.ServiceCharge = req.ServiceCharge;
                        dbContext.tblOrders.Add(tor);
                        dbContext.SaveChanges();

                        orderId = tor.OrderGUID;
                        //Move selected products to this new order
                        foreach (var item in req.SplitProduct)
                        {
                            for (int i = 0; i < item.ProductQty; i++)
                            {
                                var op = dbContext.tblOrderParts.Where(x => x.OrderGUID == req.OrderGUID && x.ProductID == item.ProductID).FirstOrDefault();
                                op.OrderGUID = orderId;
                                dbContext.Entry(op).State = EntityState.Modified;
                                dbContext.SaveChanges();
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

                        //rh.WriteToFile("testing Elmah");
                        to.Paid = true;
                        to.DatePaid = DateTime.Now;
                        to.PaymentMethod = "MP";
                        to.TotalPaid = (decimal)req.Amount;
                        to.GrandTotal = (decimal)req.Amount;
                        to.TipAmount = (decimal)req.TipAmount;
                        to.ServiceCharge = (decimal)req.ServiceCharge;
                        dbContext.Entry(to).State = EntityState.Modified;
                        dbContext.SaveChanges();
                        var tblOrd = dbContext.tblTableOrders.Where(x => x.OrderGUID == req.OrderGUID).FirstOrDefault();
                        tblOrd.Active = false;
                        dbContext.Entry(tblOrd).State = EntityState.Modified;
                        dbContext.SaveChanges();
                        dbContext.usp_AN_SetTableCleaning(req.OrderGUID, 11);

                    }
                    //get the PCName from web config. All MP will be assigned to primary TILL. 
                    //If primary TILL value is empty use last paid TILL
                    string primaryTILL = "";
                    primaryTILL = Convert.ToString(ConfigurationManager.AppSettings["PrimaryTILL"]);
                    var top1 = dbContext.tblOrderPayments.Where(x => x.PCName != "" || x.PCName != null).OrderByDescending(x => x.DateCreated).FirstOrDefault();
                    if (primaryTILL == "")
                        primaryTILL = top1.PCName;
                    tblOrderPayment topy = new tblOrderPayment();
                    topy.OrderGUID = orderId;
                    topy.PaymentGUID = Guid.NewGuid();
                    topy.DateCreated = DateTime.Now;
                    topy.LastModified = DateTime.Now;
                    topy.PaymentValue = (decimal)req.Amount;
                    topy.TipAmount = (decimal)req.TipAmount;
                    topy.ServiceCharge = (decimal)req.ServiceCharge;
                    topy.PaymentType = "MP";
                    topy.PCName = primaryTILL;
                    dbContext.tblOrderPayments.Add(topy);
                    dbContext.SaveChanges();

                    //Update record in tblStripePayment
                    var tsp = dbContext.tblStripePayments.Where(x => x.VendorTxCode == req.TxCode).FirstOrDefault();
                    tsp.Success = true;
                    tsp.LastModified = DateTime.Now;
                    tsp.PaymentId = req.PaymentId;
                    dbContext.Entry(tsp).State = EntityState.Modified;
                    dbContext.SaveChanges();

                    int points = cs.UpdatePointsByOrder(orderId,req.Amount);
                    //Print Payment Confirmation slip for the Order
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
                   
                    response = "<h2 style=\"color:green;text-align:center;\">Payment Confirmation</h2> <p>Thank you for your payment. Our staff will bring you your confirmation slip.</p>";
                    if (points > 0)
                        response += "<br/><h2 style=\"color:darkred\";\"text-align:center\";>Congratulations</h2> <p>you have just earned " + points + " reward points for this order. You may redeem them in your next order with us.</p>";
                }
            }
            catch (Exception ex)
            {
                //response = ex.Message;
                //if(ex.InnerException != null)
                //response = ex.InnerException.StackTrace;
                response = "Thank you for the payment. Your table payment is not updated on our TILL. Kindly show this message to manager. We apologise the inconvenience caused. Thanks.";
                //throw;
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult FailedTablePayment(Payment req)
        {
            string response = "";
            Guid orderId = new Guid();
            orderId = req.OrderGUID;
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {

                    var to = dbContext.tblOrders.Where(x => x.OrderGUID == req.OrderGUID).FirstOrDefault();

                    //Update tblStripePayment to update failed payment
                    var tsp = dbContext.tblStripePayments.Where(x => x.VendorTxCode == req.TxCode).FirstOrDefault();
                    tsp.Success = false;
                    tsp.FailureMessage = req.FailureMessage;
                    dbContext.Entry(tsp).State = EntityState.Modified;
                    dbContext.SaveChanges();

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

        public ActionResult GetStripePaymentIntent(Payment req)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["TCBROMSConnection"].ConnectionString;
            string res = "";
            bool productMismatch = false;
           
            if (!req.isSplitPayment)
            {
                Models.OrderService os = new Models.OrderService();
                TableOrder to = os.GetOrderItems(req.OrderGUID);
                foreach (var item in req.OrderedProducts)
                {
                    var pr = to.tableProducts.Find(x => x.ProductID == item.ProductID && x.ProductQty == item.ProductQty);
                    if (pr == null)
                    {
                        productMismatch = true;
                        req.OrderedProducts = to.tableProducts;
                        break;
                    }
                }
            }

            if (!productMismatch)
            {
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
                    req.ClientSecret = paymentIntent.ClientSecret;
                    req.FailureMessage = "";
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
                    dbContext.tblStripePayments.Add(tsp);
                    dbContext.SaveChanges();
                }

                catch (Exception ex)
                {
                    req.FailureMessage = "We could not complete your payment request. Kindly try later. Thanks";
                    req.ClientSecret = "";
                    //return Json(ex.Message, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                req.FailureMessage = "Some items have changed in your order. Kindly refresh";
                req.ClientSecret = "";
            }
            return Json(req, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDineInProducts(Guid orderId)
        {
            GetProductsResponse gpr = new GetProductsResponse();
            Models.ProductService ps = new Models.ProductService();
            gpr = ps.GetDineInProductsV2(orderId);
            return Json(gpr, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetProductsForRedemption()
        {
            Models.ProductService ps = new Models.ProductService();
            var prds = ps.GetProductsForRedemptionV2();
            return Json(prds, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SubmitProductsForRedemption(ProductRedemptionModel req)
        {
            ProductRedemptionResponse prr = new ProductRedemptionResponse();
            prr.Logout = false;
            string response = "";
            if (dbContext.tblOrders.Any(x => x.OrderGUID == req.OrderId && (x.Paid == true || x.DelInd == true))
                || dbContext.tblTableOrders.Any(x => x.OrderGUID == req.OrderId && x.Active == false))
            {
                prr.Logout = true;
                prr.Message = "Unable to redeem products as table already paid/code expired. Kindly contact our staff";
            }
            else
            {
                var totalRedemeedPoints = 0;
                var totalEarnedPoints = 0;

                //add redeemed products
                foreach (var item in req.RedemptionProducts)
                {
                    for (int i = 0; i < item.ProductQty; i++)
                    {
                        try
                        {
                            totalRedemeedPoints += item.RedemptionPoints;
                            tblOrderPart top = new tblOrderPart();
                            top.DateCreated = DateTime.Now;
                            top.LastModified = DateTime.Now;
                            top.DelInd = false;
                            top.OrderGUID = req.OrderId;
                            top.Price = (decimal?)(-1 * item.Price);
                            top.ProductID = item.ProductID;
                            top.Qty = 1;
                            top.Total = (decimal?)(-1 * item.Price);
                            top.UserID = 0;
                            dbContext.tblOrderParts.Add(top);
                            dbContext.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            totalRedemeedPoints -= item.RedemptionPoints;
                            prr.IsRedeemed = true;
                            prr.Message = "We couldn't redeem the some of the products. Please try again";
                            break;
                        }
                    }
                    prr.IsRedeemed = true;
                }
                //call TCBAPI to deduct customer points
                if (totalRedemeedPoints > 0)
                {
                    if ((cs.UpdateCustomerPoints(req.CustomerId, totalRedemeedPoints, totalEarnedPoints, req.OrderId.ToString()) >= 0))
                    {
                        prr.CustomerPoints = req.CustomerPoints - totalRedemeedPoints;
                        prr.Message = "Points redeemed successfully";
                    }
                    else
                        prr.Message = "Products have been redeemed. Points will be updated shortly";
                }
            }
            return Json(prr, JsonRequestBehavior.AllowGet);
        }


        public ActionResult SubmitOrderV2(TableOrder t)

        {
            Models.OrderService os = new Models.OrderService();
            OrderSubmitResponse r = os.SubmitOrderV2(t);
            return Json(r, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdatePointsByOrder(Guid orderId)
        {

            int points = cs.UpdatePointsByOrder(orderId);
            return Json("success", JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddRewardPointsByCustomerId(int custId, int points,string orderId)
        {

            int pts = cs.AddRewardPointsByCustomerId(custId, points,orderId);
            return Json("success", JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetOrderItems(Guid orderId)
        {
            ChineseTillEntities1 _context = new ChineseTillEntities1();

            TableOrder to = new TableOrder();
            //to.BuffetItems = (from a in _context.tblOrderBuffetItems
            //                  join b in _context.tblProducts on a.ProductId equals b.ProductID
            //                  where a.OrderGUID == orderId
            //                  group a by new { a.ProductId, b.Description, a.Printed } into g

            //                  select new OrderBuffetItem
            //                  {
            //                      Description = g.Key.Description,
            //                      Printed = g.Key.Printed,
            //                      Qty = g.Sum(a => a.Qty)
            //                  }).OrderByDescending(b => b.Printed).ThenBy(b => b.Description).ToList();
            to.BuffetItems = (from a in _context.tblPrintBuffetItems
                              join b in _context.tblProducts on (int)a.ProductId equals b.ProductID
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
                                group a by new { a.ProductID, b.Description, a.Price } into g
                                select new Entity.Product
                                {
                                    ProductID = g.Key.ProductID,
                                    Description = g.Key.Description,
                                    Price = (float)g.Key.Price,
                                    ProductQty = (int)g.Sum(a => a.Qty),
                                    ProductTotal = ((float)g.Key.Price * (int)g.Sum(a => a.Qty))
                                    
                                }).ToList();
            //foreach (var item in collection)
            //{

            //}
            //var options = (from a in _context.tblOrderPartOptions
            //               join b in _context.tblProducts on a.ProductOptionID equals b.ProductID
            //               join c in _context.tblOrderParts on a.OrderPartId equals c.OrderPartID
            //               where c.OrderGUID == orderId
            //               select new
            //               {
            //                   Description = b.Description
            //               }).ToList();
            //if (options != null && options.Count > 0)
            //{
            //foreach (var op in options)
            //{
            //    item.Options += op.Description + ",";
            //}
            //}
            to.ServiceChargeApplicable = (bool)_context.tblOrders.Where(x => x.OrderGUID == orderId).Select(x=>x.ServiceChargeApplicable).FirstOrDefault();
           
           
            return Json(to, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetOrderedItemsbyMenu(string MenuId)
        {
            List<int> menuIds = MenuId.Split(',').Select(int.Parse).ToList();
            //List<int> menuIds = new List<int>();
            //menuIds.Add(3);
            //menuIds.Add(5);
            int maxItems = 6;
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
            //                        ChineseName = g.Key.ChineseName,
            //                        ProductID = g.Key.ProductId
            //                    }).ToList();


            //changes for new station Priority & Deserts 27/05/2021
            //MenuId for Priority =100, Dessert = 11

            //if menuid = 11 fetch items table wise and not cumulative
            List<OrderPart> orderedItems = new List<OrderPart>();
            if (Convert.ToInt32(MenuId) == 11)
            {
                orderedItems = (from a in _context.tblMenuItems
                                join b in _context.tblOrderBuffetItems on a.ProductID equals b.ProductId
                                join c in _context.tblProducts on a.ProductID equals c.ProductID
                                //where  a.MenuID == menuId &&
                                where menuIds.Contains(a.MenuID) &&
                                 DbFunctions.TruncateTime(b.DateCreated) == DbFunctions.TruncateTime(DateTime.Now)
                                group b by new { b.ProductId, b.BatchNo, c.Description, c.ChineseName, b.BatchTime, b.Processed, a.MenuID,b.OrderGUID } into g
                                select new OrderPart
                                {
                                    Name = g.Key.Description,
                                    Qty = (short)g.Count(),
                                    BatchNo = (int)g.Key.BatchNo,
                                    BatchTime = g.Key.BatchTime,
                                    Processed = g.Key.Processed,
                                    MenuId = g.Key.MenuID,
                                    ChineseName = g.Key.ChineseName,
                                    ProductID = g.Key.ProductId
                                   
                                }).ToList();
               
            }
            else if (Convert.ToInt32(MenuId) == 100)
            {
                orderedItems = (from a in _context.tblMenuItems
                                join b in _context.tblOrderBuffetItems on a.ProductID equals b.ProductId
                                join c in _context.tblProducts on a.ProductID equals c.ProductID
                                //where  a.MenuID == menuId &&
                                where a.MenuID != 11 && a.Priority == true &&
                                 DbFunctions.TruncateTime(b.DateCreated) == DbFunctions.TruncateTime(DateTime.Now)
                                group b by new { b.ProductId, b.BatchNo, c.Description, c.ChineseName, b.BatchTime, b.Processed, a.MenuID } into g
                                select new OrderPart
                                {
                                    Name = g.Key.Description,
                                    Qty = (short)g.Count(),
                                    BatchNo = (int)g.Key.BatchNo,
                                    BatchTime = g.Key.BatchTime,
                                    Processed = g.Key.Processed,
                                    MenuId = g.Key.MenuID,
                                    ChineseName = g.Key.ChineseName,
                                    ProductID = g.Key.ProductId
                                    
                                }).ToList();

            }
            else
            {
                orderedItems = (from a in _context.tblMenuItems
                                join b in _context.tblOrderBuffetItems on a.ProductID equals b.ProductId
                                join c in _context.tblProducts on a.ProductID equals c.ProductID
                                //where  a.MenuID == menuId &&
                                where menuIds.Contains(a.MenuID) && a.Priority == false &&
                                 DbFunctions.TruncateTime(b.DateCreated) == DbFunctions.TruncateTime(DateTime.Now)
                                group b by new { b.ProductId, b.BatchNo, c.Description, c.ChineseName, b.BatchTime, b.Processed, a.MenuID} into g
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
                                    
                                }).ToList();
            }


            if (orderedItems != null && orderedItems.Count > 0)
            {
                //if (Convert.ToInt32(MenuId) == 100)
                //    orderedItems = orderedItems.Where(x => x.Priority == true).ToList();
                //else
                //    orderedItems = orderedItems.Where(x => x.Priority == false).ToList();
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

                    //update batch no for new items 
                    string batchTime = DateTime.Now.ToString("HH:mm");
                    //if (ni != null && ni.Count > 0)
                    //{
                    //    //ni = ni.Where(x => menuIds.Contains(x.MenuID)).ToList();
                    //    foreach (var item in ni)
                    //    {
                    //        item.t1.BatchNo = batchNo;
                    //        item.t1.BatchTime = batchTime;
                    //        item.t1.Processed = false;
                    //    }
                    //    _context.SaveChanges();
                    //}

                    nwItems.ForEach(x => x.BatchNo = batchNo);

                    
                    var uniqueMenuId = nwItems.Select(x => x.MenuId).Distinct();
                   
                    foreach (var m in uniqueMenuId)
                    {
                        var mi = nwItems.Where(x => x.MenuId == m).ToList();
                        var mis = rh.SplitList(mi, maxItems);
                        foreach (var misi in mis)
                        {
                            OrderMenuItemsResponse omir = new OrderMenuItemsResponse();
                            omir.OrderTime = batchTime;
                            omir.BatchNo = batchNo;
                            omir.MenuItems.Add(misi);
                            omir.Processed = false;
                            omr.Add(omir);
                            foreach (var misit in misi)
                            {
                                foreach (var some in _context.tblOrderBuffetItems.Where(x => x.ProductId == misit.ProductID && x.BatchNo == 0).ToList())
                                {
                                    some.BatchNo = batchNo;
                                    some.BatchTime = batchTime;
                                }
                                _context.SaveChanges();
                            }
                            batchNo++;
                        }
                    }
                  
                    //DateTime ot = pl.OrderBy(x => x.DateCreated).Select(x => x.DateCreated).FirstOrDefault();
                   
                    //omir.MenuItems = nwItems;
                   
                }
            }
            if (omr.Count > 0)
                omr = omr.OrderByDescending(x => x.OrderTime).ThenBy(x => x.BatchNo).ToList();

            return Json(omr, JsonRequestBehavior.AllowGet);
        }
        public ActionResult RequestBill(int TableId, Guid OrderId, int UserId)

        {
            logger.Info("Request Bill - " + UserId);
            int row = 0;
            ServiceResponse response = new ServiceResponse();
            if (dbContext.tblOrders.Any(x => x.OrderGUID == OrderId && x.Paid == true && x.DelInd == false))
            {
                response.Message = "Please contact our staff at bar counter. Thanks.";
                response.Logout = true;
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
                    response.Message = "Bill requested. Our staff will handover the bill shortly";
                else
                    response.Message = "Oops!! Seems to be a connection issue. Please try again";
            }
            logger.Info("Request Bill Finished - " + UserId);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetVoucherDetails(string voucherCode, string orderId,int UserId)
        {
            logger.Info("get Voucher Details - " + UserId);
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
                if (voucherCode.Contains("GV"))
                {
                    voucherCode = voucherCode.Replace("GV", "PR");
                }
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
                        else if(vm.VoucherStatus == "TOO SOON")
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
                            else if (voucherCode.Substring(0, 2) == "PR" && vm.VoucherStatus == "VALID")
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
                                        text = vm.Description;
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
                                    }
                                    else
                                        vm.IsValid = false;
                                }
                            }
                            else if (voucherCode.Substring(0, 2) == "GV" && vm.VoucherStatus == "VALID")
                            {
                                Guid orderGUID = new Guid(orderId);
                                var orderAmount = dbContext.tblOrderParts.Where(x => x.OrderGUID == orderGUID && x.DelInd == false).Sum(x => x.Price);
                                if (orderAmount < Convert.ToDecimal(Math.Round(vm.DiscountAmount, 2)))
                                    vm.DiscountAmount = (decimal)orderAmount;
                                vm.VoucherValue = "£" + vm.VoucherValue + " Gift Voucher";
                                vm.OrderId = orderGUID;
                                vm.Price = vm.DiscountAmount;
                                //vm.Price = Convert.ToDecimal(vm.VoucherValue);
                                //vm.DiscountAmount = vm.Price;

                            }
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
                logger.Info("Voucher Details Fetched - " + UserId);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                vm.IsValid = false;
                vm.Description = ex.Message;
                if (ex.InnerException != null)
                    vm.Description = ex.InnerException.Message;
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
                var buffetItems = orderItems.Where(x => (x.Description.Contains("Buffet") || (x.EnglishName != null && x.EnglishName.Contains("Buffet")))).ToList();
                var drinkItems = orderItems.Where(x => !x.Description.Contains("Buffet") && !x.Description.Contains("Meal")).ToList();
                var appliedAdCount = 0;
                var appliedKdCount = 0;
                var appliedJnCount = 0;
                var appliedSBPCount = 0;
                var appliedPPPCount = 0;
                var singleVoucherTypes = new List<string>() { "FBI", "FTI", "FBD",  "BDY", "PBA", "PBK", "PBJ", "PBE", "PBD" };
                var multiVoucherTypes = new List<string>() { "FBA", "FBK", "FBJ", "SBP", "PPP","VAR" };
                var adBufCount = buffetItems.Where(x => (x.Description.Contains("Adult") || (x.EnglishName != null && x.EnglishName.Contains("Adult")))).Count();
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
                        var varVoucherTypes = new List<string>() { "F", "P", "S", "V" };
                        if (varVoucherTypes.Contains(item.VoucherValue.Substring(0, 1)))
                        {
                            string vv = "";
                            if (item.VoucherValue.Length > 1)
                                vv = item.VoucherValue.Substring(0, 3);
                            else if (item.VoucherValue == "V")
                                vv = "VAR";
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
                    var bufTotal = buffetItems.Where(x => (x.Description.Contains("Adult")|| (x.EnglishName != null && x.EnglishName.Contains("Adult")))).Sum(x => x.Price);
                    promotionAmount = (decimal)(bufTotal * (Convert.ToInt32(vm.VoucherValue.Substring(3)) * (decimal)0.01));
                    if (adBufCount == 0 || bufTotal == 0)
                    {
                        vm.IsValid = false;
                        vm.Message = "Add Adult buffet to apply this code";
                    }

                }
                else if (vm.VoucherValue.Contains("PBK"))
                {
                    var bufTotal = buffetItems.Where(x => (x.Description.Contains("Kid") || (x.EnglishName != null && x.EnglishName.Contains("Kid")))).Sum(x => x.Price);
                    promotionAmount = (decimal)(bufTotal * (Convert.ToInt32(vm.VoucherValue.Substring(3)) * (decimal)0.01));
                    if (kdBufCount == 0 || bufTotal == 0)
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
                    if (jnBufCount == 0 || bufTotal == 0)
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
                    else if (adBufCount > 0)
                    {
                        var bufTotal = buffetItems.Where(x => (x.Description.Contains("Adult") || ((x.EnglishName != null && x.EnglishName.Contains("Adult"))))).Sum(x => x.Price) / adBufCount;
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
                else if (vm.VoucherValue.Contains("VAR"))
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
                    if (promProducts == null || appliedPPPCount >= promProducts.Count || promProducts.Count == 0)
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
                         orderItems.Where(x => (x.Description.Contains("Adult") || ((x.EnglishName != null && x.EnglishName.Contains("Adult"))))).Count() < 2)
                {
                    vm.IsValid = false;
                    vm.Message = "Add Adult Buffet to apply this code";
                }

            }

            //vm.VoucherValue = promotionAmount.ToString();

            return vm;
        }

        public ActionResult RedeemVoucher(VoucherModel vm)
        {
            logger.Info("Redeem Voucher - " + vm.UserId);
            decimal promotionAmount = vm.DiscountAmount;
            //Call API to mark voucher redeemed in DB
            int prDscountId = Convert.ToInt32(ConfigurationManager.AppSettings["PromotionDiscount"]);
            int onlineDepositId = Convert.ToInt32(ConfigurationManager.AppSettings["OnlineDepositId"]);
            int giftVoucherId = Convert.ToInt32(ConfigurationManager.AppSettings["GiftVoucherId"]);
            int restId = Convert.ToInt32(ConfigurationManager.AppSettings["RestaurantId"]);
            if (vm.VoucherCode.Substring(0, 2) == "PR" || vm.VoucherCode.Substring(0, 2) == "GV")
            {
                vm.VoucherType = VoucherTypes.Promotion.ToString();
                if (vm.VoucherCode.Substring(0, 2) == "GV")
                    vm.VoucherCode = vm.VoucherCode.Replace("GV", "PR");
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

                //Validations based on already applied vouchers
                if (promotionAmount > 0)
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

                //check to confirm the code is already used
                //string rCode = vm.VoucherCode.Substring(2);
                //var reservationCodeDetail = (from a in dbContext.tblReservations
                //                       join b in dbContext.tblOrders on a.OrderGUID equals b.OrderGUID
                //                       join c in dbContext.tblTables on b.TableID equals c.TableID
                //                       where a.UniqueCode.Contains(rCode) && a.UniqueCodeUsed == true
                //                       select new
                //                       {
                //                           TableNumber = c.TableNumber,
                //                           DateRedeemed = b.DateCreated
                //                       }).FirstOrDefault();
                //var orderVoucherDetail = (from a in dbContext.tblOrderVouchers
                //                          join b in dbContext.tblOrders on a.OrderGUID equals b.OrderGUID
                //                          join c in dbContext.tblTables on b.TableID equals c.TableID
                //                          where a.VoucherNumber.Contains(rCode) 
                //                          select new
                //                          {
                //                              TableNumber = c.TableNumber,
                //                              DateRedeemed = b.DateCreated
                //                          }).FirstOrDefault();
                //if (dbContext.tblReservations.Any(x => x.UniqueCode.Contains(rCode) && x.UniqueCodeUsed == true)
                //    || (dbContext.tblOrderVouchers.Any(x => x.VoucherNumber.Contains(rCode))))
                //if (reservationCodeDetail != null || orderVoucherDetail != null)
                //{
                //    vm.Message = "Voucher already used" + reservationCodeDetail != null ? (reservationCodeDetail.TableNumber + reservationCodeDetail.DateRedeemed.ToString("dd-MMM")) :
                //                                          orderVoucherDetail != null ? (orderVoucherDetail.TableNumber + orderVoucherDetail.DateRedeemed.ToString("dd-MMM")) : "";
                //}
                vm.Message = rh.ReservationVoucherUsed(vm.VoucherCode);
                
                if(vm.Message == "")
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
                    var res = dbContext.tblReservations.Where(x => x.UniqueCode == vm.VoucherCode).FirstOrDefault();
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
            logger.Info("Redeem Voucher End - " + vm.UserId);

            return Json(vm.Message, JsonRequestBehavior.AllowGet);
        }
        //[AllowCrossSite]
        [HttpPost]
        public ActionResult VerifyOrderProducts(TableOrder req)
        {
            VerifyOrderProductsResponse response = new VerifyOrderProductsResponse();
            response.IsSuccess = true;
            response.Message = "";
            response.ShowOptionButtons = false;
            string unAvailableProducts = "";
            int approxDeliveryTime = 12;
            var foodItems = req.tableProducts.Where(x => x.FoodRefil == true).ToList();
            try
            {
                if (foodItems != null && foodItems.Count > 0)
                {
                    //check for inactive products
                    using (var _context = new ChineseTillEntities1())
                    {
                        DateTime updatedTime = DateTime.Now.AddMinutes(-180);
                        var unAvailableproductIds = _context.tblMenuItems.Where(x => x.LastModified != null && x.LastModified >= updatedTime && x.Active == false).Select(x => x.ProductID).Distinct().ToList();
                        if (unAvailableproductIds != null && unAvailableproductIds.Count > 0)
                        {
                            foreach (var item in foodItems)
                            {
                                if (unAvailableproductIds.Contains(item.ProductID))
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
                            var totalUnPrintedItems = _context.tblPrintBuffetItems.Where(x => x.Printed == false && DbFunctions.TruncateTime(x.DateOrdered) == DbFunctions.TruncateTime(DateTime.Now)).ToList();

                            var orderUnPrintedItems = totalUnPrintedItems.Where(x => x.OrderGUID == req.OrderGUID).ToList();
                            if (totalUnPrintedItems != null && totalUnPrintedItems.Count > 0)
                            {
                                var totalUnprintedItemsCount = totalUnPrintedItems.Sum(x => x.Qty);
                                if (totalUnprintedItemsCount <= 75)
                                    approxDeliveryTime = 12;
                                else if (totalUnprintedItemsCount <= 100)
                                    approxDeliveryTime = 15;
                                else
                                    approxDeliveryTime = 18;
                            }

                            if (foodItems.Sum(x => x.ProductQty) > maxDishesAllowed)
                            {
                                response.IsSuccess = false;
                                response.ShowOptionButtons = true;
                                //response.Message = "This order exceeds the " + maxDishesAllowed + " dishes per order for your table. It may take twice as long to deliver. To meet the current delivery time of approx" + approxDeliveryTime + " minutes, please reduce the number of dishes.";
                                response.Message = "Your order exceeds the recommended THREE dishes/person/time, delaying delivery. For a quicker delivery, reduce the amount in the basket before resubmitting.";
                            }

                            if (orderUnPrintedItems != null && orderUnPrintedItems.Count > 0)
                            {
                                var unPrintedItemsCount = orderUnPrintedItems.Sum(x => x.Qty);
                                if (unPrintedItemsCount + foodItems.Count > maxDishesAllowed)
                                {
                                    response.IsSuccess = false;
                                    //response.Message = "The accumulative order exceeds " + maxDishesAllowed + " dishes per order for your table.  It may take twice as long to deliver. To meet the current delivery time of approx " + approxDeliveryTime + " minutes, please reduce the number of dishes.";
                                    response.ShowOptionButtons = true;
                                    response.Message = "Your order exceeds the recommended THREE dishes/person/time, delaying delivery. For a quicker delivery, reduce the amount in the basket before resubmitting.";

                                }
                            }

                        }
                    }
                }

            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = "Connection error. Only submit again once you have checked your order history to avoid duplicate orders.";
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetTablesBySection(int sectionId, int UserId)
        {
            logger.Info("Get All Tables By Section - " + UserId);
            Models.UserService us = new Models.UserService();
            TablesList tablesList = new TablesList();
            tablesList = us.GetTablesListV2(sectionId);
            //if (sectionId == 100)
            //    tablesList = us.GetTablesListV2();
            //else
            //    tablesList = us.GetTablesBySection(sectionId);
            logger.Info("Get All Tables By Section Fetched - " + UserId);
            return Json(tablesList, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetAllPrintBatches()
        {
            List<PrintingBatch> pb = new List<PrintingBatch>();
            Models.OrderService os = new Models.OrderService();
            List<string> printers = new List<string>();
            using (var _context = new ChineseTillEntities1())
            {
               printers =  _context.tblPrinters.Where(x => x.KitchenPrinter == true).Select(x => x.PrinterName).ToList();
            }
            pb = os.GetPrintBatches(printers);

            return Json(pb, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAllPrintBatchesByTable()
        {
            List<TablePrintingBatch> tpbList = new List<TablePrintingBatch>();
            List<PrintingBatch> pb = new List<PrintingBatch>();
            List<string> printers = new List<string>();
            using (var _context = new ChineseTillEntities1())
            {
                printers= _context.tblPrinters.Where(x => x.KitchenPrinter == true).Select(x => x.PrinterName).ToList();
            }

            Models.OrderService os = new Models.OrderService();

            pb = os.GetPrintBatches(printers);
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

       

        public ActionResult ReprintKitchenReceipt(int pqId, int userId)
        {
            string response = "";
            try
            {
                using (var _context = new ChineseTillEntities1())
                {
                    var receipt = _context.tblPrintQueues.Where(x => x.PrintQueueID == pqId).FirstOrDefault();
                    
                    //int latestBatchNo = _context.tblPrintQueues.Where(x => DbFunctions.TruncateTime(x.DateCreated) == DbFunctions.TruncateTime(DateTime.Now)).OrderByDescending(x => x.BatchNo).Select(x => x.BatchNo).FirstOrDefault();
                    if (receipt != null)
                    {
                        string newreciept = "***Duplicate***" + Environment.NewLine + receipt.Receipt;
                        receipt.Receipt = newreciept;
                        tblPrintQueue tp = new tblPrintQueue();
                        receipt.DateCreated = DateTime.Now;
                        receipt.DatePrinted = null;
                        receipt.Processed = false;
                        receipt.OldBatchNo = receipt.BatchNo;
                        receipt.TicketNo = receipt.TicketNo + "R";
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
        public ActionResult GetTableOrder(int t, int UserId)
        {
            logger.Info("Getting OrderedItems - " + UserId);
            Models.OrderService os = new Models.OrderService();
            TableOrder to = os.GetTableOrderV2(t);
            logger.Info("OrderedItems Fetched - " + UserId);
            return Json(to, JsonRequestBehavior.AllowGet);
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
                    if (!to.PayAsYouGo || (to.PayAsYouGo && _context.tblOrderParts.Any(x => x.OrderGUID == to.OrderGUID)))
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
                    string appSecondaryURL = ConfigurationManager.AppSettings["AppSecondaryUrl"];

                    string qrcodeText = Convert.ToString(code) + ";" + appURL + ";" + appSecondaryURL;
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
                    using (var _context = new ChineseTillEntities1())
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
                    if (item.Substring(0, 1) != "-")
                        q.AllocTable += item + ",";
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



        
            return Json(r, JsonRequestBehavior.AllowGet);
        }
    }

}