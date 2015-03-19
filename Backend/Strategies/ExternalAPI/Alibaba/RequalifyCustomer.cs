namespace Ezbob.Backend.Strategies.ExternalAPI.Alibaba {
	using System;
	using System.Linq;
	using ConfigManager;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.Exceptions;
	using Ezbob.Backend.Strategies.MainStrategy;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Loans;
	using StructureMap;

	public class RequalifyCustomer : AStrategy {
		public override string Name {
			get { return "RequalifyCustomer"; }
		}

		public RequalifyCustomer(int customerID, int aliMemberID) {
			this.CustomerID = customerID;
			this.AliMemberID = aliMemberID;
		}

		/// <exception cref="StrategyAlert">Condition. </exception>
		public override void Execute() {

			//this.Result.Value = "STARTED";

			// check customer exists
			ICustomerRepository custRep = ObjectFactory.GetInstance<ICustomerRepository>();
			Customer customer = custRep.Get(this.CustomerID);

			if (customer == null) {
			//	this.Result.Value = "CUSTOMER_NOT_FOUND";
				return;
			}

			if (customer.IsAlibaba == false) {
			//	this.Result.Value = "ALIMEMBER_NOT_FOUND";
				return;
			}

			Log.Info("====================================================" + customer.Id);

			// 1. create cash request

			ILoanTypeRepository loanTypes = ObjectFactory.GetInstance<LoanTypeRepository>();
			ILoanSourceRepository loanSources = ObjectFactory.GetInstance<LoanSourceRepository>();
			IDiscountPlanRepository discounts = ObjectFactory.GetInstance<DiscountPlanRepository>();

			LoanType loanType = customer.IsAlibaba ? loanTypes.ByName("Alibaba Loan") : loanTypes.GetDefault();
			var loanSource = loanSources.GetDefault();

			int? experianScore = customer.ExperianConsumerScore;
			DateTime now = DateTime.UtcNow;
			var cashRequest = new CashRequest {
				CreationDate = DateTime.UtcNow,
				Customer = customer,
				InterestRate = 0.06M,
				LoanType = loanType,
				RepaymentPeriod = loanSource.DefaultRepaymentPeriod ?? loanType.RepaymentPeriod,
				ApprovedRepaymentPeriod = loanSource.DefaultRepaymentPeriod ?? loanType.RepaymentPeriod,
				UseSetupFee = CurrentValues.Instance.SetupFeeEnabled,
				UseBrokerSetupFee = (customer.Broker != null) || CurrentValues.Instance.BrokerCommissionEnabled,
				DiscountPlan = discounts.GetDefault(),
				IsLoanTypeSelectionAllowed = 1,
				OfferValidUntil = now.AddDays(1),
				OfferStart = now,
				LoanSource = loanSource,
				IsCustomerRepaymentPeriodSelectionAllowed = loanSource.IsCustomerRepaymentPeriodSelectionAllowed,
				ExpirianRating = experianScore,
				Originator = CashRequestOriginator.RequalifyCustomerStrategy
			};

			try {

				var cachRequestAdded = customer.CashRequests.Add(cashRequest);

				custRep.Save(customer);

				var cr = customer.CashRequests.OrderByDescending(p => p.CreationDate).FirstOrDefault();

				if (cr != null) {
					Log.Info("cachRequestAdded? {0}, {1}, {2}", cachRequestAdded, cr.Id, cr.CreationDate);
				}

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				throw new StrategyAlert(this, string.Format("Failed to add cash request for 'RequalifyCustomer' customer: {0}, {1}", customer.Id, ex.Message), ex);
			}

			// 2. run main strategy
			try {

				MainStrategy strategy = new MainStrategy(customer.Id, NewCreditLineOption.UpdateEverythingAndApplyAutoRules, 0, null);
				strategy.Execute();

				// ReSharper disable once CatchAllClause
			} catch (Exception ex1) {
				throw new StrategyAlert(this, string.Format("Failed to run main strategy for 'RequalifyCustomer' customer: {0}, {1}", customer.Id, ex1.Message), ex1);
			}
		}

		public int AliMemberID { get; private set; }
		public int CustomerID { get; private set; }

	}
}
