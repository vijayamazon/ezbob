namespace EzBob.Web.Areas.Customer.Controllers {
	using System;
	using System.Linq;
	using System.Web.Mvc;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using CommonLib;
	using Code;
	using Ezbob.Logger;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using Infrastructure;
	using Infrastructure.Attributes;
	using Infrastructure.csrf;
	using PaymentServices.Calculators;
	using ServiceClientProxy;
	using StructureMap;
	using Web.Models;

	public class ScheduleController : Controller {
		public ScheduleController(IEzbobWorkplaceContext context, LoanBuilder loanBuilder, ILoanLegalRepository llrepo) {
			this.context = context;
			this.loanBuilder = loanBuilder;
			this.aprCalc = new APRCalculator();
			this.serviceClient = new ServiceClient();
			this.loanLegalRepo = llrepo;
		} // constructor

		[Ajax]
		[HttpGet]
		[ValidateJsonAntiForgeryToken]
		// [Transactional]
		public JsonResult Calculate(int amount, int loanType, int repaymentPeriod) {
			log.Debug(
				"Calculating schedule for customer {0}: {1} of type {2} for {3} repayments.",
				this.context.Customer.Stringify(),
				amount.ToString("C0"),
				loanType,
				repaymentPeriod
			);

			// el: slider of offer display (customer app)
			LoanOffer loanOffer = CalculateLoan(amount, loanType, repaymentPeriod);

			log.Debug(
				"Calculating schedule for customer {0}: {1} of type {2} for {3} repayments - loan offer is {4}.",
				this.context.Customer.Stringify(),
				amount.ToString("C0"),
				loanType,
				repaymentPeriod,
				loanOffer == null ? "-- null --" : "-- not null --"
			);

			if (loanOffer == null)
				return Json(new { error = "Invalid customer state" }, JsonRequestBehavior.AllowGet);

			log.Debug(
				"Calculating schedule for customer {0}: {1} of type {2} for {3} repayments - hunting for legal docs...",
				this.context.Customer.Stringify(),
				amount.ToString("C0"),
				loanType,
				repaymentPeriod
			);

			var productSubTypeID = this.context.Customer.LastCashRequest.ProductSubTypeID;
			var originId = this.context.Customer.CustomerOrigin.CustomerOriginID;
			var isRegulated = this.context.Customer.PersonalInfo.TypeOfBusiness.IsRegulated();

			loanOffer.Templates = this.serviceClient.Instance.GetLegalDocs(
				this.context.UserId,
				this.context.UserId,
				originId,
				isRegulated,
				productSubTypeID ?? 0
			).LoanAgreementTemplates.ToList();

			log.Debug(
				"Completed calculating schedule for customer {0}: {1} of type {2} for {3} repayments.",
				this.context.Customer.Stringify(),
				amount.ToString("C0"),
				loanType,
				repaymentPeriod
			);

			return Json(loanOffer, JsonRequestBehavior.AllowGet);
		} // Calculate

		private LoanOffer CalculateLoan(int amount, int loanType, int repaymentPeriod) {
			if (!this.context.Customer.CreditSum.HasValue) {
				log.Warn(
					"Cannot calculate loan ({0} of type {1} for {2} repayments) for customer {3}: credit sum is null.",
					amount.ToString("C0"),
					loanType,
					repaymentPeriod,
					this.context.Customer.Stringify()
				);
				return null;
			} // if

			if (this.context.Customer.Status != Status.Approved) {
				log.Warn(
					"Cannot calculate loan ({0} of type {1} for {2} repayments) for customer {3}: customer status is '{4}'.",
					amount.ToString("C0"),
					loanType,
					repaymentPeriod,
					this.context.Customer.Stringify(),
					this.context.Customer.Status
				);

				return null;
			} // if

			var creditSum = this.context.Customer.CreditSum.Value;

			this.context.Customer.ValidateOfferDate();

			if (amount < 0)
				amount = (int)Math.Floor(creditSum);

			if (amount > creditSum) {
				log.Warn(
					"Attempt to calculate schedule for amount({0}) bigger than credit sum value({1})",
					amount,
					creditSum
				);
				amount = (int)Math.Floor(creditSum);
			} // if

			var cr = this.context.Customer.LastCashRequest;

			if (this.context.Customer.IsLoanTypeSelectionAllowed == 1) {
				var oDBHelper = ObjectFactory.GetInstance<IDatabaseDataHelper>() as DatabaseDataHelper;

				if (oDBHelper != null)
					cr.LoanType = oDBHelper.LoanTypeRepository.Get(loanType);
			} // if

			if (cr.IsCustomerRepaymentPeriodSelectionAllowed)
				cr.RepaymentPeriod = repaymentPeriod;

			var loan = this.loanBuilder.CreateLoan(cr, amount, DateTime.UtcNow);

			var schedule = loan.Schedule;
			var apr = this.aprCalc.Calculate(amount, schedule, loan.SetupFee, loan.Date);

			var b = new AgreementsModelBuilder(this.loanLegalRepo);
			var agreement = b.Build(this.context.Customer, amount, loan);

			// TODO calculate offer
			log.Debug("calculate offer for customer {0}", this.context.Customer.Id);

			return LoanOffer.InitFromLoan(loan, apr, agreement, cr);
		} // CalculateLoan

		private readonly ILoanLegalRepository loanLegalRepo;
		private readonly APRCalculator aprCalc;
		private readonly IEzbobWorkplaceContext context;
		private readonly LoanBuilder loanBuilder;
		private readonly ServiceClient serviceClient;

		private static readonly ASafeLog log = new SafeILog(typeof(ScheduleController));
	} // class ScheduleController
} // namespace
