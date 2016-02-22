namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Methods {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions;
	using Ezbob.Utils;
	using Ezbob.Utils.Lingvo;

	internal class CalculateAprMethod : AMethod {
		
		public CalculateAprMethod(ALoanCalculator calculator, DateTime? aprDate): base(calculator, false) {

			MaxIterationLimit = DefaultMaxIterationsLimit;
			CalculationAccuracy = DefaultCalculationAccuracy;

			AprDate = (aprDate ?? DateTime.UtcNow).Date;

		} // constructor


		public DateTime AprDate { get; set; }
		private double calculationAccuracy;
		private ProgressCounter calculationProgress;
		private const ulong DefaultMaxIterationsLimit = 100000;
		private const double DefaultCalculationAccuracy = 1e-7;

		private readonly List<AmountDueDate> loanPlan = new List<AmountDueDate>();

		//private double issuedAmount;

		/// <summary>
		/// Calculation stops when this accuracy was reached.
		/// </summary>
		public double CalculationAccuracy {
			get { return this.calculationAccuracy; }
			set { this.calculationAccuracy = Math.Abs(value); }
		} // CalculationAccuracy

		/// <summary>
		/// Calculation stops if this number of iterations was reached.
		/// Zero means iterate as many as needed.
		/// </summary>
		public ulong MaxIterationLimit { get; set; }

		/// <exception cref="NoScheduleException">Condition. </exception>
		/// <exception cref="NoScheduleException">Condition. </exception>
		public virtual double Execute() {

			Log.Debug("Calculating APR on {3} with accuracy {0} {1} for loan model\n{2}",
					CalculationAccuracy,
					MaxIterationLimit == 0? "without iterations limit": "using up to " + MaxIterationLimit + " iterations",
					WorkingModel,
					AprDate.ToString("MMM d yyyy", Library.Instance.Culture));

			try {
				if (WorkingModel.Loan.LastHistory().Schedule == null || WorkingModel.Loan.LastHistory().Schedule.Count == 0) {
					var poPlanu = new CreateScheduleMethod(Calculator);
					poPlanu.Execute();
				}
				// ReSharper disable once CatchAllClause
			} catch (Exception createScheduleEx) {
				Log.Error("Failed to get schedule err: {0}", createScheduleEx);
				// ReSharper disable once ThrowFromCatchWithNoInnerException
				throw new NoScheduleException();
			}

			// debug
			WorkingModel.Loan.LastHistory().Schedule.ForEach(s => Log.Debug(s.ToStringAsTable()));
		
			foreach (var s in WorkingModel.Loan.LastHistory().Schedule) {
				this.loanPlan.Add(new AmountDueDate(s.PlannedDate, (double)s.AmountDue, AprDate));
			}
			
			this.calculationProgress = new ProgressCounter("{0} iterations performed during APR calculation.",WriteToLog ? Log : null );

			double x = 1;

			bool accuracyReached = false;

			for (; ; ) {
				bool timeToSay = this.calculationProgress.Increment();

				double y = AprFunction(x);

				if (Math.Abs(y) <= CalculationAccuracy) {
					accuracyReached = true;
					break;
				} // if

				double dx = y / AprDerivative(x);

				x -= dx;

				if (TooManyIterations())
					break;

				if (timeToSay)
					Log.Debug("Current APR variable value is {0}, last delta was {1}.", x, dx);
			} // for

			this.calculationProgress.Log();

			double apr = x * 100;

			if (WriteToLog) {
				Log.Debug(
					"\nOn {4} APR = {0} (after {2}{3})" +
					"\nAmount due dates:\n\t{5}" +
					"\nLoan model:\n{1}\n",
					apr,
					WorkingModel,
					Grammar.Number(this.calculationProgress.CurrentPosition, "iteration"),
					accuracyReached ? " accuracy reached" : " max iterations limit reached",
					this.AprDate.ToString("MMM d yyyy", Library.Instance.Culture),
					string.Join("\n\t", this.loanPlan)
				);
			} // if

			return apr;
			
		} // Execute


	

		private bool TooManyIterations() {
			if (MaxIterationLimit == 0)
				return false;

			return this.calculationProgress.CurrentPosition >= MaxIterationLimit;
		} // TooManyIterations
		
		
		private class AmountDueDate {

			public AmountDueDate(DateTime repaymentDate, double repaymentAmount, DateTime aprDate ) {
				this.date = repaymentDate; // PlannedDate
				this.amount = repaymentAmount; // AmountDue
				this.t = -((repaymentDate - aprDate).Days) / 365.0;
			} // constructor

			public double ForFunction(double x) {
				return (this.amount * Math.Pow(1.0 + x, this.t));
			} // ForFunction

			public double ForDerivative(double x) {
				return (this.t * this.amount * Math.Pow(1.0 + x, this.t + 1));
			} // ForDerivative

		
			public override string ToString() {
				return string.Format(
					"{0} on {1}, days count = {2}",
					this.amount.ToString("C2", Library.Instance.Culture),
					this.date.ToString("MMM dd yyyy", Library.Instance.Culture),
					-this.t * 365.0
				);
			} // ToString

			private readonly DateTime date;
			private readonly double amount;
			private readonly double t;
		} // class AmountDueDate

		private double AprFunction(double x) {
			return this.loanPlan.Sum(s => s.ForFunction(x)) - (double)Calculator.initialAmount;
		} // AprFunction

		private double AprDerivative(double x) {
			return this.loanPlan.Sum(add => add.ForDerivative(x));
		} // AprDerivative

	
	} // class CalculateAprMethod
} // namespace
