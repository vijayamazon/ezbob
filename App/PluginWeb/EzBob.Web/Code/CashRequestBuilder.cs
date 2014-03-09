﻿namespace EzBob.Web.Code {
	using EZBob.DatabaseLib.Model;
	using EzServiceReference;
	using System;
	using System.Linq;
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Loans;
	using Infrastructure;

	public class CashRequestBuilder {
		#region constructor

		public CashRequestBuilder(
			ILoanTypeRepository loanTypes,
			IDiscountPlanRepository discounts,
			IUsersRepository users,
			IEzBobConfiguration config,
			IConfigurationVariablesRepository configurationVariables,
			ILoanSourceRepository loanSources,
			IDecisionHistoryRepository historyRepository,
			LoanLimit limit
		) {
			m_oServiceClient = new ServiceClient();
			_loanTypes = loanTypes;
			_discounts = discounts;
			_users = users;
			_config = config;
			this.configurationVariables = configurationVariables;
			_loanSources = loanSources;
			_historyRepository = historyRepository;
			// _limit = limit;
		} // constructor

		#endregion constructor

		#region method CreateCashRequest

		public CashRequest CreateCashRequest(Customer customer) {
			var loanType = _loanTypes.GetDefault();
			var loanSource = _loanSources.GetDefault();

			var cashRequest = new CashRequest {
				CreationDate = DateTime.UtcNow,
				Customer = customer,
				InterestRate = 0.06M,
				LoanType = loanType,
				RepaymentPeriod = loanSource.DefaultRepaymentPeriod ?? loanType.RepaymentPeriod,
				UseSetupFee = configurationVariables.GetByNameAsBool("SetupFeeEnabled"),
				UseBrokerSetupFee = configurationVariables.GetByNameAsBool("BrokerCommissionEnabled"),
				DiscountPlan = _discounts.GetDefault(),
				IsLoanTypeSelectionAllowed = 1,
				OfferValidUntil = DateTime.UtcNow.AddDays(1),
				OfferStart = DateTime.UtcNow,
				LoanSource = loanSource,
				IsCustomerRepaymentPeriodSelectionAllowed = loanSource.IsCustomerRepaymentPeriodSelectionAllowed
			};

			customer.CashRequests.Add(cashRequest);

			return cashRequest;
		} // CreateCashRequest

		#endregion method CreateCashRequest

		#region method CreateQuickOfferCashRequest

		public CashRequest CreateQuickOfferCashRequest(Customer customer) {
			var loanType = _loanTypes.GetDefault();
			var loanSource = _loanSources.GetDefault();

			const string sReason = "Quick offer taken.";

			var user = _users.GetAll().FirstOrDefault(x => x.Id == 21); // TODO: do something really really really better than this.

			var cashRequest = new CashRequest {
				CreationDate = DateTime.UtcNow,
				Customer = customer,
				InterestRate = customer.QuickOffer.ImmediateInterestRate,
				LoanType = loanType,
				RepaymentPeriod = customer.QuickOffer.ImmediateTerm,
				UseSetupFee = customer.QuickOffer.ImmediateSetupFee > 0,
				UseBrokerSetupFee = false,
				DiscountPlan = _discounts.GetDefault(),
				IsLoanTypeSelectionAllowed = 0,
				OfferValidUntil = DateTime.UtcNow.AddDays(1),
				OfferStart = DateTime.UtcNow,
				LoanSource = loanSource, // TODO: can it be EU loan?
				IsCustomerRepaymentPeriodSelectionAllowed = false,

				ManualSetupFeePercent = customer.QuickOffer.ImmediateSetupFee,
				SystemCalculatedSum = (double)customer.QuickOffer.Amount,
				ManagerApprovedSum = (double)customer.QuickOffer.Amount,
				QuickOffer = customer.QuickOffer,
				SystemDecision = SystemDecision.Approve,
				SystemDecisionDate = DateTime.UtcNow,
				UnderwriterDecision = CreditResultStatus.Approved,
				UnderwriterDecisionDate = DateTime.UtcNow,
				UnderwriterComment = sReason,
				IdUnderwriter = user.Id,
			};

			customer.CashRequests.Add(cashRequest);

			customer.DateApproved = DateTime.UtcNow;
			customer.ApprovedReason = sReason;
			customer.UnderwriterName = user.Name;
			customer.Status = Status.Approved;
			customer.CreditResult = CreditResultStatus.Approved;
			customer.CreditSum = customer.QuickOffer.Amount;
			customer.ManagerApprovedSum = customer.QuickOffer.Amount;
			customer.OfferStart = cashRequest.OfferStart;
			customer.OfferValidUntil = cashRequest.OfferValidUntil;
			customer.IsLoanTypeSelectionAllowed = 0;

			// _appCreator.ApprovedUser(user, customer, customer.QuickOffer.Amount); // TODO: enable? disable?

			_historyRepository.LogAction(DecisionActions.Approve, sReason, user, customer);

			return cashRequest;
		} // CreateQuickOfferCashRequest

		#endregion method CreateQuickOfferCashRequest

		#region method ForceEvaluate

		public void ForceEvaluate(int underwriterId, Customer customer, NewCreditLineOption newCreditLineOption, bool isUnderwriterForced, bool isSync) {
			bool bUpdateMarketplaces =
				newCreditLineOption == NewCreditLineOption.UpdateEverythingAndApplyAutoRules ||
				newCreditLineOption == NewCreditLineOption.UpdateEverythingAndGoToManualDecision;

			bUpdateMarketplaces = bUpdateMarketplaces && customer.CustomerMarketPlaces.Any(x =>
				x.UpdatingEnd != null && (DateTime.UtcNow - x.UpdatingEnd.Value).Days > _config.UpdateOnReapplyLastDays
			);

			if (bUpdateMarketplaces) {
				// Update all marketplaces
				foreach (var mp in customer.CustomerMarketPlaces)
				{
					mp.UpdatingEnd = null;
					m_oServiceClient.Instance.UpdateMarketplace(customer.Id, mp.Id, false);
				}
			} // if

			if (!isUnderwriterForced) {
				if (isSync)
					m_oServiceClient.Instance.MainStrategySync1(underwriterId, _users.Get(customer.Id).Id, newCreditLineOption, Convert.ToInt32(customer.IsAvoid));
				else
					m_oServiceClient.Instance.MainStrategy1(underwriterId, _users.Get(customer.Id).Id, newCreditLineOption, Convert.ToInt32(customer.IsAvoid));
			}
			else
				m_oServiceClient.Instance.MainStrategy2(underwriterId, _users.Get(customer.Id).Id, newCreditLineOption, Convert.ToInt32(customer.IsAvoid), true);
		} // ForceEvaluate

		#endregion method ForceEvaluate

		#region private

		private readonly ILoanTypeRepository _loanTypes;
		private readonly IDiscountPlanRepository _discounts;
		private readonly IUsersRepository _users;
		private readonly IEzBobConfiguration _config;
		private readonly IConfigurationVariablesRepository configurationVariables;
		private readonly ILoanSourceRepository _loanSources;
		private readonly IDecisionHistoryRepository _historyRepository;
		// private readonly LoanLimit _limit; // TODO: if needed...
		private readonly ServiceClient m_oServiceClient;

		#endregion private
	} // class CashRequestBuilder
} // namespace
