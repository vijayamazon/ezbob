namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Methods {
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Logger;

	internal abstract class AMethod {
		protected AMethod(ALoanCalculator calculator, bool writeToLog) {
			Calculator = calculator;
			WriteToLog = writeToLog;
		} // constructor


		protected AMethod(ALoanCalculator calculator) {
			Calculator = calculator;
		} // constructor

		protected virtual NL_Model WorkingModel {
			get { return Calculator.WorkingModel; }
		} // WorkingModel

		protected virtual ALoanCalculator Calculator { get; private set; }

		protected virtual bool WriteToLog { get; private set; }

		protected virtual ASafeLog Log {
			get { return WriteToLog ? Library.Instance.Log : logStub; }
		} // Log

		


		

		private static readonly ASafeLog logStub = new SafeLog();
	} // class AMethod
} // namespace
