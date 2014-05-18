namespace EzBob.Web.Areas.Underwriter
{
	using System.Web.Mvc;
	using System.Configuration;

	public class UnderwriterAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get
			{
				return "Underwriter";
			}
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			if (!bool.Parse(ConfigurationManager.AppSettings["UnderwriterEnabled"])) return;
			context.MapRoute(
				"Underwriter_default",
				"Underwriter/{controller}/{action}/{id}",
				new { action = "Index", id = UrlParameter.Optional }
			);
		}
	}
}
