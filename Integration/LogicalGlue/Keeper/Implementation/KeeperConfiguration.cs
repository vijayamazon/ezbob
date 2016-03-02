namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation {
	using System;

	internal class KeeperConfiguration {
		public string MonthlyPaymentModeName {
			get { return MonthlyPaymentMode.ToString(); }

			set {
				MonthlyPaymentMode mpm;
				MonthlyPaymentMode = Enum.TryParse(value, true, out mpm) ? mpm : MonthlyPaymentMode.NoOpenNoInterest;
			} // set
		} // MonthlyPaymentModeName

		public MonthlyPaymentMode MonthlyPaymentMode { get; private set; }
	} // class KeeperConfiguration
} // namespace
