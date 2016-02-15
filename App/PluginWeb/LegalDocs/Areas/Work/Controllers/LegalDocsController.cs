namespace LegalDocs.Areas.Work.Controllers {
    using System;
    using System.Web.Mvc;
    using System.Web.Script.Serialization;
    using Ezbob.Backend.ModelsWithDB.LegalDocs;
    using EzBob.Backend.Models;
    using EzBob.Web.Code;
    using LegalDocs.Code.Filters;
    using LegalDocs.Properties;
    using ServiceClientProxy;

    public class LegalDocsController : Controller {

        public ServiceClient serviceClient = new ServiceClient();


        public ViewResult Index() {
            return View();
        } // Index


        [Permission(Name= "LegalDocsApprove,LegalDocsReview")]
        public ActionResult LegalDocs() {
            return View();
        }

        [HttpPost]
        [Permission(Name= "LegalDocsApprove,LegalDocsReview")]
        public ActionResult AddLegalDoc(LoanAgreementTemplate loanAgreementTemplate) {

            loanAgreementTemplate.Id = 0;
            loanAgreementTemplate.IsApproved = false;
            loanAgreementTemplate.IsReviewed = false;
            loanAgreementTemplate.ReleaseDate = DateTime.UtcNow;

            var data = this.serviceClient.Instance.AddLegalDoc(1, 1, loanAgreementTemplate).LoanAgreementTemplate;

            var result = new { Success = "True", Data = data };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Permission(Name= "LegalDocsApprove,LegalDocsReview")]
        public ActionResult SaveLegalDoc(LoanAgreementTemplate loanAgreementTemplate) {

            loanAgreementTemplate.ReleaseDate = DateTime.UtcNow;

            var data = this.serviceClient.Instance.SaveLegalDoc(1, 1, loanAgreementTemplate).Value;

            var result = new { Success = "True", Data = data };
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [Permission(Name= "LegalDocsApprove,LegalDocsReview")]
        public ActionResult ReviewLegalDoc(int loanAgreementTemplateId) {
            this.serviceClient.Instance.ReviewLegalDoc(1, 1, loanAgreementTemplateId);
            var result = new { Success = "True" };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Permission(Name= "LegalDocsApprove,LegalDocsReview")]
        public ActionResult ApproveLegalDoc(int loanAgreementTemplateId) {
            this.serviceClient.Instance.ApproveLegalDoc(1, 1, loanAgreementTemplateId);
            var result = new { Success = "True" };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Permission(Name= "LegalDocsApprove,LegalDocsReview")]
        public ActionResult DeleteLegalDoc(int loanAgreementTemplateId) {
            this.serviceClient.Instance.DeleteLegalDoc(1, 1, loanAgreementTemplateId);
            var result = new { Success = "True" };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Permission(Name= "LegalDocsApprove,LegalDocsReview")]
        public ActionResult GetLatestLegalDocs() {
            var data = this.serviceClient.Instance.GetLatestLegalDocs(1, 1);
            var result = new { Success = "True", Data = data.LoanAgreementTemplates };
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        [Permission(Name = "LegalDocsApprove,LegalDocsReview")]
        public ActionResult GetLegalDocsPendingApproval() {
            this.serviceClient.Instance.GetLegalDocsPendingApproval(1, 1);
            var result = new { Success = "True" };
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [Permission(Name= "LegalDocsApprove,LegalDocsReview")]
        public ActionResult SyncLegalDocsEnviorments() {
            var result = new { Success = "True" };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Permission(Name= "LegalDocsApprove,LegalDocsReview")]
        public ActionResult UpdateCurrentTemplate(string template) {
            Session.Add("Template", template);
            var result = new { Success = "True" };
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [Permission(Name= "LegalDocsApprove,LegalDocsReview")]
        public ActionResult Download() {
            AgreementRenderer agreementRenderer = new AgreementRenderer();

            string template = (string)Session["Template"];

            JavaScriptSerializer jss = new JavaScriptSerializer();
            AgreementModel model  = jss.Deserialize<AgreementModel>(GetDummyAgreementModel()); 

            var pdf = agreementRenderer.RenderAgreementToPdf(template, model);
            return File(pdf, "application/pdf", "test" + " Summary_" + DateTime.Now + ".pdf");
        } // Download

            private string GetDummyAgreementModel() {
                return Resources.Dummy;
            }

    }
}