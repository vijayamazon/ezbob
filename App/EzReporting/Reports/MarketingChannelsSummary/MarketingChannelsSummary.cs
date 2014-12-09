namespace Reports.MarketingChannelsSummary {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using TraficReport;

	public class MarketingChannelsSummary {

		public MarketingChannelsSummary(AConnection oDB, ASafeLog oLog) {
			m_oDB = oDB;
			m_oLog = oLog ?? new SafeLog();
		} // constructor

		public KeyValuePair<ReportQuery, DataTable> Run(Report report, DateTime from, DateTime to) {
			SortedDictionary<Source, McsRow> oData = new SortedDictionary<Source, McsRow>();

			IEnumerable<SafeReader> lst = m_oDB.ExecuteEnumerable(
				"RptMarketingChannelsSummary",
				CommandSpecies.StoredProcedure,
				new QueryParameter("DateStart", from),
				new QueryParameter("DateEnd", to)
			);

			foreach (SafeReader sr in lst) {
				InputRowTypes irt;

				string sRowType = sr["RowType"];

				if (!Enum.TryParse(sRowType, out irt)) {
					m_oLog.Alert("Failed to parse input row type from '{0}'.", sRowType);
					continue;
				} // if

				Action<McsRow> oAction = null;

				// ReSharper disable AccessToForEachVariableInClosure
				switch (irt) {
				case InputRowTypes.Visitors:
					oAction = oRow => oRow.Visitors += sr["Visitors"];
					break;

				case InputRowTypes.StartRegistration:
					oAction = oRow => oRow.StartRegistration += sr["Counter"];
					break;

				case InputRowTypes.Personal:
					oAction = oRow => oRow.Personal += sr["Counter"];
					break;

				case InputRowTypes.Company:
					oAction = oRow => oRow.Company += sr["Counter"];
					break;

				case InputRowTypes.DataSource:
					oAction = oRow => oRow.DataSource += sr["Counter"];
					break;

				case InputRowTypes.CompleteApplication:
					oAction = oRow => {
						oRow.CompleteApplication += sr["Counter"];
						oRow.RequestedAmountForComplete += sr["Amount"];
					};
					break;

				case InputRowTypes.RequestedAmount:
					oAction = oRow => oRow.RequestedAmount += sr["Amount"];
					break;

				case InputRowTypes.ApprovedRejected:
					oAction = oRow => {
						oRow.Approved += sr["NumOfApproved"];
						oRow.Rejected += sr["NumOfRejected"];
						oRow.Pending += sr["NumOfPending"];
					};
					break;

				case InputRowTypes.ApprovedDidntTake:
					oAction = oRow => oRow.ApprovedDidntTake += sr["Counter"];
					break;

				case InputRowTypes.ApprovedAmount:
					oAction = oRow => oRow.ApprovedAmount += sr["Amount"];
					break;

				case InputRowTypes.LoansGiven:
					oAction = oRow => oRow.LoansGiven += sr["Amount"];
					break;

				default:
					throw new ArgumentOutOfRangeException();
				} // switch
				// ReSharper restore AccessToForEachVariableInClosure

				Source nSource = (irt == InputRowTypes.Visitors)
					? SourceRefMapper.GetSourceByAnalytics(sr["Source"])
					: SourceRefMapper.GetSourceBySourceRef(sr["ReferenceSource"], sr["GoogleCookie"]);

				if (oData.ContainsKey(nSource))
					oAction(oData[nSource]);
				else {
					var oDataRow = new McsRow {
						Source = nSource,
					};
					oAction(oDataRow);
					oData[nSource] = oDataRow;
				} // if
			} // for each

			var reprortQuery = new ReportQuery(report) {
				DateStart = from,
				DateEnd = to
			};

			return new KeyValuePair<ReportQuery, DataTable>(reprortQuery, ToTable(oData));
		} // Run

		private readonly AConnection m_oDB;
		private readonly ASafeLog m_oLog;

		private enum InputRowTypes {
			Visitors,
			StartRegistration,
			Personal,
			Company,
			DataSource,
			CompleteApplication,
			RequestedAmount,
			ApprovedRejected,
			ApprovedDidntTake,
			ApprovedAmount,
			LoansGiven,
		} // enum InputRowTypes

		private static DataTable ToTable(SortedDictionary<Source, McsRow> oData) {
			McsRow oTotal = new McsRow {
				Source = Source.Total,
				Css = "total",
			};

			DataTable tbl = new DataTable();

			PropertyTraverser.Traverse(typeof (McsRow), (ignored, oPropInfo) => {
				object[] oAttrList = oPropInfo.GetCustomAttributes(typeof(ToStringAttribute), false);
				tbl.Columns.Add(oPropInfo.Name, oAttrList.Length > 0 ? typeof (string) : oPropInfo.PropertyType);
			});

			Source[] aryAllSources = Enum.GetValues(typeof (Source)).Cast<Source>().ToArray();

			Array.Sort(aryAllSources,
				(a, b) => string.Compare(a.ToString(), b.ToString(), System.StringComparison.InvariantCultureIgnoreCase)
			);

			for (int i = 0; i < aryAllSources.Length; i++) {
				Source nSource = (Source)aryAllSources.GetValue(i);

				McsRow oRow = oData.ContainsKey(nSource) ? oData[nSource] : new McsRow { Source = nSource, };

				ToRow(tbl, oRow);
				oTotal.Add(oRow);
			} // for each

			ToRow(tbl, oTotal);

			return tbl;
		} // ToTable

		private static void ToRow(DataTable tbl, McsRow oRow) {
			var lst = new List<object>();

			oRow.DoMath();

			oRow.Traverse((ignored, oPropInfo) => {
				object[] oAttrList = oPropInfo.GetCustomAttributes(typeof(ToStringAttribute), false);
				lst.Add(oAttrList.Length > 0 ? oPropInfo.GetValue(oRow).ToString() : oPropInfo.GetValue(oRow));
			});

			tbl.Rows.Add(lst.ToArray());
		} // ToRow

	} // class MarketingChannelsSummary
} // namespace
