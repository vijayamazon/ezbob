namespace EZBob.DatabaseLib.Model.Database.Loans {
	using EZBob.DatabaseLib.Common;

	public class InstallmentDelta {

		public InstallmentDelta(LoanScheduleItem oInstallment) {
			Installment = oInstallment;
			Principal = new NumericDelta<decimal>(Installment.LoanRepayment);
			Fees = new NumericDelta<decimal>(Installment.Fees);
			Interest = new NumericDelta<decimal>(Installment.Interest);
			Status = new NumericDelta<LoanScheduleStatus>(Installment.Status);
		} // constructor

		public void SetEndValues() {
			Principal.EndValue = Installment.LoanRepayment;
			Fees.EndValue = Installment.Fees;
			Interest.EndValue = Installment.Interest;
			Status.EndValue = Installment.Status;
		} // SetEndValues

		public bool IsNotZero {
			get { return !IsZero; } // get
		} // IsNotZero

		public bool IsZero {
			get {
				return Principal.NotChanged && Fees.NotChanged && Interest.NotChanged;
			} // get
		} // IsZero

		public LoanScheduleItem Installment { get; private set; }

		public NumericDelta<decimal> Principal { get; private set; }

		public NumericDelta<decimal> Fees { get; private set; }

		public NumericDelta<decimal> Interest { get; private set; }

		public NumericDelta<LoanScheduleStatus> Status { get; private set; }

	} // class InstallmentDelta

} // namespace EZBob.DatabaseLib.Model.Database.Loans
