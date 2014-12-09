namespace Ezbob.Utils.Extensions {
	using System;

	public static class DecimalExt {

		public static decimal Min(this decimal a, decimal b) {
			return Math.Min(a, b);
		} // Min

		public static decimal Max(this decimal a, decimal b) {
			return Math.Max(a, b);
		} // Max

		public static decimal DropHundred(this decimal nValue) {
			return nValue.RoundDownToFactor(100.0m);
		} // DropHundred

		public static decimal RoundDownToFactor(this decimal nValue, decimal nRoundFactor) {
			return Math.Truncate(nValue / nRoundFactor) * nRoundFactor;
		} // RoundDownToFactor

	} // class DecimalExt
} // namespace
