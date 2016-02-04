namespace LegalDocs.Areas.Work {
    using System.Web.Mvc;

    public class WorkAreaRegistration : AreaRegistration {
        public override string AreaName {
            get { return "Work"; }
        }

        public override void RegisterArea(AreaRegistrationContext context) {
            context.MapRoute(
                "Work_default",
                "Work/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                new[] { "LegalDocs.Areas.Work.Controllers" }
                );

        }
    }
}
