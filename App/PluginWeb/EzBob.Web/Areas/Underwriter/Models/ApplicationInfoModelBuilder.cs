namespace EzBob.Web.Areas.Underwriter.Models
{
	using System;
	using System.Linq;
	using ConfigManager;
	using Customer.Models;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Loans;
	using EzBob.Models;
	using Code;
	using System.Text;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using Ezbob.Utils.Extensions;
	using Infrastructure;
	using PaymentServices.Calculators;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;
	using StructureMap;

	public class ApplicationInfoModelBuilder
	{
		private readonly IPacNetBalanceRepository _funds;
		private readonly IPacNetManualBalanceRepository _manualFunds;
		private readonly RepaymentCalculator _repaymentCalculator = new RepaymentCalculator();
		private readonly ILoanTypeRepository _loanTypes;
		private readonly IDiscountPlanRepository _discounts;
		private readonly IApprovalsWithoutAMLRepository approvalsWithoutAMLRepository;
		private readonly IConfigurationVariablesRepository configurationVariablesRepository;
		private readonly ILoanSourceRepository _loanSources;
		private readonly ServiceClient serviceClient;
		private readonly VatReturnSummaryRepository _vatReturnSummaryRepository;
		private readonly CustomerAnalyticsRepository _customerAnalyticsRepository;
		public ApplicationInfoModelBuilder(
			IPacNetBalanceRepository funds,
			IPacNetManualBalanceRepository manualFunds,
			IApprovalsWithoutAMLRepository approvalsWithoutAMLRepository,
			IDiscountPlanRepository discounts,
			ILoanTypeRepository loanTypes,
			IConfigurationVariablesRepository configurationVariablesRepository,
			ILoanSourceRepository loanSources,
			VatReturnSummaryRepository vatReturnSummaryRepository,
			CustomerAnalyticsRepository customerAnalyticsRepository)
		{
			_funds = funds;
			_manualFunds = manualFunds;
			_discounts = discounts;
			_loanTypes = loanTypes;
			this.approvalsWithoutAMLRepository = approvalsWithoutAMLRepository;
			this.configurationVariablesRepository = configurationVariablesRepository;
			_loanSources = loanSources;
			_vatReturnSummaryRepository = vatReturnSummaryRepository;
			_customerAnalyticsRepository = customerAnalyticsRepository;
			serviceClient = new ServiceClient();
		}

		public void InitApplicationInfo(ApplicationInfoModel model, Customer customer, CashRequest cr)
		{
			if (customer == null) return;

			model.Id = customer.Id;

			if (cr != null)
			{
				var context = ObjectFactory.GetInstance<IWorkplaceContext>();
				DecimalActionResult result = serviceClient.Instance.GetLatestInterestRate(customer.Id, context.UserId);
				model.InterestRate = result.Value == -1 ? cr.InterestRate : result.Value;
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

				model.RepaymentPerion = _repaymentCalculator.ReCalculateRepaymentPeriod(cr);

				if (cr.SystemCalculatedSum.HasValue && Math.Abs(cr.SystemCalculatedSum.Value) > 0.01)
				{
					model.SystemCalculatedAmount = Convert.ToDecimal(cr.SystemCalculatedSum.Value);
				}

				model.OfferedCreditLine = Convert.ToDecimal(cr.ManagerApprovedSum ?? cr.SystemCalculatedSum);

				var calc = new SetupFeeCalculator(cr.UseSetupFee, cr.UseBrokerSetupFee, cr.ManualSetupFeeAmount, cr.ManualSetupFeePercent);
				model.SetupFee = calc.Calculate(model.OfferedCreditLine);

				model.BorrowedAmount = customer.Loans.Where(x => x.CashRequest != null && x.CashRequest.Id == cr.Id).Sum(x => x.LoanAmount);

				model.StartingFromDate = FormattingUtils.FormatDateToString(cr.OfferStart);
				model.OfferValidateUntil = FormattingUtils.FormatDateToString(cr.OfferValidUntil);

				model.Reason = cr.UnderwriterComment;

				model.IsLoanTypeSelectionAllowed = cr.IsLoanTypeSelectionAllowed;

				model.AnnualTurnover = cr.AnnualTurnover;
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


			var balance = _funds.GetBalance();
			var manualBalance = _manualFunds.GetBalance();
			var fundsAvailable = balance.Adjusted + manualBalance;
			model.FundsAvaliable = FormattingUtils.FormatPounds(fundsAvailable);

			DateTime today = DateTime.UtcNow;
			model.FundsAvailableUnderLimitClass = string.Empty;
			int relevantLimit = (today.DayOfWeek == DayOfWeek.Thursday || today.DayOfWeek == DayOfWeek.Friday) ? CurrentValues.Instance.PacnetBalanceWeekendLimit : CurrentValues.Instance.PacnetBalanceWeekdayLimit;
			if (fundsAvailable < relevantLimit)
			{
				model.FundsAvailableUnderLimitClass = "red_cell";
			}

			model.FundsReserved = FormattingUtils.FormatPounds(balance.ReservedAmount);
			//Status = "Active";
			model.Details = customer.Details;
			var isWaitingOrEscalated = customer.CreditResult == CreditResultStatus.WaitingForDecision ||
									   customer.CreditResult == CreditResultStatus.Escalated;

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

			model.OfferValidForHours = (int)configurationVariablesRepository.GetByNameAsDecimal("OfferValidForHours");

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