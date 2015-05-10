﻿namespace EzBob.Web.Areas.Underwriter.Models
{
	using System;
	using System.Linq;
	using Code;
	using ConfigManager;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Loans;
	using EzBob.Models;
	using System.Text;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using Ezbob.Backend.Models;
	using Ezbob.Utils.Extensions;
	using EZBob.DatabaseLib.Model.Database.Request;
	using PaymentServices.Calculators;
	using Web.Models;

	public class ApplicationInfoModelBuilder
	{
		private readonly ILoanTypeRepository _loanTypes;
		private readonly IDiscountPlanRepository _discounts;
		private readonly IApprovalsWithoutAMLRepository approvalsWithoutAMLRepository;
		private readonly ILoanSourceRepository _loanSources;
	    private readonly IOfferCalculationsRepository offerCalculationsRepository;
		private readonly CustomerAnalyticsRepository _customerAnalyticsRepository;
		private readonly LoanBuilder _loanBuilder;
		private readonly APRCalculator _aprCalc;

		public ApplicationInfoModelBuilder(
			IApprovalsWithoutAMLRepository approvalsWithoutAMLRepository,
			IDiscountPlanRepository discounts,
			ILoanTypeRepository loanTypes,
			ILoanSourceRepository loanSources,
			CustomerAnalyticsRepository customerAnalyticsRepository,
			LoanBuilder loanBuilder,
			APRCalculator aprCalc, 
            IOfferCalculationsRepository offerCalculationsRepository)
		{
		    this._discounts = discounts;
		    this._loanTypes = loanTypes;
			this.approvalsWithoutAMLRepository = approvalsWithoutAMLRepository;
		    this._loanSources = loanSources;
		    this._customerAnalyticsRepository = customerAnalyticsRepository;
		    this._loanBuilder = loanBuilder;
		    this._aprCalc = aprCalc;
		    this.offerCalculationsRepository = offerCalculationsRepository;
		}

		public void InitApplicationInfo(ApplicationInfoModel model, Customer customer, CashRequest cr)
		{
			if (customer == null) return;

			model.Id = customer.Id;
			model.CustomerName = customer.PersonalInfo != null ? customer.PersonalInfo.Fullname : "";
			model.TypeOfBusiness = customer.PersonalInfo != null ? (int)customer.PersonalInfo.TypeOfBusiness : 0;
			model.CustomerRefNum = customer.RefNumber;

			if (cr != null)
			{
				BuildCashRequestModel(model, customer, cr);
			}

			var loanSource = new LoanSourceModel(cr != null && cr.LoanSource != null ? cr.LoanSource : this._loanSources.GetDefault());
		    model.LoanSourceID = loanSource.Id;
		    model.LoanSource = loanSource.Name;

			model.CustomerId = customer.Id;
			model.IsTest = customer.IsTest;
			model.IsOffline = customer.IsOffline;
			model.HasYodlee = customer.GetYodleeAccounts().Any();
			model.IsAvoid = customer.IsAvoid;
			model.SystemDecision = customer.Status.ToString();

			model.AvaliableAmount = customer.CreditSum ?? 0M;
			model.OfferExpired = customer.OfferValidUntil <= DateTime.UtcNow;

			var isWaitingOrEscalated = customer.CreditResult == CreditResultStatus.WaitingForDecision ||
									   customer.CreditResult == CreditResultStatus.Escalated || 
									   customer.CreditResult == CreditResultStatus.ApprovedPending;

			model.Editable = isWaitingOrEscalated && cr != null && customer.CollectionStatus.CurrentStatus.IsEnabled;

			model.IsModified = cr != null && !string.IsNullOrEmpty(cr.LoanTemplate);

			model.LoanTypes = this._loanTypes.GetAll().Select(t => LoanTypesModel.Create(t)).ToArray();

			model.DiscountPlans = this._discounts.GetAll().Select(d => DiscountPlanModel.Create(d)).ToArray();
			DiscountPlan discountPlan;

			if (cr != null && cr.DiscountPlan != null)
			{
				discountPlan = cr.DiscountPlan;
			}
			else
			{
				discountPlan = this._discounts.GetDefault();
			}

			model.DiscountPlan = discountPlan.Name;
			model.DiscountPlanPercents = discountPlan.Discounts.Any(d => d != 0) ? string.Format("({0})", discountPlan.ValuesStr) : "";
			model.DiscountPlanId = discountPlan.Id;

			model.AllLoanSources = this._loanSources.GetAll().Select(ls => new LoanSourceModel(ls)).ToArray();

			model.OfferValidForHours = (int)Math.Truncate((decimal)CurrentValues.Instance.OfferValidForHours);

			model.AMLResult = customer.AMLResult;
			model.SkipPopupForApprovalWithoutAML = this.approvalsWithoutAMLRepository.ShouldSkipById(customer.Id);

			var company = customer.Company;
			model.EmployeeCount = new CompanyEmployeeCount().EmployeeCount;
			if (company != null)
			{
				model.EmployeeCount = (company.CompanyEmployeeCount.OrderBy(x => x.Created).LastOrDefault() ?? new CompanyEmployeeCount()).EmployeeCount;
			}

			BuildRequestedLoan(model, customer);

		    BuildAutomationOfferModel(model, customer);
		}//InitApplicationInfo

	    private static void BuildRequestedLoan(ApplicationInfoModel model, Customer customer) {
	        CustomerRequestedLoan oRequest = customer.CustomerRequestedLoan.OrderBy(x => x.Created)
	            .LastOrDefault();

	        if ((oRequest == null) || (oRequest.CustomerReason == null)) {
	            model.CustomerReasonType = -1;
	            model.CustomerReason = "";
	        } else {
	            model.CustomerReasonType = oRequest.CustomerReason.ReasonType ?? -1;

	            var os = new StringBuilder();

	            if (!string.IsNullOrWhiteSpace(oRequest.CustomerReason.Reason))
	                os.Append(oRequest.CustomerReason.Reason.Trim());

	            if (!string.IsNullOrWhiteSpace(oRequest.OtherReason)) {
	                if (!string.IsNullOrWhiteSpace(oRequest.CustomerReason.Reason))
	                    os.Append(": ");

	                os.Append(oRequest.OtherReason.Trim());
	            } // if

	            model.CustomerReason = os.ToString()
	                .Trim();
	        } // if
	    }//BuildRequestedLoan

	    private void BuildCashRequestModel(ApplicationInfoModel model, Customer customer, CashRequest cr) {
	        model.InterestRate = cr.InterestRate;
	        model.CashRequestId = cr.Id;
	     
	        model.ManualSetupFeePercent = cr.ManualSetupFeePercent;
	        model.BrokerSetupFeePercent = cr.BrokerSetupFeePercent;

	        model.AllowSendingEmail = !cr.EmailSendingBanned;

	        var loanType = cr.LoanType ?? this._loanTypes.GetDefault();

	        model.LoanType = loanType.Name;
	        model.LoanTypeId = loanType.Id;

	        cr.OfferStart = cr.OfferStart ?? customer.OfferStart;
	        cr.OfferValidUntil = cr.OfferValidUntil ?? customer.OfferValidUntil;

	        model.RepaymentPerion = cr.RepaymentPeriod;

	        if (cr.SystemCalculatedSum.HasValue && Math.Abs(cr.SystemCalculatedSum.Value) > 0.01)
	            model.SystemCalculatedAmount = Convert.ToDecimal(cr.SystemCalculatedSum.Value);

	        model.OfferedCreditLine = Convert.ToDecimal(cr.ManagerApprovedSum ?? cr.SystemCalculatedSum);

            var calc = new SetupFeeCalculator(cr.ManualSetupFeePercent, cr.BrokerSetupFeePercent);
            model.TotalSetupFee = calc.Calculate(model.OfferedCreditLine);
            model.BrokerSetupFee = calc.CalculateBrokerFee(model.OfferedCreditLine);
            model.TotalSetupFeePercent = model.OfferedCreditLine == 0 ? 0 : model.TotalSetupFee / model.OfferedCreditLine;
            model.BrokerSetupFeeActualPercent = model.OfferedCreditLine == 0 ? 0 : model.BrokerSetupFee / model.OfferedCreditLine;
            model.SetupFee = model.TotalSetupFee - model.BrokerSetupFee;
            model.SetupFeeActualPercent = model.OfferedCreditLine == 0 ? 0 : model.SetupFee / model.OfferedCreditLine;

	        model.BorrowedAmount = customer.Loans.Where(x => x.CashRequest != null && x.CashRequest.Id == cr.Id)
	            .Sum(x => x.LoanAmount);

	        model.StartingFromDate = FormattingUtils.FormatDateToString(cr.OfferStart);
	        model.OfferValidateUntil = FormattingUtils.FormatDateToString(cr.OfferValidUntil);

	        model.Reason = cr.UnderwriterComment;

	        model.IsLoanTypeSelectionAllowed = cr.IsLoanTypeSelectionAllowed;
            model.IsCustomerRepaymentPeriodSelectionAllowed = cr.IsCustomerRepaymentPeriodSelectionAllowed;

	        model.AnnualTurnover = cr.AnnualTurnover;

	        var loan = this._loanBuilder.CreateLoan(cr, cr.ApprovedSum(), cr.OfferStart ?? DateTime.UtcNow);

	        var apr = loan.LoanAmount == 0 ? 0 : this._aprCalc.Calculate(loan.LoanAmount, loan.Schedule, loan.SetupFee, loan.Date);

	        var loanOffer = LoanOffer.InitFromLoan(loan, apr, null, cr);
	        model.Apr = apr;
	        model.Air = (model.InterestRate * 100 * 12 + (model.RepaymentPerion == 0 ? 0 : (12 / (decimal)model.RepaymentPerion * model.TotalSetupFee * 100))) / 100;
	        model.RealCost = loanOffer.RealInterestCost;
	    }// BuildCashRequestModel

	    private void BuildAutomationOfferModel(ApplicationInfoModel model, Customer customer) {
	        OfferCalculations offerCalculations = this.offerCalculationsRepository.GetActiveForCustomer(customer.Id);
	        if (offerCalculations != null) {
	            model.AutomationOfferModel = new AutomationOfferModel {
	                Amount = offerCalculations.Amount,
	                InterestRate = offerCalculations.InterestRate / 100,
	                RepaymentPeriod = offerCalculations.Period,
	                SetupFeePercent = offerCalculations.SetupFee / 100,
	                SetupFeeAmount = offerCalculations.Amount * offerCalculations.SetupFee / 100
	            };
	        } else
	            model.AutomationOfferModel = new AutomationOfferModel();
	    }//BuildAutomationOfferModel
	}
}