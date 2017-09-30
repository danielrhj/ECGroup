using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Newtonsoft.Json.Linq;
using System.Web.Http;

namespace ECGroup
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapPageRoute("AccountList", "AccountList/{key}/{action}", "~/Content/page/APAccountNoList.aspx");//傳多個參數時用/分開，不能用其他符號
            routes.MapPageRoute("SignList", "SignList/{key}/{action}", "~/Content/page/APInvoiceList.aspx");//簽章路由
            
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Login", action = "Index", id = UrlParameter.Optional },
                namespaces: new string[] { "ECGroup.Controllers" }
            );
          

            ModelBinders.Binders.Add(typeof(JObject), new JObjectModelBinder()); //for dynamic model binder
        }
    }
}