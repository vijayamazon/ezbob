namespace EzBob.Web.Code {
	using ConfigManager;
	using System;
	using System.Linq;
	using DbConstants;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using EZBob.DatabaseLib.Model.Loans;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Utils.Extensions;
	using EzBob.Web.Infrastructure;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using log4net;
	using SalesForceLib.Models;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;

	public class CashRequestBuilder {
		public CashRequestBuilder(
			ILoanTypeRepository loanTypes,
			IDiscountPlanRepository discounts,
			IUsersRepository users,
			ILoanSourceRepository loanSources,
			IDecisionHistoryRepository historyRepository,
            ICustomerRepository customerRepository,
            IEzbobWorkplaceContext context
		) {
			m_oServiceClient = new ServiceClient();
			_loanTypes = loanTypes;
			_discounts = discounts;
			_users = users;
			_loanSources = loanSources;
			_historyRepository = historyRepository;
		    _customerRepository = customerRepository;
		    this.context = context;
		} // constructor

		public CashRequest CreateCashRequest(Customer customer, CashRequestOriginator originator) {
			LoanType loanType = customer.IsAlibaba ? _loanTypes.ByName("Alibaba Loan") : _loanTypes.GetDefault();
			var loanSource = _loanSources.GetDefault();

			int? experianScore = customer.ExperianConsumerScore;
			DateTime now = DateTime.UtcNow;
			var cashRequest = new CashRequest {
				CreationDate = DateTime.UtcNow,
				Customer = customer,
				InterestRate = loanSource.MaxInterest ?? 0.06M,
				LoanType = loanType,
				RepaymentPeriod = loanSource.DefaultRepaymentPeriod ?? loanType.RepaymentPeriod,
				ApprovedRepaymentPeriod = loanSource.DefaultRepaymentPeriod ?? loanType.RepaymentPeriod,
				DiscountPlan = _discounts.GetDefault(),
				OfferValidUntil = now.AddDays(1),
				OfferStart = now,
				LoanSource = loanSource,
				IsCustomerRepaymentPeriodSelectionAllowed = loanSource.IsCustomerRepaymentPeriodSelectionAllowed,
                IsLoanTypeSelectionAllowed = loanSource.IsCustomerRepaymentPeriodSelectionAllowed ? 1 : 0,
				ExpirianRating = experianScore,
				Originator = originator,
			};
            new Transactional(() => {
                customer.CashRequests.Add(cashRequest);
                this._customerRepository.SaveOrUpdate(customer);
            }).Execute();

		    m_oServiceClient.Instance.AddCashRequest(this.context.UserId, new NL_CashRequests {
		        CashRequestOriginID = (int)originator,
		        CustomerID = customer.Id,
		        OldCashRequestID = cashRequest.Id,
		        RequestTime = now,
		        UserID = this.context.UserId
		    });

            //TODO add new cash request
            Log.DebugFormat("add new cash request for customer {0}", customer.Id);
			
			if (originator != CashRequestOriginator.FinishedWizard) {
				CustomerRequestedLoan requestedLoan = customer.CustomerRequestedLoan.LastOrDefault();

				m_oServiceClient.Instance.SalesForceAddOpportunity(customer.Id, customer.Id,
					new ServiceClientProxy.EzServiceReference.OpportunityModel {
						Email = customer.Name,
						CreateDate = now,
						ExpectedEndDate = now.AddDays(7),
						RequestedAmount = requestedLoan != null ? (int?)requestedLoan.Amount : null,
						Type = OpportunityType.Resell.DescriptionAttr(),
						Stage = OpportunityStage.s5.DescriptionAttr(),
						Name = customer.PersonalInfo.Fullname + customer.CashRequests.Count()
					}
				);
			} // if

			return cashRequest;
		} // CreateCashRequest

		public CashRequest CreateQuickOfferCashRequest(Customer customer) {
			var loanType = _loanTypes.GetDefault();
			var loanSource = _loanSources.GetDefault();
		    var now = DateTime.UtcNow;
		    var discountPlan = _discounts.GetDefault();
			const string sReason = "Quick offer taken.";

			// TODO: do something really really really better than this.
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

			m_oServiceClient.Instance.ApprovedUser(
				user.Id,
				customer.Id,
				customer.QuickOffer.Amount,
				validForHours,
				customer.NumApproves == 1
			);

			_historyRepository.LogAction(DecisionActions.Approve, sReason, user, customer);

            new Transactional(() => {
                customer.CashRequests.Add(cashRequest);
                this._customerRepository.SaveOrUpdate(customer);
            }).Execute();

            var nlCashRequestID = m_oServiceClient.Instance.AddCashRequest(this.context.UserId, new NL_CashRequests {
                CashRequestOriginID = (int)CashRequestOriginator.QuickOffer,
                CustomerID = customer.Id,
                OldCashRequestID = cashRequest.Id,
                RequestTime = now,
                UserID = this.context.UserId
            });

            var nlDecisionID = m_oServiceClient.Instance.AddDecision(this.context.UserId, customer.Id, new NL_Decisions {
                CashRequestID = nlCashRequestID.Value,
                DecisionTime = now,
                Notes = CashRequestOriginator.QuickOffer.DescriptionAttr(),
                IsAmountSelectionAllowed = false,
                IsRepaymentPeriodSelectionAllowed = false,
                InterestOnlyRepaymentCount = 0,
                SendEmailNotification = false,
                DecisionNameID = (int)DecisionActions.Approve,
                UserID = user.Id
            }, null);

            var nlOfferID = m_oServiceClient.Instance.AddOffer(this.context.UserId, customer.Id, new NL_Offers {
                DecisionID = nlDecisionID.Value,
                CreatedTime = now,
                Notes = CashRequestOriginator.QuickOffer.DescriptionAttr(),
                InterestOnlyRepaymentCount = 0,
                Amount = customer.QuickOffer.Amount,
                BrokerSetupFeePercent = 0,
                SetupFeePercent = customer.QuickOffer.ImmediateSetupFee,
                DiscountPlanID = discountPlan.Id,
                EmailSendingBanned = false,
                EndTime = now.AddDays(1),
                MonthlyInterestRate = customer.QuickOffer.ImmediateInterestRate,
                RepaymentCount = customer.QuickOffer.ImmediateTerm,
                RepaymentIntervalTypeID = (int)RepaymentIntervalTypesId.Month,
                LoanSourceID = loanSource.ID,
                LoanTypeID = loanType.Id,
                StartTime = now,
                IsLoanTypeSelectionAllowed = false,
            });

            //TODO add new cash request / offer / decision
            Log.DebugFormat("add new cash request for customer {0}", customer.Id);

			return cashRequest;
		} // CreateQuickOfferCashRequest

		public ActionMetaData ForceEvaluate(
			int underwriterId,
			Customer customer,
			NewCreditLineOption newCreditLineOption,
			bool isSync
		) {
			bool bUpdateMarketplaces =
				newCreditLineOption == NewCreditLineOption.UpdateEverythingAndApplyAutoRules ||
				newCreditLineOption == NewCreditLineOption.UpdateEverythingAndGoToManualDecision;

			bUpdateMarketplaces = bUpdateMarketplaces && customer.CustomerMarketPlaces.Any(x =>
				x.UpdatingEnd != null &&
				(DateTime.UtcNow - x.UpdatingEnd.Value).Days > CurrentValues.Instance.UpdateOnReapplyLastDays
			);

			if (bUpdateMarketplaces) {
				foreach (var mp in customer.CustomerMarketPlaces)
					m_oServiceClient.Instance.UpdateMarketplace(customer.Id, mp.Id, false, underwriterId);
			} // if

			if (isSync) {
				return m_oServiceClient.Instance.MainStrategySync1(
					underwriterId,
					_users.Get(customer.Id).Id,
					newCreditLineOption,
					Convert.ToInt32(customer.IsAvoid),
					null,
					MainStrategyDoAction.Yes,
					MainStrategyDoAction.Yes
				);
			} else {
				return m_oServiceClient.Instance.MainStrategy1(
					underwriterId,
					_users.Get(customer.Id).Id,
					newCreditLineOption,
					Convert.ToInt32(customer.IsAvoid),
					null,
					MainStrategyDoAction.Yes,
					MainStrategyDoAction.Yes
				);
			} // if
		} // ForceEvaluate

		private readonly ILoanTypeRepository _loanTypes;
		private readonly IDiscountPlanRepository _discounts;
		private readonly IUsersRepository _users;
		private readonly ILoanSourceRepository _loanSources;
		private readonly IDecisionHistoryRepository _historyRepository;
		private readonly ServiceClient m_oServiceClient;
	    private static readonly ILog Log = LogManager.GetLogger(typeof (CashRequestBuilder));
	    private readonly ICustomerRepository _customerRepository;
	    private readonly IEzbobWorkplaceContext context;
	} // class CashRequestBuilder
} // namespace
