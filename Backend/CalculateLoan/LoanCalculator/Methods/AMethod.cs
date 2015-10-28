namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Methods {
	internal abstract class AMethod {
		protected AMethod(ALoanCalculator calculator, bool writeToLog) {
			Calculator = calculator;
			WriteToLog = writeToLog;
		} // constructor

	//	protected virtual NL_Model WorkingModel;

		protected virtual ALoanCalculator Calculator { get; private set; }

		protected virtual bool WriteToLog { get; private set; }
	} // class AMethod
} // namespace
