namespace Ezbob.Backend.CalculateLoan.Models.Helpers {
	using System;
	using System.Globalization;
	using DbConstants;
	using Ezbob.Backend.Extensions;

	public class Fee {
		public Fee(DateTime assignTime, decimal amount, FeeTypes ftype) {
			AssignTime = assignTime;
			Amount = amount;
			FType = ftype;
		} // constructor

		public DateTime AssignDate { get { return AssignTime.Date; } }

		public DateTime AssignTime { get; private set; }
		public decimal Amount { get; private set; }

		public FeeTypes FType{ get; private set; }

		public Fee DeepClone() {
			return new Fee(AssignTime, Amount, FType);
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
