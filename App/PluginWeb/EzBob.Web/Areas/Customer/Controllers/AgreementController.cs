﻿namespace EzBob.Web.Areas.Customer.Controllers {
	using System;
	using System.Web.Mvc;
	using StructureMap;
	using EZBob.DatabaseLib;
	using CommonLib;
	using Code;
	using Infrastructure;
	using EzBob.Models.Agreements;
	using Ezbob.Logger;

	public class AgreementController : Controller {
		public AgreementController(
			AgreementRenderer agreementRenderer,
			IEzbobWorkplaceContext context,
			AgreementsModelBuilder builder,
			AgreementsTemplatesProvider templates,
			LoanBuilder loanBuilder
		) {
			_agreementRenderer = agreementRenderer;
			_context = context;
			_builder = builder;
			_templates = templates;
			_customer = _context.Customer;
			_loanBuilder = loanBuilder;
		} // constructor

		public ActionResult Download(decimal amount, string viewName, int loanType, int repaymentPeriod) {
			var oLog = new SafeILog(log4net.LogManager.GetLogger(GetType()));

			oLog.Debug("Download agreement: amount = {0}, view = {1}, loan type = {2}, repayment period = {3}", amount, viewName, loanType, repaymentPeriod);

			string file;

			try {
				file = _templates.GetTemplateByName(viewName);
			}
			catch (Exception e) {
				oLog.Debug(e, "Agreement template not found: amount = {0}, view = {1}, loan type = {2}, repayment period = {3}", amount, viewName, loanType, repaymentPeriod);
				return RedirectToAction("NotFound", "Error", new { Area = "" });
			} // try

			var lastCashRequest = _customer.LastCashRequest;

			if (_customer.IsLoanTypeSelectionAllowed == 1) {
				var oDBHelper = ObjectFactory.GetInstance<IDatabaseDataHelper>() as DatabaseDataHelper;
				lastCashRequest.RepaymentPeriod = repaymentPeriod;
				lastCashRequest.LoanType = oDBHelper.LoanTypeRepository.Get(loanType);
			} // if

			var loan = _loanBuilder.CreateLoan(lastCashRequest, amount, DateTime.UtcNow);

			var model = _builder.Build(_customer, amount, loan);
			var pdf = _agreementRenderer.RenderAgreementToPdf(file, model);
			return File(pdf, "application/pdf", viewName + " Summary_" + DateTime.Now + ".pdf");
		} // Download

		private readonly AgreementRenderer _agreementRenderer;
		private readonly IEzbobWorkplaceContext _context;
		private readonly AgreementsModelBuilder _builder;
		private readonly AgreementsTemplatesProvider _templates;
		private readonly LoanBuilder _loanBuilder;
		private readonly EZBob.DatabaseLib.Model.Database.Customer _customer;
	} // class AgreementController
} // namespace
