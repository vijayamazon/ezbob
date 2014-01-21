namespace EzBob.Backend.Strategies {
	using Ezbob.Database;
	using Ezbob.Logger;

	#region class QuickOffer

	public class QuickOffer : AStrategy {
		#region public

		#region constructor

		public QuickOffer(int nCustomerID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_nCustomerID = nCustomerID;
			Offer = null;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name { get { return "Quick offer"; } } // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			//  1. Except brokers' clients (source ref is liqcen).
			//  2. Limited Company only.
			//  3. Offline clients only.
			//  4. The applicant should be director of the company.
			//  5. Min age: 18 years.
			//  6. No defaults in last two years (ExperianDefaultAccount join CustomerId filter "Date").
			//  7. AML > 70 (AML check in MP_ServiceLog: Authentication index).
			//  8. Personal score >= 560 (ExperianScore in MP_ExperianDataCache).
			//  9. Business score >= 31 (ExperianScore in MP_ExperianDataCache).
			// 10. Tangible equity is positive.
		} // Execute

		#endregion method Execute

		#region property Offer

		public decimal? Offer { get; private set; } // Offer

		#endregion property Offer

		#endregion public

		#region private

		private int m_nCustomerID;

		#endregion private
	} // class QuickOffer

	#endregion class QuickOffer
} // namespace EzBob.Backend.Strategies
