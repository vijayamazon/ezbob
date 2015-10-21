namespace PaymentServices.Calculators {
	using System;

	public class BrokerCommissionDefaultCalculator {
		public class Result {
			public Result(decimal brokerCommission, decimal manualSetupFee) {
				BrokerCommission = brokerCommission;
				ManualSetupFee = manualSetupFee;
			} // constructor

			public decimal BrokerCommission { get; private set; }
			public decimal ManualSetupFee { get; private set; }

			/// <summary>
			/// Returns a string that represents the current object.
			/// </summary>
			/// <returns>
			/// A string that represents the current object.
			/// </returns>
			public override string ToString() {
				return string.Format(
					"broker commission = {0}, manual setup fee = {1}",
					BrokerCommission.ToString("P6"),
					ManualSetupFee.ToString("P6")
				);
			} // ToString
		} // class Result

		/// <summary>
		/// Calculates default commission for broker clients based on logic defined in 
		/// https://drive.google.com/a/ezbob.com/file/d/0B6xR90xtvRfCeU5TWTRyMTR0bE0/view?usp=sharing
		/// </summary>
		/// <param name="amount">approved loan amount</param>
		/// <param name="firstLoanDate">date of first customer's loan (null if no loan was taken)</param>
		/// <returns>broker commission percent, ezbob commission percent</returns>
		public Result Calculate(decimal amount, DateTime? firstLoanDate) {
			DateTime now = DateTime.UtcNow;

			if (firstLoanDate.HasValue)
				if (firstLoanDate.Value.AddMonths(MonthSinceFirstLoan) < now)
					return new Result(BrokerCommissionHasLoan, EzbobCommissionHasLoan);

			if (amount >= BigLoanAmount) {
				const decimal baseBrokerCommissionAmount = BigLoanAmount * BrokerCommissionBigLoan;
				decimal restBrokerCommissionAmount = (amount - BigLoanAmount) * BrokerCommissionBigLoanRest;
				decimal brokerCommission = (baseBrokerCommissionAmount + restBrokerCommissionAmount) / amount;

				return new Result(brokerCommission, EzbobCommissionBigLoan);
			} // if

			if (amount >= MediumLoanAmount)
				return new Result(BrokerCommissionMediumLoan, EzbobCommissionMediumLoan);

			return new Result(BrokerCommissionSmallLoan, EzbobCommissionSmallLoan);
		} // Calculate

		private const int MonthSinceFirstLoan = 3;
		private const int BigLoanAmount = 50000;
		private const int MediumLoanAmount = 35000;
		private const decimal BrokerCommissionHasLoan = 0.01M;
		private const decimal EzbobCommissionHasLoan = 0.05M;
		private const decimal BrokerCommissionBigLoan = 0.05M;
		private const decimal BrokerCommissionBigLoanRest = 0.025M;
		private const decimal EzbobCommissionBigLoan = 0.02M;
		private const decimal BrokerCommissionMediumLoan = 0.05M;
		private const decimal EzbobCommissionMediumLoan = 0.015M;
		private const decimal BrokerCommissionSmallLoan = 0.05M;
		private const decimal EzbobCommissionSmallLoan = 0.02M;
	} // class BrokerCommissionDefaultCalculator
} // namespace
