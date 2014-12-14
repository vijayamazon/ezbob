using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace GoCardlessMvcTestClient
{
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Logger;

	// Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
	    public MvcApplication() {
			base.Init();
			CurrentValues.Init(DbConnectionGenerator.Get(Log), Log);
		}

	    public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

			
        }

		private static SafeILog Log {
			get {
				if (ms_oLog == null)
					ms_oLog = new SafeILog(typeof(MvcApplication));

				return ms_oLog;
			} // get
		} // Log

		private static SafeILog ms_oLog;
    }
}