namespace Ezbob.Backend.Strategies.CalculateLoan {
	using System;
	using System.Globalization;

	public class Repayment {
		public Repayment(DateTime time, decimal principal, decimal interest, decimal fees) {
			Time = time;
			Principal = principal;
			Interest = interest;
			Fees = fees;
		} // constructor

		public DateTime Time { get; private set; }

		public decimal Amount {
			get { return Principal + Interest + Fees; }
		} // Amount

		public decimal Principal { get; set; }
		public decimal Interest { get; set; }
		public decimal Fees { get; set; }

		public Repayment DeepClone() {
			return new Repayment(Time, Principal, Interest, Fees);
		} // DeepClone

		public override string ToString() {
			return string.Format(
				"on {0}: {1} = p{2} + i{3} + f{4}",
				Time.MomentStr(),
				Amount.ToString("C2", Culture),
				Principal.ToString("C2", Culture),
				Interest.ToString("C2", Culture),
				Fees.ToString("C2", Culture)
			);
		} // ToString

		private static CultureInfo Culture {
			get { return Library.Instance.Culture; }
		} // Culture
	} // class Repayment
} // namespace
