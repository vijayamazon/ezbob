namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview {
    using System.Collections.Generic;
    using System.Web.Mvc;
    using Infrastructure.Attributes;
    using EZBob.DatabaseLib.Model.Database.Request;

    public class AutomationController : Controller {
        private readonly AutoDecisionTrailRepository autoDecisionTrailRepository;
        public AutomationController(AutoDecisionTrailRepository autoDecisionTrailRepository) {
            this.autoDecisionTrailRepository = autoDecisionTrailRepository;
        }

        [Ajax]
        [HttpGet]
        public ActionResult Index(int id) {
            IEnumerable<AutoDecisionTrail> trails = this.autoDecisionTrailRepository.GetForCustomer(id);
            return Json(new { trails = trails }, JsonRequestBehavior.AllowGet);
        }

    }
}