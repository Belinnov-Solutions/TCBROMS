using Deznu.Products.Common.Utility;
using Entity;
using Stripe;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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
    public class MobileAPIController : Controller
    {
        ChineseTillEntities1 dbContext = new ChineseTillEntities1();
        ROMSHelper rh = new ROMSHelper();
        // GET: MobileAPI
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult RequestBill(int TableId, Guid OrderId)

        {
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
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult WaiterService(Guid orderId)
        {
            ServiceResponse response = new ServiceResponse();
            if (dbContext.tblOrders.Any(x => x.OrderGUID == orderId && x.Paid == true && x.DelInd == false))
            {
                response.Message = "Please contact our staff at bar counter. Thanks.";
                response.Logout = true;
            }
            else
            {
                try
                {
                    var table = (from a in dbContext.tblOrders
                                 join b in dbContext.tblTables on a.TableID equals b.TableID
                                 where a.OrderGUID == orderId
                                 select b).FirstOrDefault();
                    if (table.CurrentStatus != (int)TableState.WaiterService)
                    {
                        table.PastStatus = table.CurrentStatus;
                        table.CurrentStatus = (int)TableState.WaiterService;
                        dbContext.Entry(table).State = EntityState.Modified;
                        dbContext.SaveChanges();
                        //DateTime todayDate = 
                        var devices = dbContext.tblAppUsers.Where(x => x.DeviceType == "App" && x.LogoutDate == null && x.Token != null
                                                                     && DbFunctions.TruncateTime(x.LoginDate) == DbFunctions.TruncateTime(DateTime.Now))
                                                                    .Select(x => x.Token).ToList();
                        foreach (var item in devices)
                        {
                            rh.SendNotificationToCUs(item, "Waiter Service requested at table :" + table.TableNumber);
                        }
                    }
                    response.Message = "Our staff will serve you soon";

                }
                catch (Exception ex)
                {
                    response.Message = "Oops!! Seems to be a connection issue. Please try again";

                }
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SubmitOrder(TableOrder t)

        {
            Models.OrderService os = new Models.OrderService();
            OrderSubmitResponse r = os.SubmitOrderV1(t);
            return Json(r, JsonRequestBehavior.AllowGet);
        }



        public ActionResult ConfirmOrderPayment(Payment req)
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
                    //var dp = req.SplitProduct.Find(x => x.ProductID == 1909);
                    //if (dp != null)
                    //{
                    //    tblOrderPart top = new tblOrderPart();
                    //    top.ProductID = dp.ProductID;
                    //    top.OrderGUID = req.OrderGUID;
                    //    top.DateCreated = DateTime.Now;
                    //    top.LastModified = DateTime.Now;
                    //    top.Price = (decimal)dp.ProductTotal;
                    //    top.Total = (decimal)dp.ProductTotal;
                    //    top.DelInd = false;
                    //    top.WebUpload = false;
                    //    top.Qty = 1;
                    //    dbContext.tblOrderParts.Add(top);
                    //    dbContext.SaveChanges();

                    //}
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
                    topy.PaymentType = "MP";
                    topy.PCName = primaryTILL;
                    dbContext.tblOrderPayments.Add(topy);
                    dbContext.SaveChanges();

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
                    response = "success";
                }
            }
            catch (Exception ex)
            {

                response = ex.Message + ex.InnerException.Message;
                //response = "Thank you for the payment. Your table payment is not updated on our TILL. Kindly show this message to manager. We apologise the inconvenience caused. Thanks.";
                //throw;
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }
            
    

  

    public ActionResult CalculatePromotionDiscount(PromotionDiscount pd)
    {
        Entity.Product discountProduct = new Entity.Product();
        discountProduct = rh.CalculateDiscount(pd.billableProducts, pd.custCount);
        return Json(discountProduct, JsonRequestBehavior.AllowGet);
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
}
}