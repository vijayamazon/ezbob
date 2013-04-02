using System.Web.Mvc;
using EzBob.Web.Infrastructure;
using StructureMap;

namespace EzBob.Web.Areas.Underwriter
{
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
            var config = ObjectFactory.GetInstance<IEzBobConfiguration>();
            if (!config.ManagementPartEnabled) return;
            context.MapRoute(
                "Underwriter_default",
                "Underwriter/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
