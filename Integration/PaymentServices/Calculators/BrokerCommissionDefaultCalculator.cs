namespace PaymentServices.Calculators {
	using System;

	public class BrokerCommissionDefaultCalculator {
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

        /// <summary>
        /// Calculates default commission for broker clients based on logic defined in 
        /// https://drive.google.com/a/ezbob.com/file/d/0B6xR90xtvRfCeU5TWTRyMTR0bE0/view?usp=sharing
        /// </summary>
        /// <param name="amount">approved loan amount</param>
        /// <param name="hasLoans">has customer loans</param>
        /// <param name="firstLoanDate">date of first customer's loan</param>
        /// <returns>broker commission percent, ezbob commission percent</returns>
		public Tuple<decimal, decimal> Calculate(decimal amount, bool hasLoans, DateTime? firstLoanDate) {
		    DateTime now = DateTime.UtcNow;
            if (hasLoans && firstLoanDate.HasValue) {
                if (firstLoanDate.Value.AddMonths(MonthSinceFirstLoan) < now) {
                    return new Tuple<decimal, decimal>(BrokerCommissionHasLoan, EzbobCommissionHasLoan);
                }
            }

            if (amount >= BigLoanAmount) {
                decimal baseBrokerCommissionAmount = BigLoanAmount * BrokerCommissionBigLoan;
                decimal restBrokerCommissionAmount = (amount - BigLoanAmount) * BrokerCommissionBigLoanRest;
                decimal brokerCommission = (baseBrokerCommissionAmount + restBrokerCommissionAmount) / amount;

                return new Tuple<decimal, decimal>(brokerCommission, EzbobCommissionBigLoan);
            }

            if (amount >= MediumLoanAmount) {
                return new Tuple<decimal, decimal>(BrokerCommissionMediumLoan, EzbobCommissionMediumLoan);
            }

		    return new Tuple<decimal, decimal>(BrokerCommissionSmallLoan, EzbobCommissionSmallLoan);
		} // Calculate

    } // class BrokerCommissionDefaultCalculator
} // namespace
