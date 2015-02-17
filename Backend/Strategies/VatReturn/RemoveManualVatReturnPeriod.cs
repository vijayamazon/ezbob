namespace Ezbob.Backend.Strategies.VatReturn {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public class RemoveManualVatReturnPeriod : AVatReturnStrategy {
		public RemoveManualVatReturnPeriod(Guid oPeriodID) {
			m_oSp = new SpRemoveManualVatReturnPeriod(oPeriodID, DB, Log);
		} // constructor

		public override string Name {
			get { return "Remove VAT return period"; }
		} // Name

		public override void Execute() {
			SafeReader sr = m_oSp.GetFirst();

			int nCustomerMarketplaceID = sr.IsEmpty ? 0 : sr["CustomerMarketPlaceID"];
			int nHistoryID = sr.IsEmpty ? 0 : sr["HistoryID"];

			if (nCustomerMarketplaceID > 0)
				new CalculateVatReturnSummary(nCustomerMarketplaceID).SetHistoryRecordID(nHistoryID).Execute();
		} // Execute

		private readonly SpRemoveManualVatReturnPeriod m_oSp;

		// ReSharper disable ValueParameterNotUsed

		private class SpRemoveManualVatReturnPeriod : AStoredProc {
			public SpRemoveManualVatReturnPeriod(Guid oPeriodID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				PeriodID = oPeriodID;
			} // constructor

			public override bool HasValidParameters() {
				return PeriodID != Guid.Empty;
			} // HasValidParameters

			[UsedImplicitly]
			public Guid PeriodID { get; set; }

			[UsedImplicitly]
			public int ReasonID {
				get { return (int)DeleteReasons.ManualDeleted; }
				set { }
			} // ReasonID

			[UsedImplicitly]
			public DateTime Now {
				get { return DateTime.UtcNow; }
				set { }
			} // Now
		} // class SpRemoveManualVatReturnPeriod

		// ReSharper restore ValueParameterNotUsed

	} // class RemoveManualVatReturnPeriod
} // namespace Ezbob.Backend.Strategies.VatReturn
