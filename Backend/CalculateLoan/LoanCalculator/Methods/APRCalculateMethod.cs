namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Methods {
	using System;

	internal class APRCalculateMethod : AMethod {



		public APRCalculateMethod(ALoanCalculator calculator, decimal setupFee, DateTime? aprDate, bool writeToLog)
			: base(calculator, writeToLog) {

			this.aprDate = aprDate ?? DateTime.Today;
			this.setupFee = setupFee;

		} // constructor


		// see \ezbob\Integration\PaymentServices\Calculators\APRCalculator.cs
		public virtual decimal Execute() {
			try {
				WorkingModel.ValidateSchedule();

				//var x = 1.0;
				//for (var i = 0; i < 10000; i++) {
				//	var f_x = f(x, amount - setupFee, monthlyRepayments);
				//	if (Math.Abs(f_x) < 1e-6)
				//		break;
				//	var f_prime_x = f_prime(x, monthlyRepayments);
				//	var dx = f_x / f_prime_x;
				//	x -= dx;
				//}
				//return Math.Round(x * 100, 2);

			} catch (Exception) {
				
				throw;
			}
			
		
			return 0m;
		} // Execute

		private DateTime? aprDate;
		private decimal setupFee;



	} // class APRCalculateMethod
} // namespace
