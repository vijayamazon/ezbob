namespace Ezbob.Utils.Extensions {
	using System;

	#region class IntExt

	public static class IntExt {
		#region public

		#region method Min

		public static int Min(this int a, int b) {
			return Math.Min(a, b);
		} // Min

		#endregion method Min

		#region method Max

		public static int Max(this int a, int b) {
			return Math.Max(a, b);
		} // Max

		#endregion method Max

		#region method DropHundred

		public static int DropHundred(this int nValue) {
			return nValue.RoundDownToFactor(100);
		} // DropHundred

		#endregion method DropHundred

		#region method RoundDownToFactor

		public static int RoundDownToFactor(this int nValue, int nRoundFactor) {
			return (nValue / nRoundFactor) * nRoundFactor;
		} // RoundDownToFactor

		#endregion method RoundDownToFactor

		#endregion public
	} // class IntExt

	#endregion class IntExt
} // namespace Ezbob.Utils.Extensions
