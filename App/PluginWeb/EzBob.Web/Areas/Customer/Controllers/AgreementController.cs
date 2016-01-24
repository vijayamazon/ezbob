namespace EzBob.Web.Areas.Customer.Controllers {
    using System;
    using System.Linq;
    using System.Web.Mvc;
    using Ezbob.Backend.ModelsWithDB.LegalDocs;
    using Ezbob.Logger;
    using EzBob.CommonLib;
    using EzBob.Web.Code;
    using EzBob.Web.Infrastructure;
    using EZBob.DatabaseLib;
    using EZBob.DatabaseLib.Model.Database;
    using log4net;
    using ServiceClientProxy;
    using StructureMap;

    public class AgreementController : Controller {
		public AgreementController(
			AgreementRenderer agreementRenderer,
			IEzbobWorkplaceContext context,
			AgreementsModelBuilder builder,
			LoanBuilder loanBuilder
		) {
			this.agreementRenderer = agreementRenderer;
			this.context = context;
			this.builder = builder;
			this.customer = this.context.Customer;
			this.loanBuilder = loanBuilder;
            this.serviceClient = new ServiceClient();
		}// constructor

		public ActionResult Download(decimal amount, string viewName, int loanType, int repaymentPeriod) {
			var oLog = new SafeILog(LogManager.GetLogger(GetType()));

			oLog.Info("Download agreement: amount = {0}, view = {1}, loan type = {2}, repayment period = {3}", amount, viewName, loanType, repaymentPeriod);

			var lastCashRequest = this.customer.LastCashRequest;

			if (this.customer.IsLoanTypeSelectionAllowed == 1) {
				var oDBHelper = ObjectFactory.GetInstance<IDatabaseDataHelper>() as DatabaseDataHelper;
				lastCashRequest.RepaymentPeriod = repaymentPeriod;
				lastCashRequest.LoanType = oDBHelper.LoanTypeRepository.Get(loanType);
			} // if

			var loan = this.loanBuilder.CreateLoan(lastCashRequest, amount, DateTime.UtcNow);

			var model = this.builder.Build(this.customer, amount, loan);

            var productSubTypeID = loan.CashRequest.ProductSubTypeID;
            var originId = loan.CashRequest.Customer.CustomerOrigin.CustomerOriginID;
		    var isRegulated = this.customer.PersonalInfo.TypeOfBusiness.IsRegulated();

            LoanAgreementTemplate loanAgreementTemplate = this.serviceClient.Instance.GetLegalDocs(this.customer.Id, this.context.UserId, originId, isRegulated, productSubTypeID ?? 0).LoanAgreementTemplates.FirstOrDefault(x => x.TemplateTypeName == viewName);
		    if (loanAgreementTemplate != null) {
		        var template = loanAgreementTemplate.Template;
                var pdf = this.agreementRenderer.RenderAgreementToPdf(template, model);
                return File(pdf, "application/pdf", viewName + " Summary_" + DateTime.Now + ".pdf");
		    }
		    return null;
		} // Download

		private readonly AgreementRenderer agreementRenderer;
		private readonly IEzbobWorkplaceContext context;
		private readonly AgreementsModelBuilder builder;
        private readonly LoanBuilder loanBuilder;
		private readonly Customer customer;
        private readonly ServiceClient serviceClient;
    } // class AgreementController
} // namespace
