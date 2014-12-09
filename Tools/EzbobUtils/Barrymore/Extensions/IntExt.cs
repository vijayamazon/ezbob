namespace Ezbob.Utils.Extensions {
	using System;

	public static class IntExt {

		public static int Min(this int a, int b) {
			return Math.Min(a, b);
		} // Min

		public static int Max(this int a, int b) {
			return Math.Max(a, b);
		} // Max

		public static int DropHundred(this int nValue) {
			return nValue.RoundDownToFactor(100);
		} // DropHundred

		public static int RoundDownToFactor(this int nValue, int nRoundFactor) {
			return (nValue / nRoundFactor) * nRoundFactor;
		} // RoundDownToFactor

	} // class IntExt
} // namespace
