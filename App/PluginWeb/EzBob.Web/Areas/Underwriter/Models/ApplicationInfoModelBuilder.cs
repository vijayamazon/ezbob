namespace EzBob.Web.Areas.Underwriter.Models
{
	using System;
	using System.Linq;
	using Customer.Models;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Loans;
	using EzBob.Models;
	using Code;
	using Infrastructure;
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
		private static readonly IEzBobConfiguration config = ObjectFactory.GetInstance<IEzBobConfiguration>();

		public ApplicationInfoModelBuilder(
			IPacNetBalanceRepository funds,
			IPacNetManualBalanceRepository manualFunds,
			IApprovalsWithoutAMLRepository approvalsWithoutAMLRepository,
            IDiscountPlanRepository discounts,
			ILoanTypeRepository loanTypes, 
			IConfigurationVariablesRepository configurationVariablesRepository)
		{
			_funds = funds;
			_manualFunds = manualFunds;
            _discounts = discounts;
            _loanTypes = loanTypes;
			this.approvalsWithoutAMLRepository = approvalsWithoutAMLRepository;
			this.configurationVariablesRepository = configurationVariablesRepository;
		}

		public void InitApplicationInfo(ApplicationInfoModel model, Customer customer, CashRequest cr)
        {
            if (customer == null) return;

            model.Id = customer.Id;

            if (cr != null)
            {
                model.InterestRate = cr.InterestRate;
                model.CashRequestId = cr.Id;
                model.UseSetupFee = cr.UseSetupFee;
                model.AllowSendingEmail = !cr.EmailSendingBanned;

                var loanType = cr.LoanType ?? _loanTypes.GetDefault();

                model.LoanType = loanType.Name;
                model.LoanTypeId = loanType.Id;

                model.RepaymentPerion = _repaymentCalculator.ReCalculateRepaymentPeriod(cr);
            }

            model.CustomerId = customer.Id;
            model.IsTest = customer.IsTest;
			model.IsOffline = customer.IsOffline;
			model.HasYodlee = customer.GetYodleeAccounts().ToList().Any();
            model.IsAvoid = customer.IsAvoid;
            model.SystemDecision = customer.Status.ToString();

            if (cr.SystemCalculatedSum != null && cr.SystemCalculatedSum.Value != 0)
            {
                model.SystemCalculatedAmount = Convert.ToDecimal(cr.SystemCalculatedSum.Value);
            }

            model.OfferedCreditLine = Convert.ToDecimal(cr.ManagerApprovedSum ?? cr.SystemCalculatedSum);
            model.BorrowedAmount = customer.Loans.Where(x => x.CashRequest != null && x.CashRequest.Id == cr.Id).Sum(x => x.LoanAmount);
            model.AvaliableAmount = customer.CreditSum ?? 0M;
			model.OfferExpired = customer.OfferValidUntil <= DateTime.UtcNow;

			model.StartingFromDate = FormattingUtils.FormatDateToString(customer.OfferStart);
			model.OfferValidateUntil = FormattingUtils.FormatDateToString(customer.OfferValidUntil);

			var balance = _funds.GetBalance();
			var manualBalance = _manualFunds.GetBalance();
	        var fundsAvailable = balance.Adjusted + manualBalance;
			model.FundsAvaliable = FormattingUtils.FormatPounds(fundsAvailable);

	        DateTime today = DateTime.UtcNow;
	        model.FundsAvailableUnderLimitClass = string.Empty;
	        int relevantLimit = (today.DayOfWeek == DayOfWeek.Thursday || today.DayOfWeek == DayOfWeek.Friday) ? config.PacnetBalanceWeekendLimit : config.PacnetBalanceWeekdayLimit;
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

            model.IsModified = !string.IsNullOrEmpty(cr.LoanTemplate);

            model.LoanTypes = _loanTypes.GetAll().Select(t => LoanTypesModel.Create(t)).ToArray();

            model.DiscountPlans = _discounts.GetAll().Select(d => DiscountPlanModel.Create(d)).ToArray();
            var discountPlan = (cr.DiscountPlan ?? _discounts.GetDefault());
            model.DiscountPlan = discountPlan.Name;
            model.DiscountPlanPercents = discountPlan.Discounts.Any(d => d != 0) ? string.Format("({0})", discountPlan.ValuesStr) : "";
            model.DiscountPlanId = discountPlan.Id;

            model.Reason = cr.UnderwriterComment;

			model.IsLoanTypeSelectionAllowed = cr.IsLoanTypeSelectionAllowed;
			model.OfferValidForHours = (int)configurationVariablesRepository.GetByNameAsDecimal("OfferValidForHours");
            
			model.AMLResult = customer.AMLResult;
			model.SkipPopupForApprovalWithoutAML = approvalsWithoutAMLRepository.ShouldSkipById(customer.Id);
        }
	}
}