namespace Ezbob.Backend.Strategies.MainStrategy.Helpers {
	internal static class CashRequestIDExt {
		public static bool HasValue(this long? cashRequestID) {
			return (cashRequestID != null) && HasValue(cashRequestID.Value);
		} // HasValue

		public static bool HasValue(this long cashRequestID) {
			return cashRequestID > 0;
		} // HasValue

		public static bool LacksValue(this long? cashRequestID) {
			return !cashRequestID.HasValue();
		} // LacksValue

		public static bool LacksValue(this long cashRequestID) {
			return !cashRequestID.HasValue();
		} // LacksValue
	} // class CashRequestIDExt
} // namespace
