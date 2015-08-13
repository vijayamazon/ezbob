namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Methods {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Backend.CalculateLoan.Models.Exceptions;
	using Ezbob.Utils;
	using Ezbob.Utils.Lingvo;

	internal class CalculateAprMethod : AMethod {
		public CalculateAprMethod(ALoanCalculator calculator, DateTime? aprDate, bool writeToLog)
			: base(calculator, writeToLog)
		{
			MaxIterationLimit = DefaultMaxIterationsLimit;
			CalculationAccuracy = DefaultCalculationAccuracy;

			this.aprDate = (aprDate ?? DateTime.UtcNow).Date;
		} // constructor

		public virtual decimal Execute() {
			if (WriteToLog) {
				Log.Debug(
					"Calculating APR on {3} with accuracy {0} {1} for loan model\n{2}",
					CalculationAccuracy,
					MaxIterationLimit == 0
						? "without iterations limit"
						: "using up to " + MaxIterationLimit + " iterations",
					WorkingModel,
					this.aprDate.ToString("MMM d yyyy", Library.Instance.Culture)
				);
			} // if

			return 0;
			// TODO: revive

			/*

			try {
				this.loanPlan = new CalculatePlanMethod(Calculator, false).Execute()
					.Select(r => new AmountDueDate(this, r))
					.ToList();
			} catch (NegativeLoanAmountException negativeLoanAmountException) {
				// TODO: revive (do something)
			}

			this.issuedAmount = (double)WorkingModel.ActualIssuedAmount;

			this.calculationProgress = new ProgressCounter(
				"{0} iterations performed during APR calculation.",
				WriteToLog ? Log : null
			);

			double x = 1;

			bool accuracyReached = false;

			for ( ; ; ) {
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

			decimal apr = (decimal)x * 100m;

			if (WriteToLog) {
				Log.Debug(
					"\nOn {4} APR = {0} (after {2}{3})" +
					"\nAmount due dates:\n\t{5}" +
					"\nLoan model:\n{1}\n",
					apr,
					WorkingModel,
					Grammar.Number(this.calculationProgress.CurrentPosition, "iteration"),
					accuracyReached ? " accuracy reached" : " max iterations limit reached",
					this.aprDate.ToString("MMM d yyyy", Library.Instance.Culture),
					string.Join("\n\t", this.loanPlan)
				);
			} // if

			return apr;
			*/
		} // Execute

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

		// TODO: revive
		
		/*
		private class AmountDueDate {
			public AmountDueDate(CalculateAprMethod aprMethod, Repayment repayment) {
				this.date = repayment.Date;
				this.amount = (double)repayment.Amount;
				this.t = - ((repayment.Date - aprMethod.aprDate).Days) / 365.0;
			} // constructor

			public double ForFunction(double x) {
				return (this.amount * Math.Pow(1.0 + x, this.t));
			} // ForFunction

			public double ForDerivative(double x) {
				return (this.t * this.amount * Math.Pow(1.0 + x, this.t + 1));
			} // ForDerivative

			/// <summary>
			/// Returns a string that represents the current object.
			/// </summary>
			/// <returns>
			/// A string that represents the current object.
			/// </returns>
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
			return this.loanPlan.Sum(add => add.ForFunction(x)) - this.issuedAmount;
		} // AprFunction

		private double AprDerivative(double x) {
			return this.loanPlan.Sum(add => add.ForDerivative(x));
		} // AprDerivative

		*/

		private bool TooManyIterations() {
			if (MaxIterationLimit == 0)
				return false;

			return this.calculationProgress.CurrentPosition >= MaxIterationLimit;
		} // TooManyIterations

		private readonly DateTime aprDate;
		// TODO: revive
		// private List<AmountDueDate> loanPlan;
		private double issuedAmount;
		private double calculationAccuracy;
		private ProgressCounter calculationProgress;

		private const ulong DefaultMaxIterationsLimit = 100000;
		private const double DefaultCalculationAccuracy = 1e-7;
	} // class CalculateAprMethod
} // namespace
