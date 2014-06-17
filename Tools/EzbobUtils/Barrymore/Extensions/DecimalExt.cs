namespace Ezbob.Utils.Extensions {
	using System;

	public static class DecimalExt {
		#region method Min

		public static decimal Min(this decimal a, decimal b) {
			return Math.Min(a, b);
		} // Min

		#endregion method Min

		#region method Max

		public static decimal Max(this decimal a, decimal b) {
			return Math.Max(a, b);
		} // Max

		#endregion method Max

		#region method DropHundred

		public static decimal DropHundred(this decimal nValue) {
			return nValue.RoundDownToFactor(100.0m);
		} // DropHundred

		#endregion method DropHundred

		#region method RoundDownToFactor

		public static decimal RoundDownToFactor(this decimal nValue, decimal nRoundFactor) {
			return Math.Truncate(nValue / nRoundFactor) * nRoundFactor;
		} // RoundDownToFactor

		#endregion method RoundDownToFactor
	} // class DecimalExt
} // namespace
