using Entity;
using Entity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
//using System.Web.Script.Serialization;

namespace TCBROMS_Android_Webservice.Controllers
{
    public class KitchenController : Controller
    {
        // GET: Kitchen
        public ActionResult Index()
        {
           
            return View();
        }
        [HttpPost]
        public ActionResult Index(User ur)
        {
            try {
                Models.UserService us = new Models.UserService();
                User userInstance = us.UserLogin(ur);
                if (userInstance.UserID != 0)
                {
                    Session["UserInfo"] = new User()
                    {
                        DailyPin = userInstance.DailyPin,
                        UserID = userInstance.UserID,
                        UserName = userInstance.UserName,
                        UserPrinter = userInstance.UserPrinter,
                        UserLevel = userInstance.UserLevel

                    };
                    return RedirectToAction("KitchenOrder","Kitchen");
                }
                else
                {
                    string message = "invalid Usercode/Pin";
                    ModelState.AddModelError("", message);
                }
                return View();
            }
            catch(Exception ex)
            {
                var Message = "Something Went Wrong";
                ModelState.AddModelError(" ", Message);
                return View();
            }
        }

        public ActionResult KitchenOrder()
        {
            Response.AddHeader("Refresh", "60");
            try
            {
                if (Session["UserInfo"] == null)
                {
                    return RedirectToAction("index");
                }
                BuffetItemsSummary bis = new BuffetItemsSummary();
                HomeController hm = new HomeController();
                Models.OrderService os = new Models.OrderService();
                bis = os.GetBuffetOrderItems();
                JsonResult value = hm.GetHeadCounts() as JsonResult;
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                HeadCounts result = serializer.Deserialize<HeadCounts>(serializer.Serialize(value.Data));
                bis.HeadCount = result;
                return View(bis);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Something Went Wrong");
                return View();
            }
        }

        public ActionResult TakeAwayOrder()
        {
            Response.AddHeader("Refresh", "60");
            try
            {
                if (Session["UserInfo"] == null)
                {
                    return RedirectToAction("index");
                }
                KitchenOrderResponse kor = new KitchenOrderResponse();
                Models.OrderService os = new Models.OrderService();
                kor = os.GetKitchenOrders();
                return View(kor); 
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Something Went Wrong");
                return View();
            }
        }
        public ActionResult LogOut()
        {
            Session.Abandon();
            return RedirectToAction("index","Kitchen");
        }
    }
}