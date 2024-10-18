using Entity;
using Entity.AdminDtos;
using Entity.Enums;
using Entity.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using TCBROMS_Android_Webservice.Models;

namespace TCBROMS_Android_Webservice.Helpers
{
    public class ROMSHelper
    {
        ChineseTillEntities1 _context = new ChineseTillEntities1();
        string url = ConfigurationManager.AppSettings["TCBAPIUrl"];
        CustomerService cs = new Models.CustomerService();
        public int GenerateUniqueCode()
        {
            Random random = new Random();
            int code = 0;
            while (code == 0)
            {
                code = random.Next(1000, 9999);
                if (_context.tblTableOrders.Any(x => x.UniqueCode == code && x.Active == true))
                    code = 0;
            }
            return code;
        }


        public List<ProductGroupModel> GetBuffetMenu()
        {
            int currTime = Convert.ToInt32(DateTime.Now.ToString("HHMM"));
            int currDay = (int)DateTime.Today.DayOfWeek;
            if (currDay == 0)
                currDay = 7;
            List<ProductGroupModel> buffetList = new List<ProductGroupModel>();
            buffetList = (from a in _context.tblMenus
                          where a.DelInd == false
                          select new ProductGroupModel
                          {
                              Group = new ProductGroup
                              {
                                  Groupname = a.MenuName.Replace(System.Environment.NewLine, string.Empty),
                                  //Groupname = a.MenuName,
                                  ProductGroupID = a.MenuID,
                                  SortOrder = a.SortOrder
                              },
                              GroupProducts = (from mi in _context.tblMenuItems
                                               join pr in _context.tblProducts on mi.ProductID equals pr.ProductID
                                               join pt in _context.tblProductTimeRestrictions on mi.ProductID equals pt.ProductID
                                               where mi.MenuID == a.MenuID && mi.DelInd == false && mi.Active == true && pr.FoodRefil == true
                                               && pr.IsTakeaway == false && pt.DayID == currDay && currTime > pt.StartTime && currTime < pt.EndTime
                                               select new Entity.Product
                                               {
                                                   ProductID = pr.ProductID,
                                                   Description = pr.Description,
                                                   ProductGroupID = mi.MenuID,
                                                   ImageName = pr.ImageName,
                                                   bVegetarain = (bool)(pr.bVegetarain != null ? pr.bVegetarain : false),
                                                   bSpicy = pr.bSpicy != null ? pr.bSpicy : 0,
                                                   bGlutenFree = (bool)(pr.bGlutenFree != null ? pr.bGlutenFree : false),
                                                   vAllergens = pr.vAllergens,
                                                   menuDescripton = pr.vMenuDescription,
                                                   bOnsite = mi.bOnsite == null ? true : mi.bOnsite,
                                                   ProductAvailable = mi.Active,
                                                   SortOrder = (int)(mi.SortOrder ?? 0),
                                                   RewardPoints = (int)(pr.RewardPoints == null ? 0 : pr.RewardPoints),
                                                   RedemptionPoints = (int)(pr.RedemptionPoints == null ? 0 : pr.RedemptionPoints),
                                                   Price = (float) pr.Price,
                                                   FoodRefil = true,
                                                   Calories = pr.Calories,
                                                   EnglishName = pr.EnglishName,
                                                   ChineseName = pr.ChineseName
                                               }).OrderBy(x => x.SortOrder).ToList()

                          }).OrderBy(a => a.Group.SortOrder).ToList();
            //buffetList = (from a in _context.tblMenus
            //              where a.DelInd == false
            //              select new ProductGroupModel
            //              {
            //                  Group = new ProductGroup
            //                  {
            //                      Groupname = a.MenuName.Replace(System.Environment.NewLine, string.Empty),
            //                      //Groupname = a.MenuName,
            //                      ProductGroupID = a.ID,
            //                  }

            //              }).ToList();
            //foreach (var item in buffetList)
            //{
            //    item.GroupProducts = (from mi in _context.tblMenuItems
            //                          join pr in _context.tblProducts on mi.ProductID equals pr.ProductID
            //                          join pt in _context.tblProductTimeRestrictions on mi.ProductID equals pt.ProductID
            //                          where mi.MenuID == item.Group.ProductGroupID && mi.DelInd == false && mi.Active == true
            //                          && pt.DayID == currDay && currTime > pt.StartTime && currTime < pt.EndTime
            //                          select new Entity.Product
            //                          {
            //                              ProductID = pr.ProductID,
            //                              Description = pr.Description,
            //                              ProductGroupID = mi.MenuID,
            //                              ImageName = pr.ImageName,
            //                              bVegetarain = (bool)(pr.bVegetarain != null ? pr.bVegetarain : false),
            //                              bSpicy = pr.bSpicy != null ? pr.bSpicy : 0,
            //                              bGlutenFree = (bool)(pr.bGlutenFree != null ? pr.bGlutenFree : false),
            //                              vAllergens = pr.vAllergens,
            //                              menuDescripton = pr.vMenuDescription
            //                          }).ToList();
            //}
            buffetList.RemoveAll(noProducts);
            return buffetList;
        }

        public List<ProductGroupModel> GetBuffetMenuV3()
        {
            int currTime = Convert.ToInt32(DateTime.Now.ToString("HHMM"));
            int currDay = (int)DateTime.Today.DayOfWeek;
            if (currDay == 0)
                currDay = 7;
            List<ProductGroupModel> buffetList = new List<ProductGroupModel>();
            try
            {


                buffetList = (from a in _context.tblMenus
                              where a.DelInd == false
                              select new ProductGroupModel
                              {
                                  Group = new ProductGroup
                                  {
                                      Groupname = a.MenuName.Replace(System.Environment.NewLine, string.Empty),
                                      //Groupname = a.MenuName,
                                      ProductGroupID = a.MenuID,
                                      //ParentGroupID = a.ParentMenuId == null ? 0 : a.ParentMenuId,
                                      SortOrder = a.SortOrder == null ? 100 : a.SortOrder,
                                  },
                                  GroupProducts = (from mi in _context.tblMenuItems
                                                   join pr in _context.tblProducts on mi.ProductID equals pr.ProductID
                                                   join pt in _context.tblProductTimeRestrictions on mi.ProductID equals pt.ProductID
                                                   where mi.MenuID == a.MenuID && mi.DelInd == false && mi.Active == true
                                                   && pt.DayID == currDay && currTime > pt.StartTime && currTime < pt.EndTime
                                                   select new Entity.Product
                                                   {
                                                       ProductID = pr.ProductID,
                                                       Description = pr.Description,
                                                       ProductGroupID = mi.MenuID,
                                                       ImageName = pr.ImageName,
                                                       bVegetarain = (bool)(pr.bVegetarain != null ? pr.bVegetarain : false),
                                                       bSpicy = pr.bSpicy != null ? pr.bSpicy : 0,
                                                       bGlutenFree = (bool)(pr.bGlutenFree != null ? pr.bGlutenFree : false),
                                                       vAllergens = pr.vAllergens,
                                                       menuDescripton = pr.vMenuDescription,
                                                       bOnsite = mi.bOnsite == null ? true : mi.bOnsite,
                                                       ProductAvailable = mi.Active,
                                                       SortOrder = (int)mi.SortOrder,
                                                       RewardPoints = (int)(pr.RewardPoints == null ? 0 : pr.RewardPoints),
                                                       FoodRefil = true,

                                                   }).OrderBy(x => x.SortOrder).ToList()

                              }).OrderBy(a => a.Group.SortOrder).ToList();
            }
            catch (Exception ex)
            {

                throw;
            }
            //foreach (var item in buffetList)
            //{
            //    if (_context.tblMenus.Any(x => x.ParentMenuId == item.Group.ProductGroupID))
            //    {
            //        var childGroups = (from a in _context.tblMenus
            //                           where a.ParentMenuId == item.Group.ProductGroupID && a.DelInd == false
            //                           select new Entity.Product
            //                           {
            //                               ProductID = a.MenuID,
            //                               Description = a.MenuName,
            //                               ProductGroupID = a.MenuID,
            //                               ParentGroupID = item.Group.ProductGroupID,
            //                               ImageName = a.ImageName == null ? "" : a.ImageName,
            //                               Price = 0,
            //                               FoodRefil = false,
            //                               ProductTypeId = 0,
            //                               Type = "Group",
            //                               IsFixedProducts = true,
            //                               bOnsite = true,
            //                               ProductAvailable = true,
            //                           }).ToList();
            //        item.GroupProducts.AddRange(childGroups);
            //    }
            //}
            buffetList.RemoveAll(noProducts);
            return buffetList;
        }
        public bool noProducts(ProductGroupModel pg)
        {
            if (pg.GroupProducts.Count == 0 || pg.GroupProducts == null)
                return true;
            else
                return false;
        }

        
        //public bool noProductsinGroup(Product pr,List<ProductGroupModel> pgList)
        //{
        //    if (pgList.Find == 0 || pg.GroupProducts == null)
        //        return true;
        //    else
        //        return false;
        //}

