namespace EzBob.Web.Code {
	using System;
	using System.Linq;
	using DbConstants;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using EZBob.DatabaseLib.Model.Loans;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Utils.Extensions;
	using EzBob.Web.Infrastructure;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using log4net;
	using ServiceClientProxy;

	public class CashRequestBuilder {
		public CashRequestBuilder(
			ILoanTypeRepository loanTypes,
			IDiscountPlanRepository discounts,
			IUsersRepository users,
			ILoanSourceRepository loanSources,
			IDecisionHistoryRepository historyRepository,
			ICustomerRepository customerRepository
		) {
			m_oServiceClient = new ServiceClient();
			_loanTypes = loanTypes;
			_discounts = discounts;
			_users = users;
			_loanSources = loanSources;
			_historyRepository = historyRepository;
			_customerRepository = customerRepository;
		} // constructor

		public CashRequest CreateQuickOfferCashRequest(Customer customer, int userID ) {
			var loanType = _loanTypes.GetDefault();
			var loanSource = _loanSources.GetDefault(customer.Id);
			var now = DateTime.UtcNow;
			var discountPlan = _discounts.GetDefault();
			const string sReason = "Quick offer taken.";

			// TODO: do something really-really-really better than this.
			var user = _users.GetAll().FirstOrDefault(x => x.Id == 1);

			var cashRequest = new CashRequest {
                CreationDate = now,
				Customer = customer,
				InterestRate = customer.QuickOffer.ImmediateInterestRate,
				LoanType = loanType,
				RepaymentPeriod = customer.QuickOffer.ImmediateTerm,
				ApprovedRepaymentPeriod = customer.QuickOffer.ImmediateTerm,
				UseSetupFee = customer.QuickOffer.ImmediateSetupFee > 0,
				UseBrokerSetupFee = false,
				DiscountPlan = discountPlan,
				IsLoanTypeSelectionAllowed = 0,
				OfferValidUntil = now.AddDays(1),
				OfferStart = now,
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
				Originator = EZBob.DatabaseLib.Model.Database.CashRequestOriginator.QuickOffer,
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

			this.m_oServiceClient.Instance.ApprovedUser(
				user.Id,
				customer.Id,
				customer.QuickOffer.Amount,
				validForHours,
				customer.NumApproves == 1
			);

			this._historyRepository.LogAction(DecisionActions.Approve, sReason, user, customer);

			new Transactional(() => {
				customer.CashRequests.Add(cashRequest);
				this._customerRepository.SaveOrUpdate(customer);
			}).Execute();
			
			var nlCashRequest = this.m_oServiceClient.Instance.AddCashRequest(userID, new NL_CashRequests {
				CashRequestOriginID = (int)CashRequestOriginator.QuickOffer,
				CustomerID = customer.Id,
				OldCashRequestID = cashRequest.Id,
				RequestTime = now,
				UserID = userID
			});

			Log.DebugFormat("Added NL CashRequestID: {0}, Error: {1}", nlCashRequest.Value, nlCashRequest.Error);

			var nlDecision = this.m_oServiceClient.Instance.AddDecision(userID, customer.Id, new NL_Decisions {
				CashRequestID = nlCashRequest.Value,
				DecisionTime = now,
				Notes = CashRequestOriginator.QuickOffer.DescriptionAttr(),
				DecisionNameID = (int)DecisionActions.Approve,
				UserID = user.Id
			}, cashRequest.Id, null);

			Log.DebugFormat("Added NL DecisionID: {0}, Error: {1}", nlDecision.Value, nlDecision.Error);

			NL_OfferFees setupFee = new NL_OfferFees() { LoanFeeTypeID = (int)NLFeeTypes.SetupFee, Percent = customer.QuickOffer.ImmediateSetupFee, OneTimePartPercent = 1, DistributedPartPercent = 0 };
			if (cashRequest.SpreadSetupFee != null && cashRequest.SpreadSetupFee == true){
				setupFee.LoanFeeTypeID = (int)NLFeeTypes.ServicingFee;
				setupFee.OneTimePartPercent = 0;
				setupFee.DistributedPartPercent = 1;
			}
			NL_OfferFees[] ofeerFees = { setupFee };

			var nlOffer = this.m_oServiceClient.Instance.AddOffer(userID, customer.Id, new NL_Offers {
				DecisionID = nlDecision.Value,
				Amount = customer.QuickOffer.Amount,
				CreatedTime = now,
				StartTime = now,
				EndTime = now.AddDays(1),
				RepaymentIntervalTypeID = (int)RepaymentIntervalTypes.Month,
				LoanSourceID = loanSource.ID,
				LoanTypeID = loanType.Id,
				DiscountPlanID = discountPlan.Id,
				Notes = CashRequestOriginator.QuickOffer.DescriptionAttr(),
				InterestOnlyRepaymentCount = 0,
				BrokerSetupFeePercent = 0,
				MonthlyInterestRate = customer.QuickOffer.ImmediateInterestRate,
				RepaymentCount = customer.QuickOffer.ImmediateTerm,
				IsAmountSelectionAllowed = false,
				IsRepaymentPeriodSelectionAllowed = false,
				IsLoanTypeSelectionAllowed = false
				// ### defaults, can be ommited
			}, ofeerFees);

			Log.DebugFormat("Added NL OfferID: {0}, Error: {1}", nlOffer.Value, nlOffer.Error);
			
			//TODO add new cash request / offer / decision
			Log.DebugFormat("add new cash request for customer {0}", customer.Id);

			return cashRequest;
		} // CreateQuickOfferCashRequest

		private readonly ILoanTypeRepository _loanTypes;
		private readonly IDiscountPlanRepository _discounts;
		private readonly IUsersRepository _users;
		private readonly ILoanSourceRepository _loanSources;
		private readonly IDecisionHistoryRepository _historyRepository;
		private readonly ServiceClient m_oServiceClient;
		private readonly ICustomerRepository _customerRepository;
		private static readonly ILog Log = LogManager.GetLogger(typeof (CashRequestBuilder));
	} // class CashRequestBuilder
} // namespace
