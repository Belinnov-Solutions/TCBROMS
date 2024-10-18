using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Entity;
using System.Transactions;
using Deznu.Products.Common.Utility;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Data.Entity;
using TCBROMS_Android_Webservice.Helpers;
using System.Configuration;
using Entity.Models;
using System.Security.Cryptography;
using System.Web.UI;
using Entity.Enums;
using NLog;
using System.Data.Entity.Validation;
using System.Data.SqlClient;

namespace TCBROMS_Android_Webservice.Models
{
    public class OrderService
    {
        Logger logger = LogManager.GetLogger("databaseLogger");
        ROMSHelper rh = new ROMSHelper();
        ChineseTillEntities1 dbContext = new ChineseTillEntities1();
        int printInterval = Convert.ToInt32(ConfigurationManager.AppSettings["PrintingInerval"]);
        CustomerService cs = new CustomerService();
        public OrderSubmitResponse SubmitOrder(TableOrder to)
        {
            OrderSubmitResponse os = new OrderSubmitResponse();

            if (to.OrderGUID != null)
            {
                os.OrderGUID = to.OrderGUID;

            }
            try
            {


                using (TransactionScope scope = new TransactionScope())
                {
                    //fix for issue raised in Dec-2017//
                    //Check if there's an existing order on that table
                    ChineseTillEntities1 context = new ChineseTillEntities1();
                    var res = context.tblOrders.Where(p => p.TableID == to.tableDetails.TableID
                                && DbFunctions.TruncateTime(p.DateCreated) == DbFunctions.TruncateTime(DateTime.Now)
                                && p.DelInd == false && p.Paid == false);
                    //comment above code and uncomment below for Hushi
                    //var res = context.tblOrders.Where(p => p.TableID == to.tableDetails.TableID
                    //            && p.DelInd == false && p.Paid == false);
                    Guid existingOrderId = new Guid();
                    bool existingOrder = false;

                    if (res.Count() == 0)
                    //if (to.Type == "N")
                    //if (to.Type.Equals("N", StringComparison.InvariantCultureIgnoreCase))
                    {
                        os.OrderGUID = new Guid();
                        int custID = 0;
                        //** Insert Customer Details **//
                        //if (to.tableCustomer.Type == "N" && to.tableCustomer.Mobile != null)
                        //{
                        //    SqlDataManager manager = new SqlDataManager();
                        //    manager.AddParameter("@Name", to.tableCustomer.Name);
                        //    manager.AddParameter("@EmailID", to.tableCustomer.EmailID);
                        //    manager.AddParameter("@Mobile", to.tableCustomer.Mobile);
                        //    manager.AddParameter("@DateOfBirth", to.tableCustomer.DOB);
                        //    manager.AddOutputParameter("@CustID", System.Data.DbType.Int32, custID);
                        //    manager.ExecuteNonQuery("usp_AN_InsertCustomer");
                        //    custID = FieldConverter.To<Int32>(manager.GetParameterValue("@CustID"));
                        //}

                        string tm = DateTime.Now.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture);

                        SqlDataManager manager1 = new SqlDataManager();
                        manager1.AddParameter("@TableID", to.tableDetails.TableID);
                        manager1.AddParameter("@TableNumber", to.tableDetails.TableNumber);
                        manager1.AddParameter("@TabID", 99);
                        manager1.AddParameter("@Time", tm);
                        manager1.AddParameter("@UserID", to.tableUser.UserID);
                        if (custID > 0)
                        {
                            manager1.AddParameter("@CustID", custID);
                        }
                        else
                            manager1.AddParameter("@CustID", 0);
                        manager1.AddParameter("@CustCount", to.tableDetails.PaxCount);
                        manager1.AddParameter("@AdCount", to.tableDetails.AdCount);
                        manager1.AddParameter("@KdCount", to.tableDetails.KdCount);
                        manager1.AddParameter("@JnCount", to.tableDetails.JnCount);
                        manager1.AddParameter("@PrevCust", to.tableCustomer.PrevCust);
                        if (to.ReservedCustomer)
                        {
                            manager1.AddParameter("@ReservedCustomer", "T");
                            manager1.AddParameter("@ReservationID", to.tableCustomer.ReservationID);
                        }
                        else
                        {
                            manager1.AddParameter("@ReservedCustomer", "F");
                            manager1.AddParameter("@ReservationID", to.tableCustomer.ReservationID);
                        }
                        manager1.AddOutputParameter("@OrderID", System.Data.DbType.Guid, os.OrderGUID);
                        manager1.ExecuteNonQuery("usp_AN_InsertOrder");
                        os.OrderGUID = FieldConverter.To<Guid>(manager1.GetParameterValue("@OrderID"));
                        //version 1.55.57 changes
                        //if (os.OrderGUID == Guid.Empty)
                        //{
                        //    os.message = "Empty Order string. Please try again";
                        //    return os;
                        //}
                        //changes end
                    }
                    else
                    {
                        existingOrder = true;
                        foreach (var item in res)
                        {
                            existingOrderId = item.OrderGUID;
                        }

                        os.OrderGUID = existingOrderId;
                        //version 1.55.57 changes
                        //if (os.OrderGUID == Guid.Empty)
                        //{
                        //    os.message = "Empty Order string. Please try again";
                        //    return os;
                        //}
                        //changes end
                    }
                    bool found = false;
                    if (to.Type.Equals("E", StringComparison.InvariantCultureIgnoreCase))
                    {
                        //ChineseTillEntities1 context = new ChineseTillEntities1();
                        //var a = from p in context.tblOrders
                        //        where (p.OrderGUID.Equals(to.OrderGUID)) && p.Paid == false && p.DelInd == false
                        //        select p;

                        SqlDataManager mgr = new SqlDataManager();
                        mgr.AddParameter("@OrderGUID", to.OrderGUID);
                        DataTable results = mgr.ExecuteDataTable("usp_AN_CheckTableOrder");
                        if (results.Rows.Count > 0)
                        {
                            found = true;
                        }

                    }
                    else
                        found = true;

                    if (found)
                    {

                        if (to.Type.Equals("E", StringComparison.InvariantCultureIgnoreCase) || existingOrder)
                        {
                            if (to.tableDetails.AdCount > 0 || to.tableDetails.KdCount > 0 || to.tableDetails.JnCount > 0)
                            {
                                SqlDataManager manager4 = new SqlDataManager();
                                if (existingOrder)
                                    manager4.AddParameter("@OrderGUID", existingOrderId);
                                else
                                    manager4.AddParameter("@OrderGUID", to.OrderGUID);
                                manager4.AddParameter("@AdCount", to.tableDetails.AdCount);
                                manager4.AddParameter("@KdCount", to.tableDetails.KdCount);
                                manager4.AddParameter("@JnCount", to.tableDetails.JnCount);
                                manager4.ExecuteNonQuery("usp_AN_UpdateHeadCount");

                            }
                        }
                        foreach (var item in to.tableProducts)
                        {
                            for (int i = 0; i < item.ProductQty; i++)
                            {
                                int orderPartId = 0;
                                SqlDataManager manager2 = new SqlDataManager();
                                Guid orderGUID = new Guid();
                                if (item.Description.Contains("Buffet") || item.Description.Contains("Meal"))
                                {
                                    manager2.AddParameter("@Type", "Buffet");
                                }
                                else
                                {
                                    manager2.AddParameter("@Type", "Drink");
                                }
                                if (existingOrder)
                                {
                                    orderGUID = existingOrderId;
                                }
                                else if (to.Type == "N")
                                {
                                    orderGUID = os.OrderGUID;
                                }
                                else
                                {
                                    orderGUID = to.OrderGUID;
                                }
                                //if(orderGUID == Guid.Empty)
                                //{
                                //    os.message = "Error inserting items. Please try again";
                                //    return os;
                                //}
                                manager2.AddParameter("@OrderGUID", orderGUID);
                                manager2.AddParameter("@ProductID", item.ProductID);
                                manager2.AddParameter("@Qty", 1);
                                manager2.AddParameter("@Price", item.Price);
                                manager2.AddParameter("@Total", item.Price);
                                manager2.AddParameter("@UserID", to.tableUser.UserID);
                                manager2.AddParameter("@OptionID", item.OptionID);
                                manager2.AddParameter("@OrderType", to.Type);
                                manager2.AddParameter("@OrderNo", to.LastOrderNo);
                                manager2.AddParameter("@TableID", to.tableDetails.TableID);
                                manager2.ExecuteNonQuery("usp_AN_InsertOrderPart");
                            }

                        }
                        //Change Date : 20/11/2019
                        //Changes by Gaurav. Commenting below code to stop deposit allocation on table when waitlist item is assigned to table

                        //if (to.tableCustomer != null && to.tableCustomer.DepositPaid)
                        //{
                        //    decimal depAmt = 0;
                        //    int loopCount = 0;
                        //    if ((to.tableCustomer.NoOfGuests > 1) && (to.tableCustomer.DepositAmount % to.tableCustomer.NoOfGuests == 0))
                        //    {
                        //        depAmt = (to.tableCustomer.DepositAmount / to.tableCustomer.NoOfGuests) * -1;
                        //        loopCount = to.tableCustomer.NoOfGuests;
                        //    }
                        //    else
                        //    {
                        //        depAmt = to.tableCustomer.DepositAmount * -1;
                        //        loopCount = 1;
                        //    }
                        //    for (int i = 0; i < loopCount; i++)
                        //    {
                        //        SqlDataManager manager4 = new SqlDataManager();
                        //        manager4.AddParameter("@Type", "Buffet");
                        //        manager4.AddParameter("@OrderGUID", os.OrderGUID);
                        //        manager4.AddParameter("@ProductID", to.tableCustomer.ProductID);
                        //        manager4.AddParameter("@Qty", 1);
                        //        manager4.AddParameter("@Price", depAmt);
                        //        manager4.AddParameter("@Total", depAmt);
                        //        manager4.AddParameter("@UserID", to.tableUser.UserID);
                        //        manager4.AddParameter("@OptionID", 0);
                        //        manager4.AddParameter("@OrderType", to.Type);
                        //        manager4.AddParameter("@OrderNo", to.LastOrderNo);
                        //        manager4.AddParameter("@TableID", to.tableDetails.TableID);
                        //        manager4.ExecuteNonQuery("usp_AN_InsertOrderPart");
                        //    }
                        //    var currentRes = context.tblReservations.Where(x => x.ReservationID == to.tableCustomer.ReservationID).FirstOrDefault();
                        //    currentRes.Processed = true;
                        //    currentRes.ProcessedOrderGUID = os.OrderGUID;
                        //    currentRes.ProcessedDate = DateTime.Now;
                        //    context.tblReservations.Attach(currentRes);
                        //    context.Entry(currentRes).State = EntityState.Modified;
                        //    context.SaveChanges();
                        //    //Update TCB DB to mark this reservation complete

                        //}
                        if (to.joinTables.Count > 0)
                        {
                            foreach (var item in to.joinTables)
                            {
                                SqlDataManager mgr = new SqlDataManager();
                                mgr.AddParameter("@OrderGUID", os.OrderGUID);
                                mgr.AddParameter("@TableId", item.TableID);
                                mgr.ExecuteNonQuery("usp_AN_UpdateJoinedTables");
                            }
                        }
                        if (to.tableProducts.Count > 0)
                        {


                            String strPrint = "";
                            string nl = System.Environment.NewLine;
                            strPrint = "----------------------------" + nl;
                            strPrint = strPrint + "        " + DateTime.Now.ToString("dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture) + nl;
                            strPrint = strPrint + "Ordered by " + to.tableUser.UserName + nl;
                            strPrint = strPrint + "TABLE - " + to.tableDetails.TableNumber + nl;
                            strPrint = strPrint + nl;

                            foreach (var item in to.tableProducts)
                            {
                                //if (!item.Description.Contains("Buffet"))
                                //{
                                if (item.Options != null && item.Options != "")
                                    strPrint = strPrint + item.ProductQty + " * " + item.Description + " " + item.Options + nl;
                                else
                                    strPrint = strPrint + item.ProductQty + " * " + item.Description + " " + nl;
                                //}
                            }

                            strPrint = strPrint + nl;
                            strPrint = strPrint + "----------------------------";
                            string printer = "Bar";
                            if (to.tableUser.UserPrinter != null)
                            {
                                printer = to.tableUser.UserPrinter;
                            }
                            SqlDataManager manager3 = new SqlDataManager();
                            manager3.AddParameter("@User", to.tableUser.UserID);
                            manager3.AddParameter("@PC", "App");
                            manager3.AddParameter("@Printer", printer);
                            manager3.AddParameter("@Receipt", strPrint);
                            manager3.ExecuteNonQuery("usp_AN_InsertPrintQueue");
                        }
                        //Save Unique code in tblTableOrder



                    }
                    else
                        os.OrderGUID = new Guid();
                    if (os.OrderGUID != Guid.Empty)
                    {


                        if (!dbContext.tblTableOrders.Any(x => x.OrderGUID == os.OrderGUID && x.TableId == to.tableDetails.TableID && x.Active == true))
                        {
                            os.UniqueCode = rh.GenerateUniqueCode();
                            tblTableOrder tord = new tblTableOrder();
                            tord.Active = true;
                            tord.CustomerCount = to.tableDetails.PaxCount;
                            tord.DateCreated = DateTime.Now;
                            tord.OrderGUID = os.OrderGUID;
                            tord.TableId = to.tableDetails.TableID;
                            tord.UniqueCode = os.UniqueCode;
                            //if (!dbContext.tblOrderBuffetItems.Any(x => x.OrderGUID == os.OrderGUID)
                            //    && (to.BuffetItems != null && to.BuffetItems.Count > 0))
                            //tord.NextOrderTime = DateTime.Now;
                            dbContext.tblTableOrders.Add(tord);
                            dbContext.SaveChanges();
                        }
                        if (to.BuffetItems != null && to.BuffetItems.Count > 0)
                        {
                            var taOrd = dbContext.tblTableOrders.Where(x => x.OrderGUID == os.OrderGUID).FirstOrDefault();
                            if (taOrd.NextOrderTime == null)
                                //taOrd.NextOrderTime = DateTime.Now.AddMinutes(printInterval);
                                taOrd.NextOrderTime = DateTime.Now;
                            else
                            {
                                if (!dbContext.tblOrderBuffetItems.Any(x => x.OrderGUID == os.OrderGUID && x.Printed == false))
                                    taOrd.NextOrderTime = DateTime.Now.AddMinutes(printInterval);

                            }
                            dbContext.Entry(taOrd).State = EntityState.Modified;
                            dbContext.SaveChanges();
                            foreach (var item in to.BuffetItems)
                            {
                                for (int i = 0; i < item.Qty; i++)
                                {
                                    tblOrderBuffetItem tbi = new tblOrderBuffetItem();
                                    tbi.OrderGUID = os.OrderGUID;
                                    tbi.TableId = to.tableDetails.TableID;
                                    tbi.ProductId = item.ProductId;
                                    tbi.Printed = false;
                                    tbi.DateCreated = DateTime.Now;
                                    tbi.Qty = 1;
                                    tbi.UserType = to.UserType;
                                    tbi.UserName = to.UserName;
                                    if (to.UserType == UserType.Customer.ToString())
                                    {
                                        tbi.UserType = to.UserType;
                                        tbi.UserId = to.MobileNumber;
                                    }
                                    else
                                    {
                                        tbi.UserType = UserType.Staff.ToString();
                                        tbi.UserId = to.tableUser.UserID;
                                    }
                                    try
                                    {
                                        dbContext.tblOrderBuffetItems.Add(tbi);
                                        dbContext.SaveChanges();
                                    }
                                    catch (Exception ex)
                                    {

                                        throw;
                                    }

                                }
                            }


                        }
                    }

                    scope.Complete();
                }
                os.message = "success";


                return os;
            }
            catch (Exception ex)
            {
                os.message = ex.InnerException.Message;
                return os;
            }


        }

        public OrderSubmitResponse SubmitOrderV1(TableOrder to)
        {
            OrderSubmitResponse os = new OrderSubmitResponse();
            if (to.tableDetails.TableID == 0)
            {
                os.OrderGUID = Guid.Empty;
                os.message = "We could not submit the order. Please try again. Thanks";
                os.Logout = true;
                return os;
            }
            if (to.OrderGUID != null)
            {
                os.OrderGUID = to.OrderGUID;
                Guid emptyGuid = Guid.Empty;
                if (dbContext.tblOrders.Any(x => x.OrderGUID == os.OrderGUID && x.BillPrinted == true && x.OrderGUID != emptyGuid) && to.UserType != UserType.Staff.ToString())
                {
                    os.OrderGUID = Guid.Empty;
                    os.message = "Bill printed for this table. Thanks";
                    os.Logout = false;
                    return os;
                }
                else if (dbContext.tblOrders.Any(x => x.OrderGUID == os.OrderGUID && x.OrderGUID != emptyGuid && ((x.Paid == true && x.DelInd == false) || x.DelInd == true)) ||
                    //(dbContext.tblOrders.Any(x => x.OrderGUID == os.OrderGUID && x.DelInd == true)) ||
                    (dbContext.tblTableOrders.Any(x => x.OrderGUID == os.OrderGUID && x.Active == false && x.OrderGUID != emptyGuid)))
                {
                    os.OrderGUID = Guid.Empty;
                    os.message = "Table Paid/Code Expired. Please contact our staff for new code. Thanks";
                    os.Logout = true;
                    return os;
                }
            }
            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    //fix for issue raised in Dec-2017//
                    //Check if there's an existing order on that table
                    ChineseTillEntities1 context = new ChineseTillEntities1();
                    var res = context.tblOrders.Where(p => p.TableID == to.tableDetails.TableID
                                && DbFunctions.TruncateTime(p.DateCreated) == DbFunctions.TruncateTime(DateTime.Now)
                                && p.DelInd == false && p.Paid == false);
                    //comment above code and uncomment below for Hushi
                    //var res = context.tblOrders.Where(p => p.TableID == to.tableDetails.TableID
                    //            && p.DelInd == false && p.Paid == false);
                    Guid existingOrderId = new Guid();
                    bool existingOrder = false;

                    if (res.Count() == 0)
                    //if (to.Type == "N")
                    //if (to.Type.Equals("N", StringComparison.InvariantCultureIgnoreCase))
                    {
                        os.OrderGUID = new Guid();
                        int custID = 0;
                        //** Insert Customer Details **//
                        //if (to.tableCustomer.Type == "N" && to.tableCustomer.Mobile != null)
                        //{
                        //    SqlDataManager manager = new SqlDataManager();
                        //    manager.AddParameter("@Name", to.tableCustomer.Name);
                        //    manager.AddParameter("@EmailID", to.tableCustomer.EmailID);
                        //    manager.AddParameter("@Mobile", to.tableCustomer.Mobile);
                        //    manager.AddParameter("@DateOfBirth", to.tableCustomer.DOB);
                        //    manager.AddOutputParameter("@CustID", System.Data.DbType.Int32, custID);
                        //    manager.ExecuteNonQuery("usp_AN_InsertCustomer");
                        //    custID = FieldConverter.To<Int32>(manager.GetParameterValue("@CustID"));
                        //}

                        string tm = DateTime.Now.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture);

                        SqlDataManager manager1 = new SqlDataManager();
                        manager1.AddParameter("@TableID", to.tableDetails.TableID);
                        manager1.AddParameter("@TableNumber", to.tableDetails.TableNumber);
                        manager1.AddParameter("@TabID", 99);
                        manager1.AddParameter("@Time", tm);
                        manager1.AddParameter("@UserID", to.tableUser.UserID);
                        if (custID > 0)
                        {
                            manager1.AddParameter("@CustID", custID);
                        }
                        else
                            manager1.AddParameter("@CustID", 0);
                        manager1.AddParameter("@CustCount", to.tableDetails.PaxCount);
                        manager1.AddParameter("@AdCount", to.tableDetails.AdCount);
                        manager1.AddParameter("@KdCount", to.tableDetails.KdCount);
                        manager1.AddParameter("@JnCount", to.tableDetails.JnCount);
                        manager1.AddParameter("@PrevCust", to.tableCustomer.PrevCust);
                        if (to.ReservedCustomer)
                        {
                            manager1.AddParameter("@ReservedCustomer", "T");
                            manager1.AddParameter("@ReservationID", to.tableCustomer.ReservationID);
                        }
                        else
                        {
                            manager1.AddParameter("@ReservedCustomer", "F");
                            manager1.AddParameter("@ReservationID", to.tableCustomer.ReservationID);
                        }
                        manager1.AddOutputParameter("@OrderID", System.Data.DbType.Guid, os.OrderGUID);
                        manager1.ExecuteNonQuery("usp_AN_InsertOrder");
                        os.OrderGUID = FieldConverter.To<Guid>(manager1.GetParameterValue("@OrderID"));
                        //version 1.55.57 changes
                        //if (os.OrderGUID == Guid.Empty)
                        //{
                        //    os.message = "Empty Order string. Please try again";
                        //    return os;
                        //}
                        //changes end
                    }
                    else
                    {
                        existingOrder = true;
                        foreach (var item in res)
                        {
                            existingOrderId = item.OrderGUID;
                        }

                        os.OrderGUID = existingOrderId;
                        //version 1.55.57 changes
                        //if (os.OrderGUID == Guid.Empty)
                        //{
                        //    os.message = "Empty Order string. Please try again";
                        //    return os;
                        //}
                        //changes end
                    }
                    bool found = false;
                    if (to.Type.Equals("E", StringComparison.InvariantCultureIgnoreCase))
                    {
                        //ChineseTillEntities1 context = new ChineseTillEntities1();
                        //var a = from p in context.tblOrders
                        //        where (p.OrderGUID.Equals(to.OrderGUID)) && p.Paid == false && p.DelInd == false
                        //        select p;

                        SqlDataManager mgr = new SqlDataManager();
                        mgr.AddParameter("@OrderGUID", to.OrderGUID);
                        DataTable results = mgr.ExecuteDataTable("usp_AN_CheckTableOrder");
                        if (results.Rows.Count > 0)
                        {
                            found = true;
                        }

                    }
                    else
                        found = true;

