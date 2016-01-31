namespace Ezbob.Backend.Strategies.MainStrategy.Helpers {
	internal class InternalCashRequestID {
		public static implicit operator long(InternalCashRequestID cr) {
			return cr == null ? 0 : cr.Value;
		} // operator long

		public static implicit operator long?(InternalCashRequestID cr) {
			if (cr == null)
				return null;

			if (cr.LacksValue)
				return null;

			return cr.Value;
		} // operator long

		public InternalCashRequestID(long? cashRequestID) {
			this.cashRequestID = cashRequestID;
		} // constructor

		public bool HasValue {
			get { return this.cashRequestID.HasValue && (this.cashRequestID.Value > 0); }
		} // HasValue

		public bool LacksValue {
			get { return !HasValue; }
		} // HasValue

		public long Value {
			get { return this.cashRequestID ?? 0; }
			set { this.cashRequestID = value; }
		} // HasValue

		private long? cashRequestID;
	} // class InternalCashRequestID
} // namespace
