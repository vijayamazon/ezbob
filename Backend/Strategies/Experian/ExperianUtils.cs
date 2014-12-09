namespace EzBob.Backend.Strategies.Experian {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Logger;

	class ExperianUtils {

		public ExperianUtils(ASafeLog oLog) {
			Log = new SafeLog(oLog);
		} // constructor

		public bool IsDirector(ExperianLtd oExperianLtd, string sFirstName, string sLastName) {
			sFirstName = sFirstName.Trim().ToLowerInvariant();
			sLastName = sLastName.Trim().ToLowerInvariant();

			var lst = oExperianLtd.GetChildren<ExperianLtdDL72>();

			foreach (var oDir in lst) {
				if (
					(oDir.FirstName.Trim().ToLowerInvariant() == sFirstName) &&
					(oDir.LastName.Trim().ToLowerInvariant() == sLastName)
				)
					return true;
			} // for each

			return false;
		} // IsDirector

		public void DetectTangibleEquity(ExperianLtd oExperianLtd, out decimal nResultTangibleEquity, out decimal nResultTotalCurrentAssets) {
			nResultTangibleEquity = -1;
			nResultTotalCurrentAssets = 0;

			IEnumerable<ExperianLtdDL99> lst = oExperianLtd.GetChildren<ExperianLtdDL99>();

			ExperianLtdDL99 oCurNode = null;

			foreach (var oNode in lst) {
				if (!oNode.Date.HasValue)
					continue;

				if (oCurNode == null) {
					oCurNode = oNode;
					continue;
				} // if

				// ReSharper disable PossibleInvalidOperationException
				if (oCurNode.Date.Value < oNode.Date.Value)
					oCurNode = oNode;
				// ReSharper restore PossibleInvalidOperationException
			} // for each

			if (oCurNode == null)
				return;

			// ReSharper disable PossibleInvalidOperationException
			Log.Debug("Calculating tangible equity from data for {0}.", oCurNode.Date.Value.ToString("MMMM d yyyy", CultureInfo.InvariantCulture));
			// ReSharper restore PossibleInvalidOperationException

			decimal nTangibleEquity = 0;
			decimal nTotalCurrentAssets = 0;

			Action<decimal?> oPlus = x => nTangibleEquity += x ?? 0;
			Action<decimal?> oMinus = x => nTangibleEquity -= x ?? 0;
			Action<decimal?> oSet = x => nTotalCurrentAssets = x ?? 0;

			var oTags = new List<Tuple<decimal?, Action<decimal?>>> {
				new Tuple<decimal?, Action<decimal?>>(oCurNode.TotalShareFund, oPlus),
				new Tuple<decimal?, Action<decimal?>>(oCurNode.InTngblAssets, oMinus),
				new Tuple<decimal?, Action<decimal?>>(oCurNode.FinDirLoans, oPlus),
				new Tuple<decimal?, Action<decimal?>>(oCurNode.CredDirLoans, oPlus),
				new Tuple<decimal?, Action<decimal?>>(oCurNode.FinLbltsDirLoans, oPlus),
				new Tuple<decimal?, Action<decimal?>>(oCurNode.DebtorsDirLoans, oMinus),
				new Tuple<decimal?, Action<decimal?>>(oCurNode.OnClDirLoans, oPlus),
				new Tuple<decimal?, Action<decimal?>>(oCurNode.CurrDirLoans, oMinus),
				new Tuple<decimal?, Action<decimal?>>(oCurNode.TotalCurrAssets, oSet),
			};

			foreach (var oTag in oTags)
				oTag.Item2(oTag.Item1);

			var ci = new CultureInfo("en-GB", false);
			Log.Debug("Tangible equity is {0}.", nTangibleEquity.ToString("C2", ci));
			Log.Debug("Total current assets is {0}.", nTotalCurrentAssets.ToString("C2", ci));

			nResultTangibleEquity = nTangibleEquity;
			nResultTotalCurrentAssets = nTotalCurrentAssets;
		} // DetectTangibleEquity

		private SafeLog Log { get; set; } // Log

	} // class ExperianUtils

} // namespace EzBob.Backend.Strategies
