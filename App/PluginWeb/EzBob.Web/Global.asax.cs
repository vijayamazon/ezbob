namespace EzBob.Web {
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
	using Ezbob.Database;
	using Ezbob.Database.Pool;
	using Ezbob.Logger;
	using EzBob.Web.Areas.Underwriter.Models.Reports;
	using EzBob.Web.Infrastructure;
	using EzBob.Web.Infrastructure.Filters;
	using EzBob.Web.Models;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using log4net;
	using NHibernate;
	using NHibernateWrapper.NHibernate;
	using ServiceClientProxy;
	using SquishIt.Framework;
	using SquishIt.Less;
	using SquishIt.MsIeCoffeeScript;
	using StructureMap;

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

	public class MvcApplication : HttpApplication {
		public static ISession CurrentSession {
			get {
				const string CurrentSessionKey = "current.session";

				try {
					ISession curSession = null;

					if (System.Web.HttpContext.Current.Items.Contains(CurrentSessionKey))
						curSession = System.Web.HttpContext.Current.Items[CurrentSessionKey] as ISession;

					if (curSession == null) {
						curSession = NHibernateManager.OpenSession();
						System.Web.HttpContext.Current.Items[CurrentSessionKey] = curSession;
					} // if

					return curSession;
				} catch (Exception ex) {
					Log.Alert(ex, "Failed to fetch current session.");
					throw;
				} // try
			} // get
		} // CurrentSession

		public MvcApplication() {
			EndRequest += (sender, args) => {
				var session = System.Web.HttpContext.Current.Items["current.session"] as ISession;

				if (session != null) {
					if (session.IsOpen)
						CurrentSession.Close();

					CurrentSession.Dispose();
				} // if

				System.Web.HttpContext.Current.Items["current.session"] = null;
				ThreadContext.Properties.Clear();
			};

			InitOnStart();
		} // constructor

		public static void RegisterRoutes(RouteCollection routes) {
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
			routes.IgnoreRoute("favicon.ico");

			routes.MapRoute(
				"EmailChanged", // Route name
				"emailchanged/{code}", // URL with parameters
				new {
					controller = "ConfirmEmail",
					action = "EmailChanged"
				} // Parameter defaults
			);

			routes.MapRoute(
				"EmailConfirmation", // Route name
				"confirm/{code}", // URL with parameters
				new {
					controller = "ConfirmEmail",
					action = "Index"
				} // Parameter defaults
			);

			routes.MapRoute(
				"Default", // Route name
				"{controller}/{action}/{id}", // URL with parameters
				new {
					controller = "Home",
					action = "Index",
					id = UrlParameter.Optional
				} // Parameter defaults
			);

			routes.Insert(0, new Route("pie.htc", new RouteValueDictionary(new {
				controller = "Pie",
				action = "Index"
			}), new MvcRouteHandler()));
			routes.Insert(0, new Route("{area}/pie.htc", new RouteValueDictionary(new {
				controller = "Pie",
				action = "Index",
				area = ""
			}), new MvcRouteHandler()));
			//routes.MapRoute(null, "{Area}/{c}/{a}/pie.htc", new {controller = "Pie", action = "Index", c = "", a="", Area=""});
		}

		protected void Application_BeginRequest(Object sender, EventArgs e) {
			CheckForCSSPIE();
		} // Application_BeginRequest

		protected void Application_End() {
			Log.NotifyStop();
		} // Application_End

		protected void Application_EndRequest() {
			// Any AJAX request that ends in a redirect should get mapped to an unauthorized request
			// since it should only happen when the request is not authorized and gets automatically
			// redirected to the login page.
			var context = new HttpContextWrapper(Context);
			if (context.Response.StatusCode != 200 && context.Request.IsAjaxRequest()) {
				Log.Warn("Request: {0}  response status code: {1}", context.Request.Path, context.Response.StatusCode);
			}
			if (context.Response.StatusCode == 423 && context.Request.IsAjaxRequest()) {
				context.Response.Clear();
				Context.Response.StatusCode = 423;
			}
		} // Application_EndRequest

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

		protected void ConfigureStructureMap(IContainer container) {
			container.Configure(x => x.For<IWorkplaceContext>().Use(GetContext));
			container.Configure(x => x.AddRegistry<PluginWebRegistry>());
			container.Configure(x => x.AddRegistry<PaymentServices.PacNet.PacnetRegistry>());
			container.Configure(x => x.For<ISession>().Use(() => CurrentSession));
		} // ConfigureStructureMap

		protected void Session_End(object sender, EventArgs e) {
			string userSessionIdStr = Session["UserSessionId"] == null ? null : Session["UserSessionId"].ToString();
			int sessionId = 0;
			if (int.TryParse(userSessionIdStr, out sessionId) && sessionId > 0)
				new ServiceClient().Instance.MarkSessionEnded(sessionId, "Session time out", null, null);
		} // SessionEnd

		private static SafeILog Log {
			get {
				if (log == null)
					log = new SafeILog(typeof(MvcApplication));

				return log;
			} // get
		} // Log

		private static void ConfigureSquishIt() {
			if (System.Web.HttpContext.Current.IsDebuggingEnabled) {
				Bundle.RegisterScriptPreprocessor(new CachingPreprocessor<CoffeeScriptPreprocessor>());
				Bundle.RegisterScriptPreprocessor(new CachingPreprocessor<LessPreprocessor>());
			} else {
				Bundle.RegisterScriptPreprocessor(new CoffeeScriptPreprocessor());
				Bundle.RegisterScriptPreprocessor(new LessPreprocessor());
			} // if
		} // ConfigureSquishIt

		private static IWorkplaceContext GetContext() {
			var context = System.Web.HttpContext.Current.Items["current.context"] as IWorkplaceContext;

			if (context != null)
				return context;

			context = ObjectFactory.GetInstance<EzBobContext>();
			System.Web.HttpContext.Current.Items["current.context"] = context;
			return context;
		} // GetContext

		private static void InitAspose() {
			var license = new License();

			using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("EzBob.Web.Aspose.Total.lic")) {
				if (s != null) {
					s.Position = 0;
					license.SetLicense(s);
				} // if
			} // using
		} // InitAspose

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

		private void InitOnStart() {
			Init();

			if (_isInitialized)
				return;

			lock (this) {
				if (_isInitialized)
					return;

				try {
					new Log4Net().Init();

					Log.NotifyStart();

					var db = DbConnectionGenerator.Get(Log);

					Ezbob.Models.Library.Initialize(db.Env, db, Log);
					EZBob.DatabaseLib.Library.Initialize(db.Env, db, Log);

					CurrentValues.Init(
						db,
						Log,
						oLimitations => oLimitations.UpdateWebSiteCfg("Ezbob.Web")
					);

					DbConnectionPool.ReuseCount = CurrentValues.Instance.ConnectionPoolReuseCount;
					AConnection.UpdateConnectionPoolMaxSize(CurrentValues.Instance.ConnectionPoolMaxSize);

					Ezbob.RegistryScanner.Scanner.Register();

					ConfigureStructureMap(ObjectFactory.Container);
				} catch (Exception ex) {
					Log.Error(ex);
					throw;
				} // try

				_isInitialized = true;
			} // lock
		} // InitOnStart

		private static void RegisterGlobalFilters(GlobalFilterCollection filters) {
			const string webRole = "Web";

			var underwriterRoles = string.Join(", ", ObjectFactory.GetInstance<IRolesRepository>()
				.GetAll()
				.Where(x => x.Name != webRole)
				.Select(x => x.Name));

			if (CurrentValues.Instance.LandingPageEnabled)
				filters.Add(new WhiteListFilter(), 0);

			RoleCache roleCache = new RoleCache();

			filters.Add(
				new GlobalAreaAuthorizationFilter(
					roleCache,
					GlobalAreaAuthorizationFilter.AreaName.Underwriter,
					underwriterRoles
				),
				1
			);

			filters.Add(
				new GlobalAreaAuthorizationFilter(roleCache, GlobalAreaAuthorizationFilter.AreaName.Customer, webRole),
				1
			);

			filters.Add(new EzBobHandleErrorAttribute());

			filters.Add(new LoggingContextFilter(), 1);
		} // RegisterGlobalFilters

		// ReSharper disable RedundantDefaultFieldInitializer
		private static bool _isInitialized = false;
		// ReSharper restore RedundantDefaultFieldInitializer

		private static SafeILog log;
	} // class MvcApplication
} // namespace
