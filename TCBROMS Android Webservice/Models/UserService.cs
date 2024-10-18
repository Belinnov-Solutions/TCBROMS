using Entity;
using System.Data;
using Deznu.Products.Common.Utility;
using System.Collections.Generic;
using Entity.Models;
using System;
using System.Linq;
using System.Data.Entity;
using TCBROMS_Android_Webservice.Helpers;
using System.Data.Entity.Core.Objects;

namespace TCBROMS_Android_Webservice.Models
{
    public class UserService
    {
        ChineseTillEntities1 dbContext = new ChineseTillEntities1();
        public User UserLogin(User user)
        {
            SqlDataManager manager = new SqlDataManager();
            manager.AddParameter("@iCode", user.UserCode);
            manager.AddParameter("@iPin", user.DailyPin);
            manager.AddParameter("@DeviceID", user.DeviceID);
            manager.AddParameter("@Token", user.Token);
            DataTable results = manager.ExecuteDataTable("usp_AN_AuthenticateUser");
            User userInstance = new User();
            if (results.Rows.Count == 0)
            {
                userInstance.UserLevel = 0;
                userInstance.UserID = 0;
            }
            else
            {
                foreach (DataRow row in results.Rows)
                {

                    userInstance.UserID = FieldConverter.To<int>(row["UserID"]);
                    userInstance.UserLevel = FieldConverter.To<int>(row["UserLevelID"]);
                    userInstance.UserName = FieldConverter.To<string>(row["UserName"]);
                    userInstance.UserCode = FieldConverter.To<string>(row["UserCode"]);

                }
            }
            return userInstance;
        }

        public TablesList GetTablesList()
        {
            SqlDataManager manager = new SqlDataManager();
            DataTable results = manager.ExecuteDataTable("usp_AN_FetchTables");
            List<Table> tableslist = new List<Table>();
            TablesList tab = new TablesList();
            //TablesList tables = new TablesList();
            foreach (DataRow row in results.Rows)
            {
                Table tableInstance = new Table();
                tableInstance.TableID = FieldConverter.To<int>(row["TableID"]);
                tableInstance.TableNumber = FieldConverter.To<string>(row["TableNumber"]);
                tableInstance.CurrentStatus = FieldConverter.To<int>(row["CurrentStatus"]);
                tableInstance.CurrentTotal = FieldConverter.To<float>(row["CurrentTotal"]);
                tableInstance.OccupiedTime = FieldConverter.To<string>(row["OccupiedTime"]);
                tableInstance.AdCount = FieldConverter.To<int>(row["AdCount"]);
                tableInstance.KdCount = FieldConverter.To<int>(row["KdCount"]);
                tableInstance.JnCount = FieldConverter.To<int>(row["JnCount"]);
                tableInstance.UniqueCode = FieldConverter.To<int>(row["UniqueCode"]);
                tableInstance.OrderGUID = FieldConverter.To<Guid>(row["OrderGUID"]);

                tableInstance.ServiceRequired = FieldConverter.To<bool>(row["ServiceRequired"]);
                //v136 Changes Get Parent table number info //
                if (tableInstance.CurrentStatus == 10)
                {
                    SqlDataManager manager1 = new SqlDataManager();
                    manager1.AddParameter("@TableId", tableInstance.TableID);
                    tableInstance.ParentTableNumber = manager1.ExecuteScalar("usp_AN_GetParentTableNumber");
                }
                else
                    tableInstance.ParentTableNumber = "";
                //v136 Changes Get Parent table number info //
                //tables.tablesList.Add(tableInstance);
                tableslist.Add(tableInstance);

            }
            //userInstance = 

            tab.tablesList = tableslist;
            return tab;
            //return manager.ExecuteNonQuery("usp_AN_UserLogin");
        }