        public bool alcoholProducts(Product pg)
        {
            string[] alcoholProductTypes = ConfigurationManager.AppSettings["AlcoholProductTypes"].Split(',');
            if (Array.Exists(alcoholProductTypes, x => x == pg.ProductTypeId.ToString()))
                return true;
            else
                return false;
        }


        public bool drinksMenu(Product pg)
        {
            string[] drinksProductTypes = ConfigurationManager.AppSettings["DrinksProductTypes"].Split(',');
            if (Array.Exists(drinksProductTypes, x => x == pg.ProductTypeId.ToString()))
                return true;
            else
                return false;
        }

        public void WriteToFile(string text)
        {
            string path = "~/Content/TCBROMS_log.txt";
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(string.Format(text, DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt")));
                writer.Close();
            }
        }

        internal Product CalculateDiscount(List<Product> tableProducts, int? custCount)
        {
            float amount = 0;
            string[] discountedProductTypes = ConfigurationManager.AppSettings["DiscountedProductTypes"].Split(',');

            //int[] dps = Array.ConvertAll(discountedProductTypes,s=>int.Parse(s));

            int discountedProductId = Convert.ToInt32(ConfigurationManager.AppSettings["DiscountedProductId"]);

            foreach (var item in tableProducts)
            {
                if (item.ProductTypeId == null || item.ProductTypeId == 0)
                    item.ProductTypeId = _context.tblProducts.Where(x => x.ProductID == item.ProductID).Select(x => x.ProductTypeID).FirstOrDefault();
                if (Array.Exists(discountedProductTypes, x => x == item.ProductTypeId.ToString()))
                {
                    if (item.ProductTotal == 0 && item.Price > 0)
                        amount += item.Price;
                    else if (item.ProductTotal > 0)
                        amount += item.ProductTotal;
                }
            }
            float? discountAmount = (float)Math.Round((amount / 2), 2);
            if (discountAmount > (10 * custCount))
                discountAmount = (float)10 * custCount;
            var discountProduct = _context.tblProducts.Where(x => x.ProductID == discountedProductId).FirstOrDefault();
            Product dp = new Product();
            dp.ProductID = discountProduct.ProductID;
            dp.Description = discountProduct.Description;
            dp.Price = (float)(-1 * discountAmount);
            dp.ProductTotal = (float)(-1 * discountAmount);
            dp.ProductQty = 1;
            return dp;
        }
        public FCMResponse SendNotificationToCUs(string token, string message)
        {

            FCMResponse fr = new FCMResponse();
            string serverKey = "";
            serverKey = "AAAAS0j5dao:APA91bHJwHR69XSRBrjNaYT2XNLCWf1Ct1PVugHn95yicmaLezIjcqpnznPuyqq7x-oxnkz2W8a0R1hKRUNWUcPtukmnQ3geLTyiOeZO3dOeZsGuebjpClUnDt99Z72QbQxpYgVXeym09kcKo8r0xXmq3aV4XGe_EA";
            try
            {
                var result = "-1";
                var webAddr = "https://fcm.googleapis.com/fcm/send";
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(webAddr);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Headers.Add("Authorization:key=" + serverKey);
                httpWebRequest.Method = "POST";
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = "{\"to\":" + "\"" + token + "\"" + ",\"data\": {\"message\": \"" + message + "\"}}";
                    streamWriter.Write(json);
                    streamWriter.Flush();
                }


                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                    fr = JsonConvert.DeserializeObject<FCMResponse>(result);

                }
                // return result;
            }
            catch (Exception ex)
            {
                //  Response.Write(ex.Message);
            }
            return fr;
        }

        public string AddProductToOrder(int productId, decimal price, Guid orderId)
        {
            try
            {
                tblOrderPart top = new tblOrderPart();
                top.ProductID = productId;
                top.Price = price;
                top.DateCreated = DateTime.Now;
                top.DelInd = false;
                top.OrderGUID = orderId;
                top.WebUpload = false;
                top.Qty = 1;
                top.LastModified = DateTime.Now;
                top.Total = price;
                top.UserID = -10;
                _context.tblOrderParts.Add(top);
                _context.SaveChanges();
                return "";
            }
            catch (Exception ex)
            {

                return ex.Message;
            }

        }

        public string AddVoucherToOrder(VoucherModel vm)
        {
            try
            {
                tblOrderVoucher tov = new tblOrderVoucher();
                tov.OrderGUID = vm.OrderId;
                tov.VoucherAmount = vm.Amount;
                tov.VoucherNumber = vm.VoucherCode;
                tov.VoucherValue = vm.VoucherValue;
                tov.VoucherType = vm.VoucherType;
                tov.Mobile = vm.Mobile;
                tov.StopPromotion = false;
                tov.DateCreated = DateTime.Now;
                tov.DelInd = false;
                _context.tblOrderVouchers.Add(tov);
                _context.SaveChanges();
                return "";
            }
            catch (Exception ex)
            {

                return ex.Message;
            }
        }

        public string SpliceText(string inputText, int lineLength)
        {

            string[] stringSplit = inputText.Split(' ');
            int charCounter = 0;
            string finalString = "";

            for (int i = 0; i < stringSplit.Length; i++)
            {

                charCounter += stringSplit[i].Length;

                if (charCounter > lineLength)
                {
                    //finalString += "\n";
                    finalString += Environment.NewLine;
                    //charCounter = 0;
                    charCounter = stringSplit[i].Length;
                }
                finalString += stringSplit[i] + " ";
                charCounter += 1;
            }
            return finalString;
        }

