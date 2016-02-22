namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using ConfigManager;
	using Ezbob.Backend.Models;
	using Ezbob.Database;

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

			Result = Calculate(this.loanAmount, firstLoanDate);

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

		/// <summary>
		/// Calculates default commission for broker clients based on logic defined in 
		/// https://drive.google.com/a/ezbob.com/file/d/0B6xR90xtvRfCeU5TWTRyMTR0bE0/view?usp=sharing
		/// </summary>
		/// <param name="amount">approved loan amount</param>
		/// <param name="firstLoanDate">date of first customer's loan (null if no loan was taken)</param>
		/// <returns>broker commission percent, ezbob commission percent</returns>
		private DefaultCommissions Calculate(decimal amount, DateTime? firstLoanDate) {
			DateTime now = DateTime.UtcNow;

			if (firstLoanDate.HasValue)
				if (firstLoanDate.Value.AddMonths(MonthsSinceFirstLoan) < now)
					return new DefaultCommissions(BrokerCommissionHasLoan, EzbobCommissionHasLoan);

			if (amount >= BigLoanAmount) {
				decimal restBrokerCommissionAmount = (amount - BigLoanAmount) * BrokerCommissionBigLoanRest;
				decimal brokerCommission = (BaseBrokerCommissionAmount + restBrokerCommissionAmount) / amount;

				return new DefaultCommissions(brokerCommission, EzbobCommissionBigLoan);
			} // if

			if (amount >= MediumLoanAmount)
				return new DefaultCommissions(BrokerCommissionMediumLoan, EzbobCommissionMediumLoan);

			return new DefaultCommissions(BrokerCommissionSmallLoan, EzbobCommissionSmallLoan);
		} // Calculate

		private decimal BaseBrokerCommissionAmount { get { return BigLoanAmount * BrokerCommissionBigLoan; } }

		private int MonthsSinceFirstLoan { get { return CurrentValues.Instance.MonthsSinceFirstLoan; } }
		private int BigLoanAmount { get { return CurrentValues.Instance.BigLoanAmount; } }
		private int MediumLoanAmount { get { return CurrentValues.Instance.MediumLoanAmount; } }

		private decimal BrokerCommissionHasLoan { get { return CurrentValues.Instance.BrokerCommissionHasLoan; } }
		private decimal EzbobCommissionHasLoan { get { return CurrentValues.Instance.EzbobCommissionHasLoan; } }
		private decimal BrokerCommissionBigLoan { get { return CurrentValues.Instance.BrokerCommissionBigLoan; } }
		private decimal BrokerCommissionBigLoanRest { get { return CurrentValues.Instance.BrokerCommissionBigLoanRest; } }
		private decimal EzbobCommissionBigLoan { get { return CurrentValues.Instance.EzbobCommissionBigLoan; } }
		private decimal BrokerCommissionMediumLoan { get { return CurrentValues.Instance.BrokerCommissionMediumLoan; } }
		private decimal EzbobCommissionMediumLoan { get { return CurrentValues.Instance.EzbobCommissionMediumLoan; } }
		private decimal BrokerCommissionSmallLoan { get { return CurrentValues.Instance.BrokerCommissionSmallLoan; } }
		private decimal EzbobCommissionSmallLoan { get { return CurrentValues.Instance.EzbobCommissionSmallLoan; } }

		private readonly long cashRequestID;
		private readonly decimal loanAmount;
	} // class GetLoanCommissionDefaults
} // namespace

