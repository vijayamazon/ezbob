namespace Ezbob.Backend.Strategies.CalculateLoan.Helpers {
	using System;
	using System.Globalization;

	public class Fee {
		public Fee(DateTime assignTime, decimal amount) {
			AssignTime = assignTime;
			Amount = amount;
		} // constructor

		public DateTime AssignTime { get; private set; }
		public decimal Amount { get; private set; }

		public Fee DeepClone() {
			return new Fee(AssignTime, Amount);
		} // DeepClone

		public override string ToString() {
			return string.Format(
				"on {0}: {1}",
				AssignTime.MomentStr(),
				Amount.ToString("C2", Culture)
			);
		} // ToString

		private static CultureInfo Culture {
			get { return Library.Instance.Culture; }
		} // Culture
	} // class Fee
} // namespace
