namespace EzBob.Backend.Strategies {
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class LoadVatReturnSummary : AStrategy {
		#region public

		#region constructor

		public LoadVatReturnSummary(int nMarketplaceID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_oSp = new SpLoadVatReturnSummary(nMarketplaceID, DB, Log);
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Load VAT return summary"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			Summary = new VatReturnSummary();

			bool bTotalRow = true;

			m_oSp.ForEachRowSafe((sr, bRowsetStart) => {
				if (bTotalRow) {
					sr.Fill(Summary);
					bTotalRow = false;
				}
				else
					Summary.Quarters.Add(sr.Fill<VatReturnQuarter>());

				return ActionResult.Continue;
			});
		} // Execute

		#endregion method Execute

		#region property Summary

		public VatReturnSummary Summary { get; private set; }

		#endregion property Summary

		#endregion public

		#region private

		private readonly SpLoadVatReturnSummary m_oSp;

		#region class SpLoadVatReturnSummary

		private class SpLoadVatReturnSummary : AStoredProc {
			public SpLoadVatReturnSummary(int nMarketplaceID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				MarketplaceID = nMarketplaceID;
			} // constructor

			public override bool HasValidParameters() {
				return MarketplaceID > 0;
			} // HasValidParameters

			public int MarketplaceID { get; set; }
		} // class SpLoadVatReturnSummary

		#endregion class SpLoadVatReturnSummary

		#endregion private
	} // class LoadVatReturnSummary
} // namespace EzBob.Backend.Strategies
