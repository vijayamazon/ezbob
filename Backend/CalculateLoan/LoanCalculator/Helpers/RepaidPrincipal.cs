namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Helpers {
	using System;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions;
	using Ezbob.Backend.Extensions;

	public class RepaidPrincipal {
		/// <exception cref="Exceptions.NegativeRepaymentAmountException">Condition. </exception>
		public RepaidPrincipal(decimal amount, DateTime time) {
			Amount = amount;
			Time = time;
		} // constructor

		/// <exception cref="Exceptions.NegativeRepaymentAmountException">Condition. </exception>
		public RepaidPrincipal DeepClone() {
			return new RepaidPrincipal(Amount, Time);
		} // DeepClone

		/// <exception cref="Exceptions.NegativeRepaymentAmountException" accessor="set">Condition. </exception>
		public decimal Amount {
			get { return this.amount; }

			private set {
				if (value <= 0)
					throw new NegativeRepaymentAmountException(value);

				this.amount = value;
			} // set
		} // Amount

		public DateTime Time { get; private set; }

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		/// <exception cref="Exceptions.NegativeRepaymentAmountException">Condition. </exception>
		public override string ToString() {
			return string.Format("{0} at {1}", Amount.ToString("C2", Library.Instance.Culture), Time.MomentStr());
		} // ToString

		private decimal amount;
	} // class RepaidPrincipal
}
