using System.Web.Optimization;

namespace EzTvDashboard
{
	public class BundleConfig
	{
		// For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
		public static void RegisterBundles(BundleCollection bundles)
		{
			bundles.Add(new ScriptBundle("~/bundles/js").Include("~/Content/js/*.js"));
			
			bundles.Add(new StyleBundle("~/Content/css")
				.Include("~/Content/Css/bootstrap.css")
				.Include("~/Content/Css/bootstrap-theme.css")
				.Include("~/Content/Css/dataTables.bootstrap.css")
				.Include("~/Content/Css/font-awesome.css")
				.Include("~/Content/Css/flaty.css")
				.Include("~/Content/Css/flaty-responsive.css")
				.Include("~/Content/Css/tvdashboard.css")
				);

		}
	}
}