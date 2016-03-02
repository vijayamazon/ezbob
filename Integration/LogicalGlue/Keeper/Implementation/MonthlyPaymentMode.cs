namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation {
	using System.Linq;

	internal enum MonthlyPaymentMode {
		NoOpenNoInterest,
		WithOpenNoInterest,
		NoOpenWithInterest,
		WithOpenWithInterest,
	} // enum MonthlyPaymentMode

	internal static class MonthlyPaymentModeExt {
		public static bool In(this MonthlyPaymentMode mpm, params MonthlyPaymentMode[] vals) {
			return vals.Any(v => mpm == v);
		} // WithOpen

		public static bool WithOpen(this MonthlyPaymentMode mpm) {
			return mpm.In(MonthlyPaymentMode.WithOpenNoInterest, MonthlyPaymentMode.WithOpenWithInterest);
		} // WithOpen

		public static bool WithInterest(this MonthlyPaymentMode mpm) {
			return mpm.In(MonthlyPaymentMode.WithOpenWithInterest, MonthlyPaymentMode.NoOpenWithInterest);
		} // WithInterest
	} // class MonthlyPaymentModeExt
} // namespace
