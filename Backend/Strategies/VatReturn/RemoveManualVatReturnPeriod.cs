﻿namespace EzBob.Backend.Strategies.VatReturn {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public class RemoveManualVatReturnPeriod : AVatReturnStrategy {
		#region public

		#region constructor

		public RemoveManualVatReturnPeriod(Guid oPeriodID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_oSp = new SpRemoveManualVatReturnPeriod(oPeriodID, DB, Log);
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Remove VAT return period"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			int nCustomerMarketplaceID = m_oSp.ExecuteScalar<int>();

			if (nCustomerMarketplaceID > 0)
				new CalculateVatReturnSummary(nCustomerMarketplaceID, DB, Log).Execute();
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly SpRemoveManualVatReturnPeriod m_oSp;

		#region class SpRemoveManualVatReturnPeriod
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
		#endregion class SpRemoveManualVatReturnPeriod

		#endregion private
	} // class RemoveManualVatReturnPeriod
} // namespace EzBob.Backend.Strategies.VatReturn
