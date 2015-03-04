namespace EzBob.Web.Areas.Underwriter.Models
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
	using PaymentServices.Calculators;
	using Web.Models;

	public class ApplicationInfoModelBuilder
	{
		private readonly ILoanTypeRepository _loanTypes;
		private readonly IDiscountPlanRepository _discounts;
		private readonly IApprovalsWithoutAMLRepository approvalsWithoutAMLRepository;
		private readonly ILoanSourceRepository _loanSources;
		private readonly VatReturnSummaryRepository _vatReturnSummaryRepository;
		private readonly CustomerAnalyticsRepository _customerAnalyticsRepository;
		private readonly LoanBuilder _loanBuilder;
		private readonly APRCalculator _aprCalc;

		public ApplicationInfoModelBuilder(
			IApprovalsWithoutAMLRepository approvalsWithoutAMLRepository,
			IDiscountPlanRepository discounts,
			ILoanTypeRepository loanTypes,
			ILoanSourceRepository loanSources,
			VatReturnSummaryRepository vatReturnSummaryRepository,
			CustomerAnalyticsRepository customerAnalyticsRepository,
			LoanBuilder loanBuilder,
			APRCalculator aprCalc)
		{
			_discounts = discounts;
			_loanTypes = loanTypes;
			this.approvalsWithoutAMLRepository = approvalsWithoutAMLRepository;
			_loanSources = loanSources;
			_vatReturnSummaryRepository = vatReturnSummaryRepository;
			_customerAnalyticsRepository = customerAnalyticsRepository;
			_loanBuilder = loanBuilder;
			_aprCalc = aprCalc;
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
				model.InterestRate = cr.InterestRate;
				model.CashRequestId = cr.Id;
				model.UseSetupFee = cr.UseSetupFee;
				model.UseBrokerSetupFee = cr.UseBrokerSetupFee;
				model.ManualSetupFeeAmount = cr.ManualSetupFeeAmount;
				model.ManualSetupFeePercent = cr.ManualSetupFeePercent;
				model.AllowSendingEmail = !cr.EmailSendingBanned;

				var loanType = cr.LoanType ?? _loanTypes.GetDefault();

				model.LoanType = loanType.Name;
				model.LoanTypeId = loanType.Id;

				cr.OfferStart = cr.OfferStart ?? customer.OfferStart;
				cr.OfferValidUntil = cr.OfferValidUntil ?? customer.OfferValidUntil;

				model.RepaymentPerion = cr.RepaymentPeriod;//_repaymentCalculator.ReCalculateRepaymentPeriod(cr);

				if (cr.SystemCalculatedSum.HasValue && Math.Abs(cr.SystemCalculatedSum.Value) > 0.01)
				{
					model.SystemCalculatedAmount = Convert.ToDecimal(cr.SystemCalculatedSum.Value);
				}

				model.OfferedCreditLine = Convert.ToDecimal(cr.ManagerApprovedSum ?? cr.SystemCalculatedSum);

				var calc = new SetupFeeCalculator(cr.UseSetupFee, cr.UseBrokerSetupFee, cr.ManualSetupFeeAmount, cr.ManualSetupFeePercent);
				model.SetupFee = calc.Calculate(model.OfferedCreditLine);
				model.SetupFeePercent = model.OfferedCreditLine == 0 ? 0 : model.SetupFee / model.OfferedCreditLine;
				model.BorrowedAmount = customer.Loans.Where(x => x.CashRequest != null && x.CashRequest.Id == cr.Id).Sum(x => x.LoanAmount);

				model.StartingFromDate = FormattingUtils.FormatDateToString(cr.OfferStart);
				model.OfferValidateUntil = FormattingUtils.FormatDateToString(cr.OfferValidUntil);

				model.Reason = cr.UnderwriterComment;

				model.IsLoanTypeSelectionAllowed = cr.IsLoanTypeSelectionAllowed;

				model.AnnualTurnover = cr.AnnualTurnover;

				var loan = _loanBuilder.CreateLoan(cr, cr.ApprovedSum(), cr.OfferStart.HasValue ? cr.OfferStart.Value : DateTime.UtcNow);
				
				var apr = loan.LoanAmount == 0 ? 0 : _aprCalc.Calculate(loan.LoanAmount, loan.Schedule, loan.SetupFee, loan.Date);

				var loanOffer = LoanOffer.InitFromLoan(loan, apr, null, cr);
				model.Apr = apr;
				model.Air = (model.InterestRate*100*12 + (model.RepaymentPerion == 0 ? 0 : (12/(decimal) model.RepaymentPerion*model.SetupFeePercent*100))) / 100;
				model.RealCost = loanOffer.RealInterestCost;
			}

			model.LoanSource = new LoanSourceModel(cr != null && cr.LoanSource != null ? cr.LoanSource : _loanSources.GetDefault());

			model.CustomerId = customer.Id;
			model.IsTest = customer.IsTest;
			model.IsOffline = customer.IsOffline;
			model.HasYodlee = customer.GetYodleeAccounts().ToList().Any();
			model.IsAvoid = customer.IsAvoid;
			model.SystemDecision = customer.Status.ToString();

			model.AvaliableAmount = customer.CreditSum ?? 0M;
			model.OfferExpired = customer.OfferValidUntil <= DateTime.UtcNow;

			//Status = "Active";
			model.Details = customer.Details;
			var isWaitingOrEscalated = customer.CreditResult == CreditResultStatus.WaitingForDecision ||
									   customer.CreditResult == CreditResultStatus.Escalated || 
									   customer.CreditResult == CreditResultStatus.ApprovedPending;

			model.Editable = isWaitingOrEscalated && cr != null && customer.CollectionStatus.CurrentStatus.IsEnabled;

			model.IsModified = cr != null && !string.IsNullOrEmpty(cr.LoanTemplate);

			model.LoanTypes = _loanTypes.GetAll().Select(t => LoanTypesModel.Create(t)).ToArray();

			model.DiscountPlans = _discounts.GetAll().Select(d => DiscountPlanModel.Create(d)).ToArray();
			DiscountPlan discountPlan;

			if (cr != null && cr.DiscountPlan != null)
			{
				discountPlan = cr.DiscountPlan;
			}
			else
			{
				discountPlan = _discounts.GetDefault();
			}

			model.DiscountPlan = discountPlan.Name;
			model.DiscountPlanPercents = discountPlan.Discounts.Any(d => d != 0) ? string.Format("({0})", discountPlan.ValuesStr) : "";
			model.DiscountPlanId = discountPlan.Id;

			model.AllLoanSources = _loanSources.GetAll().Select(ls => new LoanSourceModel(ls)).ToArray();

			model.OfferValidForHours = (int)Math.Truncate((decimal)CurrentValues.Instance.OfferValidForHours);

			model.AMLResult = customer.AMLResult;
			model.SkipPopupForApprovalWithoutAML = approvalsWithoutAMLRepository.ShouldSkipById(customer.Id);

			var company = customer.Company;
			model.EmployeeCount = new CompanyEmployeeCount().EmployeeCount;
			if (company != null)
			{
				model.EmployeeCount = (company.CompanyEmployeeCount.OrderBy(x => x.Created).LastOrDefault() ?? new CompanyEmployeeCount()).EmployeeCount;
			}

			CustomerRequestedLoan oRequest = customer.CustomerRequestedLoan.OrderBy(x => x.Created).LastOrDefault();

			if ((oRequest == null) || (oRequest.CustomerReason == null))
			{
				model.CustomerReasonType = -1;
				model.CustomerReason = "";
			}
			else
			{
				model.CustomerReasonType = oRequest.CustomerReason.ReasonType ?? -1;

				var os = new StringBuilder();

				if (!string.IsNullOrWhiteSpace(oRequest.CustomerReason.Reason))
					os.Append(oRequest.CustomerReason.Reason.Trim());

				if (!string.IsNullOrWhiteSpace(oRequest.OtherReason))
				{
					if (!string.IsNullOrWhiteSpace(oRequest.CustomerReason.Reason))
						os.Append(": ");

					os.Append(oRequest.OtherReason.Trim());
				} // if

				model.CustomerReason = os.ToString().Trim();
			} // if

			if (customer.CustomerMarketPlaces.Any(x => x.Marketplace.Name == "HMRC"))
			{
				var hmrc = customer.CustomerMarketPlaces.First(x => x.Marketplace.Name == "HMRC").Id;
				var summary = _vatReturnSummaryRepository.GetLastSummary(hmrc);
				if (summary != null)
				{
					model.ValueAdded = summary.TotalValueAdded;
					model.FreeCashFlow = summary.FreeCashFlow;
				}
			}

			var analytics = _customerAnalyticsRepository.Get(customer.Id);
			if (analytics != null)
			{
				model.Turnover = analytics.AnnualTurnover;
			}

			model.SuggestedAmounts = new[]
				{
					new SuggestedAmountModel
						{
							Method = CalculationMethod.Turnover.DescriptionAttr(),
							Silver = 0.06M,
							Gold = 0.08M,
							Platinum = 0.1M,
							Diamond = 0.12M,
							Value = model.Turnover
						},

					new SuggestedAmountModel
						{
							Method = CalculationMethod.ValueAdded.DescriptionAttr(),
							Silver = 0.15M,
							Gold = 0.20M,
							Platinum = 0.25M,
							Diamond = 0.30M,
							Value = model.ValueAdded
						},
					new SuggestedAmountModel
						{
							Method = CalculationMethod.FCF.DescriptionAttr(),
							Silver = 0.29M,
							Gold = 0.38M,
							Platinum = 0.48M,
							Diamond = 0.58M,
							Value = model.FreeCashFlow
						},
				};

		} // InitApplicationInfo
	}
}