        //public string PrintTakeAwayOrder(Guid orderGUID)
        public string PrintTakeAwayOrder(TakeawayPrintModel tpm)
        {
            int ordCount = 0;
            foreach (var ord in tpm.Orders)
            {
                ordCount += 1;
                var order = (from a in _context.tblTakeAwayOrders
                             join b in _context.tblAddresses on a.AddressID equals b.AddressID
                             where a.OrderGUID == ord
                             select new
                             {
                                 a = a,
                                 Address = b.AddressFull
                             }).FirstOrDefault();
                string strReceipt = "";
                string foodDetails = "";
                string orderDetails = "";
                decimal itemsTotal = 0;
                decimal dueAmount = 0;
                int deliveryChargeProductId = Convert.ToInt32(ConfigurationManager.AppSettings["DeliveryChargeProductId"]);
                if (order.a.HasPrinted == 1)
                {
                    orderDetails += "**** DUPLICATE ****" + Environment.NewLine;
                    foodDetails += "**** DUPLICATE ****" + Environment.NewLine;
                }
                else
                {
                    order.a.HasPrinted = 1;
                    _context.Entry(order.a).State = EntityState.Modified;
                    _context.SaveChanges();
                }
                string orderType = order.a.bDelivery == true ? "DELIVERY" : "COLLECTION";
                orderDetails += "**** " + orderType + Environment.NewLine;
                orderDetails += " **** Group -" + tpm.GroupId + ";" + ordCount + "/" + tpm.GroupCount + Environment.NewLine;
                foodDetails += "**** " + orderType + Environment.NewLine;
                foodDetails += " **** Group -" + tpm.GroupId + ";" + ordCount + "/" + tpm.GroupCount + Environment.NewLine;
                orderDetails += "     Order No : " + order.a.OrderNumber + Environment.NewLine;
                foodDetails += "     Order No : " + order.a.OrderNumber + Environment.NewLine;
                orderDetails += "     Name : " + order.a.NAME + Environment.NewLine;
                orderDetails += "     Mobile : " + order.a.Phone + Environment.NewLine;
                orderDetails += "     " + DateTime.Now.ToString("dd-MM-yyyy") + " " + order.a.CollectionTime + Environment.NewLine;
                foodDetails += "     " + DateTime.Now.ToString("dd-MM-yyyy") + " " + order.a.CollectionTime + Environment.NewLine;
                orderDetails += SpliceText(order.Address, 25);
                orderDetails += "------------------------" + Environment.NewLine;
                foodDetails += "------------------------" + Environment.NewLine;
                var orderPart = (from a in _context.tblOrderParts
                                 join b in _context.tblProducts on a.ProductID equals b.ProductID
                                 join c in _context.tblProductTypes on b.ProductTypeID equals c.ProductTypeID
                                 where a.DelInd == false && a.OrderGUID == order.a.OrderGUID && a.Price >= 0
                                 //group a by new { a.OrderPartID, b.Description, a.ProductID,c.TypeDescription } into g
                                 select new BuffetBox
                                 {
                                     OrderPartId = a.OrderPartID,
                                     Name = b.Description,
                                     ProductType = c.TypeDescription,
                                     ProductId = a.ProductID,
                                     //Qty = (int)g.Sum(x => x.Qty),
                                     Price = (decimal)a.Price,
                                     ChineseName = b.ChineseName,
                                     Qty = (int)a.Qty
                                 }).ToList();
                foreach (var item in orderPart)
                {
                    itemsTotal += item.Qty * item.Price;
                    if (item.ProductId != deliveryChargeProductId)
                        orderDetails += item.Qty + "    " + SpliceText(item.Name, 25) + Environment.NewLine;
                    if (!item.ProductType.Contains("Wet") && item.ProductId != deliveryChargeProductId)
                    {
                        //if (item.ProductId != deliveryChargeProductId)
                        //{
                        string it = item.Qty + " - " + item.ChineseName + " - " + item.Name;
                        it = SpliceText(it, 25);
                        //foodDetails += item.Qty + "    " + SpliceText(item.Name, 25) + Environment.NewLine;
                        foodDetails += it + Environment.NewLine;

                        if (_context.tblOrderPartOptions.Any(x => x.OrderPartId == item.OrderPartId))
                        {
                            var partOptions = (from a in _context.tblOrderPartOptions
                                               join b in _context.tblProducts on a.ProductOptionID equals b.ProductID

                                               where a.OrderPartId == item.OrderPartId
                                               select new
                                               {
                                                   Name = b.Description,
                                                   ChineseName= b.ChineseName
                                               }).ToList();
                            foreach (var option in partOptions)
                            {
                                string itp = "     " + option.ChineseName + " - " + option.Name;
                                itp = SpliceText(itp, 17);
                                //foodDetails += "        " + SpliceText(option.Name, 17) + Environment.NewLine;
                                foodDetails += itp + Environment.NewLine;
                            }
                        }
                        //}
                    }

                }
                decimal paidAmount = 0;
                itemsTotal = (decimal)_context.tblOrderParts.Where(x => x.OrderGUID == ord && x.DelInd == false).Sum(x => x.Total);
                if (_context.tblOrderPayments.Any(x => x.OrderGUID == ord))
                    paidAmount = _context.tblOrderPayments.Where(x => x.OrderGUID == ord).Sum(x => x.PaymentValue - x.Change);
                dueAmount = itemsTotal - paidAmount;
                if (dueAmount > 0)
                    orderDetails += "***Amount Payable : " + "£" + Math.Round(dueAmount, 2);
                else
                    orderDetails += "***Order Paid***";
                tblPrintQueue tp = new tblPrintQueue();
                tp.DateCreated = DateTime.Now;
                tp.PCName = "TILL";
                tp.Receipt = orderDetails;
                tp.ToPrinter = "Kitchen2";
                tp.UserFK = -10;
                //tp.TableNumber = 
                _context.tblPrintQueues.Add(tp);
                _context.SaveChanges();

                tblPrintQueue tp1 = new tblPrintQueue();
                tp1.DateCreated = DateTime.Now;
                tp1.PCName = "TILL";
                tp1.Receipt = foodDetails;
                tp1.ToPrinter = "Kitchen2";
                tp1.UserFK = -10;
                _context.tblPrintQueues.Add(tp1);
                _context.SaveChanges();

            }
            return "Order printed succesfully";
        }

