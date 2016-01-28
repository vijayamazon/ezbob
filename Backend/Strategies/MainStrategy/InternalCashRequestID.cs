namespace Ezbob.Backend.Strategies.MainStrategy {
	using System;
	using Ezbob.Backend.Strategies.Exceptions;

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

		public void Validate(MainStrategy stra) {
			if (HasValue)
				return;

			throw new StrategyAlert(
				stra,
				String.Format(
					"No cash request to update (neither specified nor created) for customer {0} by underwriter {1}.",
					stra.CustomerID,
					stra.UnderwriterID
				)
			);
		} // Validate

		private long? cashRequestID;
	} // class InternalCashRequestID
} // namespace
