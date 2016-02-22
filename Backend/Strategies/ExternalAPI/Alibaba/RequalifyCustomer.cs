namespace Ezbob.Backend.Strategies.ExternalAPI.Alibaba {
	using System;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.Exceptions;
	using Ezbob.Backend.Strategies.MainStrategy;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using StructureMap;

	public class RequalifyCustomer : AStrategy {
		public RequalifyCustomer(int customerID, long aliMemberID) {
			CustomerID = customerID;
			AliMemberID = aliMemberID;
		} // constructor

		public override string Name {
			get { return "RequalifyCustomer"; }
		} // Name

		/// <exception cref="StrategyAlert">Condition. </exception>
		public override void Execute() {
			//this.Result.Value = "STARTED";

			// check customer exists
			ICustomerRepository custRep = ObjectFactory.GetInstance<ICustomerRepository>();
			Customer customer = custRep.Get(this.CustomerID);

			if (customer == null) {
				// this.Result.Value = "CUSTOMER_NOT_FOUND";
				return;
			} // if

			if (customer.IsAlibaba == false) {
				// this.Result.Value = "ALIMEMBER_NOT_FOUND";
				return;
			} // if

			try {
				MainStrategy strategy = new MainStrategy(new MainStrategyArguments {
					UnderwriterID = 1, // TODO: apply real underwriter ID
					CustomerID = customer.Id,
					NewCreditLine = NewCreditLineOption.UpdateEverythingAndApplyAutoRules,
					AvoidAutoDecision = 0,
					FinishWizardArgs = null,
					CashRequestID = null,
					CashRequestOriginator = CashRequestOriginator.RequalifyCustomerStrategy
				});
				strategy.Execute();
			} catch (Exception ex1) {
				throw new StrategyAlert(
					this,
					string.Format(
						"Failed to run main strategy for 'RequalifyCustomer' customer: {0}, {1}",
						customer.Id,
						ex1.Message
					),
					ex1
				);
			} // try
		} // Execute

		public long AliMemberID { get; private set; }
		public int CustomerID { get; private set; }
	} // class RequalifyCustomer
} // namespace
