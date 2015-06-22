namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Methods {
	using Ezbob.Backend.CalculateLoan.Models;

	internal abstract class AMethod {
		protected AMethod(ALoanCalculator calculator, bool writeToLog) {
			Calculator = calculator;
			WriteToLog = writeToLog;
		} // constructor

		protected virtual LoanCalculatorModel WorkingModel {
			get { return Calculator.WorkingModel; }
		} // WorkingModel

		protected virtual ALoanCalculator Calculator { get; private set; }

		protected virtual bool WriteToLog { get; private set; }
	} // class AMethod
} // namespace
