
using System;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using ApplicationMng.Model;
using Aspose.Cells;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Model.Database;
using EzBob.Web.Areas.Underwriter.Models.Reports;
using EzBob.Web.Infrastructure;
using EzBob.Web.Infrastructure.Filters;
using EzBob.Web.Models;
using Scorto.Web;
using Scorto.Web.Services;
using SquishIt.Less;
using SquishIt.MsIeCoffeeScript;
using SquishIt.Framework;
using StructureMap;

namespace EzBob.Web
{
    public class MvcApplication : ScortoHttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            var config = ObjectFactory.GetInstance<IEzBobConfiguration>();

            if(config.LandingPageEnabled)
            {
                filters.Add(new WhiteListFilter(), 0);
            }

            filters.Add(new GlobalAreaAuthorizationFilter("Underwriter", "Underwriter, manager, crm, Collector", true), 1);
            filters.Add(new GlobalAreaAuthorizationFilter("Customer", "Web", false, true), 1);
            filters.Add(new EzBobHandleErrorAttribute());
            filters.Add(new LoggingContextFilter(), 1);
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("favicon.ico");

            routes.MapRoute(
                "EmailConfirmation", // Route name
                "confirm/{code}", // URL with parameters
                new { controller = "ConfirmEmail", action = "Index" } // Parameter defaults
            );

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

            routes.Insert(0, new Route("pie.htc", new RouteValueDictionary(new {controller = "Pie", action = "Index"}), new MvcRouteHandler()));
            routes.Insert(0, new Route("{area}/pie.htc", new RouteValueDictionary(new {controller = "Pie", action = "Index", area = ""}), new MvcRouteHandler()));
            //routes.MapRoute(null, "{Area}/{c}/{a}/pie.htc", new {controller = "Pie", action = "Index", c = "", a="", Area=""});
        }

        protected void Application_Start()
        {
            MvcHandler.DisableMvcResponseHeader = true;

            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            ModelBinders.Binders.Add(typeof(DayDate), new DayDateModelBinder());
            ModelBinders.Binders.Add(typeof(DateTime), new Iso8601DateTimeBinder());
            ModelBinders.Binders.Add(typeof(DateTime?), new Iso8601DateTimeBinder());
            ModelBinders.Binders.Add(typeof(decimal), new DecimalModelBinder());
            ModelBinders.Binders.Add(typeof(decimal?), new DecimalModelBinder());

            Scorto.NHibernate.NHibernateManager.FluentAssemblies.Add(typeof(Application).Assembly);
            Scorto.NHibernate.NHibernateManager.FluentAssemblies.Add(typeof(Customer).Assembly);
            Scorto.NHibernate.NHibernateManager.FluentAssemblies.Add(typeof(DbString).Assembly);
            Scorto.NHibernate.NHibernateManager.HbmAssemblies.Add(typeof(PerformencePerUnderwriterDataRow).Assembly);
            ControllerBuilder.Current.SetControllerFactory(new StructureMapControllerFactory());
            InitAspose();
            ConfigureSquishIt();

            var bs = ObjectFactory.GetInstance<MarketPlacesBootstrap>();
            bs.InitValueTypes();
            bs.InitDatabaseMarketPlaceTypes();

        }

        private static void ConfigureSquishIt()
        {
            if (HttpContext.Current.IsDebuggingEnabled)
            {
                Bundle.RegisterScriptPreprocessor(new CachingPreprocessor<CoffeeScriptPreprocessor>());
                Bundle.RegisterScriptPreprocessor(new CachingPreprocessor<LessPreprocessor>());
            }
            else
            {
                Bundle.RegisterScriptPreprocessor(new CoffeeScriptPreprocessor());
                Bundle.RegisterScriptPreprocessor(new LessPreprocessor());
            }
        }

        static void InitAspose()
        {
            var license = new License();

            using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("EzBob.Web.Aspose.Total.lic"))
            {
                s.Position = 0;
                license.SetLicense(s);
            }
        }

        private static IWorkplaceContext GetContext()
        {
            var context = HttpContext.Current.Items["current.context"] as IWorkplaceContext;
            if (context != null) return context;
            context = ObjectFactory.GetInstance<EzBobContext>();
            HttpContext.Current.Items["current.context"] = context;
            return context;
        }

        protected override void ConfigureStructureMap(IContainer container)
        {
            container.Configure(x => x.For<IWorkplaceContext>().Use(GetContext));
            container.Configure(x => x.AddRegistry<PluginWebRegistry>());
            container.Configure(x => x.AddRegistry<PaymentServices.PacNet.PacnetRegistry>());
            base.ConfigureStructureMap(container);
        }

        protected void Application_BeginRequest(Object sender, EventArgs e)
        {
            CheckForCSSPIE();
        }

        private void CheckForCSSPIE()
        {
            if (!Regex.IsMatch(Request.Url.ToString(), "CSS3PIE"))
            {
                return;
            }

            const string appRelativePath = "~/Content/PIE.htc";
            var path = VirtualPathUtility.ToAbsolute(appRelativePath);
            Response.Clear();
            Response.StatusCode = (int)HttpStatusCode.MovedPermanently;
            Response.RedirectLocation = path;
            Response.End();
        }
    }


    public class Iso8601DateTimeBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var name = bindingContext.ModelName;
            var value = bindingContext.ValueProvider.GetValue(name);
            if (value == null) return null;

            return parseIso8601Date(value) ?? base.BindModel(controllerContext, bindingContext);
        }

        private DateTime? parseIso8601Date(ValueProviderResult value)
        {
            DateTime date;
            const string format = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK";
            return DateTime.TryParseExact(value.AttemptedValue, format, null, DateTimeStyles.RoundtripKind, out date) ? date : null as DateTime?;
        }
    }

}