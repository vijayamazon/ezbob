namespace EzBob.Backend.Strategies.VatReturn {
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class LoadVatReturnFullData : AStrategy {
		#region public

		#region constructor

		public LoadVatReturnFullData(int nCustomerID, int nCustomerMarketplaceID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_oRaw = new LoadVatReturnRawData(nCustomerMarketplaceID, DB, Log);
			m_oSummary = new LoadVatReturnSummary(nCustomerID, nCustomerMarketplaceID, DB, Log);
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Load VAT return full data"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			m_oRaw.Execute();
			m_oSummary.Execute();
		} // Execute

		#endregion method Execute

		public VatReturnRawData[] VatReturnRawData { get { return m_oRaw.VatReturnRawData; } }

		public RtiTaxMonthRawData[] RtiTaxMonthRawData { get { return m_oRaw.RtiTaxMonthRawData; } }

		public VatReturnSummary[] Summary { get { return m_oSummary.Summary; } }

		#endregion public

		#region private

		private readonly LoadVatReturnRawData m_oRaw;
		private readonly LoadVatReturnSummary m_oSummary;

		#endregion private
	} // class LoadVatReturnFullData
} // namespace EzBob.Backend.Strategies.VatReturn
