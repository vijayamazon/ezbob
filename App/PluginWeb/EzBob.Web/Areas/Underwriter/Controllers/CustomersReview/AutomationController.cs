namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview {
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using EzBob.Web.Areas.Underwriter.Models;
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
            List<AutoDecisionTrail> trails = this.autoDecisionTrailRepository.GetForCustomer(id).ToList();

            List<AutoDecisionModel> trailModels = trails.Select(x => new AutoDecisionModel {
            TrailID = x.TrailID,
            DecisionTime = x.DecisionTime,
            TrailTag = x.TrailTag,
            DecisionName = x.DecisionName,
            DecisionStatus = x.DecisionStatus
            }).ToList();

            return Json(new {
                trails = trailModels,
                current = trails.FirstOrDefault(),
            }, JsonRequestBehavior.AllowGet);
        }

        [Ajax]
        [HttpGet]
        public ActionResult GetTrail(long id) {
            var trail = this.autoDecisionTrailRepository.Get(id);
            
            return Json(new {
                current = trail,
            }, JsonRequestBehavior.AllowGet);
        }

    }
}