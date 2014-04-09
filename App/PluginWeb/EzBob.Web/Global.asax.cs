namespace EzBob.Web
{
	using System;
	using System.Globalization;
	using System.Linq;
	using System.Net;
	using System.Reflection;
	using System.Text.RegularExpressions;
	using System.Web;
	using System.Web.Mvc;
	using System.Web.Routing;
	using ApplicationMng.Model;
	using ApplicationMng.Repository;
	using Aspose.Cells;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using Areas.Underwriter.Models.Reports;
	using Infrastructure;
	using Infrastructure.Filters;
	using Models;
	using SquishIt.Less;
	using SquishIt.MsIeCoffeeScript;
	using SquishIt.Framework;
	using StructureMap;
	using NHibernate;
	using Scorto.Configuration;
	using NHibernateWrapper.NHibernate;
	using log4net;
	using log4net.Config;
	using NHibernateWrapper.Web;

	public class MvcApplication : HttpApplication
	{
		private static readonly ILog _log = LogManager.GetLogger(typeof(MvcApplication));

		private static bool _isInitialized = false;

		public static ISession CurrentSession
		{
			get
			{
				try
				{
					if (HttpContext.Current.Items["current.session"] as ISession == null)
					{
						HttpContext.Current.Items["current.session"] = NHibernateManager.OpenSession();
					}
					return HttpContext.Current.Items["current.session"] as ISession;
				}
				catch (Exception ex)
				{
					_log.Error(ex);
					throw;
				}
			}
		}

		public MvcApplication()
		{
			EndRequest += (sender, args) =>
				{
					var session = HttpContext.Current.Items["current.session"] as ISession;
					if (session != null)
					{
						if (session.IsOpen)
						{
							CurrentSession.Close();
						}
						CurrentSession.Dispose();
					}
					HttpContext.Current.Items["current.session"] = null;
					ThreadContext.Properties.Clear();
				};

			InitOnStart();
		}


		public virtual void InitOnStart()
		{
			base.Init();

			if (_isInitialized) return;

			lock (this)
			{
				if (_isInitialized) return;

				try
				{
					var configuration = ConfigurationRoot.GetConfiguration();
					XmlConfigurator.Configure(configuration.XmlElementLog);

					if (configuration.NHProfEnabled)
					{
						HibernatingRhinos.Profiler.Appender.NHibernate.NHibernateProfiler.Initialize();
					}

					// Scorto.RegistryScanner.Scanner.Register();
					Ezbob.RegistryScanner.Scanner.Register();

					ConfigureStructureMap(ObjectFactory.Container);
				}
				catch (Exception ex)
				{
					_log.Error(ex);
					throw;
				}

				_isInitialized = true;
			}
		}

		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			var config = ObjectFactory.GetInstance<IEzBobConfiguration>();

			var underwriterRoles = string.Join(", ", ObjectFactory.GetInstance<IRolesRepository>().GetAll().Where(x => x.Name != "Web").Select(x => x.Name));

			if (config.LandingPageEnabled)
			{
				filters.Add(new WhiteListFilter(), 0);
			}

			filters.Add(new GlobalAreaAuthorizationFilter("Underwriter", underwriterRoles, true), 1);
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

			routes.Insert(0, new Route("pie.htc", new RouteValueDictionary(new { controller = "Pie", action = "Index" }), new MvcRouteHandler()));
			routes.Insert(0, new Route("{area}/pie.htc", new RouteValueDictionary(new { controller = "Pie", action = "Index", area = "" }), new MvcRouteHandler()));
			//routes.MapRoute(null, "{Area}/{c}/{a}/pie.htc", new {controller = "Pie", action = "Index", c = "", a="", Area=""});
		}

		protected void Application_Start()
		{
			MvcHandler.DisableMvcResponseHeader = true;

			AreaRegistration.RegisterAllAreas();
			RegisterRoutes(RouteTable.Routes);

			ModelBinders.Binders.Add(typeof(DayDate), new DayDateModelBinder());
			ModelBinders.Binders.Add(typeof(DateTime), new Iso8601DateTimeBinder());
			ModelBinders.Binders.Add(typeof(DateTime?), new Iso8601DateTimeBinder());
			ModelBinders.Binders.Add(typeof(decimal), new DecimalModelBinder());
			ModelBinders.Binders.Add(typeof(decimal?), new DecimalModelBinder());

			NHibernateWrapper.NHibernate.NHibernateManager.FluentAssemblies.Add(typeof(Application).Assembly);
			NHibernateWrapper.NHibernate.NHibernateManager.FluentAssemblies.Add(typeof(Customer).Assembly);
			NHibernateWrapper.NHibernate.NHibernateManager.FluentAssemblies.Add(typeof(DbString).Assembly);
			NHibernateWrapper.NHibernate.NHibernateManager.HbmAssemblies.Add(typeof(PerformencePerUnderwriterDataRow).Assembly);
			ControllerBuilder.Current.SetControllerFactory(new StructureMapControllerFactory());
			InitAspose();
			ConfigureSquishIt();

			var bs = ObjectFactory.GetInstance<MarketPlacesBootstrap>();
			bs.InitValueTypes();
			bs.InitDatabaseMarketPlaceTypes();

			RegisterGlobalFilters(GlobalFilters.Filters);
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

		protected void ConfigureStructureMap(IContainer container)
		{
			container.Configure(x => x.For<IWorkplaceContext>().Use(GetContext));
			container.Configure(x => x.AddRegistry<PluginWebRegistry>());
			container.Configure(x => x.AddRegistry<PaymentServices.PacNet.PacnetRegistry>());
			container.Configure(x =>
			{
				x.For<ISession>().Use(() => CurrentSession);
			});
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