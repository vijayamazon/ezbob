namespace Ezbob.Backend.CalculateLoan.Models.Helpers {
	using System;
	using Ezbob.Backend.CalculateLoan.Models.Exceptions;
	using Ezbob.Backend.Extensions;

	public class OpenPrincipal {
		public DateTime Date {
			get { return this.date; }
			set { this.date = value.Date; }
		} // Date

		/// <exception cref="NegativeOpenPrincipalException" accessor="set">Condition. </exception>
		public decimal Amount {
			get { return this.amount; }
			set {
				if (value < 0)
					throw new NegativeOpenPrincipalException(value);

				this.amount = value;
			} // set
		} // Amount

		public OpenPrincipal DeepClone() {
			return new OpenPrincipal {
				Date = Date,
				Amount = Amount,
			};
		} // DeepClone

		/// <exception cref="NegativeOpenPrincipalException">Condition. </exception>
		public override string ToString() {
			return string.Format("on {0}: {1}",
				Date.DateStr(),
				Amount.ToString("C2", Models.Library.Instance.Culture)
			);
		} // ToString

		private DateTime date;
		private decimal amount;
	} // class OpenPrincipal
} // namespace
