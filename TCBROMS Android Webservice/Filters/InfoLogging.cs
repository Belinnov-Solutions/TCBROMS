using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace TCBROMS_Android_Webservice.Filters
{
    public class InfoLogging : ActionFilterAttribute
    {
        Logger logger = LogManager.GetLogger("databaseLogger");
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var Value = "";
            var Param = filterContext.HttpContext.Request.Params;
            var Querystring = filterContext.HttpContext.Request.QueryString.ToString();
            var form = filterContext.HttpContext.Request.Form;
            var ActionParameter = filterContext.ActionParameters;
            if (ActionParameter != null)
            {
                Value = new JavaScriptSerializer().Serialize(ActionParameter.Values);
            }
            var controllerName = filterContext.RouteData.Values["controller"];
            var actionName = filterContext.RouteData.Values["action"];
            logger.Info(controllerName + ":" + actionName + ":" + "Query=" + Querystring + "---" + "ActionParameter=" + Value);
        }
    }
}