        public TablesList GetTablesListV2(int sectionId = 100)
        {
            List<Table> tableslist = new List<Table>();
            List<Table> tables = new List<Table>();

            TablesList tab = new TablesList();
            
            try
            {
                using (var _context = new ChineseTillEntities1())
                {
                    //var headCounts = _context.usp_AN_GetHeadCounts();
                    //foreach (var row in headCounts)
                    //{
                    //    TimeSpan ts = DateTime.Now - row.DateCreated.Value;
                    //    if (ts.TotalMinutes < 20)
                    //        tab.headCounts.Starters += (int) row.HeadCount;
                    //    else if (ts.TotalMinutes >= 20 && ts.TotalMinutes < 65)
                    //        tab.headCounts.MainCourse += (int)row.HeadCount;
                    //    else
                    //        tab.headCounts.Deserts += (int)row.HeadCount;
                    //}
                    //tables = (from a in _context.tblTables
                    //          where a.DelInd == false && a.SectionId == sectionId
                    //          select new Table
                    //          {
                    //              TableID = a.TableID,
                    //              TableNumber = a.TableNumber,
                    //              CurrentStatus = a.CurrentStatus,
                    //              CurrentTotal = 0,
                    //              OccupiedTime = a.OccupiedTime,
                    //              AdCount = 0,
                    //              KdCount = 0,
                    //              JnCount = 0,
                    //              UniqueCode = 0,
                    //              ServiceRequired = false,
                    //              ParentTableNumber = ""
                    //          }).ToList();

                    //Check for un-released joined tables
                    var joinedTables = _context.tblJoinedTables.Where(x => x.Released == false && DbFunctions.TruncateTime(x.DateCreated) < DbFunctions.TruncateTime(DateTime.Now)).ToList();
                    if(joinedTables != null && joinedTables.Count > 0)
                    {

                        foreach (var item in joinedTables)
                        {
                            _context.Database.ExecuteSqlCommand("Update tblJoinedTables set Released = 1,DateReleased = GETDATE() where Released = 0 and JoinedTablesId =" + item.JoinedTablesId);
                            _context.Database.ExecuteSqlCommand("Update tblTables set CurrentStatus = 0 where CurrentStatus = 10 and TableId =" + item.TableId);
                        }
                    }
                    //else if((joinedTables == null || (joinedTables != null && joinedTables.Count ==0)) && 
                    //    _context.tblTables.Any(x=>x.CurrentStatus == 10))
                    //{
                    //    var unreleasedTables = _context.tblTables.Where(x => x.CurrentStatus == 10).ToList();
                    //    unreleasedTables.ForEach(a => a.CurrentStatus = 0);
                    //    _context.SaveChanges();
                    //}
                    tables = (from a in _context.tblTables
                              where a.DelInd == false 
                              select new Table
                              {
                                  TableID = a.TableID,
                                  TableNumber = a.TableNumber,
                                  CurrentStatus = a.CurrentStatus,
                                  CurrentTotal = 0,
                                  OccupiedTime = a.OccupiedTime,
                                  AdCount = 0,
                                  KdCount = 0,
                                  JnCount = 0,
                                  UniqueCode = 0,
                                  ServiceRequired = false,
                                  ParentTableNumber = "",
                                  SectionId  = a.SectionId ?? 100
                              }).ToList();
                    if (sectionId != 100)
                    {
                        tables = tables.Where(x => x.SectionId == sectionId).ToList();   
                    }
                    
                    //tableslist.AddRange(tables);
                    int[] occupiedStatus = new int[] {1,2,4,5,6,7,8,12,14};
                    //var occupiedTables = tables.Where(x => occupiedStatus.Contains(x.CurrentStatus)).ToList();
                    var occupiedTablesId = tables.Where(x => occupiedStatus.Contains(x.CurrentStatus)).Select(x => x.TableID).Distinct();
                    if (occupiedTablesId != null && occupiedTablesId.Count() > 0)
                    {
                        //foreach (var item in occupiedTables)
                        //{
                        //    var tableFields = (from a in _context.tblOrders
                        //                       join b in _context.tblOrderParts on a.OrderGUID equals b.OrderGUID
                        //                       where a.TableID == item.TableID && a.DelInd == false && a.Paid == false
                        //                       group b by new { a.OrderGUID, a.AdCount, a.KdCount, a.JnCount, a.ServiceRequired } into g
                        //                       select new
                        //                       {
                        //                           OrderGUID = g.Key.OrderGUID,
                        //                           CurrentTotal = (float)(g.Sum(b => b.Total) ?? 0),
                        //                           AdCount = g.Key.AdCount ?? 0,
                        //                           JnCount = g.Key.JnCount ?? 0,
                        //                           KdCount = g.Key.KdCount ?? 0,
                        //                           ServiceRequired = g.Key.ServiceRequired ?? false
                        //                       }).FirstOrDefault();
                        //    item.CurrentTotal = tableFields.CurrentTotal;
                        //    item.AdCount = tableFields.AdCount;
                        //    item.JnCount = tableFields.JnCount;
                        //    item.KdCount = tableFields.KdCount;
                        //    item.ServiceRequired = tableFields.ServiceRequired;
                        //    item.UniqueCode = (int)_context.tblTableOrders.Where(x => x.TableId == item.TableID && x.Active == true && x.OrderGUID == tableFields.OrderGUID)
                        //                    .Select(x => x.UniqueCode).FirstOrDefault();
                        //}

                        foreach (var item in occupiedTablesId)
                        {
                            var occupiedTable = tables.Where(x => x.TableID == item).FirstOrDefault();
                            //var tableFields = (from a in _context.tblOrders
                            //                   join b in _context.tblOrderParts on a.OrderGUID equals b.OrderGUID
                            //                   where a.TableID == item && a.DelInd == false && a.Paid == false
                            //                   group b by new { a.OrderGUID, a.AdCount, a.KdCount, a.JnCount, a.ServiceRequired } into g
                            //                   select new
                            //                   {
                            //                       OrderGUID = g.Key.OrderGUID,
                            //                       CurrentTotal = (float)(g.Sum(b => b.Total) ?? 0),
                            //                       AdCount = g.Key.AdCount ?? 0,
                            //                       JnCount = g.Key.JnCount ?? 0,
                            //                       KdCount = g.Key.KdCount ?? 0,
                            //                       ServiceRequired = g.Key.ServiceRequired ?? false
                            //                   }).FirstOrDefault();
                            var tableFields = (from a in _context.tblOrders
                                               where a.TableID == item && a.DelInd == false && a.Paid == false
                                               && DbFunctions.TruncateTime(a.DateCreated) == DbFunctions.TruncateTime(DateTime.Now)
                                               select new
                                               {
                                                   OrderGUID = a.OrderGUID,
                                                   //CurrentTotal = (float)(g.Sum(b => b.Total) ?? 0),
                                                   AdCount = a.AdCount ?? 0,
                                                   JnCount = a.JnCount ?? 0,
                                                   KdCount = a.KdCount ?? 0,
                                                   ServiceRequired = a.ServiceRequired ?? false,
                                                   HideDrinkMenu = a.HideDrinkMenu ?? false
                                               }).FirstOrDefault();
                            if (tableFields != null)
                            {
                                //occupiedTable.CurrentTotal = tableFields.CurrentTotal;
                                occupiedTable.OrderGUID = tableFields.OrderGUID;
                                occupiedTable.AdCount = tableFields.AdCount;
                                occupiedTable.JnCount = tableFields.JnCount;
                                occupiedTable.KdCount = tableFields.KdCount;
                                occupiedTable.ServiceRequired = tableFields.ServiceRequired;
                                occupiedTable.HideDrinkMenu = tableFields.HideDrinkMenu;
                                //occupiedTable.UniqueCode = (int)_context.tblTableOrders.Where(x => x.TableId == item && x.Active == true && x.OrderGUID == tableFields.OrderGUID)
                                //                .Select(x => x.UniqueCode).FirstOrDefault();
                            }
                        }
                    }
                    var joinedTablesId = tables.Where(x => x.CurrentStatus == 10).Select(x => x.TableID).Distinct();
                    if (joinedTablesId != null && joinedTablesId.Count() > 0)
                    {
                        foreach (var item in joinedTablesId)
                        {
                            
                            var joinedOrderId = _context.tblJoinedTables.Where(x => x.TableId == item && x.Released == false && DbFunctions.TruncateTime(x.DateCreated) == DbFunctions.TruncateTime(DateTime.Now)).Select(x => x.OrderGuid).FirstOrDefault();
                            if (joinedOrderId != null && joinedOrderId != Guid.Empty)
                            {
                                var joinedTable = tables.Where(x => x.TableID == item).FirstOrDefault();
                                var tn = (from a in _context.tblOrders
                                          join b in _context.tblTables on a.TableID equals b.TableID
                                          where a.OrderGUID == joinedOrderId
                                          select new
                                          {
                                              b.TableNumber
                                          }).FirstOrDefault();
                                joinedTable.ParentTableNumber = tn.TableNumber;
                            }
                            else
                            {
                                _context.Database.ExecuteSqlCommand("Update tblTables set CurrentStatus = 0 where CurrentStatus = 10 and TableId =" + item);
                            }
                        }
                    }
                    //tables = tables.OrderBy(x => x.TableNumber).ToList();
                    tables = tables.OrderBy(x => x.TableNumber, new CustomComparer()).ToList();
                    tableslist.AddRange(tables);
                    //tableslist.OrderBy(x => x.TableNumber).ToList();
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            //Get All tables
            tab.tablesList = tableslist;
            return tab;
            //return manager.ExecuteNonQuery("usp_AN_UserLogin");
        }

        public TablesList GetTablesBySection(int sectionid)
        {
            SqlDataManager manager = new SqlDataManager();
            manager.AddParameter("@SectionId", sectionid);
            DataTable results = manager.ExecuteDataTable("usp_AN_FetchTablesV1");
            List<Table> tableslist = new List<Table>();
            TablesList tab = new TablesList();
            //TablesList tables = new TablesList();
            foreach (DataRow row in results.Rows)
            {
                Table tableInstance = new Table();
                tableInstance.TableID = FieldConverter.To<int>(row["TableID"]);
                tableInstance.TableNumber = FieldConverter.To<string>(row["TableNumber"]);
                tableInstance.CurrentStatus = FieldConverter.To<int>(row["CurrentStatus"]);
                tableInstance.CurrentTotal = FieldConverter.To<float>(row["CurrentTotal"]);
                tableInstance.OccupiedTime = FieldConverter.To<string>(row["OccupiedTime"]);
                tableInstance.AdCount = FieldConverter.To<int>(row["AdCount"]);
                tableInstance.KdCount = FieldConverter.To<int>(row["KdCount"]);
                tableInstance.JnCount = FieldConverter.To<int>(row["JnCount"]);
                tableInstance.UniqueCode = FieldConverter.To<int>(row["UniqueCode"]);
                tableInstance.ServiceRequired = FieldConverter.To<bool>(row["ServiceRequired"]);
                tableInstance.OrderGUID = FieldConverter.To<Guid>(row["OrderGUID"]);

                //v136 Changes Get Parent table number info //
                if (tableInstance.CurrentStatus == 10)
                {
                    SqlDataManager manager1 = new SqlDataManager();
                    manager1.AddParameter("@TableId", tableInstance.TableID);
                    tableInstance.ParentTableNumber = manager1.ExecuteScalar("usp_AN_GetParentTableNumber");
                }
                else
                    tableInstance.ParentTableNumber = "";
                //v136 Changes Get Parent table number info //
                //tables.tablesList.Add(tableInstance);
                tableslist.Add(tableInstance);

            }
            //userInstance = 

            tab.tablesList = tableslist;
            return tab;
            //return manager.ExecuteNonQuery("usp_AN_UserLogin");
        }


        //public TablesList GetTablesList()
        //{
        //    SqlDataManager manager = new SqlDataManager();
        //    DataTable results = manager.ExecuteDataTable("usp_AN_FetchTables");
        //    List<NTable> tableslist = new List<NTable>();
        //    TablesList tab = new TablesList();
        //    //TablesList tables = new TablesList();
        //    foreach (DataRow row in results.Rows)
        //    {
        //        NTable tableInstance = new NTable();
        //        tableInstance.ID = FieldConverter.To<int>(row["TableID"]);
        //        tableInstance.No = FieldConverter.To<string>(row["TableNumber"]);
        //        tableInstance.CS = FieldConverter.To<int>(row["CurrentStatus"]);
        //        tableInstance.CT = FieldConverter.To<float>(row["CurrentTotal"]);
        //        tableInstance.OT = FieldConverter.To<string>(row["OccupiedTime"]);
        //        tableInstance.AC = FieldConverter.To<int>(row["AdCount"]);
        //        tableInstance.KC = FieldConverter.To<int>(row["KdCount"]);
        //        tableInstance.JC = FieldConverter.To<int>(row["JnCount"]);
        //        //tables.tablesList.Add(tableInstance);
        //        tableslist.Add(tableInstance);

        //    }
        //    //userInstance = 

        //    tab.tablesList = tableslist;
        //    return tab;
        //    //return manager.ExecuteNonQuery("usp_AN_UserLogin");
        //}

        public AllProducts GetProductsList()
        {
            SqlDataManager manager = new SqlDataManager();
            DataTable results = manager.ExecuteDataTable("usp_AN_GetProducts");
            List<Product> productslist = new List<Product>();
            AllProducts prod = new AllProducts();
            //TablesList tables = new TablesList();
            foreach (DataRow row in results.Rows)
            {
                Product p = new Product();
                p.ProductID = FieldConverter.To<int>(row["ProductID"]);
                p.Description = FieldConverter.To<string>(row["Description"]);
                p.Price = FieldConverter.To<float>(row["Price"]);
                p.ProductGroupID = FieldConverter.To<int>(row["ProductGroupID"]);
                p.GroupName = FieldConverter.To<string>(row["GroupName"]);
                p.ParentGroupID = FieldConverter.To<int>(row["ParentGroupID"]);
                p.ProductAvailable = FieldConverter.To<bool>(row["ProductAvailable"]);
                p.HasLinkedProducts = FieldConverter.To<bool>(row["HasLinkedProducts"]);
                productslist.Add(p);

            }

            
            prod.productsList = productslist;
            return prod;
            
        }
    }
}