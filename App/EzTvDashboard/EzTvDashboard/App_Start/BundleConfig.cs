using System.Web.Optimization;

namespace EzTvDashboard
{
	public class BundleConfig
	{
		// For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
		public static void RegisterBundles(BundleCollection bundles)
		{
			bundles.Add(new ScriptBundle("~/bundles/js")
				.Include("~/Content/js/jquery.js")
				.Include("~/Content/js/underscore.js")
				.Include("~/Content/js/backbone.js")
				.Include("~/Content/js/jquery.*")
				.Include("~/Content/js/bootstrap.js")
				.Include("~/Content/js/dataTables.bootstrap.js")
				.IncludeDirectory("~/Content/js/jqplot", "*.js")
				.IncludeDirectory("~/Content/js/jqplot/plugins", "*.js")
				.Include("~/Content/js/flaty.js")
				.Include("~/Content/js/eztvdashboard.js")
				);
			
			bundles.Add(new StyleBundle("~/Content/css")
				.Include("~/Content/Css/bootstrap.css")
				.Include("~/Content/Css/bootstrap-theme.css")
				.Include("~/Content/Css/dataTables.bootstrap.css")
				.Include("~/Content/Css/font-awesome.css")
				.Include("~/Content/Css/flaty.css")
				.Include("~/Content/Css/flaty-responsive.css")
				.Include("~/Content/Css/jquery.plot.css")
				.Include("~/Content/Css/tvdashboard.css")
				);

		}
	}
}