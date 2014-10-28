namespace EzBob.Web.Code
{
	using ConfigManager;
	using System;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using EZBob.DatabaseLib.Model.Loans;
	using Ezbob.Backend.Models;
	using ServiceClientProxy;

	public class CashRequestBuilder
	{
		#region constructor

		public CashRequestBuilder(
			ILoanTypeRepository loanTypes,
			IDiscountPlanRepository discounts,
			IUsersRepository users,
			ILoanSourceRepository loanSources,
			IDecisionHistoryRepository historyRepository
		)
		{
			m_oServiceClient = new ServiceClient();
			_loanTypes = loanTypes;
			_discounts = discounts;
			_users = users;
			_loanSources = loanSources;
			_historyRepository = historyRepository;
		} // constructor

		#endregion constructor

		#region method CreateCashRequest

		public CashRequest CreateCashRequest(Customer customer, CashRequestOriginator originator)
		{
			LoanType loanType = customer.IsAlibaba ? _loanTypes.ByName("Alibaba Loan") : _loanTypes.GetDefault();
			var loanSource = _loanSources.GetDefault();
			var score = customer.ScoringResults.OrderByDescending(x => x.ScoreDate).FirstOrDefault();

			int? experianScore = customer.ExperianConsumerScore;

			var cashRequest = new CashRequest
				{
					CreationDate = DateTime.UtcNow,
					Customer = customer,
					InterestRate = 0.06M,
					LoanType = loanType,
					RepaymentPeriod = loanSource.DefaultRepaymentPeriod ?? loanType.RepaymentPeriod,
					ApprovedRepaymentPeriod = loanSource.DefaultRepaymentPeriod ?? loanType.RepaymentPeriod,
					UseSetupFee = CurrentValues.Instance.SetupFeeEnabled,
					UseBrokerSetupFee = (customer.Broker != null) || CurrentValues.Instance.BrokerCommissionEnabled,
					DiscountPlan = _discounts.GetDefault(),
					IsLoanTypeSelectionAllowed = 1,
					OfferValidUntil = DateTime.UtcNow.AddDays(1),
					OfferStart = DateTime.UtcNow,
					LoanSource = loanSource,
					IsCustomerRepaymentPeriodSelectionAllowed = loanSource.IsCustomerRepaymentPeriodSelectionAllowed,
					ScorePoints = score != null ? score.ScorePoints : 0,
					ExpirianRating = experianScore,
					Originator = originator
				};

			customer.CashRequests.Add(cashRequest);

			return cashRequest;
		} // CreateCashRequest

		#endregion method CreateCashRequest

		#region method CreateQuickOfferCashRequest

		public CashRequest CreateQuickOfferCashRequest(Customer customer)
		{
			var loanType = _loanTypes.GetDefault();
			var loanSource = _loanSources.GetDefault();

			const string sReason = "Quick offer taken.";

			var user = _users.GetAll().FirstOrDefault(x => x.Id == 21); // TODO: do something really really really better than this.

			var cashRequest = new CashRequest
				{

					CreationDate = DateTime.UtcNow,
					Customer = customer,
					InterestRate = customer.QuickOffer.ImmediateInterestRate,
					LoanType = loanType,
					RepaymentPeriod = customer.QuickOffer.ImmediateTerm,
					ApprovedRepaymentPeriod = customer.QuickOffer.ImmediateTerm,
					UseSetupFee = customer.QuickOffer.ImmediateSetupFee > 0,
					UseBrokerSetupFee = false,
					DiscountPlan = _discounts.GetDefault(),
					IsLoanTypeSelectionAllowed = 0,
					OfferValidUntil = DateTime.UtcNow.AddDays(1),
					OfferStart = DateTime.UtcNow,
					LoanSource = loanSource, // TODO: can it be EU loan?
					IsCustomerRepaymentPeriodSelectionAllowed = false,

					ManualSetupFeePercent = customer.QuickOffer.ImmediateSetupFee,
					SystemCalculatedSum = (double) customer.QuickOffer.Amount,
					ManagerApprovedSum = (double) customer.QuickOffer.Amount,
					QuickOffer = customer.QuickOffer,
					SystemDecision = SystemDecision.Approve,
					SystemDecisionDate = DateTime.UtcNow,
					UnderwriterDecision = CreditResultStatus.Approved,
					UnderwriterDecisionDate = DateTime.UtcNow,
					UnderwriterComment = sReason,
					IdUnderwriter = user.Id,
					Originator = CashRequestOriginator.QuickOffer,
					ExpirianRating = customer.ExperianConsumerScore
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
			int validForHours = (int)(cashRequest.OfferValidUntil - cashRequest.OfferStart).Value.TotalHours;
			m_oServiceClient.Instance.ApprovedUser(user.Id, customer.Id, customer.QuickOffer.Amount, validForHours, customer.NumApproves == 1);

			_historyRepository.LogAction(DecisionActions.Approve, sReason, user, customer);

			return cashRequest;
		} // CreateQuickOfferCashRequest

		#endregion method CreateQuickOfferCashRequest

		#region method ForceEvaluate

		public void ForceEvaluate(int underwriterId, Customer customer, NewCreditLineOption newCreditLineOption, bool isUnderwriterForced, bool isSync) {
			m_oServiceClient.Instance.FraudCheckerAsync(customer.Id, FraudMode.FullCheck);
			bool bUpdateMarketplaces =
				newCreditLineOption == NewCreditLineOption.UpdateEverythingAndApplyAutoRules ||
				newCreditLineOption == NewCreditLineOption.UpdateEverythingAndGoToManualDecision;

			bUpdateMarketplaces = bUpdateMarketplaces && customer.CustomerMarketPlaces.Any(x =>
				x.UpdatingEnd != null && (DateTime.UtcNow - x.UpdatingEnd.Value).Days > CurrentValues.Instance.UpdateOnReapplyLastDays
			);

			if (bUpdateMarketplaces)
			{
				// Update all marketplaces
				foreach (var mp in customer.CustomerMarketPlaces)
				{
					m_oServiceClient.Instance.UpdateMarketplace(customer.Id, mp.Id, false, underwriterId);
				}
			} // if

			if (!isUnderwriterForced)
			{
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
		private readonly ILoanSourceRepository _loanSources;
		private readonly IDecisionHistoryRepository _historyRepository;
		private readonly ServiceClient m_oServiceClient;

		#endregion private
	} // class CashRequestBuilder
} // namespace
