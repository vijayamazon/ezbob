namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Methods {
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions;
	using Ezbob.Backend.CalculateLoan.Models.Exceptions;
	using Ezbob.Backend.ModelsWithDB.NewLoan;

	internal class FeesOnLoanCreationMethod : AMethod {

		/// <exception cref="NoInitialDataException">Condition. </exception>
		public FeesOnLoanCreationMethod(ALoanCalculator calculator, NL_Model loanModel): base(calculator, false) {

			if (loanModel == null)
				throw new NoInitialDataException();

			this.loanModel = loanModel;

		} // constructor

		/// <exception cref="NoScheduleException">Condition. </exception>
		public virtual void Execute () {

			if (this.loanModel.Schedule == null)
				throw new NoScheduleException();

			if (this.loanModel.Schedule.Count == 0)
				throw new NoScheduleException();

			

		}


	
		private readonly NL_Model loanModel;



	} // class FeesOnLoanCreationMethod
} // namespace