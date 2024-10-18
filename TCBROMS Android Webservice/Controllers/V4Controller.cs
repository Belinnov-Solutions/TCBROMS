using Entity;
using Entity.Enums;
using NLog;
using Stripe;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using TCBROMS_Android_Webservice.Helpers;
using TCBROMS_Android_Webservice.Models;

namespace TCBROMS_Android_Webservice.Controllers
{

    //[AllowCrossSite]
    public class V4Controller : Controller
    {
        ROMSHelper rh = new ROMSHelper();
        Logger logger = LogManager.GetLogger("databaseLogger");


        // GET: V4
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SubmitOrder(TableOrder t)
        {
            Models.OrderService os = new Models.OrderService();
            OrderSubmitResponse r = os.SubmitOrderV4(t);
            return Json(r, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTableOrder(Guid orderid, int UserId)
        {
            logger.Info("Getting OrderedItems - " + UserId);
            Models.OrderService os = new Models.OrderService();
            TableOrder to = os.GetTableOrderV4(orderid);
            logger.Info("OrderedItems Fetched - " + UserId);
            return Json(to, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Method to delete 15 days old data from tblPrintBuffetItems, tblOrderbUffetItems, tblPrintQueue, tblTableOrder
        /// Delete all older QR images to preserve disk space
        /// Rebuild Indexes
        /// </summary>
        /// <returns></returns>
        public ActionResult DataCleanup()
        {
            string response = "";
            ROMSHelper rh = new ROMSHelper();
            try
            {
                //Delete Data from tables
                rh.Executestoredprocedurewithoutparameters("usp_BS_DeleteOldData");
                response += "; Data deleted";


                //Delete QR code images from 
                string QRImageUrl = ConfigurationManager.AppSettings["QRImageUrl"];
                string folderPath = "~/Content/Images/QRCode/";
                folderPath = Server.MapPath(folderPath);
                response += folderPath;
                //var deletionDate = DateTime.Now.AddDays(-15);
                //using (var _context = new ChineseTillEntities1())
                //{
                //    //var codes = _context.tblTableOrders.Where(x => x.DateCreated < deletionDate).Select(x => x.UniqueCode).ToList();
                //    var codes = _context.tblTableOrders.Select(x => x.UniqueCode).ToList();

                //    if (codes != null & codes.Count > 0)
                //    {
                //        foreach (var code in codes)
                //        {
                //            string imagePath = "~/Content/Images/QRCode/" + code.ToString() + ".jpg";
                //            if (System.IO.File.Exists(imagePath))
                //            {
                //                System.IO.File.Delete(imagePath);
                //                response += imagePath;
                //            }
                //        }
                //    }
                //}
                
                //folderPath = folderPath.Replace("~", QRImageUrl);
                response += rh.DeleteImageFiles(folderPath);
                //Console.WriteLine("Image files deleted successfully.");
                //response = "Images deleted";

               

                //Rebuild indexes with fragmentation over 50%
                rh.Executestoredprocedurewithoutparameters("usp_BS_RebuildIndexes");
                response += "; Index Rebuild";

            }
            catch (Exception ex)
            {
                response += ex.Message;
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        //[AllowCrossSite]
        //public ActionResult ConfirmStripePayment(Payment req)
        //{
        //    //string response = "";
        //    Guid orderId = new Guid();
        //    PaymentResponse pr = new PaymentResponse();
        //    pr.IsSuccess = req.IsSuccess;
        //    bool giftVouchersBought = false;
        //    try
        //    {
        //        using (var dbContext = new ChineseTillEntities1())
        //        {
        //            //var to = dbContext.tblOrders.Where(x => x.OrderGUID == req.OrderGUID).FirstOrDefault();
        //            // var totalCount = 0;

        //            //foreach (var item in req.SplitProduct)
        //            //{
        //            //    totalCount += item.ProductQty;
        //            //}
        //            //var orderPartCount = dbContext.tblOrderParts.Where(x => x.OrderGUID == req.OrderGUID && x.DelInd == false).Sum(x => x.Qty);
        //            //if (orderPartCount > totalCount)
        //            //    req.isSplitPayment = true;
        //            if (req.IsSuccess)
        //            {
        //                pr = rh.CompletePaymentStripe(req);
        //                //Update Activity
        //                string activityType = ActivityType.DineIn.ToString();
        //            }
        //            else
        //            {
        //                using (TransactionScope scope = new TransactionScope())
        //                {
        //                    //Print Payment Confirmation slip for the Order
        //                    var tsp = dbContext.tblStripePayments.Where(x => x.VendorTxCode == req.TxCode && x.OrderGUID != null && x.OrderGUID != Guid.Empty).FirstOrDefault();
        //                    if (tsp != null)
        //                    {
        //                        var tableNumber = (from a in dbContext.tblOrders
        //                                           join b in dbContext.tblTables on a.TableID equals b.TableID
        //                                           where a.OrderGUID == tsp.OrderGUID
        //                                           select new
        //                                           {
        //                                               b.TableNumber
        //                                           }).FirstOrDefault();
        //                        try
        //                        {
        //                            if (req.FailureMessage != "ABORT")
        //                            {
        //                                string confirmationStr = "";
        //                                confirmationStr += "       " + DateTime.Now.ToString("dd-MM-yyyy") + Environment.NewLine;
        //                                confirmationStr += "Failed payment of " + Environment.NewLine;
        //                                confirmationStr += "£" + String.Format("{0:0.00}", tsp.Amount) + " for Table-" + tableNumber.TableNumber + Environment.NewLine;
        //                                confirmationStr += Environment.NewLine;
        //                                confirmationStr += "We regret the payment failed." + Environment.NewLine;
        //                                confirmationStr += req.FailureMessage + Environment.NewLine;
        //                                confirmationStr += "-------------------------------------------------" + Environment.NewLine;
        //                                tblPrintQueue tp = new tblPrintQueue();
        //                                tp.Receipt = confirmationStr;
        //                                tp.PCName = "App";
        //                                tp.ToPrinter = "Bar";
        //                                tp.UserFK = -10;
        //                                tp.DateCreated = DateTime.Now;
        //                                dbContext.tblPrintQueues.Add(tp);
        //                                dbContext.SaveChanges();
        //                            }
        //                            tsp.Success = false;
        //                            tsp.LastModified = DateTime.Now;
        //                            tsp.PaymentId = req.PaymentId;
        //                            tsp.VendorTxCode = req.TransactionID;
        //                            //tsp.OrderID = orderId;
        //                            tsp.FailureMessage = req.FailureMessage;
        //                            dbContext.Entry(tsp).State = EntityState.Modified;
        //                            dbContext.SaveChanges();
        //                            scope.Complete();
        //                            pr.Message = "Payment Failed for amount " + "£" + String.Format("{0:0.00}", tsp.Amount) + Environment.NewLine + req.FailureMessage;

        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            pr.Message = ex.Message;
        //                            //response = ex.Message;
        //                        }
        //                    }
        //                }
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

        //    return Json(pr, JsonRequestBehavior.AllowGet);
        //}

        //[AllowCrossSite]

        public ActionResult ConfirmStripePayment(Payment req)
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

                    pr = rh.CompletePaymentStripe(req);

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

        public ActionResult GetStripePaymentIntent(Payment req)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["TCBROMSConnection"].ConnectionString;
            string res = "";
            bool error = false;
            req.TablePaid = false;
            bool splitPayment = false;
            PaymentResponse prs = new PaymentResponse();
            string vxcode = "1-" + Guid.NewGuid().ToString();
            decimal payableAmount = 0;
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
                    //if (req.Amount != (payableAmount * 100))
                    //    req.Amount = (payableAmount + req.TipAmount + req.ServiceCharge) * 100;
                    if (req.Amount != (payableAmount))
                        req.Amount = (payableAmount + req.TipAmount + req.ServiceCharge) ;
                    if (req.Amount <= 0)
                    {
                        req.Amount = 0;
                        prs = rh.CompletePaymentStripe(req);
                        req.ClientSecret = "";
                        req.FailureMessage = prs.Message;
                    }
                    else
                    {
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
                            Amount = (long)req.Amount * 100,
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
                            req.TxCode = vxcode;
                            //Insert a record in tblStripPayment for recording purpose
                            tblStripePayment tsp = new tblStripePayment();
                            tsp.Amount = (decimal)req.Amount;
                            tsp.ClientSecret = req.ClientSecret;
                            tsp.DateCreated = DateTime.Now;
                            tsp.LastModified = DateTime.Now;
                            tsp.OrderGUID = req.OrderGUID;
                            tsp.Success = false;
                            tsp.SplitPayment = splitPayment;
                            tsp.VendorTxCode = req.TxCode;
                            tsp.DeviceType = req.DeviceType;
                            tsp.MobileNo = req.Mobile;
                            tsp.TipAmount = (int)req.TipAmount;
                            tsp.SCAmount = (double)req.ServiceCharge;
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
    }
}