namespace Ezbob.Backend.Strategies.MainStrategy.Helpers {
	using System;

	internal class GradeRangeSubproduct {
		public decimal MinSetupFee { get; set; }
		public decimal MaxSetupFee { get; set; }
		public decimal MinInterestRate { get; set; }
		public decimal MaxInterestRate { get; set; }
		public decimal MinLoanAmount { get; set; }
		public decimal MaxLoanAmount { get; set; }
		public int MinTerm { get; set; }
		public int MaxTerm { get; set; }
		public int LoanSourceID { get; set; }
		public int LoanTypeID { get; set; }

		public decimal SetupFee {
			get { return (MaxSetupFee + MinSetupFee) / 2M; }
		} // SetupFee

		public decimal InterestRate {
			get { return (MaxInterestRate + MinInterestRate) / 2M; }
		} // SetupFee

		public int LoanAmount(decimal requestedAmount) {
			decimal result;

			if (requestedAmount < MinLoanAmount)
				result = MinLoanAmount;
			else if (requestedAmount > MaxLoanAmount)
				result = MaxLoanAmount;
			else
				result = requestedAmount;


			if (result > Int32.MaxValue)
				return Int32.MaxValue;

			if (result <= 0)
				return 0;

			return (int)result;
		} // LoanAmount

		public int Term(int requestedTerm) {
			if (requestedTerm < MinTerm)
				return MinTerm;

			if (requestedTerm > MaxTerm)
				return MaxTerm;

			return requestedTerm;
		} // Term
	} // class GradeRangeSubproduct
} // namespace
