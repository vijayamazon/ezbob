﻿namespace EzBob.Web.Areas.Customer.Controllers {
	 using System;
	 using System.Web.Mvc;
	 using EZBob.DatabaseLib;
	 using EZBob.DatabaseLib.Model.Database;
	 using CommonLib;
	 using Code;
	 using Infrastructure;
	 using Infrastructure.Attributes;
	 using Infrastructure.csrf;
	 using PaymentServices.Calculators;
	 using StructureMap;
	 using Web.Models;

	 public class ScheduleController : Controller {
		 private readonly APRCalculator aprCalc;
		 private readonly Customer customer;
		 private readonly LoanBuilder loanBuilder;
		 protected static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(ScheduleController));

		 public ScheduleController(IEzbobWorkplaceContext context, LoanBuilder loanBuilder) {
			 this.loanBuilder = loanBuilder;
			 this.customer = context.Customer;
			 this.aprCalc = new APRCalculator();
		 } // constructor

		 [Ajax]
		 [HttpGet]
		 [ValidateJsonAntiForgeryToken]
		 [Transactional]
		 public JsonResult Calculate(int amount, int loanType, int repaymentPeriod) {
			 // el: slider of offer display (customer app)
			 LoanOffer loanOffer = CalculateLoan(amount, loanType, repaymentPeriod);

			 if (loanOffer == null)
				 return Json(new { error = "Invalid customer state" }, JsonRequestBehavior.AllowGet);

			 return Json(loanOffer, JsonRequestBehavior.AllowGet);
		 } // Calculate

		 private LoanOffer CalculateLoan(int amount, int loanType, int repaymentPeriod) {
			 if (!this.customer.CreditSum.HasValue || !this.customer.Status.HasValue || this.customer.Status.Value != Status.Approved)
				 return null;

			 var creditSum = this.customer.CreditSum.Value;

			 this.customer.ValidateOfferDate();

			 if (amount < 0)
				 amount = (int)Math.Floor(creditSum);

			 if (amount > creditSum) {
				 Log.WarnFormat("Attempt to calculate schedule for amount({0}) bigger than credit sum value({1})", amount, creditSum);
				 amount = (int)Math.Floor(creditSum);
			 } // if

			 var cr = this.customer.LastCashRequest;

			 if (this.customer.IsLoanTypeSelectionAllowed == 1) {
				 var oDBHelper = ObjectFactory.GetInstance<IDatabaseDataHelper>() as DatabaseDataHelper;

				 if (oDBHelper != null) {

					 cr.LoanType = oDBHelper.LoanTypeRepository.Get(loanType);
				 } // if
			 } // if

			 if (cr.IsCustomerRepaymentPeriodSelectionAllowed) {
				 cr.RepaymentPeriod = repaymentPeriod;
			 }

			 var loan = this.loanBuilder.CreateLoan(cr, amount, DateTime.UtcNow);

			 var schedule = loan.Schedule;
			 var apr = this.aprCalc.Calculate(amount, schedule, loan.SetupFee, loan.Date);

			 var b = new AgreementsModelBuilder();
			 var agreement = b.Build(this.customer, amount, loan);

			 //TODO calculate offer
			 Log.DebugFormat("calculate offer for customer {0}", this.customer.Id);

			 return LoanOffer.InitFromLoan(loan, apr, agreement, cr);
		 } // CalculateLoan
	 } // class ScheduleController
 } // namespace
