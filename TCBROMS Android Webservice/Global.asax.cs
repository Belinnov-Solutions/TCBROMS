using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace TCBROMS_Android_Webservice
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_BeginRequest()
        {

       //     if (Request.Headers.AllKeys.Contains("Origin", StringComparer.OrdinalIgnoreCase) &&
       //Request.HttpMethod == "OPTIONS")
       //     {
       //         Response.Headers.Add("Access-Control-Allow-Origin", "*");
       //         Response.Headers.Add("Access-Control-Allow-Methods", "OPTIONS, GET, POST, PUT, DELETE");
       //         Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Accept, X-Requested-With, Session");
       //         Response.Flush();
       //     }

            //if (Request.Headers.AllKeys.Contains("Origin"))
            //{
            //    Response.Headers.Add("Access-Control-Allow-Origin", "https://apps.thechinesebuffet.com");
            //    Response.Headers.Add("Access-Control-Allow-Methods", "OPTIONS, GET, POST, PUT, DELETE");
            //    Response.Headers.Add("Access-Control-Allow-Headers", "Access-Control-Allow-Methods, Access-Control-Allow-Origin, Content-Type, Accept, X-Requested-With, Session");
            //    //handle CORS pre-flight requests
            //    if (Request.HttpMethod == "OPTIONS")
            //        Response.Flush();
            //}

            //if (HttpContext.Current.Request.HttpMethod == "OPTIONS")
            //{
            //    HttpContext.Current.Response.StatusCode = 204;
            //    HttpContext.Current.Response.End();

            //}
            //HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "https://apps.thechinesebuffet.com");
            //HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept,User-Agent");
            //HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            //HttpContext.Current.Response.AddHeader("Access-Control-Allow-Credentials", "true");
        }

      
}
}