        public IEnumerable<List<T>> SplitList<T>(List<T> locations, int nSize = 30)
        {
            for (int i = 0; i < locations.Count; i += nSize)
            {
                yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
            }
        }
        public PaymentResponse CompletePayment(Payment req)
        {
            PaymentResponse pr = new PaymentResponse();
            string response = "";
            Guid orderId = new Guid();
            bool giftVouchersBought = false;
            List<Entity.Product> giftVoucherProducts = new List<Entity.Product>();
            List<int> gvProducts = new List<int>();
            gvProducts.Add(1777);
            gvProducts.Add(1776);
            try
            {
                using (var scope = new TransactionScope())
                {

                    using (var dbContext = new ChineseTillEntities1())
                    {
                        var to = dbContext.tblOrders.Where(x => x.OrderGUID == req.OrderGUID).FirstOrDefault();

                        //check to see if we received whole payment amount. Fix for full payment received
                        //and products marked as split
                        //var intentAmount = dbContext.tblStripePayments.Where(x => x.VendorTxCode == req.TxCode)
                        //                    .Select(x => x.Amount).FirstOrDefault();
                        //if (req.Amount == intentAmount / 100)
                        //    req.isSplitPayment = false;


                        
                        if (req.isSplitPayment)
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
                                //for (int i = 0; i < item.ProductQty; i++)
                                //{
                                    var op = dbContext.tblOrderParts.Where(x => x.OrderGUID == req.OrderGUID && x.ProductID == item.ProductID && x.DelInd == false && x.Paid == false).FirstOrDefault();
                                    op.OrderGUID = orderId;
                                    op.Paid = true;
                                    dbContext.Entry(op).State = EntityState.Modified;
                                    dbContext.SaveChanges();
                                //}
                            }
                            to.SplitBill = true;
                            to.LockedForPayment = false;
                            dbContext.Entry(to).State = EntityState.Modified;
                            dbContext.SaveChanges();

                            //Check if any vouchers bought with this order
                            giftVoucherProducts = req.SplitProduct.Where(x=>gvProducts.Contains(x.ProductID)).ToList();
                        }
                        else
                        {
                            orderId = req.OrderGUID;
                            //rh.WriteToFile("testing Elmah");
                            to.Paid = true;
                            to.DatePaid = DateTime.Now;
                            to.PaymentMethod = "MP";
                            to.TotalPaid = (decimal)req.Amount;
                            to.GrandTotal = (decimal)req.Amount;
                            to.TipAmount = (decimal)req.TipAmount;
                            to.ServiceCharge = (decimal)req.ServiceCharge;
                            to.LockedForPayment = false;
                            dbContext.Entry(to).State = EntityState.Modified;
                            dbContext.SaveChanges();
                            var tblOrd = dbContext.tblTableOrders.Where(x => x.OrderGUID == req.OrderGUID).FirstOrDefault();
                            if(tblOrd != null)
                            tblOrd.Active = false;
                            dbContext.Entry(tblOrd).State = EntityState.Modified;
                            dbContext.SaveChanges();
                            dbContext.usp_AN_SetTableCleaning(req.OrderGUID, 11);

                            //Check if any vouchers bought with this order
                            giftVoucherProducts = (from a in dbContext.tblOrderParts
                                                   join b in dbContext.tblProducts on a.ProductID equals b.ProductID
                                                   where a.OrderGUID == req.OrderGUID && b.ProductGroupID == 17
                                                   select new Entity.Product
                                                   {
                                                       ProductID = a.ProductID,
                                                       Price = (float)b.Price,
                                                       Description = b.Description
                                                   }).ToList();

                        }

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
                        topy.PGName = "Stripe";

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

                        Entity.Customer cust = cs.GetRewardPointsForOrder(orderId, req.Amount, false);

                       
                        if (req.Amount > 0)
                        {
                            //Added this check by Parth on 18th Oct 2024 to not update TblStripePayment in Record Manual Payment.
                            if (req.TxCode != null && req.TxCode != "")
                            { 
                            var tsp = dbContext.tblStripePayments.Where(x => x.VendorTxCode == req.TxCode).FirstOrDefault();
                                tsp.Success = true;
                                tsp.LastModified = DateTime.Now;
                                tsp.PaymentId = req.PaymentId;
                                tsp.OrderGUID = orderId;
                                dbContext.Entry(tsp).State = EntityState.Modified;
                                dbContext.SaveChanges();
                            }
                        }
                        if (giftVoucherProducts != null && giftVoucherProducts.Count > 0)
                        {
                            giftVouchersBought = true;
                            List<PromotionDto> promList = new List<PromotionDto>();
                            // create unique code and send to customer
                            foreach (var item in giftVoucherProducts)
                            {
                                PromotionDto pd = new PromotionDto();
                                pd.MobileNo = req.Mobile;
                                pd.PromoCode = item.Price.ToString() + "xphau";
                                promList.Add(pd);
                            }
                            var myContent = JsonConvert.SerializeObject(promList);
                            var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
                            var byteContent = new ByteArrayContent(buffer);
                            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                            using (var client = new HttpClient())
                            {
                                client.BaseAddress = new Uri(url);
                                var responseTask = client.PostAsync("v2/AvailPromotions", byteContent).Result;
                                //var responseTask = client.GetAsync("v1/CustomerRegistration" + cusId + "&redPoints=" + redPoints + "&earnPoints=" + earnPoints + "&resId=" + resId + "&orderId=" + orderid.ToString() + "&activityType=DineIn");
                                //responseTask.Wait();
                                //var result = responseTask.Result;
                                if (responseTask.IsSuccessStatusCode)
                                {
                                    var readTask = responseTask.Content.ReadAsAsync<string>();
                                    readTask.Wait();
                                    //giftVoucherresponse = readTask.Result;
                                }
                                else //web api sent error response 
                                {
                                    //giftVoucherresponse = "failure";
                                    //log response status here..
                                }
                            }
                        }

                        response = "<h2 style=\"color:green;text-align:center;\">Payment Confirmation</h2> <p>Thank you for your payment. Our staff will bring you your confirmation slip.</p>";
                        if (cust.OrderPoints > 0)
                            response += "<br/><h2 style=\"color:darkred\";\"text-align:center\";>Congratulations</h2> <p>you have just earned " + cust.OrderPoints + " reward points for this order and will be linked to below mobile number. You may redeem them in your next order with us.</p>";
                        if (giftVouchersBought)
                            response += "<br/><p>Your voucher codes will be sent to your mobile shortly.</p>";

                        pr.IsSuccess = true;
                        pr.Mobile = req.Mobile;
                        pr.OrderGUID = orderId;
                        pr.Points = cust.OrderPoints;
                        pr.Message = response;
                    }
                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                pr.Message = ex.Message;
            }
            return pr;
        }
        public PaymentResponse CompletePayment1(Payment req)
        {
            PaymentResponse pr = new PaymentResponse();
            string response = "";
            Guid orderId = new Guid();
            bool giftVouchersBought = false;
            List<Entity.Product> giftVoucherProducts = new List<Entity.Product>();
            List<int> gvProducts = new List<int>();
            gvProducts.Add(1777);
            gvProducts.Add(1776);

            try
            {
                using (var scope = new TransactionScope())
                {

                    using (var dbContext = new ChineseTillEntities1())
                    {
                        //var orderDetails = (from a in dbContext.tblSagePayments
                        //                    join b in dbContext.tblOrders on a.OrderID equals b.OrderGUID
                        //                    join c in dbContext.tblTables on b.TableID equals c.TableID
                        //                    where a.VendorTXCode == req.TxCode
                        //                    select new
                        //                    {
                        //                        OrderGUID = b.OrderGUID,
                        //                        TableNumber = c.TableNumber,
                        //                        SplitPayment = a.SplitPayment
                        //                    });
                        var tsp = dbContext.tblSagePayments.Where(x => x.VendorTXCode == req.TxCode).FirstOrDefault();

                        var to = dbContext.tblOrders.Where(x => x.OrderGUID == tsp.OrderID).FirstOrDefault();
                        var table = dbContext.tblTables.Where(x => x.TableID == to.TableID && x.DelInd == false).FirstOrDefault();
                        //check to see if we received whole payment amount. Fix for full payment received
                        //and products marked as split
                        //var intentAmount = dbContext.tblStripePayments.Where(x => x.VendorTxCode == req.TxCode)
                        //                    .Select(x => x.Amount).FirstOrDefault();
                        //if (req.Amount == intentAmount / 100)
                        //    req.isSplitPayment = false;



                        if (tsp.SplitPayment)
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
                            tor.TotalPaid = (decimal)tsp.PaymentPrice;
                            tor.GrandTotal = (decimal)tsp.PaymentPrice;
                            tor.TipAmount = (decimal)tsp.TipAmount;
                            tor.DateCreated = DateTime.Now;
                            tor.LastModified = DateTime.Now;
                            tor.CustomerId = req.CustomerId;
                            tor.ServiceCharge = (decimal?)tsp.SCAmount;
                            dbContext.tblOrders.Add(tor);
                            dbContext.SaveChanges();
                            orderId = tor.OrderGUID;
                            //Move selected products to this new order
                            var splitProducts = dbContext.tblOrderParts.Where(x => x.VendorTxCode == req.TxCode).ToList();
                            foreach (var item in splitProducts)
                            {
                                //for (int i = 0; i < item.ProductQty; i++)
                                //{
                                var op = dbContext.tblOrderParts.Where(x => x.VendorTxCode == req.TxCode && x.ProductID == item.ProductID && x.DelInd == false && x.Paid == false).FirstOrDefault();
                                if (op != null)
                                {
                                    op.OrderGUID = orderId;
                                    op.Paid = true;
                                    dbContext.Entry(op).State = EntityState.Modified;
                                    dbContext.SaveChanges();
                                }
                                //}
                            }
                            to.SplitBill = true;
                            to.LockedForPayment = false;
                            dbContext.Entry(to).State = EntityState.Modified;
                            dbContext.SaveChanges();

                            //Check if any vouchers bought with this order
                            //giftVoucherProducts = req.SplitProduct.Where(x => gvProducts.Contains(x.ProductID)).ToList();
                        }
                        else
                        {
                            orderId = tsp.OrderID;
                            //rh.WriteToFile("testing Elmah");
                            to.Paid = true;
                            to.DatePaid = DateTime.Now;
                            to.PaymentMethod = "MP";
                            to.TotalPaid = (decimal)tsp.PaymentPrice;
                            to.GrandTotal = (decimal)tsp.PaymentPrice;
                            to.TipAmount = (decimal)tsp.TipAmount;
                            to.ServiceCharge = (decimal?)tsp.SCAmount;
                            //to.ServiceCharge = (decimal)req.ServiceCharge;
                            to.LockedForPayment = false;
                            dbContext.Entry(to).State = EntityState.Modified;
                            dbContext.SaveChanges();
                            var tblOrd = dbContext.tblTableOrders.Where(x => x.OrderGUID == tsp.OrderID).FirstOrDefault();
                            if (tblOrd != null)
                                tblOrd.Active = false;
                            dbContext.Entry(tblOrd).State = EntityState.Modified;
                            dbContext.SaveChanges();
                            dbContext.usp_AN_SetTableCleaning(tsp.OrderID, 11);

                            //Check if any vouchers bought with this order
                            giftVoucherProducts = (from a in dbContext.tblOrderParts
                                                   join b in dbContext.tblProducts on a.ProductID equals b.ProductID
                                                   where a.OrderGUID == tsp.OrderID && b.ProductGroupID == 17
                                                   select new Entity.Product
                                                   {
                                                       ProductID = a.ProductID,
                                                       Price = (float)b.Price,
                                                       Description = b.Description
                                                   }).ToList();

                        }

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
                        topy.PaymentValue = (decimal)tsp.PaymentPrice;
                        topy.TipAmount = (decimal)tsp.TipAmount;
                        topy.ServiceCharge = (decimal?)tsp.SCAmount;
                        topy.PaymentType = "MP";
                        topy.PCName = primaryTILL;
                        topy.PGName = "Sage";
                        dbContext.tblOrderPayments.Add(topy);
                        dbContext.SaveChanges();


                        //Print Payment Confirmation slip for the Order
                        string confirmationStr = "";
                        confirmationStr += "       " + DateTime.Now.ToString("dd-MM-yyyy") + Environment.NewLine;
                        confirmationStr += "Confirmation payment of " + Environment.NewLine;
                        confirmationStr += "£" + String.Format("{0:0.00}", tsp.PaymentPrice) + " for Table-" + table.TableNumber + Environment.NewLine;
                        confirmationStr += Environment.NewLine;
                        confirmationStr += "Thank you for your payment." + Environment.NewLine;
                        confirmationStr += "Please give this confirmation" + Environment.NewLine;
                        confirmationStr += "slip to the cashier " + Environment.NewLine;
                        confirmationStr += "on your way out." + Environment.NewLine;
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

                        Entity.Customer cust = cs.GetRewardPointsForOrder(orderId, (decimal)tsp.PaymentPrice, false);
                        if (cust.OrderPoints > 0)
                        {
                            tblCustomerActivity tca = new tblCustomerActivity();
                            tca.RewardPoints = cust.OrderPoints;
                            tca.Mobile = req.Mobile;
                            tca.ActivityType = ActivityType.DineIn.ToString();
                            tca.FullName = req.FullName;
                            tca.OrderGUID = orderId;
                            cs.UpdateCustomerActivity(tca);
                        }
                        if (tsp.PaymentPrice > 0)
                        {
                            //Update record in tblStripePayment
                            tsp.IsSuccess = true;
                            tsp.StatusUpdated = DateTime.Now;
                            tsp.SecurityKey = req.PaymentId;
                            tsp.TransactionID = req.TransactionID;
                            tsp.OrderID = orderId;
                            dbContext.Entry(tsp).State = EntityState.Modified;
                            dbContext.SaveChanges();

                        }
                        if (giftVoucherProducts != null && giftVoucherProducts.Count > 0)
                        {
                            giftVouchersBought = true;
                            List<PromotionDto> promList = new List<PromotionDto>();
                            // create unique code and send to customer
                            foreach (var item in giftVoucherProducts)
                            {
                                PromotionDto pd = new PromotionDto();
                                pd.MobileNo = req.Mobile;
                                pd.PromoCode = item.Price.ToString() + "xphau";
                                promList.Add(pd);
                            }
                            var myContent = JsonConvert.SerializeObject(promList);
                            var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
                            var byteContent = new ByteArrayContent(buffer);
                            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                            using (var client = new HttpClient())
                            {
                                client.BaseAddress = new Uri(url);
                                var responseTask = client.PostAsync("v2/AvailPromotions", byteContent).Result;
                                //var responseTask = client.GetAsync("v1/CustomerRegistration" + cusId + "&redPoints=" + redPoints + "&earnPoints=" + earnPoints + "&resId=" + resId + "&orderId=" + orderid.ToString() + "&activityType=DineIn");
                                //responseTask.Wait();
                                //var result = responseTask.Result;
                                if (responseTask.IsSuccessStatusCode)
                                {
                                    var readTask = responseTask.Content.ReadAsAsync<string>();
                                    readTask.Wait();
                                    //giftVoucherresponse = readTask.Result;
                                }
                                else //web api sent error response 
                                {
                                    //giftVoucherresponse = "failure";
                                    //log response status here..
                                }
                            }
                        }

                        response = "Thank you for your payment. Our staff will bring you your confirmation slip.";
                        if (cust.OrderPoints > 0)
                            response += "Congratulations you have just earned " + cust.OrderPoints + " reward points for this order and will be linked to below mobile number. You may redeem them in your next order with us.";
                        if (giftVouchersBought)
                            response += "Your voucher codes will be sent to your mobile shortly.";

                        pr.IsSuccess = true;
                        pr.Mobile = tsp.MobileNo;
                        pr.OrderGUID = orderId;
                        pr.Points = cust.OrderPoints;
                        pr.Message = response;
                    }
                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                pr.Message = ex.Message;
            }
            return pr;
        }
        public PaymentResponse CompletePaymentStripe(Payment req)
        {
            PaymentResponse pr = new PaymentResponse();
            string response = "";
            Guid orderId = new Guid();
            bool giftVouchersBought = false;
            List<Entity.Product> giftVoucherProducts = new List<Entity.Product>();
            List<int> gvProducts = new List<int>();
            gvProducts.Add(1777);
            gvProducts.Add(1776);

            try
            {
                using (var scope = new TransactionScope())
                {

                    using (var dbContext = new ChineseTillEntities1())
                    {
                        //var orderDetails = (from a in dbContext.tblSagePayments
                        //                    join b in dbContext.tblOrders on a.OrderID equals b.OrderGUID
                        //                    join c in dbContext.tblTables on b.TableID equals c.TableID
                        //                    where a.VendorTXCode == req.TxCode
                        //                    select new
                        //                    {
                        //                        OrderGUID = b.OrderGUID,
                        //                        TableNumber = c.TableNumber,
                        //                        SplitPayment = a.SplitPayment
                        //                    });
                        var tsp = dbContext.tblStripePayments.Where(x => x.VendorTxCode == req.TxCode).FirstOrDefault();

                        var to = dbContext.tblOrders.Where(x => x.OrderGUID == tsp.OrderGUID).FirstOrDefault();
                        var table = dbContext.tblTables.Where(x => x.TableID == to.TableID && x.DelInd == false).FirstOrDefault();
                        //check to see if we received whole payment amount. Fix for full payment received
                        //and products marked as split
                        //var intentAmount = dbContext.tblStripePayments.Where(x => x.VendorTxCode == req.TxCode)
                        //                    .Select(x => x.Amount).FirstOrDefault();
                        //if (req.Amount == intentAmount / 100)
                        //    req.isSplitPayment = false;



                        if (tsp.SplitPayment)
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
                            tor.TotalPaid = (decimal)tsp.Amount;
                            tor.GrandTotal = (decimal)tsp.Amount;
                            tor.TipAmount = (decimal)tsp.TipAmount;
                            tor.DateCreated = DateTime.Now;
                            tor.LastModified = DateTime.Now;
                            tor.CustomerId = req.CustomerId;
                            tor.ServiceCharge = (decimal?)tsp.SCAmount;
                            dbContext.tblOrders.Add(tor);
                            dbContext.SaveChanges();
                            orderId = tor.OrderGUID;
                            //Move selected products to this new order
                            var splitProducts = dbContext.tblOrderParts.Where(x => x.VendorTxCode == req.TxCode).ToList();
                            foreach (var item in splitProducts)
                            {
                                //for (int i = 0; i < item.ProductQty; i++)
                                //{
                                var op = dbContext.tblOrderParts.Where(x => x.VendorTxCode == req.TxCode && x.ProductID == item.ProductID && x.DelInd == false && x.Paid == false).FirstOrDefault();
                                if (op != null)
                                {
                                    op.OrderGUID = orderId;
                                    op.Paid = true;
                                    dbContext.Entry(op).State = EntityState.Modified;
                                    dbContext.SaveChanges();
                                }
                                //}
                            }
                            to.SplitBill = true;
                            to.LockedForPayment = false;
                            dbContext.Entry(to).State = EntityState.Modified;
                            dbContext.SaveChanges();

                            //Check if any vouchers bought with this order
                            //giftVoucherProducts = req.SplitProduct.Where(x => gvProducts.Contains(x.ProductID)).ToList();
                        }
                        else
                        {
                            orderId = tsp.OrderGUID;
                            //rh.WriteToFile("testing Elmah");
                            to.Paid = true;
                            to.DatePaid = DateTime.Now;
                            to.PaymentMethod = "MP";
                            to.TotalPaid = (decimal)tsp.Amount;
                            to.GrandTotal = (decimal)tsp.Amount;
                            to.TipAmount = (decimal)tsp.TipAmount;
                            to.ServiceCharge = (decimal?)tsp.SCAmount;
                            //to.ServiceCharge = (decimal)req.ServiceCharge;
                            to.LockedForPayment = false;
                            dbContext.Entry(to).State = EntityState.Modified;
                            dbContext.SaveChanges();
                            var tblOrd = dbContext.tblTableOrders.Where(x => x.OrderGUID == tsp.OrderGUID).FirstOrDefault();
                            if (tblOrd != null)
                                tblOrd.Active = false;
                            dbContext.Entry(tblOrd).State = EntityState.Modified;
                            dbContext.SaveChanges();
                            dbContext.usp_AN_SetTableCleaning(tsp.OrderGUID, 11);

                            //Check if any vouchers bought with this order
                            giftVoucherProducts = (from a in dbContext.tblOrderParts
                                                   join b in dbContext.tblProducts on a.ProductID equals b.ProductID
                                                   where a.OrderGUID == tsp.OrderGUID && b.ProductGroupID == 17
                                                   select new Entity.Product
                                                   {
                                                       ProductID = a.ProductID,
                                                       Price = (float)b.Price,
                                                       Description = b.Description
                                                   }).ToList();

                        }

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
                        topy.PaymentValue = (decimal)tsp.Amount;
                        topy.TipAmount = (decimal)tsp.TipAmount;
                        topy.ServiceCharge = (decimal?)tsp.SCAmount;
                        topy.PaymentType = "MP";
                        topy.PCName = primaryTILL;
                        topy.PGName = "Stripe";
                        dbContext.tblOrderPayments.Add(topy);
                        dbContext.SaveChanges();


                        //Print Payment Confirmation slip for the Order
                        string confirmationStr = "";
                        confirmationStr += "       " + DateTime.Now.ToString("dd-MM-yyyy") + Environment.NewLine;
                        confirmationStr += "Confirmation payment of " + Environment.NewLine;
                        confirmationStr += "£" + String.Format("{0:0.00}", tsp.Amount) + " for Table-" + table.TableNumber + Environment.NewLine;
                        confirmationStr += Environment.NewLine;
                        confirmationStr += "Thank you for your payment." + Environment.NewLine;
                        confirmationStr += "Please give this confirmation" + Environment.NewLine;
                        confirmationStr += "slip to the cashier " + Environment.NewLine;
                        confirmationStr += "on your way out." + Environment.NewLine;
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

                        Entity.Customer cust = cs.GetRewardPointsForOrder(orderId, (decimal)tsp.Amount, false);
                        if (cust.OrderPoints > 0)
                        {
                            tblCustomerActivity tca = new tblCustomerActivity();
                            tca.RewardPoints = cust.OrderPoints;
                            tca.Mobile = req.Mobile;
                            tca.ActivityType = ActivityType.DineIn.ToString();
                            tca.FullName = req.FullName;
                            tca.OrderGUID = orderId;
                            cs.UpdateCustomerActivity(tca);
                        }
                        if (tsp.Amount > 0)
                        {
                            //Update record in tblStripePayment
                            tsp.Success = true;
                            tsp.LastModified = DateTime.Now;
                            tsp.PaymentId = req.PaymentId;
                            tsp.VendorTxCode = req.TxCode;
                            tsp.OrderGUID = orderId;
                            dbContext.Entry(tsp).State = EntityState.Modified;
                            dbContext.SaveChanges();

                        }
                        if (giftVoucherProducts != null && giftVoucherProducts.Count > 0)
                        {
                            giftVouchersBought = true;
                            List<PromotionDto> promList = new List<PromotionDto>();
                            // create unique code and send to customer
                            foreach (var item in giftVoucherProducts)
                            {
                                PromotionDto pd = new PromotionDto();
                                pd.MobileNo = req.Mobile;
                                pd.PromoCode = item.Price.ToString() + "xphau";
                                promList.Add(pd);
                            }
                            var myContent = JsonConvert.SerializeObject(promList);
                            var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
                            var byteContent = new ByteArrayContent(buffer);
                            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                            using (var client = new HttpClient())
                            {
                                client.BaseAddress = new Uri(url);
                                var responseTask = client.PostAsync("v2/AvailPromotions", byteContent).Result;
                                //var responseTask = client.GetAsync("v1/CustomerRegistration" + cusId + "&redPoints=" + redPoints + "&earnPoints=" + earnPoints + "&resId=" + resId + "&orderId=" + orderid.ToString() + "&activityType=DineIn");
                                //responseTask.Wait();
                                //var result = responseTask.Result;
                                if (responseTask.IsSuccessStatusCode)
                                {
                                    var readTask = responseTask.Content.ReadAsAsync<string>();
                                    readTask.Wait();
                                    //giftVoucherresponse = readTask.Result;
                                }
                                else //web api sent error response 
                                {
                                    //giftVoucherresponse = "failure";
                                    //log response status here..
                                }
                            }
                        }

                        response = "Thank you for your payment. Our staff will bring you your confirmation slip.";
                        if (cust.OrderPoints > 0)
                            response += "Congratulations you have just earned " + cust.OrderPoints + " reward points for this order and will be linked to below mobile number. You may redeem them in your next order with us.";
                        if (giftVouchersBought)
                            response += "Your voucher codes will be sent to your mobile shortly.";

                        pr.IsSuccess = true;
                        pr.Mobile = tsp.MobileNo;
                        pr.OrderGUID = orderId;
                        pr.Points = cust.OrderPoints;
                        pr.Message = response;
                    }
                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                pr.Message = ex.Message;
            }
            return pr;
        }


        public void PrintTicket(string tableNumber,int custCount, string orderTime, string menuType,List<OrderBuffetItem> items)
        {
            string printer = "";
            if (menuType == "STARTERS")
                printer = ConfigurationManager.AppSettings["StarterPrinter"];
            else if (menuType == "DESSERTS")
                printer = ConfigurationManager.AppSettings["DessertPrinter"];
            else if (menuType == "")
                printer = ConfigurationManager.AppSettings["MCPrinter"];
            string receipt = "";
            string itemStr = "";
            int batchNo = _context.tblPrintQueues.Where(x => x.ToPrinter != "BAR").OrderByDescending(x => x.PrintQueueID).Select(x => x.BatchNo).FirstOrDefault();
            batchNo = batchNo + 1;
            receipt += custCount + "   " + orderTime + "              " + batchNo + Environment.NewLine + Environment.NewLine;
            receipt += menuType + Environment.NewLine;
            int itemsOnThisTicket = 0;
            foreach (var item in items)
            {
                itemsOnThisTicket += item.Qty;
                string it = item.Qty + " - " + item.ChineseName + " - " + item.EnglishName;
                it = SpliceText(it, 25);
                itemStr += it + Environment.NewLine;
            }
           

            receipt += itemStr + Environment.NewLine;
            receipt += "Reprint" + " " +menuType +"  " + DateTime.Now.ToString("HH:mm") + Environment.NewLine;

            receipt += itemsOnThisTicket + "   " + 1 + "/" + 1 + "              " + tableNumber + Environment.NewLine;

            tblPrintQueue tpd = new tblPrintQueue();
            tpd.Receipt = receipt;
            tpd.PCName = "DineIn";
            tpd.ToPrinter = printer;
            tpd.UserFK = -10;
            tpd.DateCreated = DateTime.Now;
            tpd.BatchNo = batchNo;
            tpd.TicketNo = batchNo.ToString();
            tpd.TableNumber = tableNumber;
            tpd.OrderGUID = items[0].OrderGUID;
            _context.tblPrintQueues.Add(tpd);
            _context.SaveChanges();
        }
        public string ReservationVoucherUsed(string code)
        {
            string rCode = code.Substring(2);
            string message = "";
            try
            {
                using (var _dbContext = new ChineseTillEntities1())
                {
                    var reservationCodeDetail = (from a in _dbContext.tblReservations
                                                 join b in _dbContext.tblOrders on a.OrderGUID equals b.OrderGUID
                                                 join c in _dbContext.tblTables on b.TableID equals c.TableID
                                                 where a.UniqueCode.Contains(rCode) && a.UniqueCodeUsed == true
                                                 select new
                                                 {
                                                     TableNumber = c.TableNumber,
                                                     DateRedeemed = b.DateCreated
                                                 }).FirstOrDefault();
                    var orderVoucherDetail = (from a in _dbContext.tblOrderVouchers
                                              join b in _dbContext.tblOrders on a.OrderGUID equals b.OrderGUID
                                              join c in _dbContext.tblTables on b.TableID equals c.TableID
                                              where a.VoucherNumber.Contains(rCode)
                                              select new
                                              {
                                                  TableNumber = c.TableNumber,
                                                  DateRedeemed = b.DateCreated
                                              }).FirstOrDefault();
                    //if (dbContext.tblReservations.Any(x => x.UniqueCode.Contains(rCode) && x.UniqueCodeUsed == true)
                    //    || (dbContext.tblOrderVouchers.Any(x => x.VoucherNumber.Contains(rCode))))
                    string tNumber = "";
                    string rDate = "";
                    if (reservationCodeDetail != null)
                    {
                        tNumber = reservationCodeDetail.TableNumber;
                        rDate = reservationCodeDetail.DateRedeemed.ToString("dd-MMM");
                    }
                    else if (orderVoucherDetail != null)
                    {
                        tNumber = orderVoucherDetail.TableNumber;
                        rDate = orderVoucherDetail.DateRedeemed.ToString("dd-MMM");
                    }
                    if (reservationCodeDetail != null || orderVoucherDetail != null)
                    {
                        message = "Voucher already used at table - " + tNumber + " on " + rDate;
                    }

                }
            }
            catch (Exception ex)
            {

                message = "";
            }
            return message;

        }

        public string DeleteImageFiles(string folderPath)
        {
            string response = "";
            try
            {
                // Check if the folder exists
                if (Directory.Exists(folderPath))
                {
                    // Get all files with image extensions in the folder
                    string[] imageFiles = Directory.GetFiles(folderPath, "*.jpg");
                    //response += folderPath;
                    // Delete each image file
                    foreach (string filePath in imageFiles)
                    {
                        //response += filePath;
                        File.Delete(filePath);
                        
                    }
                }
                else
                {
                    response += ($"Folder does not exist: {folderPath}");
                }
            }
            catch (Exception ex)
            {
                response += ex.Message;
            }
            return response;
        }

        public void Executestoredprocedurewithoutparameters(string storedprocedurename)
        {
            SqlDataManager manager = new SqlDataManager();
            manager.ExecuteNonQuery(storedprocedurename);
        }
        //public PaymentResponse CompletePaymentStripe(Payment req)
        //{
        //    PaymentResponse pr = new PaymentResponse();
        //    string response = "";
        //    Guid orderId = new Guid();
        //    bool giftVouchersBought = false;
        //    List<Entity.Product> giftVoucherProducts = new List<Entity.Product>();
        //    List<int> gvProducts = new List<int>();
        //    gvProducts.Add(1777);
        //    gvProducts.Add(1776);
        //    try
        //    {
        //        using (var scope = new TransactionScope())
        //        {

        //            using (var dbContext = new ChineseTillEntities1())
        //            {
        //                var tsp = dbContext.tblStripePayments.Where(x => x.VendorTxCode == req.TxCode).FirstOrDefault();
        //                var to = dbContext.tblOrders.Where(x => x.OrderGUID == tsp.OrderGUID).FirstOrDefault();
        //                var table = dbContext.tblTables.Where(x => x.TableID == to.TableID && x.DelInd == false).FirstOrDefault();
        //                req.isSplitPayment = tsp.SplitPayment;
        //                //check to see if we received whole payment amount. Fix for full payment received
        //                //and products marked as split
        //                //var intentAmount = dbContext.tblStripePayments.Where(x => x.VendorTxCode == req.TxCode)
        //                //                    .Select(x => x.Amount).FirstOrDefault();
        //                //if (req.Amount == intentAmount / 100)
        //                //    req.isSplitPayment = false;



        //                if (req.isSplitPayment)
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
        //                     tor.TotalPaid = (decimal)tsp.Amount;
        //                    tor.GrandTotal = (decimal)tsp.Amount;
        //                    tor.TipAmount = (decimal)tsp.TipAmount;
        //                    tor.DateCreated = DateTime.Now;
        //                    tor.LastModified = DateTime.Now;
        //                    tor.CustomerId = to.CustomerId;
        //                    tor.ServiceCharge = req.ServiceCharge;
        //                    dbContext.tblOrders.Add(tor);
        //                    dbContext.SaveChanges();
        //                    orderId = tor.OrderGUID;
        //                    //Move selected products to this new order
        //                    foreach (var item in req.SplitProduct)
        //                    {
        //                        //for (int i = 0; i < item.ProductQty; i++)
        //                        //{
        //                        var op = dbContext.tblOrderParts.Where(x => x.OrderGUID == req.OrderGUID && x.ProductID == item.ProductID && x.DelInd == false && x.Paid == false).FirstOrDefault();
        //                        op.OrderGUID = orderId;
        //                        op.Paid = true;
        //                        dbContext.Entry(op).State = EntityState.Modified;
        //                        dbContext.SaveChanges();
        //                        //}
        //                    }
        //                    to.SplitBill = true;
        //                    to.LockedForPayment = false;
        //                    dbContext.Entry(to).State = EntityState.Modified;
        //                    dbContext.SaveChanges();

        //                    //Check if any vouchers bought with this order
        //                    giftVoucherProducts = req.SplitProduct.Where(x => gvProducts.Contains(x.ProductID)).ToList();
        //                }
        //                else
        //                {
        //                    orderId = req.OrderGUID;
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
        //                    if (tblOrd != null)
        //                        tblOrd.Active = false;
        //                    dbContext.Entry(tblOrd).State = EntityState.Modified;
        //                    dbContext.SaveChanges();
        //                    dbContext.usp_AN_SetTableCleaning(req.OrderGUID, 11);

        //                    //Check if any vouchers bought with this order
        //                    giftVoucherProducts = (from a in dbContext.tblOrderParts
        //                                           join b in dbContext.tblProducts on a.ProductID equals b.ProductID
        //                                           where a.OrderGUID == req.OrderGUID && b.ProductGroupID == 17
        //                                           select new Entity.Product
        //                                           {
        //                                               ProductID = a.ProductID,
        //                                               Price = (float)b.Price,
        //                                               Description = b.Description
        //                                           }).ToList();

        //                }

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
        //                topy.PGName = "Stripe";

        //                dbContext.tblOrderPayments.Add(topy);
        //                dbContext.SaveChanges();


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

        //                Entity.Customer cust = cs.GetRewardPointsForOrder(orderId, req.Amount, false);


        //                if (req.Amount > 0)
        //                {
        //                    //Update record in tblStripePayment
        //                    //var tsp = dbContext.tblStripePayments.Where(x => x.VendorTxCode == req.TxCode).FirstOrDefault();
        //                    tsp.Success = true;
        //                    tsp.LastModified = DateTime.Now;
        //                    tsp.PaymentId = req.PaymentId;
        //                    tsp.OrderGUID = orderId;
        //                    dbContext.Entry(tsp).State = EntityState.Modified;
        //                    dbContext.SaveChanges();

        //                }
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

        //                response = "<h2 style=\"color:green;text-align:center;\">Payment Confirmation</h2> <p>Thank you for your payment. Our staff will bring you your confirmation slip.</p>";
        //                if (cust.OrderPoints > 0)
        //                    response += "<br/><h2 style=\"color:darkred\";\"text-align:center\";>Congratulations</h2> <p>you have just earned " + cust.OrderPoints + " reward points for this order and will be linked to below mobile number. You may redeem them in your next order with us.</p>";
        //                if (giftVouchersBought)
        //                    response += "<br/><p>Your voucher codes will be sent to your mobile shortly.</p>";

        //                pr.IsSuccess = true;
        //                pr.Mobile = req.Mobile;
        //                pr.OrderGUID = orderId;
        //                pr.Points = cust.OrderPoints;
        //                pr.Message = response;
        //            }
        //            scope.Complete();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        pr.Message = ex.Message;
        //    }
        //    return pr;
        //}
    }
    public class CustomComparer : IComparer<string>
    {
        public int Compare(string s1, string s2)
        {
            int x, y;
            bool xInt, yInt;
            xInt = int.TryParse(s1, out x);
            yInt = int.TryParse(s2, out y);
            if (xInt && yInt)
                return x.CompareTo(y);
            if (xInt && !yInt)
            {
                if (this.SplitInt(s2, out y, out s2))
                {
                    return x.CompareTo(y);
                }
                else
                {
                    return -1;
                }
            }
            if (!xInt && yInt)
            {
                if (this.SplitInt(s1, out x, out s1))
                {
                    return y.CompareTo(x);
                }
                else
                {
                    return 1;
                }
            }

            return s1.CompareTo(s2);
        }

        private bool SplitInt(string sin, out int x, out string sout)
        {
            x = 0;
            sout = null;
            int i = -1;
            bool isNumeric = false;
            var numbers = Enumerable.Range(0, 10).Select(it => it.ToString());
            var ie = sin.GetEnumerator();
            while (ie.MoveNext() && numbers.Contains(ie.Current.ToString()))
            {
                isNumeric |= true;
                ++i;
            }

            if (isNumeric)
            {
                sout = sin.Substring(i + 1);
                sin = sin.Substring(0, i + 1);
                int.TryParse(sin, out x);
            }

            return false;
        }

       


    }

}