using System.Web;
using System.Web.Mvc;
using TCBROMS_Android_Webservice.Filters;
namespace TCBROMS_Android_Webservice
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new InfoLogging());
        }
    }
}
