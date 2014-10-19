namespace EzBob.Web {
	#region using

	using System;
	using System.Globalization;
	using System.Linq;
	using System.Net;
	using System.Reflection;
	using System.Text.RegularExpressions;
	using System.Web;
	using System.Web.Mvc;
	using System.Web.Routing;
	using Aspose.Cells;
	using ConfigManager;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using Areas.Underwriter.Models.Reports;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Infrastructure;
	using Infrastructure.Filters;
	using Models;
	using SquishIt.Less;
	using SquishIt.MsIeCoffeeScript;
	using SquishIt.Framework;
	using StructureMap;
	using NHibernate;
	using NHibernateWrapper.NHibernate;
	using log4net;

	#endregion using

	#region class MvcApplication

	public class MvcApplication : HttpApplication {
		public static ISession CurrentSession {
			get {
				try {
					if (HttpContext.Current.Items["current.session"] as ISession == null)
						HttpContext.Current.Items["current.session"] = NHibernateManager.OpenSession();

					return HttpContext.Current.Items["current.session"] as ISession;
				}
				catch (Exception ex) {
					Log.Alert(ex, "Failed to fetch current session.");
					throw;
				} // try
			} // get
		} // CurrentSession

		public MvcApplication() {
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
			EndRequest += (sender, args) => {
				var session = HttpContext.Current.Items["current.session"] as ISession;

				if (session != null) {
					if (session.IsOpen)
						CurrentSession.Close();

					CurrentSession.Dispose();
				} // if

				HttpContext.Current.Items["current.session"] = null;
				ThreadContext.Properties.Clear();
			};

			InitOnStart();
		} // constructor

		private void InitOnStart() {
			base.Init();

			if (_isInitialized)
				return;

			lock (this) {
				if (_isInitialized)
					return;

				try {
					new Log4Net().Init();

					Log.NotifyStart();

					CurrentValues.Init(DbConnectionGenerator.Get(Log), Log);

					Ezbob.RegistryScanner.Scanner.Register();

					ConfigureStructureMap(ObjectFactory.Container);
				}
				catch (Exception ex) {
					Log.Error(ex);
					throw;
				} // try

				_isInitialized = true;
			} // lock
		} // InitOnStart

		public static void RegisterGlobalFilters(GlobalFilterCollection filters) {
			var underwriterRoles = string.Join(", ", ObjectFactory.GetInstance<IRolesRepository>().GetAll().Where(x => x.Name != "Web").Select(x => x.Name));

			if (CurrentValues.Instance.LandingPageEnabled)
				filters.Add(new WhiteListFilter(), 0);

			filters.Add(new GlobalAreaAuthorizationFilter("Underwriter", underwriterRoles, true), 1);
			filters.Add(new GlobalAreaAuthorizationFilter("Customer", "Web", false, true), 1);
			filters.Add(new EzBobHandleErrorAttribute());
			filters.Add(new LoggingContextFilter(), 1);
		} // RegisterGlobalFilters

		public static void RegisterRoutes(RouteCollection routes) {
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
			routes.IgnoreRoute("favicon.ico");

			routes.MapRoute(
				"EmailChanged", // Route name
				"emailchanged/{code}", // URL with parameters
				new { controller = "ConfirmEmail", action = "EmailChanged" } // Parameter defaults
			);

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

			routes.Insert(0, new Route("pie.htc", new RouteValueDictionary(new { controller = "Pie", action = "Index" }), new MvcRouteHandler()));
			routes.Insert(0, new Route("{area}/pie.htc", new RouteValueDictionary(new { controller = "Pie", action = "Index", area = "" }), new MvcRouteHandler()));
			//routes.MapRoute(null, "{Area}/{c}/{a}/pie.htc", new {controller = "Pie", action = "Index", c = "", a="", Area=""});
		} // RegisterRoutes

		protected void Application_Start() {
			MvcHandler.DisableMvcResponseHeader = true;

			AreaRegistration.RegisterAllAreas();
			RegisterRoutes(RouteTable.Routes);

			ModelBinders.Binders.Add(typeof(DayDate), new DayDateModelBinder());
			ModelBinders.Binders.Add(typeof(DateTime), new Iso8601DateTimeBinder());
			ModelBinders.Binders.Add(typeof(DateTime?), new Iso8601DateTimeBinder());
			ModelBinders.Binders.Add(typeof(decimal), new DecimalModelBinder());
			ModelBinders.Binders.Add(typeof(decimal?), new DecimalModelBinder());

			NHibernateManager.FluentAssemblies.Add(typeof(User).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof(Customer).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof(DbString).Assembly);
			NHibernateManager.HbmAssemblies.Add(typeof(PerformencePerUnderwriterDataRow).Assembly);
			ControllerBuilder.Current.SetControllerFactory(new StructureMapControllerFactory());
			InitAspose();
			ConfigureSquishIt();

			var bs = ObjectFactory.GetInstance<MarketPlacesBootstrap>();
			bs.InitValueTypes();
			bs.InitDatabaseMarketPlaceTypes();

			RegisterGlobalFilters(GlobalFilters.Filters);
		} // Application_Start

		protected void Application_End() {
			Log.NotifyStop();
		} // Application_End

		private static void ConfigureSquishIt() {
			if (HttpContext.Current.IsDebuggingEnabled) {
				Bundle.RegisterScriptPreprocessor(new CachingPreprocessor<CoffeeScriptPreprocessor>());
				Bundle.RegisterScriptPreprocessor(new CachingPreprocessor<LessPreprocessor>());
			}
			else {
				Bundle.RegisterScriptPreprocessor(new CoffeeScriptPreprocessor());
				Bundle.RegisterScriptPreprocessor(new LessPreprocessor());
			} // if
		} // ConfigureSquishIt

		static void InitAspose() {
			var license = new License();

			using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("EzBob.Web.Aspose.Total.lic")) {
				s.Position = 0;
				license.SetLicense(s);
			} // using
		} // InitAspose

		private static IWorkplaceContext GetContext() {
			var context = HttpContext.Current.Items["current.context"] as IWorkplaceContext;

			if (context != null)
				return context;

			context = ObjectFactory.GetInstance<EzBobContext>();
			HttpContext.Current.Items["current.context"] = context;
			return context;
		} // GetContext

		protected void ConfigureStructureMap(IContainer container) {
			container.Configure(x => x.For<IWorkplaceContext>().Use(GetContext));
			container.Configure(x => x.AddRegistry<PluginWebRegistry>());
			container.Configure(x => x.AddRegistry<PaymentServices.PacNet.PacnetRegistry>());
			container.Configure(x => {
				x.For<ISession>().Use(() => CurrentSession);
			});
		} // ConfigureStructureMap

		protected void Application_BeginRequest(Object sender, EventArgs e) {
			CheckForCSSPIE();
		} // Application_BeginRequest

		private void CheckForCSSPIE() {
			if (!Regex.IsMatch(Request.Url.ToString(), "CSS3PIE"))
				return;

			const string appRelativePath = "~/Content/PIE.htc";
			var path = VirtualPathUtility.ToAbsolute(appRelativePath);
			Response.Clear();
			Response.StatusCode = (int)HttpStatusCode.MovedPermanently;
			Response.RedirectLocation = path;
			Response.End();
		} // CheckForCSSPIE

		private static bool _isInitialized = false;

		private static SafeILog Log {
			get {
				if (ms_oLog == null)
					ms_oLog = new SafeILog(LogManager.GetLogger(typeof(MvcApplication)));

				return ms_oLog;
			} // get
		} // Log

		private static SafeILog ms_oLog;
	} // class MvcApplication

	#endregion class MvcApplication

	#region class Iso8601DateTimeBinder

	public class Iso8601DateTimeBinder : DefaultModelBinder {
		public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext) {
			var name = bindingContext.ModelName;

			var value = bindingContext.ValueProvider.GetValue(name);

			if (value == null)
				return null;

			return parseIso8601Date(value) ?? base.BindModel(controllerContext, bindingContext);
		} // BindModel

		private DateTime? parseIso8601Date(ValueProviderResult value) {
			DateTime date;
			const string format = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK";
			return DateTime.TryParseExact(value.AttemptedValue, format, null, DateTimeStyles.RoundtripKind, out date) ? date : null as DateTime?;
		} // parseIso8601Date
	} // class Iso8601DateTimeBinder

	#endregion class Iso8601DateTimeBinder
} // namespace
