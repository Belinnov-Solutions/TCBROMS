using Entity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace TCBROMS_Android_Webservice.Models
{
    public class CustomerService
    {
        ChineseTillEntities1 _context = new ChineseTillEntities1();
        string url = ConfigurationManager.AppSettings["TCBAPIUrl"];

        public int CalculateOrderPoints(Guid orderId, decimal? orderAmount = 0)
        {
            Customer cust = GetRewardPointsForOrder(orderId, orderAmount);
            int pnts = 0;
            if (cust.OrderPoints > 0 && cust.CustomerID > 0)
            {
                pnts = UpdateCustomerPoints(cust.CustomerID, 0, cust.OrderPoints, orderId.ToString());
            }
            return cust.OrderPoints;
        }
        public int UpdatePointsByOrder(Guid orderId, decimal? orderAmount = 0)
        {
            Customer cust = GetRewardPointsForOrder(orderId, orderAmount);
            int pnts = 0;
            if (cust.OrderPoints > 0 && cust.CustomerID > 0)
            {
                pnts = UpdateCustomerPoints(cust.CustomerID, 0, cust.OrderPoints, orderId.ToString());
            }
            return cust.OrderPoints;
        }

        public Customer GetRewardPointsForOrder(Guid orderId, decimal? orderAmount, bool custDetailsRequired= false)
        {
            Customer cust = new Customer();
            
            var products = (from a in _context.tblOrders
                            join b in _context.tblOrderParts on a.OrderGUID equals b.OrderGUID
                            join c in _context.tblProducts on b.ProductID equals c.ProductID
                            where a.OrderGUID == orderId && c.RewardPoints > 0 && b.Price > 0 && b.DelInd == false
                            && (c.RewardStartDate != null && c.RewardStartDate >= DateTime.Now) && (c.RedeemEndDate != null && c.RewardEndDate <= DateTime.Now)
                            group c by new { a.CustomerId, c.ProductID } into g
                            select new
                            {
                                CustomerId = g.Key.CustomerId,
                                totalPoints = g.Sum(x => x.RewardPoints)
                            });

            if (products != null && products.Count() > 0)
            {
                foreach (var item in products)
                {
                    if (item.CustomerId == null)
                        cust.CustomerID = 0;
                    else
                        cust.CustomerID = (int)item.CustomerId;
                    cust.OrderPoints += (int)item.totalPoints;
                }
            }
            //check if user earned some points for any pound spent
            //2023-08-20 Modifying below code to give points as per new logic
            //100 points for every 10 pounds spend
            var pointPerPound = Convert.ToInt32(ConfigurationManager.AppSettings["PointsPerPound"]);

            int pointsRange = (int) orderAmount / 10;
            

            //cust.OrderPoints += Convert.ToInt32(pointPerPound * orderAmount);
            cust.OrderPoints += Convert.ToInt32(pointPerPound * pointsRange * 10);

            //check if there are some points for dine in
            var dineInPoints = Convert.ToInt32(ConfigurationManager.AppSettings["DineInPoints"]);
            cust.OrderPoints += dineInPoints;
            //if (cust.CustomerPoints > 0)
            //{
            //    if (cust.CustomerID == 0)
            //        cust.CustomerID = (int)_context.tblOrders.Where(x => x.OrderGUID == orderId && x.CustomerId != null).Select(x => x.CustomerId).FirstOrDefault();
            //}


            //get Customer Details from admin server
            if(cust.CustomerID > 0 && custDetailsRequired)
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(url);
                    var responseTask = client.GetAsync("v2/GetCustomerById?cusId=" + cust.CustomerID);
                    responseTask.Wait();
                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsAsync<Customer>();
                        readTask.Wait();
                        cust = readTask.Result;
                    }
                    else //web api sent error response 
                    {
                        //log response status here..
                    }
                }
            }
            
            return cust;
        }

        public int UpdateCustomerPoints(int cusId, int redPoints, int earnPoints, string orderid)
        {
            int updatedPoints = -1;
            try
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
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(url);
                    var responseTask = client.GetAsync("v2/UpdateCustomerPoints?cusId=" + cusId + "&redPoints=" + redPoints + "&earnPoints=" + earnPoints + "&resId=" + resId + "&orderId=" + orderid.ToString() + "&activityType=DineIn");
                    responseTask.Wait();
                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsAsync<int>();
                        readTask.Wait();
                        updatedPoints = readTask.Result;
                    }
                    else //web api sent error response 
                    {
                        //log response status here..
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return updatedPoints;
        }

        internal int AddRewardPointsByCustomerId(int custId, int points, string orderId)
        {
            int pnts = 0;
            pnts = UpdateCustomerPoints(custId, 0, points, orderId);
            return points;
        }

        public Customer CustomerRegistration(Customer cust)
        {
            int updatedPoints = -1;
            try
            {
                var myContent = JsonConvert.SerializeObject(cust);
                var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
                var byteContent = new ByteArrayContent(buffer);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(url);
                    var responseTask = client.PostAsync("v1/CustomerRegistration", byteContent).Result;
                    //var responseTask = client.GetAsync("v1/CustomerRegistration" + cusId + "&redPoints=" + redPoints + "&earnPoints=" + earnPoints + "&resId=" + resId + "&orderId=" + orderid.ToString() + "&activityType=DineIn");
                    //responseTask.Wait();
                    //var result = responseTask.Result;
                    if (responseTask.IsSuccessStatusCode)           
                    {
                        var readTask = responseTask.Content.ReadAsAsync<Customer>();
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

            }
            return cust;
        }

        public Customer VerifyRegistrationOTP(string otp,string mobile)
        {
            Customer cust = new Customer();
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(url);
                    var responseTask = client.GetAsync("VerifyOTP?otp=" + otp + "&mobile=" + mobile);
                    responseTask.Wait();
                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsAsync<Customer>();
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

            }
            return cust;
        }

        public string UpdateCustomerActivity(tblCustomerActivity tca)
        {
            string response = "success";
            try
            {
                using (var _context = new ChineseTillEntities1())
                {
                    tca.DateCreated = DateTime.Now;
                    tca.WebUpload = false;
                    tca.Paid = false;
                    tca.DateCreated = DateTime.Now;
                    _context.tblCustomerActivities.Add(tca);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                response = ex.Message;
                if (ex.InnerException != null)
                    response = ex.InnerException.Message;
            }
            return response;
        }
    }
}