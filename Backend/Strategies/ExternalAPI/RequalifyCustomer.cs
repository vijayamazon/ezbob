namespace Ezbob.Backend.Strategies.ExternalAPI {
	using System;
	using ConfigManager;
	using Ezbob.Backend.Models;
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

		public RequalifyCustomer(string email) {
			CustomerEmail = email;
		}

		public override void Execute() {

			// check customer exists
			ICustomerRepository custRep = ObjectFactory.GetInstance<ICustomerRepository>();
			Customer customer = custRep.TryGetByEmail(CustomerEmail);

			if (customer == null) {
				Result = false;
				return;
			}

			Result = true;

			Console.WriteLine("====================================================" + customer.Id);

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
				Originator = CashRequestOriginator.Other // new type should be added ("RequalifyCustomer")
			};

			bool cachRequestAdded = false;
			try {
				cachRequestAdded = customer.CashRequests.Add(cashRequest);
			} catch (Exception ex) {
				Log.Debug("Failed to add cash request for 'RequalifyCustomer' customer: {0}, {1}", customer.Id, ex.Message);
				Result = false;
				return;
			}

			Console.WriteLine("cachRequestAdded? {0}", cachRequestAdded);

			// 2. run main strategy
			/*try {
				if (cachRequestAdded) {
					MainStrategy strategy = new MainStrategy(customer.Id, NewCreditLineOption.UpdateEverythingAndApplyAutoRules, 0, null);
					strategy.Execute();
				}
			} catch (Exception ex1) {
				Log.Debug("Failed to run main strategy for 'RequalifyCustomer' customer: {0}, {1}", customer.Id, ex1.Message);
				Result = false;
			}*/

			Console.WriteLine("Result: {0}", Result);
		}


		public string CustomerEmail { get; set; }
		public Boolean Result { get; set; }

	}
}
