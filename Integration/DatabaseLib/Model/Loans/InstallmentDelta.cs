namespace EZBob.DatabaseLib.Model.Database.Loans {
	#region class InstallmentDelta

	public class InstallmentDelta {
		#region public

		#region constructor

		public InstallmentDelta(LoanScheduleItem oInstallment) {
			Installment = oInstallment;
			Principal = new NumericDelta<decimal>(Installment.LoanRepayment);
			Fees = new NumericDelta<decimal>(Installment.Fees);
			Interest = new NumericDelta<decimal>(Installment.Interest);
			Status = new NumericDelta<LoanScheduleStatus>(Installment.Status);
		} // constructor

		#endregion constructor

		#region method SetEndValues

		public void SetEndValues() {
			Principal.EndValue = Installment.LoanRepayment;
			Fees.EndValue = Installment.Fees;
			Interest.EndValue = Installment.Interest;
			Status.EndValue = Installment.Status;
		} // SetEndValues

		#endregion method SetEndValues

		#region property IsNotZero

		public bool IsNotZero {
			get { return !IsZero; } // get
		} // IsNotZero

		#endregion property IsNotZero

		#region property IsZero

		public bool IsZero {
			get {
				return Principal.NotChanged && Fees.NotChanged && Interest.NotChanged;
			} // get
		} // IsZero

		#endregion property IsZero

		#region property Installment

		public LoanScheduleItem Installment { get; private set; }

		#endregion property Installment

		#region property Principal

		public NumericDelta<decimal> Principal { get; private set; }

		#endregion property Principal

		#region property Fees

		public NumericDelta<decimal> Fees { get; private set; }

		#endregion property Fees

		#region property Interest

		public NumericDelta<decimal> Interest { get; private set; }

		#endregion property Interest

		#region property Status

		public NumericDelta<LoanScheduleStatus> Status { get; private set; }

		#endregion property Status

		#endregion public
	} // class InstallmentDelta

	#endregion class InstallmentDelta
} // namespace EZBob.DatabaseLib.Model.Database.Loans
