namespace EzBob.Backend.Strategies.VatReturn {
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public class LoadManualVatReturnPeriods : AStrategy {
		#region public

		#region constructor

		public LoadManualVatReturnPeriods(int nCustomerID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_oSp = new SpLoadManualVatReturnPeriods(nCustomerID, DB, Log);
			Periods = new List<VatReturnPeriod>();
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Load manual VAT return periods"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			Periods = m_oSp.Fill<VatReturnPeriod>();
		} // Execute

		#endregion method Execute

		#region property Periods

		public List<VatReturnPeriod> Periods { get; private set; } // Periods

		#endregion property Periods

		#endregion public

		#region private

		private readonly SpLoadManualVatReturnPeriods m_oSp;

		#region class SpLoadManualVatReturnPeriods

		private class SpLoadManualVatReturnPeriods : AStoredProc {
			public SpLoadManualVatReturnPeriods(int nCustomerID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				CustomerID = nCustomerID;
			} // constructor

			public override bool HasValidParameters() {
				return CustomerID > 0;
			} // HasValidParameters

			[UsedImplicitly]
			public int CustomerID { get; set; }
		} // class SpLoadManualVatReturnPeriods

		#endregion class SpLoadManualVatReturnPeriods

		#endregion private
	} // class LoadManualVatReturnPeriods
} // namespace EzBob.Backend.Strategies.VatReturn
