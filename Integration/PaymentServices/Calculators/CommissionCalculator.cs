namespace PaymentServices.Calculators {
	using System;
	using ConfigManager;
	using Ezbob.Backend.Models;

	/// <summary>
	/// Calculates default commission for broker clients based on logic defined in 
	/// https://drive.google.com/a/ezbob.com/file/d/0B6xR90xtvRfCeU5TWTRyMTR0bE0/view?usp=sharing
	/// </summary>
	public class CommissionCalculator {
		/// <param name="amount">approved loan amount</param>
		/// <param name="firstLoanDate">date of first customer's loan (null if no loan was taken)</param>
		public CommissionCalculator(decimal amount, DateTime? firstLoanDate) {
			this.amount = amount;
			this.firstLoanDate = firstLoanDate;
		} // constructor

		/// <returns>broker commission percent, ezbob commission percent</returns>
		public DefaultCommissions Calculate() {
			if (this.amount <= 0.01M)
				return new DefaultCommissions(0, 0);

			DateTime now = DateTime.UtcNow;

			if (this.firstLoanDate.HasValue)
				if (this.firstLoanDate.Value.AddMonths(MonthsSinceFirstLoan) < now)
					return new DefaultCommissions(BrokerCommissionHasLoan, EzbobCommissionHasLoan);

			if (this.amount >= BigLoanAmount) {
				decimal restBrokerCommissionAmount = (this.amount - BigLoanAmount) * BrokerCommissionBigLoanRest;
				decimal brokerCommission = (BaseBrokerCommissionAmount + restBrokerCommissionAmount) / this.amount;

				return new DefaultCommissions(brokerCommission, EzbobCommissionBigLoan);
			} // if

			if (this.amount >= MediumLoanAmount)
				return new DefaultCommissions(BrokerCommissionMediumLoan, EzbobCommissionMediumLoan);

			return new DefaultCommissions(BrokerCommissionSmallLoan, EzbobCommissionSmallLoan);
		} // Calculate

		private readonly decimal amount;
		private readonly DateTime? firstLoanDate;

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
	} // class CommissionCalculator
} // namespace
