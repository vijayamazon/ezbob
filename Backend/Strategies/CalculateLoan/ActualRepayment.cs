namespace Ezbob.Backend.Strategies.CalculateLoan {
	using System;
	using System.Globalization;

	public class ActualRepayment {
		public ActualRepayment(DateTime time, decimal amount) {
			Time = time;
			Amount = amount;
		} // constructor

		public DateTime Time { get; private set; }
		public decimal Amount { get; private set; }

		public ActualRepayment DeepClone() {
			return new ActualRepayment(Time, Amount);
		} // DeepClone

		public override string ToString() {
			return string.Format(
				"on {0}: {1}",
				Time.MomentStr(),
				Amount.ToString("C2", Culture)
			);
		} // ToString

		private static CultureInfo Culture {
			get { return Library.Instance.Culture; }
		} // Culture
	} // class ActualRepayment
} // namespace