                    if (found)
                    {

                        if (to.Type.Equals("E", StringComparison.InvariantCultureIgnoreCase) || existingOrder)
                        {
                            if (to.tableDetails.AdCount > 0 || to.tableDetails.KdCount > 0 || to.tableDetails.JnCount > 0)
                            {
                                SqlDataManager manager4 = new SqlDataManager();
                                if (existingOrder)
                                    manager4.AddParameter("@OrderGUID", existingOrderId);
                                else
                                    manager4.AddParameter("@OrderGUID", to.OrderGUID);
                                manager4.AddParameter("@AdCount", to.tableDetails.AdCount);
                                manager4.AddParameter("@KdCount", to.tableDetails.KdCount);
                                manager4.AddParameter("@JnCount", to.tableDetails.JnCount);
                                manager4.ExecuteNonQuery("usp_AN_UpdateHeadCount");

                            }
                        }
                        foreach (var item in to.tableProducts)
                        {
                            for (int i = 0; i < item.ProductQty; i++)
                            {
                                int orderPartId = 0;
                                SqlDataManager manager2 = new SqlDataManager();
                                Guid orderGUID = new Guid();
                                if (item.Description.Contains("Buffet") || item.Description.Contains("Meal"))
                                {
                                    manager2.AddParameter("@Type", "Buffet");
                                }
                                else
                                {
                                    manager2.AddParameter("@Type", "Drink");
                                }
                                if (existingOrder)
                                {
                                    orderGUID = existingOrderId;
                                }
                                else if (to.Type == "N")
                                {
                                    orderGUID = os.OrderGUID;
                                }
                                else
                                {
                                    orderGUID = to.OrderGUID;
                                }
                                //if(orderGUID == Guid.Empty)
                                //{
                                //    os.message = "Error inserting items. Please try again";
                                //    return os;
                                //}
                                manager2.AddParameter("@OrderGUID", orderGUID);
                                manager2.AddParameter("@ProductID", item.ProductID);
                                manager2.AddParameter("@Qty", 1);
                                manager2.AddParameter("@Price", item.Price);
                                manager2.AddParameter("@Total", item.Price);
                                manager2.AddParameter("@UserID", to.tableUser.UserID);
                                manager2.AddParameter("@OptionID", item.OptionID);
                                manager2.AddParameter("@OrderType", to.Type);
                                manager2.AddParameter("@OrderNo", to.LastOrderNo);
                                manager2.AddParameter("@TableID", to.tableDetails.TableID);
                                manager2.ExecuteNonQuery("usp_AN_InsertOrderPart");
                            }

                        }

                        //fix for drinks order moving to food menu
                        if (to.BuffetItems != null && to.BuffetItems.Count > 0)
                        {
                            foreach (var item in to.BuffetItems)
                            {
                                var p1 = dbContext.tblProducts.Where(x => x.ProductID == item.ProductId && x.FoodRefil == false).FirstOrDefault();
                                if (p1 != null)
                                {
                                    for (int i = 0; i < item.Qty; i++)
                                    {
                                        int orderPartId = 0;
                                        SqlDataManager manager2 = new SqlDataManager();
                                        Guid orderGUID = new Guid();
                                        manager2.AddParameter("@Type", "Drink");
                                        if (existingOrder)
                                        {
                                            orderGUID = existingOrderId;
                                        }
                                        else if (to.Type == "N")
                                        {
                                            orderGUID = os.OrderGUID;
                                        }
                                        else
                                        {
                                            orderGUID = to.OrderGUID;
                                        }
                                        //if(orderGUID == Guid.Empty)
                                        //{
                                        //    os.message = "Error inserting items. Please try again";
                                        //    return os;
                                        //}
                                        manager2.AddParameter("@OrderGUID", orderGUID);
                                        manager2.AddParameter("@ProductID", item.ProductId);
                                        manager2.AddParameter("@Qty", 1);
                                        manager2.AddParameter("@Price", (decimal)p1.Price);
                                        manager2.AddParameter("@Total", (decimal)p1.Price);
                                        manager2.AddParameter("@UserID", to.tableUser.UserID);
                                        manager2.AddParameter("@OptionID", 0);
                                        manager2.AddParameter("@OrderType", to.Type);
                                        manager2.AddParameter("@OrderNo", to.LastOrderNo);
                                        manager2.AddParameter("@TableID", to.tableDetails.TableID);
                                        manager2.ExecuteNonQuery("usp_AN_InsertOrderPart");
                                    }
                                }
                            }
                        }

                        if (to.joinTables.Count > 0)
                        {
                            foreach (var item in to.joinTables)
                            {
                                SqlDataManager mgr = new SqlDataManager();
                                mgr.AddParameter("@OrderGUID", os.OrderGUID);
                                mgr.AddParameter("@TableId", item.TableID);
                                mgr.ExecuteNonQuery("usp_AN_UpdateJoinedTables");
                            }
                        }
                        if (to.tableProducts.Count > 0)
                        {


                            String strPrint = "";
                            string nl = System.Environment.NewLine;
                            strPrint = "----------------------------" + nl;
                            strPrint = strPrint + "        " + DateTime.Now.ToString("dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture) + nl;
                            strPrint = strPrint + "Ordered by " + to.tableUser.UserName + nl;
                            strPrint = strPrint + "TABLE - " + to.tableDetails.TableNumber + nl;
                            strPrint = strPrint + nl;

                            foreach (var item in to.tableProducts)
                            {
                                //if (!item.Description.Contains("Buffet"))
                                //{
                                if (item.Options != null && item.Options != "")
                                {
                                    string it = item.ProductQty + " * " + item.Description + " " + item.Options;
                                    it = SpliceText(it, 25);
                                    strPrint = strPrint + it + nl;
                                }
                                else
                                    strPrint = strPrint + item.ProductQty + " * " + item.Description + " " + nl;
                                //}
                            }

                            strPrint = strPrint + nl;
                            strPrint = strPrint + "----------------------------";
                            string printer = "Bar";
                            if (to.tableUser.UserPrinter != null)
                            {
                                printer = to.tableUser.UserPrinter;
                            }
                            SqlDataManager manager3 = new SqlDataManager();
                            manager3.AddParameter("@User", to.tableUser.UserID);
                            manager3.AddParameter("@PC", "App");
                            manager3.AddParameter("@Printer", printer);
                            manager3.AddParameter("@Receipt", strPrint);
                            manager3.ExecuteNonQuery("usp_AN_InsertPrintQueue");
                        }

                        //fix for drinks item appearing in food menu
                        if (to.BuffetItems != null && to.BuffetItems.Count > 0)
                        {

                            bool drinkFound = false;
                            String strPrint = "";
                            string nl = System.Environment.NewLine;
                            strPrint = "----------------------------" + nl;
                            strPrint = strPrint + "        " + DateTime.Now.ToString("dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture) + nl;
                            strPrint = strPrint + "Ordered by " + to.tableUser.UserName + nl;
                            strPrint = strPrint + "TABLE - " + to.tableDetails.TableNumber + nl;
                            strPrint = strPrint + nl;

                            foreach (var item in to.BuffetItems)
                            {
                                if (dbContext.tblProducts.Any(x => x.ProductID == item.ProductId && x.FoodRefil == false))
                                {
                                    drinkFound = true;
                                    strPrint = strPrint + item.Qty + " * " + item.Description + " " + nl;
                                }
                            }
                            if (drinkFound)
                            {
                                strPrint = strPrint + nl;
                                strPrint = strPrint + "----------------------------";
                                string printer = "Bar";
                                if (to.tableUser.UserPrinter != null)
                                {
                                    printer = to.tableUser.UserPrinter;
                                }
                                SqlDataManager manager3 = new SqlDataManager();
                                manager3.AddParameter("@User", to.tableUser.UserID);
                                manager3.AddParameter("@PC", "App");
                                manager3.AddParameter("@Printer", printer);
                                manager3.AddParameter("@Receipt", strPrint);
                                manager3.ExecuteNonQuery("usp_AN_InsertPrintQueue");
                            }
                        }
                        //Save Unique code in tblTableOrder



                    }
                    else
                        os.OrderGUID = new Guid();
                    if (!dbContext.tblTableOrders.Any(x => x.OrderGUID == os.OrderGUID && x.TableId == to.tableDetails.TableID && x.Active == true))
                    {
                        os.UniqueCode = rh.GenerateUniqueCode();
                        tblTableOrder tord = new tblTableOrder();
                        tord.Active = true;
                        tord.CustomerCount = to.tableDetails.PaxCount;
                        tord.DateCreated = DateTime.Now;
                        tord.OrderGUID = os.OrderGUID;
                        tord.TableId = to.tableDetails.TableID;
                        tord.UniqueCode = os.UniqueCode;
                        dbContext.tblTableOrders.Add(tord);
                        dbContext.SaveChanges();
                    }
                    if (to.BuffetItems != null && to.BuffetItems.Count > 0)
                    {
                        var taOrd = dbContext.tblTableOrders.Where(x => x.OrderGUID == os.OrderGUID).FirstOrDefault();
                        if (taOrd.NextOrderTime == null)
                            //taOrd.NextOrderTime = DateTime.Now.AddMinutes(printInterval);
                            taOrd.NextOrderTime = DateTime.Now;
                        else
                        {
                            if (!dbContext.tblOrderBuffetItems.Any(x => x.OrderGUID == os.OrderGUID && x.Printed == false))
                                taOrd.NextOrderTime = DateTime.Now.AddMinutes(printInterval);

                        }
                        foreach (var item in to.BuffetItems)
                        {
                            if (dbContext.tblProducts.Any(x => x.ProductID == item.ProductId && x.FoodRefil == true))
                            {
                                for (int i = 0; i < item.Qty; i++)
                                {
                                    tblOrderBuffetItem tbi = new tblOrderBuffetItem();
                                    tbi.OrderGUID = os.OrderGUID;
                                    tbi.TableId = to.tableDetails.TableID;
                                    tbi.ProductId = item.ProductId;
                                    tbi.Printed = false;
                                    tbi.DateCreated = DateTime.Now;
                                    tbi.Qty = 1;
                                    tbi.UserType = to.UserType;
                                    tbi.UserName = to.UserName;
                                    if (to.UserType == UserType.Customer.ToString())
                                    {
                                        tbi.UserType = to.UserType;
                                        tbi.UserId = to.MobileNumber;
                                    }
                                    else
                                    {
                                        tbi.UserType = UserType.Staff.ToString();
                                        tbi.UserId = to.tableUser.UserID;
                                    }
                                    try
                                    {
                                        dbContext.tblOrderBuffetItems.Add(tbi);
                                        dbContext.SaveChanges();
                                    }
                                    catch (Exception ex)
                                    {

                                        throw;
                                    }

                                }
                            }
                        }
                        //var taOrd = dbContext.tblTableOrders.Where(x => x.OrderGUID == os.OrderGUID).FirstOrDefault();
                        //if (taOrd.NextOrderTime == null)
                        //    //taOrd.NextOrderTime = DateTime.Now.AddMinutes(printInterval);
                        //    taOrd.NextOrderTime = DateTime.Now;
                        //else
                        //    taOrd.NextOrderTime = DateTime.Now.AddMinutes(printInterval);
                        //dbContext.Entry(taOrd).State = EntityState.Modified;
                        //dbContext.SaveChanges();

                    }

                    scope.Complete();
                }
                if (os.OrderGUID == Guid.Empty || os.OrderGUID == null)
                {
                    os.message = "Table Paid/Code Expired. Please contact our staff for new code. Thanks";
                    os.Logout = true;
                }
                else
                    os.message = "Order Submitted Successfully";

                return os;
            }
            catch (Exception ex)
            {
                os.message = ex.InnerException.Message + ex.InnerException.StackTrace + ex.InnerException.InnerException;

                return os;
            }


        }


        public OrderSubmitResponse SubmitOrderV2(TableOrder to)
        {
            OrderSubmitResponse os = new OrderSubmitResponse();
            int totalRedeemedPoints = 0;
            int totalEarnedPoints = 0;
            string unAvailableProducts = "";
            bool errorFound = false;
            if (to.tableDetails.TableID == 0)
            {
                os.OrderGUID = Guid.Empty;
                os.message = "We could not submit the order. Please try again. Thanks";
                os.Logout = true;
                return os;
            }
            if (to.OrderGUID != null)
            {
                os.OrderGUID = to.OrderGUID;
                Guid emptyGuid = Guid.Empty;
                if (dbContext.tblOrders.Any(x => x.OrderGUID == os.OrderGUID && x.BillPrinted == true && x.OrderGUID != emptyGuid) && to.UserType != UserType.Staff.ToString())
                {
                    os.OrderGUID = Guid.Empty;
                    //os.message = "Bill printed for this table. Thanks";
                    os.message = "Your order is not sent through as the bill is printed for this table. Please ask for waiter service.Thanks";

                    os.Logout = false;
                    return os;
                }
                else if (dbContext.tblOrders.Any(x => x.OrderGUID == os.OrderGUID && x.OrderGUID != emptyGuid && ((x.Paid == true && x.DelInd == false) || x.DelInd == true)) ||
                    //(dbContext.tblOrders.Any(x => x.OrderGUID == os.OrderGUID && x.DelInd == true)) ||
                    (dbContext.tblTableOrders.Any(x => x.OrderGUID == os.OrderGUID && x.Active == false && x.OrderGUID != emptyGuid)))
                {
                    os.OrderGUID = Guid.Empty;
                    os.message = "Table Paid/Code Expired. Please contact our staff for new code. Thanks";
                    os.Logout = true;
                    return os;
                }
            }
            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    //fix for issue raised in Dec-2017//
                    //Check if there's an existing order on that table
                    ChineseTillEntities1 context = new ChineseTillEntities1();
                    var res = context.tblOrders.Where(p => p.TableID == to.tableDetails.TableID
                                && DbFunctions.TruncateTime(p.DateCreated) == DbFunctions.TruncateTime(DateTime.Now)
                                && p.DelInd == false && p.Paid == false);
                    //comment above code and uncomment below for Hushi
                    //var res = context.tblOrders.Where(p => p.TableID == to.tableDetails.TableID
                    //            && p.DelInd == false && p.Paid == false);
                    Guid existingOrderId = new Guid();
                    bool existingOrder = false;

                    if (res.Count() == 0)
                    //if (to.Type == "N")
                    //if (to.Type.Equals("N", StringComparison.InvariantCultureIgnoreCase))
                    {
                        os.OrderGUID = new Guid();
                        int custID = 0;
                        custID = to.CustomerId;
                        string tm = DateTime.Now.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture);

                        SqlDataManager manager1 = new SqlDataManager();
                        manager1.AddParameter("@TableID", to.tableDetails.TableID);
                        manager1.AddParameter("@TableNumber", to.tableDetails.TableNumber);
                        manager1.AddParameter("@TabID", 99);
                        manager1.AddParameter("@Time", tm);
                        manager1.AddParameter("@UserID", to.tableUser.UserID);
                        if (custID > 0)
                        {
                            manager1.AddParameter("@CustID", custID);
                        }
                        else
                            manager1.AddParameter("@CustID", 0);
                        manager1.AddParameter("@CustCount", to.tableDetails.PaxCount);
                        manager1.AddParameter("@AdCount", to.tableDetails.AdCount);
                        manager1.AddParameter("@KdCount", to.tableDetails.KdCount);
                        manager1.AddParameter("@JnCount", to.tableDetails.JnCount);
                        manager1.AddParameter("@PrevCust", to.tableCustomer.PrevCust);
                        if (to.ReservedCustomer)
                        {
                            manager1.AddParameter("@ReservedCustomer", "T");
                            manager1.AddParameter("@ReservationID", to.tableCustomer.ReservationID);
                        }
                        else
                        {
                            manager1.AddParameter("@ReservedCustomer", "F");
                            manager1.AddParameter("@ReservationID", to.tableCustomer.ReservationID);
                        }
                        manager1.AddOutputParameter("@OrderID", System.Data.DbType.Guid, os.OrderGUID);
                        manager1.ExecuteNonQuery("usp_AN_InsertOrder_V1");
                        os.OrderGUID = FieldConverter.To<Guid>(manager1.GetParameterValue("@OrderID"));
                        //version 1.55.57 changes
                        //if (os.OrderGUID == Guid.Empty)
                        //{
                        //    os.message = "Empty Order string. Please try again";
                        //    return os;
                        //}
                        //changes end
                    }
                    else
                    {
                        existingOrder = true;
                        foreach (var item in res)
                        {
                            existingOrderId = item.OrderGUID;
                        }

                        os.OrderGUID = existingOrderId;
                        //update customerId if orderd from user app
                        if (to.CustomerId > 0)
                        {
                            var order = dbContext.tblOrders.Where(x => x.OrderGUID == os.OrderGUID).FirstOrDefault();
                            order.CustomerId = to.CustomerId;
                            dbContext.Entry(order).State = EntityState.Modified;
                            dbContext.SaveChanges();
                        }
                    }
                    bool found = false;
                    if (to.Type.Equals("E", StringComparison.InvariantCultureIgnoreCase))
                    {
                        //ChineseTillEntities1 context = new ChineseTillEntities1();
                        //var a = from p in context.tblOrders
                        //        where (p.OrderGUID.Equals(to.OrderGUID)) && p.Paid == false && p.DelInd == false
                        //        select p;

                        SqlDataManager mgr = new SqlDataManager();
                        mgr.AddParameter("@OrderGUID", to.OrderGUID);
                        DataTable results = mgr.ExecuteDataTable("usp_AN_CheckTableOrder");
                        if (results.Rows.Count > 0)
                        {
                            found = true;
                        }

                    }
                    else
                        found = true;

