﻿namespace EzBob.Backend.Strategies {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ConfigManager;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	using SafeAction = System.Action<System.Collections.Generic.SortedDictionary<int, Ezbob.Backend.Models.VatReturnSummary>, Ezbob.Database.SafeReader>;

	public class LoadVatReturnSummary : AStrategy {
		#region public

		#region constructor

		public LoadVatReturnSummary(int nCustomerID, int nMarketplaceID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_oSp = new SpLoadVatReturnSummary(nMarketplaceID, DB, Log);
			m_nCustomerID = nCustomerID;
			Summary = new VatReturnSummary[0];
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Load VAT return summary"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			var oResults = new SortedDictionary<int, VatReturnSummary>();

			var oProcessors = new Queue<SafeAction>();
			oProcessors.Enqueue(ProcessSummary);
			oProcessors.Enqueue(ProcessQuarter);

			SafeAction oCurAction = null;

			m_oSp.ForEachRowSafe((sr, bRowsetStart) => {
				if (bRowsetStart)
					oCurAction = oProcessors.Dequeue();

				oCurAction(oResults, sr);

				return ActionResult.Continue;
			});

			decimal factor = CurrentValues.Instance.FCFFactor;

			if (Math.Abs(factor) > 0.0000001m) {
				var getExperianAccountsCurrentBalance = new GetExperianAccountsCurrentBalance(m_nCustomerID, DB, Log);
				getExperianAccountsCurrentBalance.Execute();
				decimal newActualLoansRepayment = getExperianAccountsCurrentBalance.CurrentBalance / factor;

				foreach (KeyValuePair<int, VatReturnSummary> pair in oResults) {
					pair.Value.ActualLoanRepayment = newActualLoansRepayment;
					pair.Value.FreeCashFlow -= newActualLoansRepayment;
				} // for each summary
			} // if

			Summary = oResults.Values.ToArray();
		} // Execute

		#endregion method Execute

		#region property Summary

		public VatReturnSummary[] Summary { get; private set; }

		#endregion property Summary

		#endregion public

		#region private

		private readonly int m_nCustomerID;
		private readonly SpLoadVatReturnSummary m_oSp;

		#region class SpLoadVatReturnSummary

		private class SpLoadVatReturnSummary : AStoredProc {
			public SpLoadVatReturnSummary(int nMarketplaceID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				MarketplaceID = nMarketplaceID;
			} // constructor

			public override bool HasValidParameters() {
				return MarketplaceID > 0;
			} // HasValidParameters

			[UsedImplicitly]
			public int MarketplaceID { get; set; }
		} // class SpLoadVatReturnSummary

		#endregion class SpLoadVatReturnSummary

		#region method ProcessSummary

		private static void ProcessSummary(SortedDictionary<int, VatReturnSummary> oResults, SafeReader sr) {
			var oSummary = sr.Fill<VatReturnSummary>();
			oResults[oSummary.SummaryID] = oSummary;
		} // ProcessSummary

		#endregion method ProcessSummary

		#region method ProcessQuarter

		private static void ProcessQuarter(SortedDictionary<int, VatReturnSummary> oResults, SafeReader sr) {
			var oQuarter = sr.Fill<VatReturnQuarter>();
			oResults[oQuarter.SummaryID].Quarters.Add(oQuarter);
		} // ProcessQuarter

		#endregion method ProcessQuarter

		#endregion private
	} // class LoadVatReturnSummary
} // namespace EzBob.Backend.Strategies
