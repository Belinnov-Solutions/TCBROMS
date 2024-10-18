using Entity;
using Entity.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TCBROMS_Android_Webservice.Models;

namespace TCBROMS_Android_Webservice.Controllers
{

    public class MenuController : Controller
    {
        string url = System.Configuration.ConfigurationManager.AppSettings["AppUrl"];
        GetProductsResponse ProductResponse = new GetProductsResponse();
        // GET: Menu
        public ActionResult GetPrinterReceipts()
        {
            Response.AddHeader("Refresh", "60");
            try
            {
                List<KitchenReceipt> model = new List<KitchenReceipt>();
                OrderService om = new OrderService();
                model = om.GetPrinterReceipts();

                return View(model);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public ActionResult GetDineInMenu()
        {
            try
            {
                GetProductsResponse gpr = new GetProductsResponse();
                Models.ProductService ps = new Models.ProductService();
                gpr = ps.GetDineInProducts();
                ProductResponse = gpr;
                foreach (var item in gpr.BuffetMenu)
                {
                    item.Group.Groupname = item.Group.Groupname.Replace(System.Environment.NewLine, string.Empty);
                }
                //return View(gpr);
                return View("MenuitemTest", gpr);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public ActionResult uplodProductExcel()
        {
            return View();
        }
        [HttpPost]
        public JsonResult uplodProductExcel(HttpPostedFileBase ExcelUpload)
        {
            List<MenuTimingDto> mn = new List<MenuTimingDto>();
            try
            {
                ChineseTillEntities1 _context = new ChineseTillEntities1();
                ProductTimeRestriction model = new ProductTimeRestriction();
                string dirName = Server.MapPath("~/FileBackup ");
                string[] files = Directory.GetFiles(dirName);

                foreach (string file in files)
                {
                    FileInfo fi = new FileInfo(file);
                    if (fi.LastAccessTime < DateTime.Now.AddMonths(-3))
                        fi.Delete();
                }

                if (Request.Files["ExcelUpload"].ContentLength > 0)
                {
                    string fileExtension = System.IO.Path.GetExtension(Request.Files["ExcelUpload"].FileName);
                    model.FileExtension = fileExtension;
                    model.FileName = Request.Files["ExcelUpload"].FileName;
                    //model.FileCreatedDate = DateTime.Now;
                    if (fileExtension == ".xls" || fileExtension == ".xlsx")
                    {
                        DataSet ds = new DataSet();
                        string filename = Server.MapPath("~/FileBackup/" + ExcelUpload.FileName + "");
                        //Same FileName Check
                        int count = 1;
                        string fileNameOnly = Path.GetFileNameWithoutExtension(filename);
                        string extension = Path.GetExtension(filename);
                        string path = Path.GetDirectoryName(filename);
                        while (System.IO.File.Exists(filename))
                        {
                            string tempFileName = string.Format("{0}({1})", fileNameOnly, count++);
                            filename = Path.Combine(path, tempFileName + extension);
                        }

                        ExcelUpload.SaveAs(filename);

                        using (OleDbConnection conn = new OleDbConnection())
                        {
                            if (fileExtension == ".xls")

                                conn.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + filename + ";" + "Extended Properties='HTML Import;HDR=YES;'";
                            if (fileExtension == ".xlsx")
                                conn.ConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filename + ";" + "Extended Properties='Excel 12.0 Xml;HDR=YES;'";
                            using (OleDbCommand comm = new OleDbCommand())
                            {
                                conn.Open();
                                DataTable dtExcelSchema;
                                dtExcelSchema = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                                string SheetName = dtExcelSchema.Rows[0]["TABLE_NAME"].ToString();
                                conn.Close();
                                comm.CommandText = "Select * from [" + SheetName + "]";

                                comm.Connection = conn;

                                using (OleDbDataAdapter da = new OleDbDataAdapter())
                                {
                                    da.SelectCommand = comm;
                                    da.Fill(ds);
                                }

                            }
                        }

                        for (int x = 0; x < ds.Tables[0].Rows.Count; x++)
                        {
                            try
                            {


                                string ProductId = ds.Tables[0].Rows[x][0].ToString();


                                string MSL = ds.Tables[0].Rows[x][2].ToString();
                                if (MSL == "True")
                                {

                                    //tblProductTimeRestriction pr = new tblProductTimeRestriction();
                                    for (int i = 1; i < 7; i++)
                                    {
                                        try
                                        {
                                            MenuTimingDto pr = new MenuTimingDto();
                                            pr.ProductID = Convert.ToInt32(ProductId);
                                            pr.DayID = i;
                                            pr.StartTime = 1000;
                                            pr.EndTime = 1649;
                                            mn.Add(pr);

                                        }
                                        catch (Exception ex)
                                        {
                                            throw ex;
                                        }
                                    }
                                }
                                //new
                                string MTE = ds.Tables[0].Rows[x][3].ToString();
                                if (MTE == "True")
                                {
                                    //tblProductTimeRestriction pd = new tblProductTimeRestriction();
                                    for (int i = 1; i < 5; i++)
                                    {
                                        var m = mn.Find(mp => mp.ProductID == Convert.ToInt32(ProductId));
                                        if (m != null)
                                        {
                                            m.EndTime = 2300;
                                        }
                                        else
                                        {
                                            MenuTimingDto pd = new MenuTimingDto();
                                            pd.ProductID = Convert.ToInt32(ProductId);
                                            pd.DayID = i;
                                            pd.StartTime = 1700;
                                            pd.EndTime = 2300;
                                            mn.Add(pd);
                                        }
                                    }

                                }

                                string FSE = ds.Tables[0].Rows[x][4].ToString();
                                if (FSE == "True")
                                {
                                    for (int i = 5; i < 7; i++)
                                    {
                                        var m = mn.Find(mp => mp.ProductID == Convert.ToInt32(ProductId));
                                        if (m != null)
                                        {
                                            m.EndTime = 2300;
                                        }
                                        else
                                        {
                                            MenuTimingDto pd = new MenuTimingDto();
                                            pd.ProductID = Convert.ToInt32(ProductId);
                                            pd.DayID = i;
                                            pd.StartTime = 1700;
                                            pd.EndTime = 2300;
                                            mn.Add(pd);
                                        }
                                    }
                                }
                                string FSS = ds.Tables[0].Rows[x][5].ToString();
                                if (FSS == "True")
                                {
                                    MenuTimingDto pd = new MenuTimingDto();
                                    pd.ProductID = Convert.ToInt32(ProductId);
                                    pd.DayID = 7;
                                    pd.StartTime = 1000;
                                    pd.EndTime = 2300;
                                    mn.Add(pd);
                                }
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                                //model.errormessage = ex.Message.ToString();
                            }
                        }
                        foreach (var item in mn)
                        {
                            tblProductTimeRestriction pr = new tblProductTimeRestriction();
                            pr.ProductID = item.ProductID;
                            pr.DayID = item.DayID;
                            pr.StartTime = item.StartTime;
                            pr.EndTime = item.EndTime;
                            pr.DelInd = false;
                            // _context.tblProductTimeRestrictions.Add(pr);
                            _context.SaveChanges();
                        }
                    }
                }

                return Json(new { model });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public class ProductTimeRestriction
        {
            public int RestrictionID { get; set; }
            public int ProductID { get; set; }
            public int DayID { get; set; }
            public int StartTime { get; set; }
            public int EndTime { get; set; }
            public bool DelInd { get; set; }
            public string FileExtension { get; set; }
            public string FileName { get; set; }
        }
        public class MenuTimingDto
        {
            public int ProductID { get; set; }
            public int DayID { get; set; }
            public int StartTime { get; set; }
            public int EndTime { get; set; }
            public bool DelInd { get; set; }

        }

        public JsonResult GetProductInfo(int ProductID)
        {
            tblProduct pd = new tblProduct();
            Models.ProductService ps = new Models.ProductService();
            pd = ps.GetProductDetail(ProductID);
            return Json(pd, JsonRequestBehavior.AllowGet);
        }

        public ActionResult FoodDisclaimer(bool payAsYouGo = false)
        {
            string disclaimerString = "";
            if (!payAsYouGo)
            disclaimerString = "We will bring your food and drink orders to your table.<br> <br /> Enjoy as many authentic Chinese dishes as you like, but PLEASE DO NOT WASTE FOOD.<br /> <br />Wastage is charged at £3 per dish.";
            else
                disclaimerString = "We will modify this to put disclaimer for payAsYouGo";
            ViewBag.discstring = disclaimerString;
            return View();
        }

        public ActionResult GetMenuAgainstTime(string MenuId)
        //public ActionResult GetMenuAgainstTime(List<int> MenuId)

        {

            Response.AddHeader("Refresh", "60");
            try
            {
                if (MenuId == null)
                {
                    MenuId = "0";
                }
                var classes = new List<OrderMenuItemsResponse>();
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(url);

                    var responseTask = client.GetAsync("/tcbroms/v3/GetOrderedItemsbyMenu?MenuId=" + MenuId);
                    responseTask.Wait();

                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsAsync<List<OrderMenuItemsResponse>>();
                        readTask.Wait();
                        classes = readTask.Result;
                        //Testing Only;
                        //classes = null;
                        return View(classes);
                    }
                    else //web api sent error response 
                    {
                        ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                    }
                    return View();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //[HttpGet]
        //public async Task<String> MenuAutomate()
        //{
        //    ChineseTillEntities1 _localContext = new ChineseTillEntities1();
        //    var today = DateTime.Today;
        //    var yesterday = today.AddDays(-1);
        //    var dayOfWeek = today.DayOfWeek;
        //    var yesterdayDayOfWeek = yesterday.DayOfWeek;
        //    var currentDayId = (int)dayOfWeek;
        //    var yesterdayDayId = (int)yesterdayDayOfWeek;
        //    int result = 0;

        //    // Revert yesterday's changes
        //    var fullDayMenuYesterday = await _localContext.tblFullDayMenus
        //        .FirstOrDefaultAsync(fdm => fdm.DateModified == yesterday);

        //    if (fullDayMenuYesterday != null)
        //    {
        //        var productTimeRestrictionsYesterday = await _localContext.tblProductTimeRestrictions
        //            .Where(ptr => ptr.DayID == yesterdayDayId + 10)
        //            .ToListAsync();

        //        if (productTimeRestrictionsYesterday != null)
        //        {
        //            var newProductTimeRestrictionsYesterday = await _localContext.tblProductTimeRestrictions
        //                .Where(ptr => ptr.DayID == yesterdayDayId)
        //                .ToListAsync();

        //            if (newProductTimeRestrictionsYesterday.Any())
        //            {
        //                _localContext.tblProductTimeRestrictions.RemoveRange(newProductTimeRestrictionsYesterday);

        //                foreach (var ptr in productTimeRestrictionsYesterday)
        //                {
        //                    ptr.DayID -= 10;
        //                }
        //            }

        //            await _localContext.SaveChangesAsync();
        //        }

        //        await _localContext.SaveChangesAsync();
        //    }
        //    string restaurantId = System.Configuration.ConfigurationManager.AppSettings["RestaurantId"];
        //    string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["AdminServerConnection"].ConnectionString;
        //    List<tblFullDayMenu> menulist = new List<tblFullDayMenu>();
        //    using (SqlConnection con = new SqlConnection(connectionString))
        //    {
        //        con.Open();
        //        SqlCommand cmd = new SqlCommand("usp_BS_GetDayFullMenuList", con);
        //        cmd.CommandType = CommandType.StoredProcedure;
        //        cmd.Parameters.AddWithValue("@RestaurantId", restaurantId);
        //        using (SqlDataReader reader = cmd.ExecuteReader())
        //        {
        //            while (reader.Read())
        //            {
        //                tblFullDayMenu menu = new tblFullDayMenu
        //                {
        //                    RestaurantId = reader.GetInt32(reader.GetOrdinal("RestaurantId")),
        //                    RestaurantName = reader.GetString(reader.GetOrdinal("RestaurantName")),
        //                    Day = reader.GetString(reader.GetOrdinal("Day")),
        //                    DelInd = reader.GetBoolean(reader.GetOrdinal("DelInd")),
        //                    DateModified = reader.GetDateTime(reader.GetOrdinal("EventDate")),
        //                    DateCreated = reader.GetDateTime(reader.GetOrdinal("DateCreated")),
        //                    ModifiedBy = reader.GetString(reader.GetOrdinal("ModifiedBy")),
        //                    UserId = reader.GetInt32(reader.GetOrdinal("UserId"))
        //                };
        //                menulist.Add(menu);
        //            }
        //        }
        //        con.Close();
        //    }
        //    if (menulist.Count > 0)
        //    {
        //        foreach (var menu in menulist)
        //        {
        //            tblFullDayMenu addmenu = new tblFullDayMenu();
        //            addmenu.RestaurantId = menu.RestaurantId;
        //            addmenu.RestaurantName = menu.RestaurantName;
        //            addmenu.Day = menu.Day;
        //            addmenu.DelInd = menu.DelInd;
        //            addmenu.DateModified = menu.DateModified;
        //            addmenu.DateCreated = menu.DateCreated;
        //            addmenu.ModifiedBy = menu.ModifiedBy;
        //            addmenu.UserId = menu.UserId;
        //            _localContext.tblFullDayMenus.Add(addmenu);
        //            _localContext.SaveChanges();
        //        }
        //        using (SqlConnection con = new SqlConnection(connectionString))
        //        {
        //            con.Open();
        //            SqlCommand updateCmd = new SqlCommand("UPDATE tblFullDayMenu SET LocalUpload = 1 WHERE RestaurantId = @RestaurantId AND LocalUpload = 0", con);
        //            updateCmd.Parameters.AddWithValue("@RestaurantId", restaurantId);
        //            updateCmd.ExecuteNonQuery();
        //            con.Close();
        //        }
        //    }
        //    var localmenulist = _localContext.tblFullDayMenus
        //        .Where(x => x.DelInd == false
        //                    && x.DateModified.HasValue
        //                    && x.DateModified.Value.Year == today.Year
        //                    && x.DateModified.Value.Month == today.Month
        //                    && x.DateModified.Value.Day == today.Day)
        //        .ToList();
        //    if (localmenulist.Count > 0)
        //    {
        //        // Success, proceed with updating DayId in ProductTimeRestrictions
        //        var productTimeRestrictionsToday = await _localContext.tblProductTimeRestrictions
        //            .Where(ptr => ptr.DayID == currentDayId)
        //            .ToListAsync();

        //        foreach (var ptr in productTimeRestrictionsToday)
        //        {
        //            ptr.DayID += 10;
        //        }

        //        await _localContext.SaveChangesAsync();

        //        var productTimeRestrictionsWithDayId7 = await _localContext.tblProductTimeRestrictions
        //            .Where(ptr => ptr.DayID == 7)
        //            .ToListAsync();

        //        var newProductTimeRestrictionsToday = productTimeRestrictionsWithDayId7.Select(ptr => new tblProductTimeRestriction
        //        {
        //            ProductID = ptr.ProductID,
        //            RestrictionID = ptr.RestrictionID,
        //            StartTime = ptr.StartTime,
        //            EndTime = ptr.EndTime,
        //            DelInd = false,
        //            DayID = currentDayId,
        //        }).ToList();

        //        _localContext.tblProductTimeRestrictions.AddRange(newProductTimeRestrictionsToday);
        //        await _localContext.SaveChangesAsync();

        //        return "Menu automated successfully.";
        //    }
        //    else if (localmenulist.Count == 0)
        //    {
        //        return "No records to upload.";
        //    }
        //    else
        //    {
        //        return "Error during synchronization.";
        //    }
        //}

        [HttpGet]
        public async Task<String> MenuAutomate()
        {
            ChineseTillEntities1 _localContext = new ChineseTillEntities1();
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);
            var dayOfWeek = today.DayOfWeek;
            var yesterdayDayOfWeek = yesterday.DayOfWeek;
            var currentDayId = (int)dayOfWeek;
            var yesterdayDayId = (int)yesterdayDayOfWeek;

            // Revert yesterday's changes
            var fullDayMenuYesterday = await _localContext.tblFullDayMenus
                .FirstOrDefaultAsync(fdm => fdm.DateModified == yesterday);

            if (fullDayMenuYesterday != null)
            {
                var productTimeRestrictionsYesterday = await _localContext.tblProductTimeRestrictions
                    .Where(ptr => ptr.DayID == yesterdayDayId + 10)
                    .ToListAsync();

                if (productTimeRestrictionsYesterday.Any())
                {
                    var newProductTimeRestrictionsYesterday = await _localContext.tblProductTimeRestrictions
                        .Where(ptr => ptr.DayID == yesterdayDayId)
                        .ToListAsync();

                    _localContext.tblProductTimeRestrictions.RemoveRange(newProductTimeRestrictionsYesterday);

                    foreach (var ptr in productTimeRestrictionsYesterday)
                    {
                        ptr.DayID -= 10;
                    }

                    await _localContext.SaveChangesAsync();
                }
            }

            string restaurantId = System.Configuration.ConfigurationManager.AppSettings["RestaurantId"];
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["AdminServerConnection"].ConnectionString;
            List<tblFullDayMenu> menulist = new List<tblFullDayMenu>();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("usp_BS_GetDayFullMenuList", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@RestaurantId", restaurantId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tblFullDayMenu menu = new tblFullDayMenu
                        {
                            RestaurantId = reader.GetInt32(reader.GetOrdinal("RestaurantId")),
                            RestaurantName = reader.GetString(reader.GetOrdinal("RestaurantName")),
                            Day = reader.GetString(reader.GetOrdinal("Day")),
                            DelInd = reader.GetBoolean(reader.GetOrdinal("DelInd")),
                            DateModified = reader.GetDateTime(reader.GetOrdinal("EventDate")),
                            DateCreated = reader.GetDateTime(reader.GetOrdinal("DateCreated")),
                            ModifiedBy = reader.GetString(reader.GetOrdinal("ModifiedBy")),
                            UserId = reader.GetInt32(reader.GetOrdinal("UserId"))
                        };
                        menulist.Add(menu);
                    }
                }
                con.Close();
            }

            if (menulist.Any())
            {
                foreach (var menu in menulist)
                {
                    if (!_localContext.tblFullDayMenus.Any(fdm => fdm.RestaurantId == menu.RestaurantId && fdm.DateModified == menu.DateModified && fdm.DelInd == false))
                    {
                        _localContext.tblFullDayMenus.Add(menu);
                    }
                }
                await _localContext.SaveChangesAsync();

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlCommand updateCmd = new SqlCommand("UPDATE tblFullDayMenu SET LocalUpload = 1 WHERE RestaurantId = @RestaurantId AND LocalUpload = 0", con);
                    updateCmd.Parameters.AddWithValue("@RestaurantId", restaurantId);
                    updateCmd.ExecuteNonQuery();
                    con.Close();
                }
            }

            var localmenulist = _localContext.tblFullDayMenus
                .Where(x => !x.DelInd && x.DateModified.HasValue &&
                            x.DateModified.Value.Year == today.Year &&
                            x.DateModified.Value.Month == today.Month &&
                            x.DateModified.Value.Day == today.Day)
                .ToList();

            if (localmenulist.Any())
            {
                var productTimeRestrictionsToday = await _localContext.tblProductTimeRestrictions
                    .Where(ptr => ptr.DayID == currentDayId)
                    .ToListAsync();

                foreach (var ptr in productTimeRestrictionsToday)
                {
                    ptr.DayID += 10;
                }

                await _localContext.SaveChangesAsync();

                var productTimeRestrictionsWithDayId7 = await _localContext.tblProductTimeRestrictions
                    .Where(ptr => ptr.DayID == 7)
                    .ToListAsync();

                var newProductTimeRestrictionsToday = productTimeRestrictionsWithDayId7.Select(ptr => new tblProductTimeRestriction
                {
                    ProductID = ptr.ProductID,
                    RestrictionID = ptr.RestrictionID,
                    StartTime = ptr.StartTime,
                    EndTime = ptr.EndTime,
                    DelInd = false,
                    DayID = currentDayId,
                }).ToList();

                _localContext.tblProductTimeRestrictions.AddRange(newProductTimeRestrictionsToday);
                await _localContext.SaveChangesAsync();

                return "Menu automated successfully.";
            }

            return "No records to upload.";
        }
    }
}