namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using ConfigManager;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using PaymentServices.Calculators;

	public class GetLoanCommissionDefaults : AStrategy {
		public GetLoanCommissionDefaults(long cashRequestID, decimal loanAmount) {
			this.cashRequestID = cashRequestID;
			this.loanAmount = loanAmount;
		} // constructor

		public override string Name {
			get { return "GetLoanCommissionDefaults"; }
		} // Name

		public override void Execute() {
			Log.Debug(
				"Looking for default commissions for cash request {0} with loan amount {1}.",
				this.cashRequestID,
				this.loanAmount.ToString("C2", Library.Instance.Culture)
			);

			SafeReader sr = DB.GetFirst(
				"GetCashRequestForLoanCommissionDefaults",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CashRequestID", this.cashRequestID)
			);

			if (sr.IsEmpty) {
				Result = new DefaultCommissions(0, 0);
				Log.Debug("No cash request found by id {0}, result is {1}.", this.cashRequestID, Result);
				return;
			} // if

			IsBrokerCustomer = sr["IsBrokerCustomer"];

			Result = new DefaultCommissions(sr["BrokerSetupFeePercent"], sr["ManualSetupFeePercent"]);

			if (!IsBrokerCustomer) {
				Log.Debug("Cash request {0} belongs to a non-broker customer, result is {1}.", this.cashRequestID, Result);
				return;
			} // if

			DateTime? firstLoanDate = sr["FirstLoanDate"];

			Result = new CommissionCalculator(this.loanAmount, firstLoanDate).Calculate();

			Log.Debug(
				"Cash request {0} belongs to a customer with {1}, result is {2}.",
				this.cashRequestID,
				firstLoanDate.HasValue
					? "first loan taken on " + firstLoanDate.Value.ToString("d/MMM/yyyy", Library.Instance.Culture)
					: "no loans taken",
				Result
			);
		} // Execute

		public DefaultCommissions Result { get; private set; }

		public bool IsBrokerCustomer { get; private set; }

		private readonly long cashRequestID;
		private readonly decimal loanAmount;
	} // class GetLoanCommissionDefaults
} // namespace

