using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace PlatformaDeStiri
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "News",
                    action = "Index",
                    id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Delete",
                url: "News/Delete/{id}",
                defaults: new
                {
                    controller = "News",
                    action = "Delete",
                    id = UrlParameter.Optional
                }
            );
        }
    }
}