                    if (found)
                    {

                        if (to.Type.Equals("E", StringComparison.InvariantCultureIgnoreCase) || existingOrder)
                        {
                            if (to.tableDetails.AdCount > 0 || to.tableDetails.KdCount > 0 || to.tableDetails.JnCount > 0)
                            {
                                SqlDataManager manager4 = new SqlDataManager();
                                if (existingOrder)
                                    manager4.AddParameter("@OrderGUID", existingOrderId);
                                else
                                    manager4.AddParameter("@OrderGUID", to.OrderGUID);
                                manager4.AddParameter("@AdCount", to.tableDetails.AdCount);
                                manager4.AddParameter("@KdCount", to.tableDetails.KdCount);
                                manager4.AddParameter("@JnCount", to.tableDetails.JnCount);
                                manager4.ExecuteNonQuery("usp_AN_UpdateHeadCount");

                            }
                        }
                        foreach (var item in to.tableProducts)
                        {
                            for (int i = 0; i < item.ProductQty; i++)
                            {

                                totalEarnedPoints += item.RewardPoints;
                                int orderPartId = 0;
                                SqlDataManager manager2 = new SqlDataManager();
                                Guid orderGUID = new Guid();
                                if (item.Description.Contains("Buffet") || item.Description.Contains("Meal"))
                                {
                                    manager2.AddParameter("@Type", "Buffet");
                                }
                                else
                                {
                                    manager2.AddParameter("@Type", "Drink");
                                }
                                if (existingOrder)
                                {
                                    orderGUID = existingOrderId;
                                }
                                else if (to.Type == "N")
                                {
                                    orderGUID = os.OrderGUID;
                                }
                                else
                                {
                                    orderGUID = to.OrderGUID;
                                }
                                //if(orderGUID == Guid.Empty)
                                //{
                                //    os.message = "Error inserting items. Please try again";
                                //    return os;
                                //}
                                manager2.AddParameter("@OrderGUID", orderGUID);
                                manager2.AddParameter("@ProductID", item.ProductID);
                                manager2.AddParameter("@Qty", 1);
                                manager2.AddParameter("@Price", item.Price);
                                manager2.AddParameter("@Total", item.Price);
                                manager2.AddParameter("@UserID", to.tableUser.UserID);
                                manager2.AddParameter("@OptionID", item.OptionID);
                                manager2.AddParameter("@OrderType", to.Type);
                                manager2.AddParameter("@OrderNo", to.LastOrderNo);
                                manager2.AddParameter("@TableID", to.tableDetails.TableID);
                                manager2.ExecuteNonQuery("usp_AN_InsertOrderPart");

                                if (item.IsRedemptionProduct == true)
                                {
                                    totalRedeemedPoints += item.RedemptionPoints;
                                    tblRedeemedProduct tr = new tblRedeemedProduct();
                                    tr.ProductId = item.ProductID;
                                    tr.OrderGUID = orderGUID;
                                    tr.OrderType = OrderType.DineIn.ToString();
                                    tr.DateCreated = DateTime.Now;
                                    tr.DelInd = false;
                                    tr.Points = item.RedemptionPoints;
                                    tr.Qty = 1;
                                    tr.Price = (decimal)item.Price;
                                    tr.WebUpload = false;
                                    try
                                    {
                                        dbContext.tblRedeemedProducts.Add(tr);
                                        dbContext.SaveChanges();
                                    }
                                    catch (Exception ex)
                                    {

                                        os.message = ex.Message;
                                        if (ex.InnerException != null)
                                            os.message = ex.InnerException.StackTrace;
                                        os.OrderGUID = Guid.Empty;
                                        os.Logout = false;
                                        return os;
                                    }

                                }
                            }

                        }

                        //fix for drinks order moving to food menu
                        if (to.BuffetItems != null && to.BuffetItems.Count > 0)
                        {
                            //Check if any menu item got updated recently. In case it's updated, show message to user


                            foreach (var item in to.BuffetItems)
                            {
                                var p1 = dbContext.tblProducts.Where(x => x.ProductID == item.ProductId && x.FoodRefil == false).FirstOrDefault();
                                if (p1 != null)
                                {
                                    for (int i = 0; i < item.Qty; i++)
                                    {
                                        int orderPartId = 0;
                                        SqlDataManager manager2 = new SqlDataManager();
                                        Guid orderGUID = new Guid();
                                        manager2.AddParameter("@Type", "Drink");
                                        if (existingOrder)
                                        {
                                            orderGUID = existingOrderId;
                                        }
                                        else if (to.Type == "N")
                                        {
                                            orderGUID = os.OrderGUID;
                                        }
                                        else
                                        {
                                            orderGUID = to.OrderGUID;
                                        }
                                        //if(orderGUID == Guid.Empty)
                                        //{
                                        //    os.message = "Error inserting items. Please try again";
                                        //    return os;
                                        //}
                                        manager2.AddParameter("@OrderGUID", orderGUID);
                                        manager2.AddParameter("@ProductID", item.ProductId);
                                        manager2.AddParameter("@Qty", 1);
                                        manager2.AddParameter("@Price", (decimal)p1.Price);
                                        manager2.AddParameter("@Total", (decimal)p1.Price);
                                        manager2.AddParameter("@UserID", to.tableUser.UserID);
                                        manager2.AddParameter("@OptionID", 0);
                                        manager2.AddParameter("@OrderType", to.Type);
                                        manager2.AddParameter("@OrderNo", to.LastOrderNo);
                                        manager2.AddParameter("@TableID", to.tableDetails.TableID);
                                        manager2.ExecuteNonQuery("usp_AN_InsertOrderPart");
                                    }
                                }

                            }
                        }

                        if (to.joinTables.Count > 0)
                        {
                            foreach (var item in to.joinTables)
                            {
                                SqlDataManager mgr = new SqlDataManager();
                                mgr.AddParameter("@OrderGUID", os.OrderGUID);
                                mgr.AddParameter("@TableId", item.TableID);
                                mgr.ExecuteNonQuery("usp_AN_UpdateJoinedTables");
                            }
                        }
                        if (to.tableProducts.Count > 0)
                        {


                            String strPrint = "";
                            string nl = System.Environment.NewLine;
                            strPrint = "----------------------------" + nl;
                            strPrint = strPrint + "        " + DateTime.Now.ToString("dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture) + nl;
                            strPrint = strPrint + "Ordered by " + to.tableUser.UserName + nl;
                            strPrint = strPrint + "TABLE - " + to.tableDetails.TableNumber + nl;
                            strPrint = strPrint + nl;

                            foreach (var item in to.tableProducts)
                            {
                                //if (!item.Description.Contains("Buffet"))
                                //{
                                if (item.Options != null && item.Options != "")
                                {
                                    string it = item.ProductQty + " * " + item.Description + " " + item.Options;
                                    it = SpliceText(it, 25);
                                    strPrint = strPrint + it + nl;
                                }
                                else
                                    strPrint = strPrint + item.ProductQty + " * " + item.Description + " " + nl;
                                //}
                            }

                            strPrint = strPrint + nl;
                            strPrint = strPrint + "----------------------------";
                            string printer = "Bar";
                            if (to.tableUser.UserPrinter != null)
                            {
                                printer = to.tableUser.UserPrinter;
                            }
                            SqlDataManager manager3 = new SqlDataManager();
                            manager3.AddParameter("@User", to.tableUser.UserID);
                            manager3.AddParameter("@PC", "App");
                            manager3.AddParameter("@Printer", printer);
                            manager3.AddParameter("@Receipt", strPrint);
                            manager3.ExecuteNonQuery("usp_AN_InsertPrintQueue");
                        }

                        //fix for drinks item appearing in food menu
                        if (to.BuffetItems != null && to.BuffetItems.Count > 0)
                        {

                            bool drinkFound = false;
                            String strPrint = "";
                            string nl = System.Environment.NewLine;
                            strPrint = "----------------------------" + nl;
                            strPrint = strPrint + "        " + DateTime.Now.ToString("dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture) + nl;
                            strPrint = strPrint + "Ordered by " + to.tableUser.UserName + nl;
                            strPrint = strPrint + "TABLE - " + to.tableDetails.TableNumber + nl;
                            strPrint = strPrint + nl;

                            foreach (var item in to.BuffetItems)
                            {
                                if (dbContext.tblProducts.Any(x => x.ProductID == item.ProductId && x.FoodRefil == false))
                                {
                                    drinkFound = true;
                                    strPrint = strPrint + item.Qty + " * " + item.Description + " " + nl;
                                }
                            }
                            if (drinkFound)
                            {
                                strPrint = strPrint + nl;
                                strPrint = strPrint + "----------------------------";
                                string printer = "Bar";
                                if (to.tableUser.UserPrinter != null)
                                {
                                    printer = to.tableUser.UserPrinter;
                                }
                                SqlDataManager manager3 = new SqlDataManager();
                                manager3.AddParameter("@User", to.tableUser.UserID);
                                manager3.AddParameter("@PC", "App");
                                manager3.AddParameter("@Printer", printer);
                                manager3.AddParameter("@Receipt", strPrint);
                                manager3.ExecuteNonQuery("usp_AN_InsertPrintQueue");
                            }
                        }
                        //Save Unique code in tblTableOrder



                    }
                    else
                        os.OrderGUID = new Guid();
                    if (!dbContext.tblTableOrders.Any(x => x.OrderGUID == os.OrderGUID && x.TableId == to.tableDetails.TableID && x.Active == true))
                    {
                        os.UniqueCode = rh.GenerateUniqueCode();
                        tblTableOrder tord = new tblTableOrder();
                        tord.Active = true;
                        tord.CustomerCount = to.tableDetails.PaxCount;
                        tord.DateCreated = DateTime.Now;
                        tord.OrderGUID = os.OrderGUID;
                        tord.TableId = to.tableDetails.TableID;
                        tord.UniqueCode = os.UniqueCode;
                        dbContext.tblTableOrders.Add(tord);
                        dbContext.SaveChanges();
                    }
                    if (to.BuffetItems != null && to.BuffetItems.Count > 0)
                    {
                        var taOrd = dbContext.tblTableOrders.Where(x => x.OrderGUID == os.OrderGUID).FirstOrDefault();
                        if (taOrd.NextOrderTime == null)
                            //taOrd.NextOrderTime = DateTime.Now.AddMinutes(printInterval);
                            taOrd.NextOrderTime = DateTime.Now;
                        else
                        {
                            if (!dbContext.tblOrderBuffetItems.Any(x => x.OrderGUID == os.OrderGUID && x.Printed == false))
                                taOrd.NextOrderTime = DateTime.Now.AddMinutes(printInterval);

                        }

                        DateTime updatedTime = DateTime.Now.AddMinutes(-180);
                        var products = dbContext.tblMenuItems.Where(x => x.LastModified != null && x.LastModified >= updatedTime && (x.bOnsite == false || x.Active == false)).ToList();
                        foreach (var item in to.BuffetItems)
                        {
                            if (dbContext.tblProducts.Any(x => x.ProductID == item.ProductId && x.FoodRefil == true))
                            {

                                bool itemAvailable = true;
                                if (products != null & products.Count > 0)
                                {
                                    if (products.Any(x => x.ProductID == item.ProductId))
                                    {
                                        itemAvailable = false;
                                        unAvailableProducts += item.Description + ",";
                                    }
                                }
                                if (itemAvailable)
                                {
                                    for (int i = 0; i < item.Qty; i++)
                                    {
                                        tblOrderBuffetItem tbi = new tblOrderBuffetItem();
                                        tbi.OrderGUID = os.OrderGUID;
                                        tbi.TableId = to.tableDetails.TableID;
                                        tbi.ProductId = item.ProductId;
                                        tbi.Printed = false;
                                        tbi.DateCreated = DateTime.Now;
                                        tbi.Qty = 1;
                                        tbi.UserType = to.UserType;
                                        tbi.UserName = to.UserName;
                                        if (to.UserType == UserType.Customer.ToString())
                                        {
                                            tbi.UserType = to.UserType;
                                            tbi.UserId = to.MobileNumber;
                                        }
                                        else
                                        {
                                            tbi.UserType = UserType.Staff.ToString();
                                            tbi.UserId = to.tableUser.UserID;
                                        }
                                        try
                                        {
                                            dbContext.tblOrderBuffetItems.Add(tbi);
                                            dbContext.SaveChanges();
                                        }
                                        catch (DbEntityValidationException e)
                                        {
                                            var newException = new FormattedDbEntityValidationException(e);
                                            throw newException;
                                        }
                                        catch (Exception ex)
                                        {

                                            os.message = ex.Message;
                                            if (ex.InnerException != null)
                                                os.message = ex.InnerException.Message;
                                            os.OrderGUID = Guid.Empty;
                                            os.Logout = false;
                                            return os;

                                        }

                                    }
                                }
                            }
                        }
                        //var taOrd = dbContext.tblTableOrders.Where(x => x.OrderGUID == os.OrderGUID).FirstOrDefault();
                        //if (taOrd.NextOrderTime == null)
                        //    //taOrd.NextOrderTime = DateTime.Now.AddMinutes(printInterval);
                        //    taOrd.NextOrderTime = DateTime.Now;
                        //else
                        //    taOrd.NextOrderTime = DateTime.Now.AddMinutes(printInterval);
                        //dbContext.Entry(taOrd).State = EntityState.Modified;
                        //dbContext.SaveChanges();

                    }

                    //Update customer points
                    os.CustomerPoints = to.tableCustomer.CustomerPoints;
                    if (totalRedeemedPoints > 0 && to.CustomerId > 0)
                    {
                        int updatedPoints = cs.UpdateCustomerPoints(to.CustomerId, totalRedeemedPoints, 0, os.OrderGUID.ToString());
                        if (updatedPoints >= 0)
                            os.CustomerPoints = updatedPoints;
                    }
                    scope.Complete();
                }
                if (os.OrderGUID == Guid.Empty || os.OrderGUID == null)
                {
                    os.message = "Table Paid/Code Expired. Please contact our staff for new code. Thanks";
                    os.Logout = true;
                }
                else
                {
                    os.message = "Order Submitted Successfully.";
                    if (unAvailableProducts != "")
                        os.message += " Unfortunately " + unAvailableProducts + " got out of stock. Kindly order some other dish.";
                }
                if (os.OrderGUID != null && os.OrderGUID != Guid.Empty)
                    os.CurrentTotal = (float)dbContext.tblOrderParts.Where(x => x.OrderGUID == os.OrderGUID).Sum(x => x.Price);
                return os;
            }
            catch (Exception ex)
            {
                os.message = ex.Message;
                if (ex.InnerException != null)
                    os.message = ex.InnerException.StackTrace;

                return os;
            }
        }



        //public OrderSubmitResponse SubmitOrderV3(TableOrder to)
        //{
        //    logger.Info("Order Submission Started - " + to.tableUser.UserID);
        //    OrderSubmitResponse os = new OrderSubmitResponse();

        //    int totalRedeemedPoints = 0;
        //    string unAvailableProducts = "";
        //    bool errorFound = false;
        //    Guid emptyGuid = Guid.Empty;
        //    bool existingOrder = false;
        //    int custCount = 0;
        //    int adCount = 0;
        //    int kdCount = 0;
        //    int jnCount = 0;
        //    int batchNo = 0;
        //    int coverBuffet = 0;
        //    string itemsStr = "";
        //    string reservationUniqueCode = "";
        //    List<OrderBuffetItem> buffetItems = new List<OrderBuffetItem>();
        //    List<Product> orderedItems = new List<Product>();

        //    //Calculate custCounts from items ordered
        //    if (to.tableProducts.Count > 0)
        //    {
        //        adCount = to.tableProducts.Where(x => (x.EnglishName != null && x.EnglishName.Contains("Buffet Adult")) || (x.Description.Contains("Buffet Adult"))).Sum(x=>x.ProductQty);
        //        kdCount = to.tableProducts.Where(x => (x.EnglishName != null && x.EnglishName.Contains("Buffet Kids")) || (x.Description.Contains("Buffet Kids"))).Sum(x => x.ProductQty);
        //        jnCount = to.tableProducts.Where(x => (x.EnglishName != null && x.EnglishName.Contains("Buffet Junior")) || (x.Description.Contains("Buffet Junior"))).Sum(x => x.ProductQty);
        //        coverBuffet = to.tableProducts.Where(x => (x.EnglishName != null && x.EnglishName.Contains("Buffet cover")) || (x.Description.Contains("Buffet cover"))).Sum(x => x.ProductQty);
        //        custCount = adCount + kdCount + jnCount + coverBuffet;
        //    }
        //    if (to.tableDetails.TableID == 0)
        //    {
        //        os.OrderGUID = Guid.Empty;
        //        os.message = "We could not submit the order. Please try again. Thanks";
        //        os.Logout = true;
        //        return os;
        //    }

        //    try
        //    {
        //        using (TransactionScope scope = new TransactionScope())
        //        {
        //            using (var _context = new ChineseTillEntities1())
        //            {
        //                if (to.OrderGUID != null && to.OrderGUID != emptyGuid)
        //                {
        //                    os.OrderGUID = to.OrderGUID;
        //                    existingOrder = true;
        //                    //If bill printed, do not submit order
        //                    if (_context.tblOrders.Any(x => x.OrderGUID == os.OrderGUID && x.BillPrinted == true && x.OrderGUID != emptyGuid) && to.UserType != UserType.Staff.ToString())
        //                    {
        //                        os.OrderGUID = Guid.Empty;
        //                        os.message = "Your order is not sent through as the bill is printed for this table. Please ask for waiter service.Thanks";
        //                        os.Logout = false;
        //                        errorFound = true;
        //                    }
        //                    else if (_context.tblOrders.Any(x => x.OrderGUID == os.OrderGUID && x.OrderGUID != emptyGuid && ((x.Paid == true && x.DelInd == false) || x.DelInd == true)) ||
        //                        (_context.tblTableOrders.Any(x => x.OrderGUID == os.OrderGUID && x.Active == false && x.OrderGUID != emptyGuid)))
        //                    {
        //                        os.OrderGUID = Guid.Empty;
        //                        os.message = "Table Paid/Code Expired. Please contact our staff for new code. Thanks";
        //                        os.Logout = true;
        //                        errorFound = true;
        //                    }

        //                    if (!errorFound)
        //                    {
        //                        //Delete any orders on the table with other orderguids (fix for issue created in Blackpool)
        //                        var tblOrders = _context.tblOrders.Where(p => p.TableID == to.tableDetails.TableID
        //                           && DbFunctions.TruncateTime(p.DateCreated) == DbFunctions.TruncateTime(DateTime.Now)
        //                           && p.DelInd == false && p.Paid == false && p.OrderGUID != to.OrderGUID).ToList();
        //                        if (tblOrders != null && tblOrders.Count > 0)
        //                        {
        //                            tblOrders.ForEach(a => a.DelInd = true);
        //                            _context.SaveChanges();
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    var tblOrders = _context.tblOrders.Where(p => p.TableID == to.tableDetails.TableID
        //                    && DbFunctions.TruncateTime(p.DateCreated) == DbFunctions.TruncateTime(DateTime.Now)
        //                    && p.DelInd == false && p.Paid == false).FirstOrDefault();
        //                    if (tblOrders != null)
        //                    {
        //                        existingOrder = true;
        //                        os.OrderGUID = tblOrders.OrderGUID;
        //                    }
        //                }
        //                if (!errorFound)
        //                {
        //                    tblOrder currentOrder = new tblOrder();
        //                    //Insert new order
        //                    if (!existingOrder)
        //                    {

        //                        ObjectParameter myOutputParamGuid = new ObjectParameter("OrderID", typeof(Guid));
        //                        int custID = 0;
        //                        if (to.CustomerId > 0)
        //                            custID = to.CustomerId;
        //                        string tm = DateTime.Now.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture);
        //                        var v = _context.usp_AN_InsertOrder_V1(to.tableDetails.TableID, 99, to.tableDetails.TableNumber, tm, to.tableUser.UserID, custID, custCount, adCount, kdCount, jnCount, to.tableCustomer.PrevCust, to.ReservedCustomer ? "T" : "F", to.tableCustomer.ReservationID, myOutputParamGuid);
        //                        os.OrderGUID = new Guid(myOutputParamGuid.Value.ToString());
        //                        currentOrder.DateCreated = DateTime.Now;
        //                        //new order if cust count > 0, create unique code
        //                        if (custCount > 0)
        //                        {
        //                            tblTableOrder tord = new tblTableOrder();
        //                            if (!_context.tblTableOrders.Any(x => x.OrderGUID == os.OrderGUID && x.TableId == to.tableDetails.TableID && x.Active == true))
        //                            {
        //                                os.UniqueCode = rh.GenerateUniqueCode();
        //                                tord.Active = true;
        //                                tord.CustomerCount = custCount;
        //                                tord.DateCreated = DateTime.Now;
        //                                tord.OrderGUID = os.OrderGUID;
        //                                tord.TableId = to.tableDetails.TableID;
        //                                tord.UniqueCode = os.UniqueCode;
        //                                _context.tblTableOrders.Add(tord);
        //                                _context.SaveChanges();
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        //update customer id & counts in tblOrder if ordered by customer
        //                        currentOrder = _context.tblOrders.Where(p => p.OrderGUID == os.OrderGUID).FirstOrDefault();
        //                        if (to.CustomerId > 0 && to.UserType != UserType.Staff.ToString() && (currentOrder.CustomerId == null || (currentOrder.CustomerId != null && currentOrder.CustomerId == 0)))
        //                            currentOrder.CustomerId = to.CustomerId;
        //                        if (custCount > 0)
        //                        {
        //                            currentOrder.AdCount += adCount;
        //                            currentOrder.KdCount += kdCount;
        //                            currentOrder.JnCount += jnCount;
        //                            custCount = (int)(currentOrder.AdCount + currentOrder.KdCount + currentOrder.JnCount);
        //                            currentOrder.CustCount = custCount;

        //                        }
        //                        _context.Entry(currentOrder).State = EntityState.Modified;
        //                        _context.SaveChanges();

        //                        //create unique code if custCount > 0 and code not created earlier for existing order 
        //                        tblTableOrder tord = new tblTableOrder();
        //                        tord = _context.tblTableOrders.Where(x => x.OrderGUID == os.OrderGUID).FirstOrDefault();
        //                        if (custCount > 0 && (tord == null || (tord != null && tord.Id == 0)))
        //                        {
        //                            tord = new tblTableOrder();
        //                            os.UniqueCode = rh.GenerateUniqueCode();
        //                            tord.Active = true;
        //                            tord.CustomerCount = custCount;
        //                            tord.DateCreated = DateTime.Now;
        //                            tord.OrderGUID = os.OrderGUID;
        //                            tord.TableId = to.tableDetails.TableID;
        //                            tord.UniqueCode = os.UniqueCode;
        //                            _context.tblTableOrders.Add(tord);
        //                            _context.SaveChanges();
        //                        }
        //                        else if (custCount > 0 && tord.CustomerCount != custCount)
        //                        {
        //                            tord.CustomerCount = custCount;
        //                            _context.Entry(tord).State = EntityState.Modified;
        //                            _context.SaveChanges();
        //                        }

        //                    }

        //                    //Update table status
        //                    var currentTable = _context.tblTables.Where(x => x.TableID == to.tableDetails.TableID).FirstOrDefault();
        //                    if (currentTable.CurrentStatus != (int)TableState.DrinksOrdered)
        //                    {
        //                        if (currentTable.CurrentStatus != (int)TableState.WaiterService)
        //                            currentTable.CurrentStatus = (int)TableState.Occupied;
        //                        else
        //                            currentTable.PastStatus = (int)TableState.Occupied;
        //                        _context.Entry(currentTable).State = EntityState.Modified;
        //                        _context.SaveChanges();
        //                    }

        //                    //Update join tables
        //                    if (to.joinTables.Count > 0)
        //                    {
        //                        foreach (var item in to.joinTables)
        //                        {
        //                            _context.usp_AN_UpdateJoinedTables(os.OrderGUID, item.TableID);
        //                        }
        //                    }


        //                    //Additional checks to assure drinks items are not added to buffet items and vice versa
        //                    if (to.BuffetItems.Count > 0)
        //                    {
        //                        var pIds = to.BuffetItems.Select(x => x.ProductId).Distinct().ToList();
        //                        var drItems = _context.tblProducts.Where(x => pIds.Contains(x.ProductID) && x.FoodRefil == false && x.Price > 0).ToList();
        //                        if (drItems == null || (drItems != null && drItems.Count == 0))
        //                            buffetItems = to.BuffetItems;
        //                        else
        //                        {
        //                            var drIds = drItems.Select(x => x.ProductID).ToList();
        //                            //buffetItems = to.BuffetItems.Where(x => !drIds.Contains(x.ProductId)).ToList();
        //                            var nonBuffetItems = to.BuffetItems.Where(x => drIds.Contains(x.ProductId)).ToList();
        //                            foreach (var item in nonBuffetItems)
        //                            {
        //                                Product pr = new Product();
        //                                pr.ProductID = item.ProductId;
        //                                pr.Price = (float)drItems.Where(x => x.ProductID == item.ProductId).Select(x => x.Price).FirstOrDefault();
        //                                pr.ProductQty = item.Qty;
        //                                to.tableProducts.Add(pr);
        //                            }
        //                        }
        //                    }
        //                    if (to.tableProducts.Count > 0)
        //                    {

        //                        var pIds = to.tableProducts.Select(x => x.ProductID).Distinct().ToList();
        //                        var foodItems = _context.tblProducts.Where(x => pIds.Contains(x.ProductID) && x.FoodRefil == true).ToList();
        //                        if (foodItems == null || (foodItems != null && foodItems.Count == 0))
        //                            orderedItems = to.tableProducts;
        //                        else
        //                        {
        //                            var fdIds = foodItems.Select(x => x.ProductID).ToList();
        //                            //orderedItems = to.tableProducts.Where(x => !fdIds.Contains(x.ProductID)).ToList();
        //                            var nondrinksItems = to.tableProducts.Where(x => fdIds.Contains(x.ProductID)).ToList();
        //                            foreach (var item in nondrinksItems)
        //                            {
        //                                OrderBuffetItem pr = new OrderBuffetItem();
        //                                pr.ProductId = item.ProductID;
        //                                //pr.Price = (float)drItems.Where(x => x.ProductID == item.ProductId).Select(x => x.Price).FirstOrDefault();
        //                                pr.Qty = item.ProductQty;
        //                                to.BuffetItems.Add(pr);
        //                            }
        //                        }
        //                    }

        //                    //Insert Order Parts

        //                    if (to.tableProducts.Count > 0)
        //                    {
        //                        batchNo = _context.tblOrderParts.Where(x => DbFunctions.TruncateTime(x.DateCreated) == DbFunctions.TruncateTime(DateTime.Now)).OrderByDescending(x => x.BatchNo).Select(x=>x.BatchNo).FirstOrDefault();
        //                        batchNo ++;
        //                        foreach (var item in to.tableProducts)
        //                        {
        //                            for (int i = 0; i < item.ProductQty; i++)
        //                            {
        //                                tblOrderPart top = new tblOrderPart();
        //                                top.OrderGUID = os.OrderGUID;
        //                                top.ProductID = item.ProductID;
        //                                top.Qty = 1;
        //                                top.Price = (decimal?)item.Price;
        //                                top.Total = (decimal?)item.Price;
        //                                top.UserID = to.tableUser.UserID;
        //                                top.DateCreated = DateTime.Now;
        //                                if ((item.EnglishName != null && item.EnglishName.Contains("Buffet")) || item.Description.Contains("Buffet"))
        //                                    top.DateServed = DateTime.Now;
        //                                top.LastModified = DateTime.Now;
        //                                top.DelInd = false;
        //                                top.OrderNo = to.LastOrderNo;
        //                                top.WebUpload = false;
        //                                top.BatchNo = batchNo;
        //                                top.UserName = to.UserName;
        //                                if (to.UserType == UserType.Customer.ToString())
        //                                    top.Mobile = to.MobileNumber.ToString();
        //                                _context.tblOrderParts.Add(top);
        //                                _context.SaveChanges();
        //                                if (item.OptionID > 0)
        //                                {
        //                                    tblOrderPartOption torp = new tblOrderPartOption();
        //                                    torp.OrderPartId = top.OrderPartID;
        //                                    torp.ProductOptionID = item.OptionID;
        //                                    torp.OrderGUID = os.OrderGUID;
        //                                    torp.LastModified = DateTime.Now;
        //                                    //torp.UserID = to.tableUser.UserID;
        //                                    //torp.Price = (decimal) item.Price;
        //                                    torp.DelInd = false;
        //                                    _context.tblOrderPartOptions.Add(torp);
        //                                    _context.SaveChanges();
        //                                }
        //                            }

        //                            //insert any redemption products
        //                            if (item.IsRedemptionProduct == true)
        //                            {
        //                                for (int i = 0; i < item.ProductQty; i++)
        //                                {
        //                                    totalRedeemedPoints += item.RedemptionPoints;
        //                                    tblRedeemedProduct tr = new tblRedeemedProduct();
        //                                    tr.ProductId = item.ProductID;
        //                                    tr.OrderGUID = os.OrderGUID;
        //                                    tr.OrderType = OrderType.DineIn.ToString();
        //                                    tr.DateCreated = DateTime.Now;
        //                                    tr.DelInd = false;
        //                                    tr.Points = item.RedemptionPoints;
        //                                    tr.Qty = 1;
        //                                    tr.Price = (decimal)item.Price;
        //                                    tr.WebUpload = false;
        //                                    _context.tblRedeemedProducts.Add(tr);
        //                                }
        //                                _context.SaveChanges();
        //                            }

        //                            if (item.Options != null && item.Options != "")
        //                            {
        //                                string it = item.ProductQty + " * " + item.Description + " " + item.Options;
        //                                it = SpliceText(it, 25);
        //                                itemsStr += it + Environment.NewLine;
        //                            }
        //                            else
        //                                itemsStr += item.ProductQty + " * " + item.Description + " " + Environment.NewLine;
        //                        }

        //                        //Insert all drinks into tblDrinks 
        //                        var drinks = to.tableProducts.Where(x => !x.Description.Contains("Buffet")).ToList();
        //                        if (drinks.Count > 0)
        //                        {
        //                            //check if this is an additiona drink or first one. Any drink ordered after 10 mins is additional
        //                            bool additionalDrink = false;
        //                            bool secondRound = true;
        //                            if (currentOrder.DateCreated.AddMinutes(10) < DateTime.Now)
        //                            {
        //                                additionalDrink = true;
        //                            }
        //                            if (to.LastOrderNo == 1)
        //                            {
        //                                secondRound = false;
        //                            }
        //                            foreach (var dr in drinks)
        //                            {
        //                                int ptID = (int)_context.tblProducts.Where(x => x.ProductID == dr.ProductID).Select(x => x.ProductTypeID).FirstOrDefault();
        //                                for (int i = 0; i < dr.ProductQty; i++)
        //                                {
        //                                    tblDrinksSold tds = new tblDrinksSold();
        //                                    tds.AdditionalDrink = additionalDrink;
        //                                    tds.DateCreated = DateTime.Now;
        //                                    tds.ProductID = dr.ProductID;
        //                                    tds.Qty = 1;
        //                                    tds.SecondRound = secondRound;
        //                                    tds.TableID = to.tableDetails.TableID;
        //                                    tds.UserID = to.tableUser.UserID;
        //                                    tds.WebUpload = false;
        //                                    tds.ProductTypeID = ptID;
        //                                    _context.tblDrinksSolds.Add(tds);
        //                                }
        //                            }
        //                            if (currentTable.CurrentStatus != (int)TableState.WaiterService)
        //                                currentTable.CurrentStatus = (int)TableState.DrinksOrdered;
        //                            else
        //                                currentTable.PastStatus = (int)TableState.DrinksOrdered;
        //                            _context.Entry(currentTable).State = EntityState.Modified;
        //                            _context.SaveChanges();
        //                        }

        //                    }

        //                    //Insert Buffet Items
        //                    if (to.BuffetItems.Count > 0)
        //                    {
        //                        //update next order time for table
        //                        var taOrd = _context.tblTableOrders.Where(x => x.OrderGUID == os.OrderGUID).FirstOrDefault();
        //                        if (taOrd == null || (taOrd != null && taOrd.CustomerCount == 0))
        //                        {
        //                            os.OrderGUID = Guid.Empty;
        //                            os.message = "Cannot submit order. Please add buffet to table. Thanks";
        //                            //os.Logout = true;
        //                            errorFound = true;
        //                        }
        //                        else
        //                        {
        //                            if (taOrd.NextOrderTime == null)
        //                                taOrd.NextOrderTime = DateTime.Now;
        //                            else
        //                            {
        //                                if (!_context.tblOrderBuffetItems.Any(x => x.OrderGUID == os.OrderGUID && x.Printed == false))
        //                                    taOrd.NextOrderTime = DateTime.Now.AddMinutes(printInterval);
        //                            }

        //                            //check for inactive products
        //                            DateTime updatedTime = DateTime.Now.AddMinutes(-180);
        //                            var products = _context.tblMenuItems.Where(x => x.LastModified != null && x.LastModified >= updatedTime && (x.bOnsite == false || x.Active == false)).ToList();
        //                            foreach (var item in to.BuffetItems)
        //                            {
        //                                bool itemAvailable = true;
        //                                if (products != null & products.Count > 0)
        //                                {
        //                                    if (products.Any(x => x.ProductID == item.ProductId))
        //                                    {
        //                                        itemAvailable = false;
        //                                        unAvailableProducts += item.Description + ",";
        //                                    }
        //                                }
        //                                if (itemAvailable)
        //                                {
        //                                    for (int i = 0; i < item.Qty; i++)
        //                                    {
        //                                        tblOrderBuffetItem tbi = new tblOrderBuffetItem();
        //                                        tbi.OrderGUID = os.OrderGUID;
        //                                        tbi.TableId = to.tableDetails.TableID;
        //                                        tbi.ProductId = item.ProductId;
        //                                        tbi.Printed = false;
        //                                        tbi.DateCreated = DateTime.Now;
        //                                        tbi.Qty = 1;
        //                                        tbi.UserType = to.UserType;
        //                                        tbi.UserName = to.UserName;
        //                                        tbi.DeviceType = item.DeviceType;
        //                                        if (to.UserType == UserType.Customer.ToString())
        //                                        {
        //                                            tbi.UserType = to.UserType;
        //                                            tbi.UserId = to.MobileNumber;
        //                                        }
        //                                        else
        //                                        {
        //                                            tbi.UserType = UserType.Staff.ToString();
        //                                            tbi.UserId = to.tableUser.UserID;
        //                                        }
        //                                        _context.tblOrderBuffetItems.Add(tbi);
        //                                    }
        //                                    _context.SaveChanges();
        //                                }
        //                            }
        //                        }
        //                    }

        //                    //Update customer points

        //                    //New logic to update redeem points locally
        //                    os.CustomerPoints = to.tableCustomer.CustomerPoints;
        //                    //if (totalRedeemedPoints > 0 && to.CustomerId > 0)
        //                    //{
        //                    //    int updatedPoints = cs.UpdateCustomerPoints(to.CustomerId, totalRedeemedPoints, 0, os.OrderGUID.ToString());

        //                    //    if (updatedPoints >= 0)
        //                    //        os.CustomerPoints = updatedPoints;
        //                    //}

        //                    if (totalRedeemedPoints > 0)
        //                    {
        //                        os.CustomerPoints = os.CustomerPoints - totalRedeemedPoints;
        //                        tblCustomerActivity tca = new tblCustomerActivity();
        //                        tca.FullName = to.UserName;
        //                        tca.Mobile = to.MobileNumber.ToString();
        //                        tca.OrderGUID = os.OrderGUID;
        //                        tca.ActivityType = ActivityType.RedeemPoints.ToString();
        //                        tca.RedeemPoints = totalRedeemedPoints;
        //                        tca.RewardPoints = 0;
        //                        cs.UpdateCustomerActivity(tca);
        //                    }

        //                    //Get current total - used for points redemption
        //                    if (os.OrderGUID != null && os.OrderGUID != Guid.Empty)
        //                        os.CurrentTotal = (float)_context.tblOrderParts.Where(x => x.OrderGUID == os.OrderGUID).Sum(x => x.Price);

        //                    //Print Bar items
        //                    if (itemsStr != "")
        //                    {
        //                        string strPrint = "";
        //                        string nl = System.Environment.NewLine;
        //                        strPrint = "----------------------------" + nl;
        //                        strPrint += "        " + DateTime.Now.ToString("dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture) + nl;
        //                        strPrint += "Ordered by " + to.tableUser.UserName + nl;
        //                        strPrint += "TABLE - " + to.tableDetails.TableNumber + nl;
        //                        strPrint += nl;
        //                        strPrint += itemsStr;
        //                        strPrint += nl;
        //                        strPrint += "----------------------------";
        //                        string printer = "Bar";
        //                        if (to.tableUser.UserPrinter != null)
        //                        {
        //                            printer = to.tableUser.UserPrinter;
        //                        }


        //                        string ticketNo = "D" + batchNo;
        //                        tblPrintQueue tpq = new tblPrintQueue();
        //                        tpq.TicketNo = ticketNo;
        //                        tpq.ToPrinter = printer;
        //                        tpq.UserFK = to.tableUser.UserID;
        //                        tpq.Receipt = strPrint;
        //                        tpq.DateCreated = DateTime.Now;
        //                        tpq.BatchNo = batchNo;
        //                        tpq.OrderGUID = os.OrderGUID;
        //                        tpq.PCName = "App";
        //                        tpq.Processed = false;
        //                        tpq.ProcessedBy = 0;
        //                        tpq.TableNumber = to.tableDetails.TableNumber;
        //                        _context.tblPrintQueues.Add(tpq);
        //                        _context.SaveChanges();
        //                        //_context.usp_AN_InsertPrintQueue(to.tableUser.UserID, "App", printer, strPrint);
        //                    }

        //                    if (!errorFound)
        //                    {
        //                        if (os.OrderGUID == Guid.Empty || os.OrderGUID == null)
        //                        {
        //                            os.message = "Table Paid/Code Expired. Please contact our staff for new code. Thanks";
        //                            os.Logout = true;
        //                        }
        //                        else
        //                        {
        //                            int approxDeliveryTime = 12;
        //                            var totalUnprintedItems = _context.tblOrderBuffetItems.Where(x => x.Printed == false && DbFunctions.TruncateTime(x.DateCreated) == DbFunctions.TruncateTime(DateTime.Now)).ToList();
        //                            if (totalUnprintedItems != null && totalUnprintedItems.Count > 0)
        //                            {
        //                                var totalUnprintedItemsCount = totalUnprintedItems.Sum(x => x.Qty);
        //                                if (totalUnprintedItemsCount <= 75)
        //                                    approxDeliveryTime = 12;
        //                                else if (totalUnprintedItemsCount <= 100)
        //                                    approxDeliveryTime = 15;
        //                                else
        //                                    approxDeliveryTime = 18;
        //                            }

        //                            os.message = "Thank you for your order. The current delivery time is approx " + approxDeliveryTime + " minutes.";
        //                            if (unAvailableProducts != "")
        //                                os.message += unAvailableProducts + " is currently unavailable.";
        //                            //if(to.BuffetItems.Count > 0 && to.BuffetItems.Any(x=>x.))
        //                        }
        //                    }

        //                }

        //                //if (to.ReservedCustomer && to.tableCustomer.ReservationID > 0)
        //                //{
        //                //    reservationUniqueCode = _context.tblReservations.Where(x => x.ReservationID > to.tableCustomer.ReservationID).Select(x => x.UniqueCode).FirstOrDefault() ?? "";
        //                //}
        //            }


        //            scope.Complete();
        //        }

        //        //If order submitted for reserved customer, auto apply the reservation code
        //        if(reservationUniqueCode != "")
        //        {

        //        }

        //    }
        //    catch (SqlException sqlex)
        //    {
        //        os.message += sqlex.Message;
        //        for (int i = 0; i < sqlex.Errors.Count; i++)
        //        {
        //            os.message +=  ("Index #" + i + "\n" +
        //                "Message: " + sqlex.Errors[i].Message + "\n" +
        //                "LineNumber: " + sqlex.Errors[i].LineNumber + "\n" +
        //                "Source: " + sqlex.Errors[i].Source + "\n" +
        //                "Procedure: " + sqlex.Errors[i].Procedure + "\n");
        //        }
        //        logger.Info("Order Submit SQL Error - " + os.message);
        //    }
        //    catch (Exception ex)
        //    {
        //        os.message = ex.Message;
        //        if (ex.InnerException != null)
        //            os.message = ex.InnerException.StackTrace;
        //        logger.Info("Order Submit Error - " + os.message);
        //    }
        //    return os;
        //}

        public OrderSubmitResponse SubmitOrderV3(TableOrder to)
        {
            logger.Info("Order Submission Started - " + to.tableUser.UserID);
            OrderSubmitResponse os = new OrderSubmitResponse();

            int totalRedeemedPoints = 0;
            string unAvailableProducts = "";
            bool errorFound = false;
            Guid emptyGuid = Guid.Empty;
            bool existingOrder = false;
            int custCount = 0;
            int adCount = 0;
            int kdCount = 0;
            int jnCount = 0;
            int batchNo = 0;
            int coverBuffet = 0;
            string itemsStr = "";
            string reservationUniqueCode = "";
            List<OrderBuffetItem> buffetItems = new List<OrderBuffetItem>();
            List<Product> orderedItems = new List<Product>();

            //Calculate custCounts from items ordered
            if (to.tableProducts.Count > 0)
            {
                adCount = to.tableProducts.Where(x => (x.EnglishName != null && x.EnglishName.Contains("Buffet Adult")) || (x.Description.Contains("Buffet Adult"))).Sum(x => x.ProductQty);
                kdCount = to.tableProducts.Where(x => (x.EnglishName != null && x.EnglishName.Contains("Buffet Kids")) || (x.Description.Contains("Buffet Kids"))).Sum(x => x.ProductQty);
                jnCount = to.tableProducts.Where(x => (x.EnglishName != null && x.EnglishName.Contains("Buffet Junior")) || (x.Description.Contains("Buffet Junior"))).Sum(x => x.ProductQty);
                coverBuffet = to.tableProducts.Where(x => (x.EnglishName != null && x.EnglishName.Contains("Buffet cover")) || (x.Description.Contains("Buffet cover"))).Sum(x => x.ProductQty);
                custCount = adCount + kdCount + jnCount + coverBuffet;
            }
            if (to.tableDetails.TableID == 0)
            {
                os.OrderGUID = Guid.Empty;
                os.message = "We could not submit the order. Please try again. Thanks";
                os.Logout = true;
                return os;
            }

            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    using (var _context = new ChineseTillEntities1())
                    {
                        if (to.OrderGUID != null && to.OrderGUID != emptyGuid)
                        {
                            os.OrderGUID = to.OrderGUID;
                            existingOrder = true;
                            //If bill printed, do not submit order
                            if (_context.tblOrders.Any(x => x.OrderGUID == os.OrderGUID && x.BillPrinted == true && x.OrderGUID != emptyGuid) && to.UserType != UserType.Staff.ToString())
                            {
                                os.OrderGUID = Guid.Empty;
                                os.message = "Your order is not sent through as the bill is printed for this table. Please ask for waiter service.Thanks";
                                os.Logout = false;
                                errorFound = true;
                            }
                            else if (_context.tblOrders.Any(x => x.OrderGUID == os.OrderGUID && x.OrderGUID != emptyGuid && ((x.Paid == true && x.DelInd == false) || x.DelInd == true)) ||
                                (_context.tblTableOrders.Any(x => x.OrderGUID == os.OrderGUID && x.Active == false && x.OrderGUID != emptyGuid)))
                            {
                                os.OrderGUID = Guid.Empty;
                                os.message = "Table Paid/Code Expired. Please contact our staff for new code. Thanks";
                                os.Logout = true;
                                errorFound = true;
                            }

                            if (!errorFound)
                            {
                                //Delete any orders on the table with other orderguids (fix for issue created in Blackpool)
                                var tblOrders = _context.tblOrders.Where(p => p.TableID == to.tableDetails.TableID
                                   && DbFunctions.TruncateTime(p.DateCreated) == DbFunctions.TruncateTime(DateTime.Now)
                                   && p.DelInd == false && p.Paid == false && p.OrderGUID != to.OrderGUID).ToList();
                                if (tblOrders != null && tblOrders.Count > 0)
                                {
                                    tblOrders.ForEach(a => a.DelInd = true);
                                    _context.SaveChanges();
                                }
                            }
                        }
                        else
                        {
                            var tblOrders = _context.tblOrders.Where(p => p.TableID == to.tableDetails.TableID
                            && DbFunctions.TruncateTime(p.DateCreated) == DbFunctions.TruncateTime(DateTime.Now)
                            && p.DelInd == false && p.Paid == false).FirstOrDefault();
                            if (tblOrders != null)
                            {
                                existingOrder = true;
                                os.OrderGUID = tblOrders.OrderGUID;
                            }
                        }
                        if (!errorFound)
                        {
                            tblOrder currentOrder = new tblOrder();
                            //Insert new order
                            if (!existingOrder)
                            {

                                ObjectParameter myOutputParamGuid = new ObjectParameter("OrderID", typeof(Guid));
                                int custID = 0;
                                if (to.CustomerId > 0)
                                    custID = to.CustomerId;
                                string tm = DateTime.Now.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                                var v = _context.usp_AN_InsertOrder_V1(to.tableDetails.TableID, 99, to.tableDetails.TableNumber, tm, to.tableUser.UserID, custID, custCount, adCount, kdCount, jnCount, to.tableCustomer.PrevCust, to.ReservedCustomer ? "T" : "F", to.tableCustomer.ReservationID, myOutputParamGuid);
                                os.OrderGUID = new Guid(myOutputParamGuid.Value.ToString());
                                currentOrder.DateCreated = DateTime.Now;
                                //new order if cust count > 0, create unique code
                                if (custCount > 0)
                                {
                                    tblTableOrder tord = new tblTableOrder();
                                    if (!_context.tblTableOrders.Any(x => x.OrderGUID == os.OrderGUID && x.TableId == to.tableDetails.TableID && x.Active == true))
                                    {
                                        os.UniqueCode = rh.GenerateUniqueCode();
                                        tord.Active = true;
                                        tord.CustomerCount = custCount;
                                        tord.DateCreated = DateTime.Now;
                                        tord.OrderGUID = os.OrderGUID;
                                        tord.TableId = to.tableDetails.TableID;
                                        tord.UniqueCode = os.UniqueCode;
                                        _context.tblTableOrders.Add(tord);
                                        _context.SaveChanges();
                                    }
                                }
                            }
                            else
                            {
                                //update customer id & counts in tblOrder if ordered by customer
                                currentOrder = _context.tblOrders.Where(p => p.OrderGUID == os.OrderGUID).FirstOrDefault();
                                if (to.CustomerId > 0 && to.UserType != UserType.Staff.ToString() && (currentOrder.CustomerId == null || (currentOrder.CustomerId != null && currentOrder.CustomerId == 0)))
                                    currentOrder.CustomerId = to.CustomerId;
                                if (custCount > 0)
                                {
                                    currentOrder.AdCount += adCount;
                                    currentOrder.KdCount += kdCount;
                                    currentOrder.JnCount += jnCount;
                                    custCount = (int)(currentOrder.AdCount + currentOrder.KdCount + currentOrder.JnCount);
                                    currentOrder.CustCount = custCount;

                                }
                                _context.Entry(currentOrder).State = EntityState.Modified;
                                _context.SaveChanges();

                                //create unique code if custCount > 0 and code not created earlier for existing order 
                                tblTableOrder tord = new tblTableOrder();
                                tord = _context.tblTableOrders.Where(x => x.OrderGUID == os.OrderGUID).FirstOrDefault();
                                if (custCount > 0 && (tord == null || (tord != null && tord.Id == 0)))
                                {
                                    tord = new tblTableOrder();
                                    os.UniqueCode = rh.GenerateUniqueCode();
                                    tord.Active = true;
                                    tord.CustomerCount = custCount;
                                    tord.DateCreated = DateTime.Now;
                                    tord.OrderGUID = os.OrderGUID;
                                    tord.TableId = to.tableDetails.TableID;
                                    tord.UniqueCode = os.UniqueCode;
                                    _context.tblTableOrders.Add(tord);
                                    _context.SaveChanges();
                                }
                                else if (custCount > 0 && tord.CustomerCount != custCount)
                                {
                                    tord.CustomerCount = custCount;
                                    _context.Entry(tord).State = EntityState.Modified;
                                    _context.SaveChanges();
                                }

                            }

                            //Update table status
                            var currentTable = _context.tblTables.Where(x => x.TableID == to.tableDetails.TableID).FirstOrDefault();
                            if (currentTable.CurrentStatus != (int)TableState.DrinksOrdered)
                            {
                                if (currentTable.CurrentStatus != (int)TableState.WaiterService)
                                    currentTable.CurrentStatus = (int)TableState.Occupied;
                                else
                                    currentTable.PastStatus = (int)TableState.Occupied;
                                _context.Entry(currentTable).State = EntityState.Modified;
                                _context.SaveChanges();
                            }

                            //Update join tables
                            if (to.joinTables.Count > 0)
                            {
                                foreach (var item in to.joinTables)
                                {
                                    _context.usp_AN_UpdateJoinedTables(os.OrderGUID, item.TableID);
                                }
                            }


                            //Additional checks to assure drinks items are not added to buffet items and vice versa
                            if (to.BuffetItems.Count > 0)
                            {
                                var pIds = to.BuffetItems.Select(x => x.ProductId).Distinct().ToList();
                                var drItems = _context.tblProducts.Where(x => pIds.Contains(x.ProductID) && x.FoodRefil == false && x.Price > 0).ToList();
                                if (drItems == null || (drItems != null && drItems.Count == 0))
                                    buffetItems = to.BuffetItems;
                                else
                                {
                                    var drIds = drItems.Select(x => x.ProductID).ToList();
                                    //buffetItems = to.BuffetItems.Where(x => !drIds.Contains(x.ProductId)).ToList();
                                    var nonBuffetItems = to.BuffetItems.Where(x => drIds.Contains(x.ProductId)).ToList();
                                    foreach (var item in nonBuffetItems)
                                    {
                                        Product pr = new Product();
                                        pr.ProductID = item.ProductId;
                                        pr.Price = (float)drItems.Where(x => x.ProductID == item.ProductId).Select(x => x.Price).FirstOrDefault();
                                        pr.ProductQty = item.Qty;
                                        to.tableProducts.Add(pr);
                                    }
                                }
                            }
                            if (to.tableProducts.Count > 0)
                            {

                                var pIds = to.tableProducts.Select(x => x.ProductID).Distinct().ToList();
                                var foodItems = _context.tblProducts.Where(x => pIds.Contains(x.ProductID) && x.FoodRefil == true).ToList();
                                if (foodItems == null || (foodItems != null && foodItems.Count == 0))
                                    orderedItems = to.tableProducts;
                                else
                                {
                                    var fdIds = foodItems.Select(x => x.ProductID).ToList();
                                    //orderedItems = to.tableProducts.Where(x => !fdIds.Contains(x.ProductID)).ToList();
                                    var nondrinksItems = to.tableProducts.Where(x => fdIds.Contains(x.ProductID)).ToList();
                                    foreach (var item in nondrinksItems)
                                    {
                                        OrderBuffetItem pr = new OrderBuffetItem();
                                        pr.ProductId = item.ProductID;
                                        //pr.Price = (float)drItems.Where(x => x.ProductID == item.ProductId).Select(x => x.Price).FirstOrDefault();
                                        pr.Qty = item.ProductQty;
                                        to.BuffetItems.Add(pr);
                                    }
                                }
                            }

                            //Insert Order Parts

                            if (to.tableProducts.Count > 0)
                            {
                                batchNo = _context.tblOrderParts.Where(x => DbFunctions.TruncateTime(x.DateCreated) == DbFunctions.TruncateTime(DateTime.Now)).OrderByDescending(x => x.BatchNo).Select(x => x.BatchNo).FirstOrDefault();
                                batchNo++;
                                foreach (var item in to.tableProducts)
                                {
                                    for (int i = 0; i < item.ProductQty; i++)
                                    {
                                        tblOrderPart top = new tblOrderPart();
                                        top.OrderGUID = os.OrderGUID;
                                        top.ProductID = item.ProductID;
                                        top.Qty = 1;
                                        top.Price = (decimal?)item.Price;
                                        top.Total = (decimal?)item.Price;
                                        top.UserID = to.tableUser.UserID;
                                        top.DateCreated = DateTime.Now;
                                        if ((item.EnglishName != null && item.EnglishName.Contains("Buffet")) || item.Description.Contains("Buffet"))
                                            top.DateServed = DateTime.Now;
                                        top.LastModified = DateTime.Now;
                                        top.DelInd = false;
                                        top.OrderNo = to.LastOrderNo;
                                        top.WebUpload = false;
                                        top.BatchNo = batchNo;
                                        top.UserName = to.UserName;
                                        if (to.UserType == UserType.Customer.ToString())
                                            top.Mobile = to.MobileNumber.ToString();
                                        _context.tblOrderParts.Add(top);
                                        _context.SaveChanges();
                                        if (item.OptionID > 0)
                                        {
                                            tblOrderPartOption torp = new tblOrderPartOption();
                                            torp.OrderPartId = top.OrderPartID;
                                            torp.ProductOptionID = item.OptionID;
                                            torp.OrderGUID = os.OrderGUID;
                                            torp.LastModified = DateTime.Now;
                                            //torp.UserID = to.tableUser.UserID;
                                            //torp.Price = (decimal) item.Price;
                                            torp.DelInd = false;
                                            _context.tblOrderPartOptions.Add(torp);
                                            _context.SaveChanges();
                                        }
                                    }

                                    //insert any redemption products
                                    if (item.IsRedemptionProduct == true)
                                    {
                                        for (int i = 0; i < item.ProductQty; i++)
                                        {
                                            totalRedeemedPoints += item.RedemptionPoints;
                                            tblRedeemedProduct tr = new tblRedeemedProduct();
                                            tr.ProductId = item.ProductID;
                                            tr.OrderGUID = os.OrderGUID;
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

                                    if (item.Options != null && item.Options != "")
                                    {
                                        string it = item.ProductQty + " * " + item.Description + " " + item.Options;
                                        it = SpliceText(it, 25);
                                        itemsStr += it + Environment.NewLine;
                                    }
                                    else
                                        itemsStr += item.ProductQty + " * " + item.Description + " " + Environment.NewLine;
                                }

                                //Insert all drinks into tblDrinks 
                                var drinks = to.tableProducts.Where(x => !x.Description.Contains("Buffet") && !x.Description.Contains("Meal")).ToList();
                                if (drinks.Count > 0)
                                {
                                    //check if this is an additiona drink or first one. Any drink ordered after 10 mins is additional
                                    bool additionalDrink = false;
                                    bool secondRound = true;
                                    if (currentOrder.DateCreated.AddMinutes(10) < DateTime.Now)
                                    {
                                        additionalDrink = true;
                                    }
                                    if (to.LastOrderNo == 1)
                                    {
                                        secondRound = false;
                                    }
                                    foreach (var dr in drinks)
                                    {
                                        int ptID = (int)_context.tblProducts.Where(x => x.ProductID == dr.ProductID).Select(x => x.ProductTypeID).FirstOrDefault();
                                        for (int i = 0; i < dr.ProductQty; i++)
                                        {
                                            tblDrinksSold tds = new tblDrinksSold();
                                            tds.AdditionalDrink = additionalDrink;
                                            tds.DateCreated = DateTime.Now;
                                            tds.ProductID = dr.ProductID;
                                            tds.Qty = 1;
                                            tds.SecondRound = secondRound;
                                            tds.TableID = to.tableDetails.TableID;
                                            tds.UserID = to.tableUser.UserID;
                                            tds.WebUpload = false;
                                            tds.ProductTypeID = ptID;
                                            _context.tblDrinksSolds.Add(tds);
                                        }
                                    }
                                    if (currentTable.CurrentStatus != (int)TableState.WaiterService)
                                        currentTable.CurrentStatus = (int)TableState.DrinksOrdered;
                                    else
                                        currentTable.PastStatus = (int)TableState.DrinksOrdered;
                                    _context.Entry(currentTable).State = EntityState.Modified;
                                    _context.SaveChanges();
                                }

                            }

                            //Insert Buffet Items
                            if (to.BuffetItems.Count > 0)
                            {
                                //update next order time for table
                                var taOrd = _context.tblTableOrders.Where(x => x.OrderGUID == os.OrderGUID).FirstOrDefault();
                                if (taOrd == null || (taOrd != null && taOrd.CustomerCount == 0))
                                {
                                    os.OrderGUID = Guid.Empty;
                                    os.message = "Cannot submit order. Please add buffet to table. Thanks";
                                    //os.Logout = true;
                                    errorFound = true;
                                }
                                else
                                {
                                    if (taOrd.NextOrderTime == null)
                                        taOrd.NextOrderTime = DateTime.Now;
                                    else
                                    {
                                        if (!_context.tblOrderBuffetItems.Any(x => x.OrderGUID == os.OrderGUID && x.Printed == false))
                                            taOrd.NextOrderTime = DateTime.Now.AddMinutes(printInterval);
                                    }

                                    //check for inactive products
                                    DateTime updatedTime = DateTime.Now.AddMinutes(-180);
                                    var products = _context.tblMenuItems.Where(x => x.LastModified != null && x.LastModified >= updatedTime && x.Active == false).ToList();
                                    foreach (var item in to.BuffetItems)
                                    {
                                        bool itemAvailable = true;
                                        if (products != null & products.Count > 0)
                                        {
                                            if (products.Any(x => x.ProductID == item.ProductId))
                                            {
                                                itemAvailable = false;
                                                unAvailableProducts += item.Description + ",";
                                            }
                                        }
                                        if (itemAvailable)
                                        {
                                            for (int i = 0; i < item.Qty; i++)
                                            {
                                                tblOrderBuffetItem tbi = new tblOrderBuffetItem();
                                                tbi.OrderGUID = os.OrderGUID;
                                                tbi.TableId = to.tableDetails.TableID;
                                                tbi.ProductId = item.ProductId;
                                                tbi.Printed = false;
                                                tbi.DateCreated = DateTime.Now;
                                                tbi.Qty = 1;
                                                tbi.UserType = to.UserType;
                                                tbi.UserName = to.UserName;
                                                tbi.DeviceType = item.DeviceType;
                                                if (to.UserType == UserType.Customer.ToString())
                                                {
                                                    tbi.UserType = to.UserType;
                                                    tbi.UserId = to.MobileNumber;
                                                }
                                                else
                                                {
                                                    tbi.UserType = UserType.Staff.ToString();
                                                    tbi.UserId = to.tableUser.UserID;
                                                }
                                                _context.tblOrderBuffetItems.Add(tbi);
                                            }
                                            _context.SaveChanges();
                                        }
                                    }
                                }
                            }

                            //Update customer points

                            //New logic to update redeem points locally
                            os.CustomerPoints = to.tableCustomer.CustomerPoints;
                            //if (totalRedeemedPoints > 0 && to.CustomerId > 0)
                            //{
                            //    int updatedPoints = cs.UpdateCustomerPoints(to.CustomerId, totalRedeemedPoints, 0, os.OrderGUID.ToString());

                            //    if (updatedPoints >= 0)
                            //        os.CustomerPoints = updatedPoints;
                            //}

                            if (totalRedeemedPoints > 0)
                            {
                                os.CustomerPoints = os.CustomerPoints - totalRedeemedPoints;
                                tblCustomerActivity tca = new tblCustomerActivity();
                                tca.FullName = to.UserName;
                                tca.Mobile = to.MobileNumber.ToString();
                                tca.OrderGUID = os.OrderGUID;
                                tca.ActivityType = ActivityType.RedeemPoints.ToString();
                                tca.RedeemPoints = totalRedeemedPoints;
                                tca.RewardPoints = 0;
                                cs.UpdateCustomerActivity(tca);
                            }

                            //Get current total - used for points redemption
                            if (os.OrderGUID != null && os.OrderGUID != Guid.Empty)
                                os.CurrentTotal = (float)_context.tblOrderParts.Where(x => x.OrderGUID == os.OrderGUID).Sum(x => x.Price);

                            //Print Bar items
                            if (itemsStr != "")
                            {
                                string strPrint = "";
                                string nl = System.Environment.NewLine;
                                strPrint = "----------------------------" + nl;
                                strPrint += "        " + DateTime.Now.ToString("dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture) + nl;
                                strPrint += "Ordered by " + to.tableUser.UserName + nl;
                                strPrint += "TABLE - " + to.tableDetails.TableNumber + nl;
                                strPrint += nl;
                                strPrint += itemsStr;
                                strPrint += nl;
                                strPrint += "----------------------------";
                                string printer = "Bar";
                                if (to.tableUser.UserPrinter != null)
                                {
                                    printer = to.tableUser.UserPrinter;
                                }


                                string ticketNo = "D" + batchNo;
                                tblPrintQueue tpq = new tblPrintQueue();
                                tpq.TicketNo = ticketNo;
                                tpq.ToPrinter = printer;
                                tpq.UserFK = to.tableUser.UserID;
                                tpq.Receipt = strPrint;
                                tpq.DateCreated = DateTime.Now;
                                tpq.BatchNo = batchNo;
                                tpq.OrderGUID = os.OrderGUID;
                                tpq.PCName = "App";
                                tpq.Processed = false;
                                tpq.ProcessedBy = 0;
                                tpq.TableNumber = to.tableDetails.TableNumber;
                                _context.tblPrintQueues.Add(tpq);
                                _context.SaveChanges();
                                //_context.usp_AN_InsertPrintQueue(to.tableUser.UserID, "App", printer, strPrint);
                            }

                            if (!errorFound)
                            {
                                if (os.OrderGUID == Guid.Empty || os.OrderGUID == null)
                                {
                                    os.message = "Table Paid/Code Expired. Please contact our staff for new code. Thanks";
                                    os.Logout = true;
                                }
                                else
                                {
                                    int approxDeliveryTime = 12;
                                    var totalUnprintedItems = _context.tblOrderBuffetItems.Where(x => x.Printed == false && DbFunctions.TruncateTime(x.DateCreated) == DbFunctions.TruncateTime(DateTime.Now)).ToList();
                                    if (totalUnprintedItems != null && totalUnprintedItems.Count > 0)
                                    {
                                        var totalUnprintedItemsCount = totalUnprintedItems.Sum(x => x.Qty);
                                        if (totalUnprintedItemsCount <= 75)
                                            approxDeliveryTime = 12;
                                        else if (totalUnprintedItemsCount <= 100)
                                            approxDeliveryTime = 15;
                                        else
                                            approxDeliveryTime = 18;
                                    }

                                    //os.message = "Thank you for your order. The current delivery time is approx " + approxDeliveryTime + " minutes.";
                                    os.message = "Thank you for your order. The current delivery time is approx " + approxDeliveryTime + " minutes. Don't forget to order your free drink by redeeming points. Checkout Rewards page for more details.";

                                    if (unAvailableProducts != "")
                                        os.message += unAvailableProducts + " is currently unavailable.";
                                    //if(to.BuffetItems.Count > 0 && to.BuffetItems.Any(x=>x.))
                                }
                            }

                        }

                        //if (to.ReservedCustomer && to.tableCustomer.ReservationID > 0)
                        //{
                        //    reservationUniqueCode = _context.tblReservations.Where(x => x.ReservationID > to.tableCustomer.ReservationID).Select(x => x.UniqueCode).FirstOrDefault() ?? "";
                        //}
                    }


                    scope.Complete();
                }

                //If order submitted for reserved customer, auto apply the reservation code
                if (reservationUniqueCode != "")
                {

                }

            }
            catch (SqlException sqlex)
            {
                os.message += sqlex.Message;
                for (int i = 0; i < sqlex.Errors.Count; i++)
                {
                    os.message += ("Index #" + i + "\n" +
                        "Message: " + sqlex.Errors[i].Message + "\n" +
                        "LineNumber: " + sqlex.Errors[i].LineNumber + "\n" +
                        "Source: " + sqlex.Errors[i].Source + "\n" +
                        "Procedure: " + sqlex.Errors[i].Procedure + "\n");
                }
                logger.Info("Order Submit SQL Error - " + os.message);
            }
            catch (Exception ex)
            {
                os.message = ex.Message;
                if (ex.InnerException != null)
                    os.message = ex.InnerException.StackTrace;
                logger.Info("Order Submit Error - " + os.message);
            }
            return os;
        }

        public OrderSubmitResponse SubmitOrderV4(TableOrder to)
        {
            logger.Info("Order Submission Started - " + to.tableUser.UserID);
            OrderSubmitResponse os = new OrderSubmitResponse();
            string errMessage = "";
            int totalRedeemedPoints = 0;
            string unAvailableProducts = "";
            bool errorFound = false;
            Guid emptyGuid = Guid.Empty;
            bool existingOrder = false;
            int custCount = 0;
            int adCount = 0;
            int kdCount = 0;
            int jnCount = 0;
            int batchNo = 0;
            int coverBuffet = 0;
            string itemsStr = "";
            string foodItemsStr = "";

            string reservationUniqueCode = "";
            List<OrderBuffetItem> buffetItems = new List<OrderBuffetItem>();
            List<Product> orderedItems = new List<Product>();

            //Calculate custCounts from items ordered
            if (to.tableProducts.Count > 0)
            {
                adCount = to.tableProducts.Where(x => (x.EnglishName != null && x.EnglishName.Contains("Buffet Adult")) || (x.Description.Contains("Buffet Adult"))).Sum(x => x.ProductQty);
                kdCount = to.tableProducts.Where(x => (x.EnglishName != null && x.EnglishName.Contains("Buffet Kids")) || (x.Description.Contains("Buffet Kids"))).Sum(x => x.ProductQty);
                jnCount = to.tableProducts.Where(x => (x.EnglishName != null && x.EnglishName.Contains("Buffet Junior")) || (x.Description.Contains("Buffet Junior"))).Sum(x => x.ProductQty);
                coverBuffet = to.tableProducts.Where(x => (x.EnglishName != null && x.EnglishName.Contains("Buffet cover")) || (x.Description.Contains("Buffet cover"))).Sum(x => x.ProductQty);
                custCount = adCount + kdCount + jnCount + coverBuffet;
            }
            if (to.tableDetails.TableID == 0)
            {
                os.OrderGUID = Guid.Empty;
                //os.message = "We could not submit the order. Please try again. Thanks";
                os.message = "Connection error. Only submit again once you have checked your order history to avoid duplicate orders.";
                os.Logout = true;
                return os;
            }

           
            
            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    using (var _context = new ChineseTillEntities1())
                    {
                        if (to.OrderGUID != null && to.OrderGUID != emptyGuid)
                        {
                            os.OrderGUID = to.OrderGUID;
                            existingOrder = true;

                            //05 Mar 2024 Changes by Gaurav to stop accepting order as per below criteria
                            //Lunch - 1.5 hours
                            //Dinner & Sunday - 2 hours
                            var ord = _context.tblOrders.Where(x => x.OrderGUID == os.OrderGUID && x.OrderGUID != emptyGuid).FirstOrDefault();
                            var now = DateTime.Now;
                            var dinnerStartTime = new DateTime(now.Year, now.Month, now.Day, 17, 0, 0);
                            var isSunday = DateTime.Now.DayOfWeek == DayOfWeek.Sunday ? true : false;
                            //If bill printed, do not submit order
                            //if (_context.tblOrders.Any(x => x.OrderGUID == os.OrderGUID && x.BillPrinted == true && x.OrderGUID != emptyGuid) && to.UserType != UserType.Staff.ToString())
                            //{
                            //    os.OrderGUID = Guid.Empty;
                            //    os.message = "Your order is not sent through as the bill is printed for this table. Please ask for waiter service.Thanks";
                            //    os.Logout = false;
                            //    errorFound = true;
                            //}
                            //else if (_context.tblOrders.Any(x => x.OrderGUID == os.OrderGUID && x.OrderGUID != emptyGuid && ((x.Paid == true && x.DelInd == false) || x.DelInd == true)) ||
                            //    (_context.tblTableOrders.Any(x => x.OrderGUID == os.OrderGUID && x.Active == false && x.OrderGUID != emptyGuid)))
                            //{
                            //    os.OrderGUID = Guid.Empty;
                            //    os.message = "Table Paid/Code Expired. Please contact our staff for new code. Thanks";
                            //    os.Logout = true;
                            //    errorFound = true;
                            //}


                           

                            if (ord != null && ord.BillPrinted == true && to.UserType != UserType.Staff.ToString())
                            {
                                os.OrderGUID = Guid.Empty;
                                os.message = "Your order is not sent through as the bill is printed for this table. Please ask for waiter service.Thanks";
                                os.Logout = false;
                                errorFound = true;
                            }
                            else if (ord != null && ((ord.Paid == true && ord.DelInd == false) || ord.DelInd == true) ||
                                (_context.tblTableOrders.Any(x => x.OrderGUID == os.OrderGUID && x.Active == false && x.OrderGUID != emptyGuid)))
                            {
                                os.OrderGUID = Guid.Empty;
                                os.message = "Table Paid/Code Expired. Please contact our staff for new code. Thanks";
                                os.Logout = true;
                                errorFound = true;
                            }
                            else if (ord != null && ord.Paid == false && ord.DelInd == false && ord.LockedForOrdering == false &&
                                       ((ord.DateCreated < dinnerStartTime && (now.Subtract(ord.DateCreated).TotalMinutes > 90) && !isSunday) ||
                                       ((ord.DateCreated > dinnerStartTime || isSunday) && (now.Subtract(ord.DateCreated).TotalMinutes > 120))))
                            {
                                os.OrderGUID = Guid.Empty;
                                os.message = "The dining time on this table has expired. Please speak to our staff if you require further assistance.";
                                os.Logout = true;
                                errorFound = true;
                            }


                            //check for duplicate orders
                            if (!errorFound)
                            {
                                //bool duplicateOrder = CheckforDuplicateOrder(os.OrderGUID, to.tableUser.UserID, to.tableProducts.Count,to.BuffetItems.Count);

                                DateTime orderTime = DateTime.Now.AddSeconds(-45);
                                bool duplicate = false;

                                if (to.tableProducts.Count > 0)
                                {
                                    var orderedPartItems = _context.tblOrderParts.Where(x => x.OrderGUID == to.OrderGUID && x.UserID == to.tableUser.UserID && x.DateCreated > orderTime).ToList();
                                    if (orderedPartItems != null && orderedPartItems.Count > 0)
                                    {
                                        if (orderedItems.Count == to.tableProducts.Count)
                                            duplicate = true;
                                    }
                                }
                                if (to.BuffetItems.Count > 0)
                                {
                                    var orderedBuffetItems = _context.tblOrderBuffetItems.Where(x => x.OrderGUID == to.OrderGUID && x.UserId == to.tableUser.UserID && x.DateCreated > orderTime).ToList();
                                    if (orderedBuffetItems != null && orderedBuffetItems.Count > 0)
                                    {
                                        if (orderedItems.Count == to.BuffetItems.Count)
                                            duplicate = true;
                                    }
                                }


                                if (duplicate)
                                {
                                    os.message = "Thank you for your order. The current delivery time is approx 12 minutes. \n\n Don't forget to order your free drink by redeeming points. \n\n Checkout Rewards page for more details.";
                                    errorFound = true;
                                    os.Logout = false;
                                }
                            }


                            if (!errorFound)
                            {
                                //Delete any orders on the table with other orderguids (fix for issue created in Blackpool)
                                var tblOrders = _context.tblOrders.Where(p => p.TableID == to.tableDetails.TableID
                                   && DbFunctions.TruncateTime(p.DateCreated) == DbFunctions.TruncateTime(DateTime.Now)
                                   && p.DelInd == false && p.Paid == false && p.OrderGUID != to.OrderGUID).ToList();
                                if (tblOrders != null && tblOrders.Count > 0)
                                {
                                    tblOrders.ForEach(a => a.DelInd = true);
                                    _context.SaveChanges();
                                }
                            }
                        }
                        else
                        {
                            var tblOrders = _context.tblOrders.Where(p => p.TableID == to.tableDetails.TableID
                            && DbFunctions.TruncateTime(p.DateCreated) == DbFunctions.TruncateTime(DateTime.Now)
                            && p.DelInd == false && p.Paid == false).FirstOrDefault();
                            if (tblOrders != null)
                            {
                                existingOrder = true;
                                os.OrderGUID = tblOrders.OrderGUID;
                            }
                        }
                        if (!errorFound)
                        {
                            tblOrder currentOrder = new tblOrder();
                            //Insert new order
                            if (!existingOrder)
                            {

                                ObjectParameter myOutputParamGuid = new ObjectParameter("OrderID", typeof(Guid));
                                int custID = 0;
                                if (to.CustomerId > 0)
                                    custID = to.CustomerId;
                                string tm = DateTime.Now.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                                string tableNumbers = to.tableDetails.TableNumber;
                                if (to.joinTables != null && to.joinTables.Count > 0)
                                {
                                    foreach (var item in to.joinTables)
                                    {
                                        tableNumbers += "," + item.TableNumber;
                                    }
                                }
                                //var v = _context.usp_AN_InsertOrder_V2(to.tableDetails.TableID, 99, to.tableDetails.TableNumber, tm, to.tableUser.UserID, custID, custCount, adCount, kdCount, jnCount, to.tableCustomer.PrevCust, to.ReservedCustomer ? "T" : "F", to.tableCustomer.ReservationID, to.PayAsYouGo, myOutputParamGuid);
                                var v = _context.usp_AN_InsertOrder_V2(to.tableDetails.TableID, 99, tableNumbers, tm, to.tableUser.UserID, custID, custCount, adCount, kdCount, jnCount, to.tableCustomer.PrevCust, to.ReservedCustomer ? "T" : "F", to.tableCustomer.ReservationID, to.PayAsYouGo, myOutputParamGuid);

                                os.OrderGUID = new Guid(myOutputParamGuid.Value.ToString());
                                currentOrder.DateCreated = DateTime.Now;
                                //new order if cust count > 0, create unique code
                                if ((!to.PayAsYouGo && custCount > 0) || (to.PayAsYouGo))
                                {
                                    tblTableOrder tord = new tblTableOrder();
                                    if (!_context.tblTableOrders.Any(x => x.OrderGUID == os.OrderGUID && x.TableId == to.tableDetails.TableID && x.Active == true))
                                    {
                                        os.UniqueCode = rh.GenerateUniqueCode();
                                        tord.Active = true;
                                        tord.CustomerCount = custCount;
                                        tord.DateCreated = DateTime.Now;
                                        tord.OrderGUID = os.OrderGUID;
                                        tord.TableId = to.tableDetails.TableID;
                                        tord.UniqueCode = os.UniqueCode;
                                        _context.tblTableOrders.Add(tord);
                                        _context.SaveChanges();
                                    }
                                }
                            }
                            else
                            {
                                //update customer id & counts in tblOrder if ordered by customer
                                currentOrder = _context.tblOrders.Where(p => p.OrderGUID == os.OrderGUID).FirstOrDefault();
                                if (to.CustomerId > 0 && to.UserType != UserType.Staff.ToString() && (currentOrder.CustomerId == null || (currentOrder.CustomerId != null && currentOrder.CustomerId == 0)))
                                    currentOrder.CustomerId = to.CustomerId;
                                if (custCount > 0)
                                {
                                    currentOrder.AdCount += adCount;
                                    currentOrder.KdCount += kdCount;
                                    currentOrder.JnCount += jnCount;
                                    custCount = (int)(currentOrder.AdCount + currentOrder.KdCount + currentOrder.JnCount);
                                    currentOrder.CustCount = custCount;

                                }
                                _context.Entry(currentOrder).State = EntityState.Modified;
                                _context.SaveChanges();

                                //create unique code if custCount > 0 and code not created earlier for existing order 
                                tblTableOrder tord = new tblTableOrder();
                                tord = _context.tblTableOrders.Where(x => x.OrderGUID == os.OrderGUID).FirstOrDefault();
                                if ((!to.PayAsYouGo && custCount > 0 && (tord == null || (tord != null && tord.Id == 0)))
                                    || (to.PayAsYouGo && (tord == null || (tord != null && tord.Id == 0))))
                                {
                                    tord = new tblTableOrder();
                                    os.UniqueCode = rh.GenerateUniqueCode();
                                    tord.Active = true;
                                    tord.CustomerCount = custCount;
                                    tord.DateCreated = DateTime.Now;
                                    tord.OrderGUID = os.OrderGUID;
                                    tord.TableId = to.tableDetails.TableID;
                                    tord.UniqueCode = os.UniqueCode;
                                    _context.tblTableOrders.Add(tord);
                                    _context.SaveChanges();
                                }
                                else if (custCount > 0 && tord.CustomerCount != custCount && !to.PayAsYouGo)
                                {
                                    tord.CustomerCount = custCount;
                                    _context.Entry(tord).State = EntityState.Modified;
                                    _context.SaveChanges();
                                }

                            }

                            //Update table status
                            var currentTable = _context.tblTables.Where(x => x.TableID == to.tableDetails.TableID).FirstOrDefault();

                            if (currentTable.CurrentStatus != (int)TableState.DrinksOrdered && !to.PayAsYouGo)
                            {
                                if (currentTable.CurrentStatus != (int)TableState.WaiterService)
                                    currentTable.CurrentStatus = (int)TableState.Occupied;
                                else
                                    currentTable.PastStatus = (int)TableState.Occupied;
                                _context.Entry(currentTable).State = EntityState.Modified;
                                _context.SaveChanges();
                            }
                            else if(to.PayAsYouGo && currentTable.CurrentStatus != (int)TableState.PayAsYouGo)
                            {
                                currentTable.CurrentStatus = (int)TableState.PayAsYouGo;
                                _context.Entry(currentTable).State = EntityState.Modified;
                                _context.SaveChanges();
                            }

                            //Update join tables
                            if (to.joinTables.Count > 0)
                            {
                                foreach (var item in to.joinTables)
                                {
                                    _context.usp_AN_UpdateJoinedTables(os.OrderGUID, item.TableID);
                                }
                            }


                            //Insert Drinks & Food Items (if Pay As you Go)
                            if (to.tableProducts.Count > 0)
                            {
                               
                                batchNo = _context.tblOrderParts.Where(x => DbFunctions.TruncateTime(x.DateCreated) == DbFunctions.TruncateTime(DateTime.Now)).OrderByDescending(x => x.BatchNo).Select(x => x.BatchNo).FirstOrDefault();
                                batchNo++;
                                foreach (var item in to.tableProducts)
                                {
                                    if (!item.FoodRefil || (to.PayAsYouGo && item.FoodRefil))
                                    {
                                        for (int i = 0; i < item.ProductQty; i++)
                                        {
                                            tblOrderPart top = new tblOrderPart();
                                            top.OrderGUID = os.OrderGUID;
                                            top.ProductID = item.ProductID;
                                            top.Qty = 1;
                                            top.Price = (decimal?)item.Price;
                                            top.Total = (decimal?)item.Price;

                                            top.DateCreated = DateTime.Now;
                                            if ((item.EnglishName != null && item.EnglishName.Contains("Buffet")) || item.Description.Contains("Buffet"))
                                                top.DateServed = DateTime.Now;
                                            top.LastModified = DateTime.Now;
                                            top.DelInd = false;
                                            top.OrderNo = to.LastOrderNo;
                                            top.WebUpload = false;
                                            top.BatchNo = batchNo;
                                            top.UserName = to.UserName;
                                            //if (to.UserType == UserType.Customer.ToString())
                                            //    top.UserID = to.MobileNumber;
                                            //else

                                            if (to.UserType == UserType.Customer.ToString())
                                            {
                                                //top.UserType = to.UserType;
                                                top.Mobile = to.MobileNumber.ToString();
                                                
                                            }
                                            else
                                            {
                                                //top.UserType = UserType.Staff.ToString();
                                                top.UserID = to.tableUser.UserID;
                                            }
                                            //top.UserID = to.tableUser.UserID;
                                            _context.tblOrderParts.Add(top);
                                            _context.SaveChanges();
                                            if (item.OptionID > 0)
                                            {
                                                tblOrderPartOption torp = new tblOrderPartOption();
                                                torp.OrderPartId = top.OrderPartID;
                                                torp.ProductOptionID = item.OptionID;
                                                torp.OrderGUID = os.OrderGUID;
                                                torp.LastModified = DateTime.Now;
                                                //torp.UserID = to.tableUser.UserID;
                                                //torp.Price = (decimal) item.Price;
                                                torp.DelInd = false;
                                                _context.tblOrderPartOptions.Add(torp);
                                                _context.SaveChanges();
                                            }
                                        }

                                    }

                                    //insert any redemption products
                                    if (item.IsRedemptionProduct == true)
                                    {
                                        for (int i = 0; i < item.ProductQty; i++)
                                        {
                                            totalRedeemedPoints += item.RedemptionPoints;
                                            tblRedeemedProduct tr = new tblRedeemedProduct();
                                            tr.ProductId = item.ProductID;
                                            tr.OrderGUID = os.OrderGUID;
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

                                    if (item.Options != null && item.Options != "")
                                    {
                                        if (!item.FoodRefil)
                                        {
                                            string it = item.ProductQty + " * " + item.Description + " " + item.Options;
                                            it = SpliceText(it, 25);
                                            itemsStr += it + Environment.NewLine;
                                        }
                                        else if (item.FoodRefil && to.PayAsYouGo)
                                        {
                                            string it = item.ProductQty + " - " + item.ChineseName + " - " + item.EnglishName + " " + item.Options;
                                            it = SpliceText(it, 25);
                                            foodItemsStr += it + Environment.NewLine;
                                        }
                                    }
                                    else
                                    {
                                        if (!item.FoodRefil)
                                        {
                                            string it = item.ProductQty + " * " + item.Description;
                                            it = SpliceText(it, 25);

                                            itemsStr += it + Environment.NewLine;
                                        }
                                        else if (item.FoodRefil && to.PayAsYouGo)
                                        {
                                            string it = item.ProductQty + " - " + item.ChineseName + " - " + item.EnglishName;
                                            it = SpliceText(it, 25);
                                            foodItemsStr += it + Environment.NewLine; ;
                                        }
                                    }
                                }

                                //Insert all drinks into tblDrinks 
                                var drinks = to.tableProducts.Where(x => !x.Description.Contains("Buffet")
                                                && !x.Description.Contains("Meal") && !x.FoodRefil).ToList();
                                if (drinks.Count > 0)
                                {
                                    //check if this is an additiona drink or first one. Any drink ordered after 10 mins is additional
                                    bool additionalDrink = false;
                                    bool secondRound = true;
                                    if (currentOrder.DateCreated.AddMinutes(10) < DateTime.Now)
                                    {
                                        additionalDrink = true;
                                    }
                                    if (to.LastOrderNo == 1)
                                    {
                                        secondRound = false;
                                    }
                                    foreach (var dr in drinks)
                                    {
                                        int ptID = (int)_context.tblProducts.Where(x => x.ProductID == dr.ProductID).Select(x => x.ProductTypeID).FirstOrDefault();
                                        for (int i = 0; i < dr.ProductQty; i++)
                                        {
                                            tblDrinksSold tds = new tblDrinksSold();
                                            tds.AdditionalDrink = additionalDrink;
                                            tds.DateCreated = DateTime.Now;
                                            tds.ProductID = dr.ProductID;
                                            tds.Qty = 1;
                                            tds.SecondRound = secondRound;
                                            tds.TableID = to.tableDetails.TableID;
                                            tds.UserID = to.tableUser.UserID;
                                            tds.WebUpload = false;
                                            tds.ProductTypeID = ptID;
                                            _context.tblDrinksSolds.Add(tds);
                                        }
                                    }
                                    if (!to.PayAsYouGo)
                                    {
                                        if (currentTable.CurrentStatus != (int)TableState.WaiterService)
                                            currentTable.CurrentStatus = (int)TableState.DrinksOrdered;
                                        else
                                            currentTable.PastStatus = (int)TableState.DrinksOrdered;
                                        _context.Entry(currentTable).State = EntityState.Modified;
                                        _context.SaveChanges();
                                    }
                                }

                            }

                            //Insert Buffet Items if not PayAs you go
                            if (to.tableProducts.Count > 0 && !to.PayAsYouGo && (to.tableProducts.Any(x => x.FoodRefil)))
                            {
                                var foodItems = to.tableProducts.Where(x => x.FoodRefil == true).ToList();
                                //update next order time for table
                                var taOrd = _context.tblTableOrders.Where(x => x.OrderGUID == os.OrderGUID).FirstOrDefault();
                                if (taOrd == null || (taOrd != null && taOrd.CustomerCount == 0))
                                {
                                    os.OrderGUID = Guid.Empty;
                                    os.message = "Cannot submit order. Please add buffet to table. Thanks";
                                    //os.Logout = true;
                                    errorFound = true;
                                }
                                else
                                {
                                    if (taOrd.NextOrderTime == null)
                                        taOrd.NextOrderTime = DateTime.Now;
                                    else
                                    {
                                        //if (!_context.tblOrderBuffetItems.Any(x => x.OrderGUID == os.OrderGUID && x.Printed == false))
                                        //    taOrd.NextOrderTime = DateTime.Now.AddMinutes(printInterval);
                                        if (!_context.tblPrintBuffetItems.Any(x => x.OrderGUID == os.OrderGUID && x.Printed == false))
                                            taOrd.NextOrderTime = DateTime.Now.AddMinutes(printInterval);
                                    }

                                    //check for inactive products
                                    DateTime updatedTime = DateTime.Now.AddMinutes(-180);
                                    var products = _context.tblMenuItems.Where(x => x.LastModified != null && x.LastModified >= updatedTime && x.Active == false).ToList();
                                    foreach (var item in foodItems)
                                    {
                                        bool itemAvailable = true;
                                        if (products != null & products.Count > 0)
                                        {
                                            if (products.Any(x => x.ProductID == item.ProductID))
                                            {
                                                itemAvailable = false;
                                                unAvailableProducts += item.Description + ",";
                                            }
                                        }
                                        if (itemAvailable)
                                        {
                                            for (int i = 0; i < item.ProductQty; i++)
                                            {
                                                tblOrderBuffetItem tbi = new tblOrderBuffetItem();
                                                tbi.OrderGUID = os.OrderGUID;
                                                tbi.TableId = to.tableDetails.TableID;
                                                tbi.ProductId = item.ProductID;
                                                tbi.Printed = false;
                                                tbi.DateCreated = DateTime.Now;
                                                tbi.Qty = 1;
                                                tbi.UserType = to.UserType;
                                                tbi.UserName = to.UserName;
                                                tbi.DeviceType = item.DeviceType;
                                                if (to.UserType == UserType.Customer.ToString())
                                                {
                                                    tbi.UserType = to.UserType;
                                                    tbi.UserId = to.MobileNumber;
                                                }
                                                else
                                                {
                                                    tbi.UserType = UserType.Staff.ToString();
                                                    tbi.UserId = to.tableUser.UserID;
                                                }
                                                _context.tblOrderBuffetItems.Add(tbi);
                                            }
                                            _context.SaveChanges();
                                        }
                                    }
                                }
                            }

                            //Update customer points

                            //New logic to update redeem points locally
                            os.CustomerPoints = to.tableCustomer.CustomerPoints;
                            //if (totalRedeemedPoints > 0 && to.CustomerId > 0)
                            //{
                            //    int updatedPoints = cs.UpdateCustomerPoints(to.CustomerId, totalRedeemedPoints, 0, os.OrderGUID.ToString());

                            //    if (updatedPoints >= 0)
                            //        os.CustomerPoints = updatedPoints;
                            //}

                            if (totalRedeemedPoints > 0)
                            {
                                os.CustomerPoints = os.CustomerPoints - totalRedeemedPoints;
                                tblCustomerActivity tca = new tblCustomerActivity();
                                tca.FullName = to.UserName;
                                tca.Mobile = to.MobileNumber.ToString();
                                tca.OrderGUID = os.OrderGUID;
                                tca.ActivityType = ActivityType.RedeemPoints.ToString();
                                tca.RedeemPoints = totalRedeemedPoints;
                                tca.RewardPoints = 0;
                                cs.UpdateCustomerActivity(tca);
                            }

                            //Get current total - used for points redemption
                            if (os.OrderGUID != null && os.OrderGUID != Guid.Empty &&
                                _context.tblOrderParts.Any(x => x.OrderGUID == os.OrderGUID))
                                os.CurrentTotal = (float)_context.tblOrderParts.Where(x => x.OrderGUID == os.OrderGUID).Sum(x => x.Price);


                            
                            //Print Bar items
                            if (itemsStr != "")
                            {
                                string strPrint = "";
                                string nl = System.Environment.NewLine;
                                strPrint = "----------------------------" + nl;
                                strPrint += "        " + DateTime.Now.ToString("dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture) + nl;

                                //18 Jan 2024. Changes by Gaurav to include username in drinks ticket
                                if (to.UserType == UserType.Customer.ToString())
                                {
                                    strPrint += "Ordered by " + to.UserName + nl;
                                }
                                else
                                {
                                    strPrint += "Ordered by " + to.tableUser.UserID.ToString() + nl; ;
                                }


                                //strPrint += "Ordered by " + to.tableUser.UserName + nl;
                                strPrint += "TABLE - " + to.tableDetails.TableNumber + nl;
                                strPrint += nl;
                                strPrint += itemsStr;
                                strPrint += nl;
                                strPrint += "----------------------------";
                                string printer = "Bar";
                                if (to.tableUser.UserPrinter != null)
                                {
                                    printer = to.tableUser.UserPrinter;
                                }


                                string ticketNo = "D" + batchNo;
                                tblPrintQueue tpq = new tblPrintQueue();
                                tpq.TicketNo = ticketNo;
                                tpq.ToPrinter = printer;
                                tpq.UserFK = to.tableUser.UserID;
                                tpq.Receipt = strPrint;
                                tpq.DateCreated = DateTime.Now;
                                tpq.BatchNo = batchNo;
                                tpq.OrderGUID = os.OrderGUID;
                                tpq.PCName = "App";
                                tpq.Processed = false;
                                tpq.ProcessedBy = 0;
                                tpq.TableNumber = to.tableDetails.TableNumber;
                                _context.tblPrintQueues.Add(tpq);
                                _context.SaveChanges();
                                //_context.usp_AN_InsertPrintQueue(to.tableUser.UserID, "App", printer, strPrint);
                            }


                            //Print Food items
                            if (foodItemsStr != "")
                            {
                                string strPrint = "";
                                string nl = System.Environment.NewLine;
                                strPrint = "----------------------------" + nl;
                                strPrint += "        " + DateTime.Now.ToString("dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture) + nl;
                                strPrint += "Ordered by " + to.UserName + nl;
                                strPrint += "TABLE - " + to.tableDetails.TableNumber + nl;
                                strPrint += "**** PAY PER ITEM ****" + nl;
                                strPrint += nl;
                                strPrint += foodItemsStr;
                                strPrint += nl;
                                strPrint += "----------------------------";
                                string printer = "Kitchen";


                                string ticketNo = "F" + batchNo;
                                tblPrintQueue tpq = new tblPrintQueue();
                                tpq.TicketNo = ticketNo;
                                tpq.ToPrinter = printer;
                                tpq.UserFK = to.tableUser.UserID;
                                tpq.Receipt = strPrint;
                                tpq.DateCreated = DateTime.Now;
                                tpq.BatchNo = batchNo;
                                tpq.OrderGUID = os.OrderGUID;
                                tpq.PCName = "App";
                                tpq.Processed = false;
                                tpq.ProcessedBy = 0;
                                tpq.TableNumber = to.tableDetails.TableNumber;
                                _context.tblPrintQueues.Add(tpq);
                                _context.SaveChanges();
                                //_context.usp_AN_InsertPrintQueue(to.tableUser.UserID, "App", printer, strPrint);
                            }
                            if (!errorFound)
                            {
                                if (os.OrderGUID == Guid.Empty || os.OrderGUID == null)
                                {
                                    os.message = "Table Paid/Code Expired. Please contact our staff for new code. Thanks";
                                    os.Logout = true;
                                }
                                else
                                {
                                    int approxDeliveryTime = 12;
                                    //var totalUnprintedItems = _context.tblOrderBuffetItems.Where(x => x.Printed == false && DbFunctions.TruncateTime(x.DateCreated) == DbFunctions.TruncateTime(DateTime.Now)).ToList();
                                    var totalUnprintedItems = _context.tblPrintBuffetItems.Where(x => x.Printed == false && DbFunctions.TruncateTime(x.DateOrdered) == DbFunctions.TruncateTime(DateTime.Now)).ToList();

                                    if (totalUnprintedItems != null && totalUnprintedItems.Count > 0)
                                    {
                                        var totalUnprintedItemsCount = totalUnprintedItems.Sum(x => x.Qty);
                                        if (totalUnprintedItemsCount <= 75)
                                            approxDeliveryTime = 12;
                                        else if (totalUnprintedItemsCount <= 100)
                                            approxDeliveryTime = 15;
                                        else
                                            approxDeliveryTime = 18;
                                    }

                                    //os.message = "Thank you for your order. The current delivery time is approx " + approxDeliveryTime + " minutes.";
                                    //os.message = "Thank you for your order. The current delivery time is approx " + approxDeliveryTime + " minutes. \n\n Don't forget to order your free drink by redeeming points. \n\n Checkout Rewards page for more details.";
                                    os.message = "Your order is submitted.";

                                    if (unAvailableProducts != "")
                                        os.message += unAvailableProducts + " is currently unavailable.";
                                    //if(to.BuffetItems.Count > 0 && to.BuffetItems.Any(x=>x.))
                                }
                            }

                        }

                        //if (to.ReservedCustomer && to.tableCustomer.ReservationID > 0)
                        //{
                        //    reservationUniqueCode = _context.tblReservations.Where(x => x.ReservationID > to.tableCustomer.ReservationID).Select(x => x.UniqueCode).FirstOrDefault() ?? "";
                        //}
                    }


                    scope.Complete();
                }

                //If order submitted for reserved customer, auto apply the reservation code
                if (reservationUniqueCode != "")
                {

                }

            }
            catch (SqlException sqlex)
            {
                errMessage += sqlex.Message;
                for (int i = 0; i < sqlex.Errors.Count; i++)
                {
                    errMessage += ("Index #" + i + "\n" +
                        "Message: " + sqlex.Errors[i].Message + "\n" +
                        "LineNumber: " + sqlex.Errors[i].LineNumber + "\n" +
                        "Source: " + sqlex.Errors[i].Source + "\n" +
                        "Procedure: " + sqlex.Errors[i].Procedure + "\n");
                }
                os.message = "Connection error. Only submit again once you have checked your order history to avoid duplicate orders.";
                logger.Info("Order Submit SQL Error - " + os.message);
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                if (ex.InnerException != null)
                    errMessage = ex.InnerException.StackTrace;
                os.message = "Connection error. Only submit again once you have checked your order history to avoid duplicate orders.";
                logger.Info("Order Submit Error - " + os.message);
            }
            return os;
        }

        private bool CheckforDuplicateOrder(Guid orderId, long userId, int orderPartsCount, int buffetItemsCount)
        {
            DateTime orderTime = DateTime.Now.AddSeconds(-45);
            bool duplicate = false;
            using (var _context = new ChineseTillEntities1())
            {
                if (orderPartsCount >0)
                {
                    var orderedItems = _context.tblOrderParts.Where(x => x.OrderGUID == orderId && x.UserID == userId && x.DateCreated > orderTime).ToList();
                    if (orderedItems != null && orderedItems.Count > 0)
                    {
                        if (orderedItems.Count == orderPartsCount)
                            duplicate = true;
                    }
                }
                else if(buffetItemsCount > 0)
                {
                    var orderedItems = _context.tblOrderBuffetItems.Where(x => x.OrderGUID == orderId && x.UserId == userId && x.DateCreated > orderTime).ToList();
                    if (orderedItems != null && orderedItems.Count > 0)
                    {
                        if (orderedItems.Count == buffetItemsCount)
                            duplicate = true;
                    }
                }
            }
            return duplicate;
        }

        public UserOrders GetUserTables(int uid)
        {
            SqlDataManager manager = new SqlDataManager();
            manager.AddParameter("@UserID", uid);
            DataTable results = manager.ExecuteDataTable("usp_AN_GetUserOrders");
            UserOrders oc = new UserOrders();
            Dictionary<Guid, TableOrder> od = new Dictionary<Guid, TableOrder>();
            foreach (DataRow row in results.Rows)
            {
                //Guid oid = (Guid)(FieldConverter.To<Guid>(row["OrderGUID"]) ?? Guid.Empty);
                Guid oid = new Guid();
                oid = FieldConverter.To<Guid>(row["OrderGUID"]);
                if (od.ContainsKey(oid))
                {
                    TableOrder o = od[oid];
                    List<ProductOrderNo> ponList = new List<ProductOrderNo>();
                    ProductOrderNo pon = new ProductOrderNo();
                    ponList = o.pOrderNo;
                    Product p = new Product();
                    p.ProductID = FieldConverter.To<int>(row["ProductID"]);
                    p.Description = FieldConverter.To<String>(row["Description"]);
                    p.Price = FieldConverter.To<float>(row["Price"]);
                    p.ProductQty = FieldConverter.To<int>(row["Qty"]);
                    o.tableDetails.CurrentTotal = (o.tableDetails.CurrentTotal + p.Price);
                    p.OrderedTime = FieldConverter.To<string>(row["DateCreated"]);
                    if (p.OrderedTime == null || p.OrderedTime == "")
                    {
                        p.OrderedTime = FieldConverter.To<string>(row["LastModified"]);
                    }
                    p.ServedTime = FieldConverter.To<string>(row["DateServed"]);
                    p.OrderNo = FieldConverter.To<int>(row["OrderNo"]);

                    //if (o.LastOrderNo < p.OrderNo) {
                    //    o.LastOrderNo = p.OrderNo;
                    //    DateTime prDt = Convert.ToDateTime(p.OrderedTime);
                    //    TimeSpan span = DateTime.Now.Subtract(prDt);
                    //    o.LastOrderTime = (int)Math.Round(span.TotalMinutes);
                    //}
                    bool found = false;
                    foreach (var item in ponList)
                    {
                        if (p.OrderNo == item.OrderNo)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        pon.OrderNo = p.OrderNo;
                        DateTime prDt = Convert.ToDateTime(p.OrderedTime);
                        TimeSpan span = DateTime.Now.Subtract(prDt);
                        pon.OrderTime = (int)Math.Round(span.TotalMinutes);
                        o.pOrderNo.Add(pon);
                    }
                    o.tableProducts.Add(p);
                }
                else
                {

                    TableOrder odr = new TableOrder();
                    Product pr = new Product();
                    odr.tableProducts = new List<Product>();
                    odr.pOrderNo = new List<ProductOrderNo>();
                    ProductOrderNo pon = new ProductOrderNo();
                    odr.OrderGUID = new Guid();
                    odr.OrderGUID = FieldConverter.To<Guid>(row["OrderGUID"]);
                    odr.tableDetails.TableID = FieldConverter.To<int>(row["TableID"]);
                    odr.tableDetails.TableNumber = FieldConverter.To<String>(row["TableNumber"]);
                    odr.tableDetails.OccupiedTime = FieldConverter.To<String>(row["OccupiedTime"]);
                    odr.tableDetails.PaxCount = FieldConverter.To<int>(row["CustCount"]);
                    odr.tableDetails.AdCount = FieldConverter.To<int>(row["AdCount"]);
                    odr.tableDetails.KdCount = FieldConverter.To<int>(row["KdCount"]);
                    odr.tableDetails.JnCount = FieldConverter.To<int>(row["JnCount"]);
                    odr.tableDetails.CurrentTotal = FieldConverter.To<float>(row["Price"]);
                    pr.ProductID = FieldConverter.To<int>(row["ProductID"]);
                    pr.Description = FieldConverter.To<String>(row["Description"]);
                    pr.Price = FieldConverter.To<float>(row["Price"]);
                    pr.OrderedTime = FieldConverter.To<string>(row["DateCreated"]);
                    if (pr.OrderedTime == null || pr.OrderedTime == "")
                    {
                        pr.OrderedTime = FieldConverter.To<string>(row["LastModified"]);
                    }
                    pr.ServedTime = FieldConverter.To<string>(row["DateServed"]);
                    pr.ProductQty = FieldConverter.To<int>(row["Qty"]);
                    pr.OrderNo = FieldConverter.To<int>(row["OrderNo"]);
                    //odr.LastOrderNo = FieldConverter.To<int>(row["OrderNo"]);
                    pon.OrderNo = FieldConverter.To<int>(row["OrderNo"]);
                    DateTime prDt = Convert.ToDateTime(pr.OrderedTime);
                    TimeSpan span = DateTime.Now.Subtract(prDt);
                    //odr.LastOrderTime = (int)Math.Round(span.TotalMinutes);
                    pon.OrderTime = (int)Math.Round(span.TotalMinutes);
                    odr.pOrderNo.Add(pon);
                    odr.tableProducts.Add(pr);
                    od.Add(odr.OrderGUID, odr);
                }

            }
            foreach (KeyValuePair<Guid, TableOrder> entry in od)
            {
                // do something with entry.Value or entry.Key

                oc.ordersList.Add(entry.Value);
            }
            return oc;
        }

        //public TableOrder GetTableOrder(int tid)
        //{
        //    SqlDataManager manager = new SqlDataManager();
        //    manager.AddParameter("@TableID", tid);
        //    DataTable results = manager.ExecuteDataTable("usp_AN_GetTableOrder");

        //    ChineseTillEntities1 context = new ChineseTillEntities1();


        //    TableOrder odr = new TableOrder();
        //    odr.tableProducts = new List<Product>();
        //    odr.pOrderNo = new List<ProductOrderNo>();
        //    //Dictionary<Guid, TableOrder> od = new Dictionary<Guid, TableOrder>();
        //    foreach (DataRow row in results.Rows)
        //    {
        //        Product pr = new Product();
        //        ProductOrderNo pon = new ProductOrderNo();
        //        odr.OrderGUID = new Guid();
        //        odr.OrderGUID = FieldConverter.To<Guid>(row["OrderGUID"]);
        //        odr.tableDetails.TableID = FieldConverter.To<int>(row["TableID"]);
        //        odr.tableDetails.TableNumber = FieldConverter.To<String>(row["TableNumber"]);
        //        odr.tableDetails.OccupiedTime = FieldConverter.To<String>(row["OccupiedTime"]);
        //        odr.tableDetails.PaxCount = FieldConverter.To<int>(row["CustCount"]);
        //        odr.tableDetails.AdCount = FieldConverter.To<int>(row["AdCount"]);
        //        odr.tableDetails.KdCount = FieldConverter.To<int>(row["KdCount"]);
        //        odr.tableDetails.JnCount = FieldConverter.To<int>(row["JnCount"]);
        //        odr.tableDetails.CurrentTotal = FieldConverter.To<float>(row["Price"]);
        //        pr.OrderPartID = FieldConverter.To<int>(row["OrderPartId"]);
        //        var items = from p in context.tblOrderPartOptions
        //                    join q in context.tblProducts on p.ProductOptionID equals q.ProductID
        //                    where p.OrderPartId == pr.OrderPartID
        //                    select new
        //                    {
        //                        OptionID = p.ProductOptionID,
        //                        OptionName = q.Description
        //                    };
        //        if (items != null)
        //        {
        //            foreach (var item in items)
        //            {
        //                pr.OptionID = item.OptionID;
        //                pr.Options = item.OptionName;
        //            }
        //        }

        //        pr.ProductID = FieldConverter.To<int>(row["ProductID"]);
        //        pr.Description = FieldConverter.To<String>(row["Description"]);
        //        pr.Price = FieldConverter.To<float>(row["Price"]);
        //        pr.OrderedTime = FieldConverter.To<string>(row["DateCreated"]);

        //        if (pr.OrderedTime == null || pr.OrderedTime == "")
        //        {
        //            pr.OrderedTime = FieldConverter.To<string>(row["LastModified"]);
        //        }
        //        pr.ServedTime = FieldConverter.To<string>(row["DateServed"]);
        //        pr.ProductQty = FieldConverter.To<int>(row["Qty"]);
        //        pr.OrderNo = FieldConverter.To<int>(row["OrderNo"]);
        //        pr.RedemptionPoints = FieldConverter.To<int>(row["RedemptionPoints"]);
        //        pon.OrderNo = FieldConverter.To<int>(row["OrderNo"]);
        //        DateTime prDt = Convert.ToDateTime(pr.OrderedTime);
        //        TimeSpan span = DateTime.Now.Subtract(prDt);
        //        pon.OrderTime = (int)Math.Round(span.TotalMinutes);
        //        if (odr.LastOrderNo < pr.OrderNo)
        //        {
        //            odr.LastOrderNo = pr.OrderNo;
        //        }
        //        odr.pOrderNo.Add(pon);
        //        odr.tableProducts.Add(pr);
        //    }
        //    odr.BuffetItems = (from a in dbContext.tblOrderBuffetItems
        //                       join b in dbContext.tblProducts on a.ProductId equals b.ProductID
        //                       where a.OrderGUID == odr.OrderGUID
        //                       group a by new { a.ProductId, b.Description, a.DateServed, a.Printed } into g
        //                       select new OrderBuffetItem
        //                       {
        //                           ProductId = g.Key.ProductId,
        //                           Description = g.Key.Description,
        //                           Printed = g.Key.Printed,
        //                           Served = g.Key.DateServed == null ? false : true,
        //                           Ordered = g.Key.Printed == false ? true : false,
        //                           Qty = g.Sum(a => a.Qty)
        //                       }).ToList();

        //    return odr;
        //}
        public TableOrder GetTableOrder(int tid)
        {
            SqlDataManager manager = new SqlDataManager();
            manager.AddParameter("@TableID", tid);
            DataTable results = manager.ExecuteDataTable("usp_AN_GetTableOrder_V1");
            TableOrder odr = new TableOrder();
            try
            {
                using (var _context = new ChineseTillEntities1())
                {

                    odr.tableProducts = new List<Product>();
                    odr.pOrderNo = new List<ProductOrderNo>();
                    //Dictionary<Guid, TableOrder> od = new Dictionary<Guid, TableOrder>();
                    foreach (DataRow row in results.Rows)
                    {
                        Product pr = new Product();
                        ProductOrderNo pon = new ProductOrderNo();
                        odr.OrderGUID = new Guid();
                        odr.OrderGUID = FieldConverter.To<Guid>(row["OrderGUID"]);
                        odr.tableDetails.TableID = FieldConverter.To<int>(row["TableID"]);
                        odr.tableDetails.TableNumber = FieldConverter.To<String>(row["TableNumber"]);
                        odr.tableDetails.OccupiedTime = FieldConverter.To<String>(row["OccupiedTime"]);
                        odr.tableDetails.PaxCount = FieldConverter.To<int>(row["CustCount"]);
                        odr.tableDetails.AdCount = FieldConverter.To<int>(row["AdCount"]);
                        odr.tableDetails.KdCount = FieldConverter.To<int>(row["KdCount"]);
                        odr.tableDetails.JnCount = FieldConverter.To<int>(row["JnCount"]);
                        odr.tableDetails.CurrentTotal = FieldConverter.To<float>(row["Price"]);
                        odr.tableDetails.UniqueCode = FieldConverter.To<int>(row["UniqueCode"]);
                        pr.OrderPartID = FieldConverter.To<int>(row["OrderPartId"]);
                        var items = from p in _context.tblOrderPartOptions
                                    join q in _context.tblProducts on p.ProductOptionID equals q.ProductID
                                    where p.OrderPartId == pr.OrderPartID
                                    select new
                                    {
                                        OptionID = p.ProductOptionID,
                                        OptionName = q.Description
                                    };
                        if (items != null)
                        {
                            foreach (var item in items)
                            {
                                pr.OptionID = item.OptionID;
                                pr.Options = item.OptionName;
                            }
                        }

                        pr.ProductID = FieldConverter.To<int>(row["ProductID"]);
                        pr.Description = FieldConverter.To<String>(row["Description"]);
                        pr.Price = FieldConverter.To<float>(row["Price"]);
                        pr.OrderedTime = FieldConverter.To<string>(row["DateCreated"]);

                        if (pr.OrderedTime == null || pr.OrderedTime == "")
                        {
                            pr.OrderedTime = FieldConverter.To<string>(row["LastModified"]);
                        }
                        pr.ServedTime = FieldConverter.To<string>(row["DateServed"]);
                        pr.ProductQty = FieldConverter.To<int>(row["Qty"]);
                        pr.OrderNo = FieldConverter.To<int>(row["OrderNo"]);
                        pr.RedemptionPoints = FieldConverter.To<int>(row["RedemptionPoints"]);
                        pon.OrderNo = FieldConverter.To<int>(row["OrderNo"]);
                        DateTime prDt = Convert.ToDateTime(pr.OrderedTime);
                        TimeSpan span = DateTime.Now.Subtract(prDt);
                        pon.OrderTime = (int)Math.Round(span.TotalMinutes);
                        if (odr.LastOrderNo < pr.OrderNo)
                        {
                            odr.LastOrderNo = pr.OrderNo;
                        }
                        odr.pOrderNo.Add(pon);
                        odr.tableProducts.Add(pr);
                    }
                    odr.BuffetItems = (from a in _context.tblOrderBuffetItems
                                       join b in _context.tblProducts on a.ProductId equals b.ProductID
                                       where a.OrderGUID == odr.OrderGUID
                                       group a by new { a.ProductId, b.Description, a.DateServed, a.Printed, a.Delivered } into g
                                       select new OrderBuffetItem
                                       {
                                           ProductId = g.Key.ProductId,
                                           Description = g.Key.Description,
                                           Printed = g.Key.Printed,
                                           Delivered = g.Key.Delivered,
                                           Served = g.Key.DateServed == null ? false : true,
                                           Ordered = g.Key.Printed == false ? true : false,
                                           Qty = g.Sum(a => a.Qty)
                                       }).ToList();

                }
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                if (ex.InnerException != null)
                {
                    error = ex.InnerException.StackTrace;
                }

                odr.Message = error;
            }

            return odr;

        }

        public TableOrder GetTableOrderV2(int tid)
        {
            SqlDataManager manager = new SqlDataManager();
            manager.AddParameter("@TableID", tid);
            DataTable results = manager.ExecuteDataTable("usp_AN_GetTableOrder_V1");
            TableOrder odr = new TableOrder();
            try
            {
                using (var _context = new ChineseTillEntities1())
                {

                    odr.tableProducts = new List<Product>();
                    odr.pOrderNo = new List<ProductOrderNo>();
                    //Dictionary<Guid, TableOrder> od = new Dictionary<Guid, TableOrder>();
                    foreach (DataRow row in results.Rows)
                    {
                        Product pr = new Product();
                        ProductOrderNo pon = new ProductOrderNo();
                        odr.OrderGUID = new Guid();
                        odr.OrderGUID = FieldConverter.To<Guid>(row["OrderGUID"]);
                        odr.PayAsYouGo = FieldConverter.To<bool>(row["PayAsYouGo"]);

                        odr.tableDetails.TableID = FieldConverter.To<int>(row["TableID"]);
                        odr.tableDetails.TableNumber = FieldConverter.To<String>(row["TableNumber"]);
                        odr.tableDetails.OccupiedTime = FieldConverter.To<String>(row["OccupiedTime"]);
                        odr.tableDetails.PaxCount = FieldConverter.To<int>(row["CustCount"]);
                        odr.tableDetails.AdCount = FieldConverter.To<int>(row["AdCount"]);
                        odr.tableDetails.KdCount = FieldConverter.To<int>(row["KdCount"]);
                        odr.tableDetails.JnCount = FieldConverter.To<int>(row["JnCount"]);
                        odr.tableDetails.CurrentTotal = FieldConverter.To<float>(row["Price"]);
                        odr.tableDetails.UniqueCode = FieldConverter.To<int>(row["UniqueCode"]);
                        pr.OrderPartID = FieldConverter.To<int>(row["OrderPartId"]);
                        var items = from p in _context.tblOrderPartOptions
                                    join q in _context.tblProducts on p.ProductOptionID equals q.ProductID
                                    where p.OrderPartId == pr.OrderPartID
                                    select new
                                    {
                                        OptionID = p.ProductOptionID,
                                        OptionName = q.Description
                                    };
                        if (items != null)
                        {
                            foreach (var item in items)
                            {
                                pr.OptionID = item.OptionID;
                                pr.Options = item.OptionName;
                            }
                        }

                        pr.ProductID = FieldConverter.To<int>(row["ProductID"]);
                        pr.Description = FieldConverter.To<String>(row["Description"]);
                        pr.EnglishName = FieldConverter.To<String>(row["EnglishName"]);
                        pr.Price = FieldConverter.To<float>(row["Price"]);
                        pr.OrderedTime = FieldConverter.To<string>(row["DateCreated"]);

                        if (pr.OrderedTime == null || pr.OrderedTime == "")
                        {
                            pr.OrderedTime = FieldConverter.To<string>(row["LastModified"]);
                        }
                        pr.ServedTime = FieldConverter.To<string>(row["DateServed"]);
                        pr.ProductQty = FieldConverter.To<int>(row["Qty"]);
                        pr.OrderNo = FieldConverter.To<int>(row["OrderNo"]);
                        pr.RedemptionPoints = FieldConverter.To<int>(row["RedemptionPoints"]);
                        pon.OrderNo = FieldConverter.To<int>(row["OrderNo"]);
                        DateTime prDt = Convert.ToDateTime(pr.OrderedTime);
                        TimeSpan span = DateTime.Now.Subtract(prDt);
                        pon.OrderTime = (int)Math.Round(span.TotalMinutes);
                        if (odr.LastOrderNo < pr.OrderNo)
                        {
                            odr.LastOrderNo = pr.OrderNo;
                        }
                        odr.pOrderNo.Add(pon);
                        odr.tableProducts.Add(pr);
                    }
                    odr.BuffetItems = (from a in _context.tblOrderBuffetItems
                                       join b in _context.tblProducts on a.ProductId equals b.ProductID
                                       where a.OrderGUID == odr.OrderGUID
                                       group a by new { a.ProductId, b.Description, a.DateServed, a.Printed, a.Delivered } into g
                                       select new OrderBuffetItem
                                       {
                                           ProductId = g.Key.ProductId,
                                           Description = g.Key.Description,
                                           Printed = g.Key.Printed,
                                           Delivered = g.Key.DateServed != null ? true : g.Key.Delivered,
                                           Served = g.Key.DateServed == null ? false : true,
                                           Ordered = true,
                                           Qty = g.Sum(a => a.Qty)
                                       }).OrderBy(x => x.Printed).ThenBy(x => x.Delivered).ToList();

                }
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                if (ex.InnerException != null)
                {
                    error = ex.InnerException.StackTrace;
                }

                odr.Message = error;
            }

            return odr;

        }

        public TableOrder GetTableOrderV3(Guid orderId)
        {
            //SqlDataManager manager = new SqlDataManager();
            //manager.AddParameter("@TableID", tid);
            //DataTable results = manager.ExecuteDataTable("usp_AN_GetTableOrder_V1");
            TableOrder odr = new TableOrder();
            try
            {
                using (var _context = new ChineseTillEntities1())
                {
                    var order = (from a in _context.tblOrders
                                 join b in _context.tblTables on a.TableID equals b.TableID
                                 join c in _context.tblTableOrders on a.OrderGUID equals c.OrderGUID
                                 where a.OrderGUID == orderId
                                 select new
                                 {
                                     OrderGUID = a.OrderGUID,
                                     PayAsYouGo = a.PayAsYouGo,
                                     TableID = a.TableID,
                                     TableNumber = b.TableNumber,
                                     OccupiedTime = b.OccupiedTime,
                                     CustCount = a.CustCount ?? 0,
                                     AdCount = a.AdCount ?? 0,
                                     KdCount = a.KdCount ?? 0,
                                     JnCount = a.JnCount ?? 0,
                                     UniqueCode = c.UniqueCode 
                                 }).FirstOrDefault();
                    odr.OrderGUID = order.OrderGUID;
                    odr.PayAsYouGo = order.PayAsYouGo;
                    odr.tableDetails.TableID = order.TableID;
                    odr.tableDetails.TableNumber = order.TableNumber;
                    odr.tableDetails.OccupiedTime = order.OccupiedTime;
                    odr.tableDetails.PaxCount = order.CustCount;
                    odr.tableDetails.AdCount = order.AdCount;
                    odr.tableDetails.KdCount = order.KdCount;
                    odr.tableDetails.JnCount = order.JnCount;
                    odr.tableDetails.UniqueCode = order.UniqueCode;

                    if (_context.tblOrderParts.Any(x => x.OrderGUID == orderId))
                    {
                        odr.tableProducts = (from a in _context.tblOrderParts
                                             join b in _context.tblProducts on a.ProductID equals b.ProductID
                                             where a.OrderGUID == orderId && a.DelInd == false
                                             select new Product
                                             {
                                                 OrderPartID = a.OrderPartID,
                                                 ProductID = a.ProductID,
                                                 Description = b.Description,
                                                 EnglishName = b.EnglishName ?? "",
                                                 Price = (float)a.Price,
                                                 DateCreated = a.DateCreated,
                                                 LastModified = a.LastModified,
                                                 ServeTime = a.DateServed,
                                                 ProductQty = (int)a.Qty,
                                                 OrderNo = a.OrderNo ?? 0,
                                                 RedemptionPoints = b.RedemptionPoints ?? 0,

                                             }).ToList();

                        foreach (var item in odr.tableProducts)
                        {
                            item.ServedTime = item.DateCreated == null ? item.LastModified == null ? "" : item.LastModified.ToString() : item.DateCreated.ToString();
                            item.OrderedTime = item.ServeTime == null ? "" : item.ServeTime.ToString();
                            var options = (from a in _context.tblOrderPartOptions
                                           join b in _context.tblProducts on a.ProductOptionID equals b.ProductID
                                           where a.OrderPartId == item.OrderPartID
                                           select new
                                           {
                                               a.ProductOptionID,
                                               b.Description
                                           }).FirstOrDefault();
                            if (options != null)
                            {
                                item.OptionID = options.ProductOptionID;
                                item.Options = options.Description;
                            }
                        }
                        odr.tableDetails.CurrentTotal = odr.tableProducts.Sum(x => x.Price);
                    }

                    //if (_context.tblOrderBuffetItems.Any(x => x.OrderGUID == order.OrderGUID))
                    //{
                    //    odr.BuffetItems = (from a in _context.tblOrderBuffetItems
                    //                       join b in _context.tblProducts on a.ProductId equals b.ProductID
                    //                       where a.OrderGUID == odr.OrderGUID
                    //                       group a by new { a.ProductId, b.Description, a.DateServed, a.Printed, a.Delivered } into g
                    //                       select new OrderBuffetItem
                    //                       {
                    //                           ProductId = g.Key.ProductId,
                    //                           Description = g.Key.Description,
                    //                           Printed = g.Key.Printed,
                    //                           Delivered = g.Key.DateServed != null ? true : g.Key.Delivered,
                    //                           Served = g.Key.DateServed == null ? false : true,
                    //                           Ordered = true,
                    //                           Qty = g.Sum(a => a.Qty)
                    //                       }).OrderBy(x => x.Printed).ThenBy(x => x.Delivered).ToList();
                    //}
                    if (_context.tblPrintBuffetItems.Any(x => x.OrderGUID == order.OrderGUID))
                    {
                        odr.BuffetItems = (from a in _context.tblPrintBuffetItems
                                           join b in _context.tblProducts on (int)a.ProductId equals b.ProductID
                                           where a.OrderGUID == odr.OrderGUID
                                           group a by new { a.ProductId, b.Description, a.DateServed, a.Printed, a.Delivered ,a.OrderGUID,b.EnglishName,b.ChineseName} into g
                                           select new OrderBuffetItem
                                           {
                                               OrderGUID = g.Key.OrderGUID,
                                               ProductId = g.Key.ProductId,
                                               Description = g.Key.Description,
                                               Printed = g.Key.Printed,
                                               Delivered = g.Key.DateServed != null ? true : g.Key.Delivered,
                                               Served = g.Key.DateServed == null ? false : true,
                                               Ordered = true,
                                               EnglishName = g.Key.EnglishName,
                                               ChineseName = g.Key.ChineseName,
                                               Qty = g.Sum(a => a.Qty)
                                           }).OrderBy(x => x.Printed).ThenBy(x => x.Delivered).ToList();
                    }

                }
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                if (ex.InnerException != null)
                {
                    error = ex.InnerException.StackTrace;
                }

                odr.Message = error;
            }

            return odr;

        }
        public TableOrder GetTableOrderV4(Guid orderId)
        {
            //SqlDataManager manager = new SqlDataManager();
            //manager.AddParameter("@TableID", tid);
            //DataTable results = manager.ExecuteDataTable("usp_AN_GetTableOrder_V1");
            TableOrder odr = new TableOrder();
            try
            {
                using (var _context = new ChineseTillEntities1())
                {
                    var order = (from a in _context.tblOrders
                                 join b in _context.tblTables on a.TableID equals b.TableID
                                 join c in _context.tblTableOrders on a.OrderGUID equals c.OrderGUID
                                 where a.OrderGUID == orderId
                                 select new
                                 {
                                     OrderGUID = a.OrderGUID,
                                     PayAsYouGo = a.PayAsYouGo,
                                     TableID = a.TableID,
                                     TableNumber = b.TableNumber,
                                     OccupiedTime = b.OccupiedTime,
                                     CustCount = a.CustCount ?? 0,
                                     AdCount = a.AdCount ?? 0,
                                     KdCount = a.KdCount ?? 0,
                                     JnCount = a.JnCount ?? 0,
                                     UniqueCode = c.UniqueCode
                                 }).FirstOrDefault();
                    odr.OrderGUID = order.OrderGUID;
                    odr.PayAsYouGo = order.PayAsYouGo;
                    odr.tableDetails.TableID = order.TableID;
                    odr.tableDetails.TableNumber = order.TableNumber;
                    odr.tableDetails.OccupiedTime = order.OccupiedTime;
                    odr.tableDetails.PaxCount = order.CustCount;
                    odr.tableDetails.AdCount = order.AdCount;
                    odr.tableDetails.KdCount = order.KdCount;
                    odr.tableDetails.JnCount = order.JnCount;
                    odr.tableDetails.UniqueCode = order.UniqueCode;
                    odr.RestaurantName = ConfigurationManager.AppSettings["RestaurantName"].ToString();

                    if (_context.tblOrderParts.Any(x => x.OrderGUID == orderId))
                    {
                        odr.tableProducts = (from a in _context.tblOrderParts
                                             join b in _context.tblProducts on a.ProductID equals b.ProductID
                                             where a.OrderGUID == orderId && a.DelInd == false
                                             select new Product
                                             {
                                                 OrderPartID = a.OrderPartID,
                                                 ProductID = a.ProductID,
                                                 Description = b.Description,
                                                 EnglishName = b.EnglishName ?? "",
                                                 Price = (float)a.Price,
                                                 DateCreated = a.DateCreated,
                                                 LastModified = a.LastModified,
                                                 ServeTime = a.DateServed,
                                                 ProductQty = (int)a.Qty,
                                                 OrderNo = a.OrderNo ?? 0,
                                                 RedemptionPoints = b.RedemptionPoints ?? 0,

                                             }).ToList();

                        foreach (var item in odr.tableProducts)
                        {
                            item.ServedTime = item.DateCreated == null ? item.LastModified == null ? "" : item.LastModified.ToString() : item.DateCreated.ToString();
                            item.OrderedTime = item.ServeTime == null ? "" : item.ServeTime.ToString();
                            var options = (from a in _context.tblOrderPartOptions
                                           join b in _context.tblProducts on a.ProductOptionID equals b.ProductID
                                           where a.OrderPartId == item.OrderPartID
                                           select new
                                           {
                                               a.ProductOptionID,
                                               b.Description
                                           }).FirstOrDefault();
                            if (options != null)
                            {
                                item.OptionID = options.ProductOptionID;
                                item.Options = options.Description;
                            }
                        }
                        odr.tableDetails.CurrentTotal = odr.tableProducts.Sum(x => x.Price);
                    }

                    //if (_context.tblOrderBuffetItems.Any(x => x.OrderGUID == order.OrderGUID))
                    //{
                    //    odr.BuffetItems = (from a in _context.tblOrderBuffetItems
                    //                       join b in _context.tblProducts on a.ProductId equals b.ProductID
                    //                       where a.OrderGUID == odr.OrderGUID
                    //                       group a by new { a.ProductId, b.Description, a.DateServed, a.Printed, a.Delivered } into g
                    //                       select new OrderBuffetItem
                    //                       {
                    //                           ProductId = g.Key.ProductId,
                    //                           Description = g.Key.Description,
                    //                           Printed = g.Key.Printed,
                    //                           Delivered = g.Key.DateServed != null ? true : g.Key.Delivered,
                    //                           Served = g.Key.DateServed == null ? false : true,
                    //                           Ordered = true,
                    //                           Qty = g.Sum(a => a.Qty)
                    //                       }).OrderBy(x => x.Printed).ThenBy(x => x.Delivered).ToList();
                    //}
                    if (_context.tblPrintBuffetItems.Any(x => x.OrderGUID == order.OrderGUID))
                    {
                        odr.BuffetItems = (from a in _context.tblPrintBuffetItems
                                           join b in _context.tblProducts on (int)a.ProductId equals b.ProductID
                                           where a.OrderGUID == odr.OrderGUID
                                           group a by new { a.ProductId, b.Description, a.DateServed, a.Printed, a.Delivered, a.OrderGUID, b.EnglishName, b.ChineseName } into g
                                           select new OrderBuffetItem
                                           {
                                               OrderGUID = g.Key.OrderGUID,
                                               ProductId = g.Key.ProductId,
                                               Description = g.Key.Description,
                                               Printed = g.Key.Printed,
                                               Delivered = g.Key.DateServed != null ? true : g.Key.Delivered,
                                               Served = g.Key.DateServed == null ? false : true,
                                               Ordered = true,
                                               EnglishName = g.Key.EnglishName,
                                               ChineseName = g.Key.ChineseName,
                                               Qty = g.Sum(a => a.Qty)
                                           }).OrderBy(x => x.Printed).ThenBy(x => x.Delivered).ToList();
                    }

                }
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                if (ex.InnerException != null)
                {
                    error = ex.InnerException.StackTrace;
                }

                odr.Message = error;
            }

            return odr;

        }

        public string UpdateServeTime(int opid, int pid, int optid, Guid oid, int ordno)
        {
            string updateTime = "";
            SqlDataManager manager = new SqlDataManager();
            manager.AddParameter("@OPid", opid);
            manager.AddParameter("@Pid", pid);
            manager.AddParameter("@Oid", oid);
            manager.AddParameter("@OrderNo", ordno);
            manager.AddParameter("@OptionId", optid);
            manager.AddOutputParameter("@Date", System.Data.DbType.String, updateTime);
            manager.ExecuteNonQuery("usp_AN_UpdateProductStatus");
            updateTime = FieldConverter.To<String>(manager.GetParameterValue("@Date"));
            return updateTime;
        }

        public string UpdateAllProductServeTime(Guid orderId, string type)
        {
            string updateTime = "";
            if (type == MenuType.Drinks.ToString())
            {
                SqlDataManager manager = new SqlDataManager();
                manager.AddParameter("@Oid", orderId);
                manager.AddOutputParameter("@Date", System.Data.DbType.String, updateTime);
                manager.ExecuteNonQuery("usp_AN_UpdateAllProductStatus");
                updateTime = FieldConverter.To<String>(manager.GetParameterValue("@Date"));
            }
            else
            {
                DateTime serveTime = DateTime.Now;
                var items = dbContext.tblPrintBuffetItems.Where(f => f.OrderGUID == orderId && f.DateServed == null && f.Printed == true).ToList();

                //var items = dbContext.tblOrderBuffetItems.Where(f => f.OrderGUID == orderId && f.DateServed == null && f.Printed == true).ToList();
                items.ForEach(a => a.DateServed = serveTime);
                dbContext.SaveChanges();
                updateTime = serveTime.ToString();
            }
            return updateTime;
        }
        public List<KitchenReceipt> GetPrinterReceipts()
        {
            var receipts = (from a in dbContext.tblPrintQueues
                                //where a.DatePrinted != null && a.ToPrinter == "Kitchen" && DbFunctions.TruncateTime(a.DateCreated) == DbFunctions.TruncateTime(DateTime.Now)
                            where (a.ToPrinter == "Kitchen" || a.ToPrinter == "Kitchen2") && DbFunctions.TruncateTime(a.DateCreated) == DbFunctions.TruncateTime(DateTime.Now)
                            select new KitchenReceipt
                            {
                                Receipt = a.Receipt,
                                DatePrinted = a.DatePrinted,
                                DateCreated = a.DateCreated
                            }).OrderByDescending(a => a.DatePrinted).ThenByDescending(a => a.DateCreated).ToList();
            return receipts;
        }
        public BuffetItemsSummary GetBuffetOrderItems()
        {
            ChineseTillEntities1 _context = new ChineseTillEntities1();
            BuffetItemsSummary bis = new BuffetItemsSummary();
            //List<OrderPrintModel> directPrintProducts = new List<OrderPrintModel>();
            int displayTime = Convert.ToInt32(ConfigurationManager.AppSettings["DisplayTime"]);
            int printTime = displayTime + Convert.ToInt32(ConfigurationManager.AppSettings["PrintTime"]);
            DateTime ordersTime = DateTime.Now.AddMinutes(-(displayTime + printTime));
            //DateTime ordersTime = DateTime.Now.AddHours(-5.5);
            //ordersTime = ordersTime.AddMinutes(-(displayTime + printTime));
            //var items = (from a in _context.tblOrders
            //             join b in _context.tblOrderBuffetItems on a.OrderGUID equals b.OrderGUID
            //             join e in _context.tblMenuItems on b.ProductId equals e.ProductID
            //             join c in _context.tblProducts on b.ProductId equals c.ProductID
            //             join d in _context.tblTables on a.TableID equals d.TableID
            //             where a.Paid == false && a.DelInd == false && b.DateCreated > ordersTime && e.DelInd == false
            //             select new
            //             {
            //                 c.Description,
            //                 d.TableNumber,
            //                 b.ProductId,
            //                 b.Qty,
            //                 b.DateCreated,
            //                 b.Printed,
            //                 e.Priority,
            //                 b.OrderGUID,
            //                 e.DirectPrint,
            //                 b.Id,
            //                 b.DateServed,
            //                 c.ProductCode,
            //                 e.MenuID,
            //                 c.ChineseName
            //             }).ToList();
            var items = (from a in _context.tblOrders
                         join b in _context.tblPrintBuffetItems on a.OrderGUID equals (Guid) b.OrderGUID
                         join e in _context.tblMenuItems on b.ProductId equals e.ProductID
                         join c in _context.tblProducts on b.ProductId equals c.ProductID
                         join d in _context.tblTables on a.TableID equals d.TableID
                         where a.Paid == false && a.DelInd == false && b.DateOrdered > ordersTime && e.DelInd == false
                         && b.MenuId == e.MenuID
                         select new
                         {
                             c.Description,
                             d.TableNumber,
                             b.ProductId,
                             b.Qty,
                             b.DateCreated,
                             b.Printed,
                             e.Priority,
                             b.OrderGUID,
                             e.DirectPrint,
                             b.Id,
                             b.DateServed,
                             c.ProductCode,
                             e.MenuID,
                             c.ChineseName
                         }).ToList();

            foreach (var item in items)
            {
                int currMins = (int)DateTime.Now.Subtract(item.DateCreated).TotalMinutes;
                //if (item.DirectPrint && !item.Printed)
                //{
                //    var p1 = _context.tblOrderBuffetItems.Where(x => x.Id == item.Id).FirstOrDefault();
                //    p1.Printed = true;
                //    _context.Entry(p1).State = EntityState.Modified;
                //    _context.SaveChanges();
                //    List<Entity.Product> products = new List<Entity.Product>();
                //    var sb = directPrintProducts.Find(x => x.OrderGUID == item.OrderGUID);
                //    if (sb != null)
                //    {
                //        products = sb.OrderedProducts;
                //        sb.TotalItems += item.Qty;
                //        var p = products.Find(x => x.ProductID == item.ProductId);
                //        if (p != null)
                //            p.ProductQty += item.Qty;
                //        else
                //        {
                //            Entity.Product pr = new Entity.Product();
                //            pr.ProductID = item.ProductId;
                //            pr.Description = item.Description;
                //            pr.ProductQty = item.Qty;
                //            pr.ChineseName = item.ChineseName;
                //            products.Add(pr);
                //        }
                //    }
                //    else
                //    {
                //        OrderPrintModel opm = new OrderPrintModel();
                //        opm.OrderedProducts = new List<Product>();
                //        opm.TableNumber = item.TableNumber;
                //        opm.OrderGUID = item.OrderGUID;
                //        opm.TotalItems = item.Qty;
                //        Entity.Product p = new Entity.Product();
                //        p.ProductID = item.ProductId;
                //        p.Description = item.Description;
                //        p.ProductQty = item.Qty;
                //        p.Priority = item.Priority;
                //        p.MenuID = item.MenuID;
                //        p.ChineseName = item.ChineseName;
                //        opm.OrderedProducts.Add(p);
                //        directPrintProducts.Add(opm);
                //    }
                //}
                if (currMins < displayTime)
                {
                    var sb = bis.PlacedOrders.Find(x => x.ProductID == item.ProductId);
                    if (sb != null)
                    {
                        sb.ProductQty += item.Qty;
                    }
                    else
                    {
                        Entity.Product p = new Entity.Product();
                        p.ProductID = item.ProductId;

                        p.ProductQty = item.Qty;
                        p.Priority = item.Priority;
                        p.ProductCode = (int)item.ProductCode;
                        if (item.ChineseName != null)
                            p.ChineseName = item.ChineseName;
                        else
                            p.ChineseName = "";
                        p.Description = item.ChineseName + "-" + item.Description;
                        p.MenuID = item.MenuID;
                        p.color = "White";
                        if (p.MenuID == 1)
                            p.color = "White";
                        else if (p.MenuID == 3)
                            p.color = "#e400ff";
                        else if (p.MenuID == 5)
                            p.color = "Grey";
                        else if (p.MenuID == 11)
                            p.color = "Green";
                        bis.PlacedOrders.Add(p);
                    }
                }
                //else if (currMins > displayTime && currMins < printTime)
                else
                {
                    if (item.DateServed == null)
                    {
                        var sb = bis.PrintedOrders.Find(x => x.ProductID == item.ProductId);
                        if (sb != null)
                        {
                            sb.ProductQty += item.Qty;
                        }
                        else
                        {
                            Entity.Product p = new Entity.Product();
                            p.ProductID = item.ProductId;

                            p.ProductQty = item.Qty;
                            p.Priority = item.Priority;
                            p.ProductCode = (int)item.ProductCode;
                            if (item.ChineseName != null)
                                p.ChineseName = item.ChineseName;
                            else
                                p.ChineseName = "";
                            p.Description = item.ChineseName + "-" + item.Description;
                            p.MenuID = item.MenuID;
                            p.color = "White";
                            if (p.MenuID == 1)
                                p.color = "White";
                            else if (p.MenuID == 3)
                                p.color = "#e400ff";
                            else if (p.MenuID == 5)
                                p.color = "Grey";
                            else if (p.MenuID == 11)
                                p.color = "Green";
                            bis.PrintedOrders.Add(p);
                        }
                    }
                }
            }
            //if (directPrintProducts.Count > 0)
            //{
            //    PrintProducts(directPrintProducts);
            //}
            if (bis.PlacedOrders.Count > 0)
                bis.PlacedOrders = bis.PlacedOrders.OrderByDescending(x => x.Priority).ThenByDescending(x => x.ProductQty).ToList();
            if (bis.PrintedOrders.Count > 0)
                bis.PrintedOrders = bis.PrintedOrders.OrderByDescending(x => x.ProductQty).ToList();
            return bis;
        }
        public KitchenOrderResponse GetKitchenOrders()
        {
            string response = "success";
            ChineseTillEntities1 _context = new ChineseTillEntities1();
            KitchenOrderResponse kor = new KitchenOrderResponse();

            int currentTime = Convert.ToInt32(DateTime.Now.ToString("HHmm"));
            //currentTime = 1530;
            int pt = Convert.ToInt32(ConfigurationManager.AppSettings["PreparationTime"]);
            int collectionTime = currentTime + pt;

            try
            {
                var orderItems = (from t in _context.tblTakeAwayOrders
                                  join a in _context.tblOrders on t.OrderGUID equals a.OrderGUID
                                  join b in _context.tblOrderParts on a.OrderGUID equals b.OrderGUID
                                  join c in _context.tblProducts on b.ProductID equals c.ProductID
                                  where t.CollectionTime <= collectionTime && t.delInd == 0 && t.HasBeenCollected == 0 && (t.HasBeenPrepared == 0 || t.HasBeenPrepared == null)
                                  && a.DelInd == false && b.DelInd == false
                                  select new
                                  {
                                      a.OrderGUID,
                                      a.TakeAwayName,
                                      a.TableID,
                                      b.OrderPartID,
                                      c.Description,
                                      b.ProductID,
                                      b.Qty,
                                      t.CollectionTime,
                                      t.HasPrinted,

                                  }).ToList();


                foreach (var item in orderItems)
                {
                    OrderPart op = new OrderPart();
                    op.OrderPartID = item.OrderPartID;
                    op.Name = item.Description;
                    op.ProductID = item.ProductID;
                    op.Qty = item.Qty;
                    var k = kor.KitchenOrders.Find(x => x.OrderGUID == item.OrderGUID);
                    if (k == null)
                    {
                        KitchenOrder ko = new KitchenOrder();
                        ko.OrderGUID = item.OrderGUID;
                        ko.OrderNumber = item.TakeAwayName;
                        ko.CollectionTime = item.CollectionTime;
                        ko.TimeRemaining = item.CollectionTime - currentTime;
                        ko.Printed = item.HasPrinted;
                        if (item.TableID > 0)
                        {
                            int tableId = orderItems[0].TableID;
                            ko.TableNumber = _context.tblTables.Where(x => x.TableID == tableId).Select(x => x.TableNumber).FirstOrDefault().ToString();
                        }


                        if (_context.tblOrderPartOptions.Any(x => x.OrderPartId == item.OrderPartID))
                        {
                            BuffetBox bb = new BuffetBox();
                            bb.Name = op.Name;
                            bb.ProductId = op.ProductID;
                            bb.Qty = (int)op.Qty;
                            bb.BoxItems = (from a1 in _context.tblOrderPartOptions
                                           join b1 in _context.tblProducts on a1.ProductOptionID equals b1.ProductID
                                           where a1.OrderPartId == item.OrderPartID
                                           select new OrderPartOption
                                           {
                                               OrderPartId = a1.OrderPartId,
                                               ProductOptionID = b1.ProductID,
                                               Name = b1.Description
                                           }).ToList();
                            foreach (var bb1 in bb.BoxItems)
                            {
                                var p = kor.ProductSummary.Find(x => x.ProductID == bb1.ProductOptionID);
                                if (p != null)
                                {
                                    p.Qty += 1;
                                }
                                else
                                {
                                    OrderPart op1 = new OrderPart();
                                    op1.Name = bb1.Name;
                                    op1.Qty = 1;
                                    op1.ProductID = bb1.ProductOptionID;
                                    kor.ProductSummary.Add(op1);
                                }

                            }
                            ko.Boxes.Add(bb);

                        }
                        else
                        {
                            ko.OrderedItems.Add(op);
                            var p = kor.ProductSummary.Find(x => x.ProductID == op.ProductID);
                            if (p != null)
                            {
                                p.AlacarteQty += (int)op.Qty;
                            }
                            else
                            {
                                OrderPart op1 = new OrderPart();
                                op1.Name = op.Name;
                                op1.Qty = 1;
                                op1.ProductID = op.ProductID;
                                kor.ProductSummary.Add(op1);
                            }

                        }
                        kor.KitchenOrders.Add(ko);
                        kor.OrderCount = kor.OrderCount + 1;
                    }
                    else
                    {

                        if (_context.tblOrderPartOptions.Any(x => x.OrderPartId == item.OrderPartID))
                        {
                            BuffetBox bb = new BuffetBox();
                            bb.Name = op.Name;
                            bb.ProductId = op.ProductID;
                            bb.Qty = (int)op.Qty;
                            bb.BoxItems = (from a1 in _context.tblOrderPartOptions
                                           join b1 in _context.tblProducts on a1.ProductOptionID equals b1.ProductID
                                           where a1.OrderPartId == item.OrderPartID
                                           select new OrderPartOption
                                           {
                                               OrderPartId = a1.OrderPartId,
                                               ProductOptionID = b1.ProductID,
                                               Name = b1.Description
                                           }).ToList();
                            k.Boxes.Add(bb);
                            foreach (var bb1 in bb.BoxItems)
                            {
                                var p = kor.ProductSummary.Find(x => x.ProductID == bb1.ProductOptionID);
                                if (p != null)
                                {
                                    p.Qty += 1;
                                }
                                else
                                {
                                    OrderPart op1 = new OrderPart();
                                    op1.Name = bb1.Name;
                                    op1.Qty = 1;
                                    op1.ProductID = bb1.ProductOptionID;
                                    kor.ProductSummary.Add(op1);
                                }

                            }

                        }
                        else
                        {
                            var oi = k.OrderedItems.Find(x => x.OrderGUID == item.OrderGUID);
                            if (oi != null)
                                oi.Qty += item.Qty;
                            else
                                k.OrderedItems.Add(op);
                            var p = kor.ProductSummary.Find(x => x.ProductID == op.ProductID);
                            if (p != null)
                            {
                                p.AlacarteQty += (int)op.Qty;
                            }
                            else
                            {
                                OrderPart op1 = new OrderPart();
                                op1.Name = op.Name;
                                op1.Qty = 1;
                                op1.ProductID = op.ProductID;
                                kor.ProductSummary.Add(op1);
                            }
                        }
                    }
                }
                foreach (var item in kor.KitchenOrders)
                {
                    if (item.Printed == 0)
                    {
                        string orderDetailStr = "";
                        string kitchenReceipt = "";
                        string barReceipt = "";
                        string ct = Convert.ToString(item.CollectionTime);
                        if (ct.Length == 3)
                        {
                            ct = "0" + ct;
                        }
                        string itemStr = "";
                        int itemCount = 0;

                        foreach (var box in item.Boxes)
                        {
                            itemStr += box.Name + "           - " + box.Qty + Environment.NewLine;
                            itemCount += box.Qty;
                            foreach (var b12 in box.BoxItems)
                            {
                                itemStr += "     " + b12.Name + Environment.NewLine;

                            }
                        }
                        foreach (var i1 in item.OrderedItems)
                        {
                            itemCount += (int)i1.Qty;
                            itemStr += i1.Name + "           - " + i1.Qty + Environment.NewLine;
                        }
                        orderDetailStr = "";
                        orderDetailStr += "        TakeAway               " + Environment.NewLine;
                        orderDetailStr += "        " + item.OrderNumber + "               " + Environment.NewLine;
                        //str += "    Total Items - " + Convert.ToString(item.Boxes.Count + item.OrderedItems.Count) + Environment.NewLine;
                        orderDetailStr += "        Total Items - " + Convert.ToString(itemCount) + Environment.NewLine;
                        orderDetailStr += "        Collection Time - " + ct.Substring(0, 2) + ":" + ct.Substring(2) + Environment.NewLine;
                        orderDetailStr += "-------------------------------------------------" + Environment.NewLine;
                        kitchenReceipt = orderDetailStr + itemStr;
                        tblPrintQueue tp = new tblPrintQueue();
                        tp.Receipt = kitchenReceipt;
                        tp.PCName = "Website";
                        tp.ToPrinter = "Kitchen";
                        tp.UserFK = -10;
                        tp.DateCreated = DateTime.Now;
                        _context.tblPrintQueues.Add(tp);
                        _context.SaveChanges();


                        //Print receipt at Bar printer
                        var custDetails = (from a in _context.tblTakeAwayOrders
                                           join b in _context.tblAddresses on a.AddressID equals b.AddressID
                                           where a.OrderGUID == item.OrderGUID
                                           select new
                                           {
                                               a.NAME,
                                               a.Phone,
                                               b.AddressFull,
                                               b.PostCode
                                           }).FirstOrDefault();
                        string custStr = "";
                        custStr += "        " + custDetails.NAME + Environment.NewLine;
                        custStr += "   " + custDetails.AddressFull + Environment.NewLine;
                        custStr += "        " + custDetails.Phone + Environment.NewLine;
                        barReceipt = custStr + orderDetailStr + itemStr;
                        tblPrintQueue tp1 = new tblPrintQueue();
                        tp1.Receipt = barReceipt;
                        tp1.PCName = "Website";
                        tp1.ToPrinter = "Bar";
                        tp1.UserFK = -10;
                        tp1.DateCreated = DateTime.Now;
                        _context.tblPrintQueues.Add(tp1);
                        _context.SaveChanges();
                        var ta = _context.tblTakeAwayOrders.Where(x => x.OrderGUID == item.OrderGUID).First();
                        ta.HasPrinted = 1;
                        _context.Entry(ta).State = EntityState.Modified;
                        _context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.InnerException.Message;
                //return Json(response, JsonRequestBehavior.AllowGet);
            }
            //kor.koList = koList;
            return kor;
        }

        private void PrintProducts(List<OrderPrintModel> directPrintProducts)
        {
            string itemStr = "";
            string orderDetailStr = "";
            string desItemStr = "";
            string desOrderDetailStr = "";
            string kitchenReceipt = "";
            int itemCount = 0;
            int dItemCount = 0;
            bool dFound = false;

            foreach (var item in directPrintProducts)
            {
                itemStr = "";
                desItemStr = "";
                itemCount = 0;
                dItemCount = 0;
                foreach (var item1 in item.OrderedProducts)
                {
                    if (item1.MenuID == 11)
                    {
                        dFound = true;
                        dItemCount += 1;
                        string it = item1.ProductQty + " - " + item1.ChineseName + " - " + item1.Description;
                        it = SpliceText(it, 25);
                        desItemStr += it + Environment.NewLine;
                    }
                    else
                    {
                        itemCount += 1;
                        string it = item1.ProductQty + " - " + item1.ChineseName + " - " + item1.Description;
                        it = SpliceText(it, 25);
                        itemStr += it + Environment.NewLine;
                    }

                }

                orderDetailStr = "";
                orderDetailStr += "*****" + Environment.NewLine;
                orderDetailStr += "        Table -      " + item.TableNumber + Environment.NewLine;
                orderDetailStr += "        Total Items - " + Convert.ToString(item.TotalItems) + Environment.NewLine;
                orderDetailStr += "-------------------------------------------------" + Environment.NewLine;
                kitchenReceipt = orderDetailStr + itemStr;
                tblPrintQueue tp = new tblPrintQueue();
                tp.Receipt = kitchenReceipt;
                tp.PCName = "DineIn";
                tp.ToPrinter = "Kitchen";
                tp.UserFK = -10;
                tp.DateCreated = DateTime.Now;
                dbContext.tblPrintQueues.Add(tp);
                dbContext.SaveChanges();
                if (dFound)
                {
                    desOrderDetailStr = "";
                    desOrderDetailStr += "        Table -      " + item.TableNumber + Environment.NewLine;
                    desOrderDetailStr += "        Total Items - " + Convert.ToString(item.TotalItems) + Environment.NewLine;
                    desOrderDetailStr += "        DESSERT" + Environment.NewLine;
                    desOrderDetailStr += "-------------------------------------------------" + Environment.NewLine;
                    kitchenReceipt = "";
                    kitchenReceipt = desOrderDetailStr + desItemStr;
                    tblPrintQueue tp1 = new tblPrintQueue();
                    tp1.Receipt = kitchenReceipt;
                    tp1.PCName = "DineIn";
                    tp1.ToPrinter = "Kitchen";
                    tp1.UserFK = -10;
                    tp1.DateCreated = DateTime.Now;
                    dbContext.tblPrintQueues.Add(tp);
                    dbContext.SaveChanges();

                }
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

        public TableOrder GetOrderItems(Guid orderId)
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
                                group a by new { a.ProductID, b.Description, a.Price } into g
                                select new Entity.Product
                                {
                                    ProductID = g.Key.ProductID,
                                    Description = g.Key.Description,
                                    Price = (float)g.Key.Price,
                                    ProductQty = (int)g.Sum(a => a.Qty),
                                    ProductTotal = ((float)g.Key.Price * (int)g.Sum(a => a.Qty))
                                }).ToList();

            return to;
        }

        public List<PrintingBatch> GetPrintBatches(List<string> printers, int threshold = 0)
        {
            List<PrintingBatch> pb = new List<PrintingBatch>();
            //List<string> printers = new List<string>();
            //printers.Add("Kitchen");
            //printers.Add("Kitchen2");
            DateTime thresholdTime = DateTime.Today;
            TimeSpan time = new TimeSpan(06, 0, 0);
            if (threshold > 0)
                thresholdTime = DateTime.Now.AddMinutes(-threshold);
            else
                thresholdTime = thresholdTime.Add(time);
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
                          //where DbFunctions.TruncateTime(a.DateCreated) == DbFunctions.TruncateTime(DateTime.Now)
                          where a.DateCreated > thresholdTime
                          && printers.Contains(a.ToPrinter) && !a.Receipt.Contains("Collection")
                          select new
                          {
                              PrintQueueId = a.PrintQueueID,
                              TableNumber = a.TableNumber != null ? a.TableNumber : (a.Receipt.Contains("Collection") ? "Collection" : ""),
                              BatchNumber = a.BatchNo,
                              Processed = a.Processed,
                              BatchTime = a.DateCreated,
                              TicketNo = a.TicketNo ?? "",
                              PrinterName = a.ToPrinter,
                              TicketStatus = a.TicketStatus,
                              TicketItems = a.Receipt
                          }).OrderByDescending(x => x.BatchTime).AsEnumerable().Select(p => new PrintingBatch()
                          {
                              PrintQueueId = p.PrintQueueId,
                              TableNumber = p.TableNumber,
                              TicketNo = p.TicketNo,
                              Processed = p.Processed,
                              BatchTime = p.BatchTime.ToString("HH:mm"),
                              RePrint = p.TicketNo.Contains("R") ? true : false,
                              PrinterName = p.PrinterName,
                              TicketStatus = p.TicketStatus,
                              TicketItems = p.TicketItems
                          }).ToList();
                }
            }
            catch (Exception ex)
            {


            }
            return pb;
        }
    }
}