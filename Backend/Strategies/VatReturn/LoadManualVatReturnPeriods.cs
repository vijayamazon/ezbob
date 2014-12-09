namespace Ezbob.Backend.Strategies.VatReturn {
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public class LoadManualVatReturnPeriods : AStrategy {

		public LoadManualVatReturnPeriods(int nCustomerID) {
			m_oSp = new SpLoadManualVatReturnPeriods(nCustomerID, DB, Log);
			Periods = new List<VatReturnPeriod>();
		} // constructor

		public override string Name {
			get { return "Load manual VAT return periods"; }
		} // Name

		public override void Execute() {
			Periods = m_oSp.Fill<VatReturnPeriod>();
		} // Execute

		public List<VatReturnPeriod> Periods { get; private set; } // Periods

		private readonly SpLoadManualVatReturnPeriods m_oSp;

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

	} // class LoadManualVatReturnPeriods
} // namespace Ezbob.Backend.Strategies.VatReturn
