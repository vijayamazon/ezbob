using System.Web.Mvc;
using EzBob.Web.Infrastructure;
using StructureMap;

namespace EzBob.Web.Areas.Underwriter
{
	using ConfigManager;

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
            if (!CurrentValues.Instance.ManagementPartEnabled) return;
            context.MapRoute(
                "Underwriter_default",
                "Underwriter/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
