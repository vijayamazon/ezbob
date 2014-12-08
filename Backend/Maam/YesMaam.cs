namespace Ezbob.Maam {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using Ezbob.Utils.Html;
	using Ezbob.Utils.Html.Attributes;
	using Ezbob.Utils.Html.Tags;
	using Ezbob.Utils.Lingvo;
	using MailApi;

	/// <summary>
	/// Yet Ezbob Strategy for Man Against A Machine verification.
	/// </summary>
	public class YesMaam {
		public YesMaam(
			int nTopCount,
			int nLastCheckedID,
			AConnection oDB,
			ASafeLog oLog
		) {
			m_nTopCount = nTopCount;
			m_nLastCheckedID = nLastCheckedID;

			m_oDB = oDB;
			m_oLog = oLog ?? new SafeLog();
		} // constructor

		public void Execute() {
			m_oLog.Debug("Loading relevant cash requests...");

			List<YesMaamInputRow> lst = YesMaamInputRow.Load(m_oDB, m_nTopCount, m_nLastCheckedID);

			m_oLog.Debug("Loading relevant cash requests complete, {0} loaded.", Grammar.Number(lst.Count, "row"));

			var pc = new ProgressCounter("{0} of " + lst.Count + " cash requests processed.", m_oLog, 10);

			var oResult = new List<YesMaamResult>();

			foreach (YesMaamInputRow row in lst) {
				var ymr = new YesMaamResult(row);
				oResult.Add(ymr);

				DoReject(ymr);
				// TODO: DoApprove(ymr);

				pc++;
			} // for each

			pc.Log();

			string sEmail = CurrentValues.Instance.MaamEmailReceiver;

			ATag rpt = ToEmail(oResult);

			if (string.IsNullOrWhiteSpace(sEmail))
				m_oLog.Debug("Not sending:\n{0}", rpt);
			else {
				new Mail().Send(
					sEmail,
					null,
					rpt.ToString(),
					CurrentValues.Instance.MailSenderEmail,
					CurrentValues.Instance.MailSenderName,
					"Man Against A Machine Result"
				);
			} // if
		} // Execute

		private void DoReject(YesMaamResult ymr) {
			EzBob.Backend.Strategies.MainStrategy.AutoDecisions.Reject.ManAgainstAMachine.SameDataAgent agent = null;

			try {
				agent = new EzBob.Backend.Strategies.MainStrategy.AutoDecisions.Reject.ManAgainstAMachine.SameDataAgent(
					ymr.Input.CustomerID,
					ymr.Input.DecisionTime,
					m_oDB,
					m_oLog
				);

				agent.Init();

				ymr.AutoReject.Data = (agent.Decide() ? "Rejected" : "Not rejected") + "<br>" + agent.Trail.UniqueID;
			}
			catch (Exception e) {
				ymr.AutoReject.Data = "Exception";

				if ((agent != null) && (agent.Trail != null))
					ymr.AutoReject.Data += "<br>" + agent.Trail.UniqueID.ToString();

				m_oLog.Alert(
					e,
					"Exception thrown while executing Reject(customer {0}, time {1}).",
					ymr.Input.CustomerID,
					ymr.Input.DecisionTime.ToString("d/MMM/yyyy H:mm:ss z", CultureInfo.InvariantCulture)
				);
			} // try
		} // DoReject

		private ATag ToEmail(IEnumerable<YesMaamResult> lst) {
			ATag tbl = new Table().Add<Ezbob.Utils.Html.Attributes.Style>("border-collapse:collapse;");

			tbl.Append(CreateEmailTableHeader());

			var tbody = new Tbody();
			tbl.Append(tbody);

			foreach (YesMaamResult res in lst)
				tbody.Append(CreateDatumRow(res));

			return new Body().Append(tbl);
		} // ToEmail

		private ATag CreateEmailTableHeader() {
			var thead = new Thead();

			var tr = new Tr();
			thead.Append(tr);

			tr.Append(CreateCell<Th>("Customer").Add<Colspan>("2"));
			tr.Append(CreateCell<Th>("Underwriter").Add<Colspan>("2"));
			tr.Append(CreateCell<Th>("Manual decision").Add<Colspan>("3"));
			tr.Append(CreateCell<Th>("Auto reject").Add<Rowspan>("2"));
			tr.Append(CreateCell<Th>("Auto approve").Add<Colspan>("2"));

			var tr2 = new Tr();
			thead.Append(tr2);

			tr2.Append(CreateCell<Th>("ID"));
			tr2.Append(CreateCell<Th>("Current status"));

			// Underwriter
			tr2.Append(CreateCell<Th>("Name"));
			tr2.Append(CreateCell<Th>("ID"));

			// Manual decision
			tr2.Append(CreateCell<Th>("Result"));
			tr2.Append(CreateCell<Th>("Time"));
			tr2.Append(CreateCell<Th>("Amount"));

			// Auto approve
			tr2.Append(CreateCell<Th>("Result"));
			tr2.Append(CreateCell<Th>("Amount"));

			return thead;
		} // CreateEmailTableHeader

		private Tr CreateDatumRow(YesMaamResult row) {
			var tr = new Tr();

			tr.Append(CreateCell<Td>(
				row.Input.CustomerID.ToString(CultureInfo.InvariantCulture)
			).Add<Ezbob.Utils.Html.Attributes.Style>("text-align:right;"));

			tr.Append(CreateCell<Td>(
				row.Input.CollectionStatus
			));

			tr.Append(CreateCell<Td>(
				row.Input.UnderwriterName
			));
			tr.Append(CreateCell<Td>(
				row.Input.UnderwriterID.ToString(CultureInfo.InvariantCulture)
			).Add<Ezbob.Utils.Html.Attributes.Style>("text-align:right;"));

			tr.Append(CreateCell<Td>(
				row.Input.Decision
			));
			tr.Append(CreateCell<Td>(
				row.Input.DecisionTime.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture)
			).Add<Ezbob.Utils.Html.Attributes.Style>("text-align:right;"));
			tr.Append(CreateCell<Td>(
				row.Input.Amount.ToString("C0", CultureInfo.InvariantCulture)
			).Add<Ezbob.Utils.Html.Attributes.Style>("text-align:right;"));

			tr.Append(CreateCell<Td>(
				row.AutoReject.Data, !row.AutoReject.Data.StartsWith(row.Input.Decision)
			));

			tr.Append(CreateCell<Td>(
				row.AutoApprove.Data, !row.AutoApprove.Data.StartsWith(row.Input.Decision)
			));
			tr.Append(CreateCell<Td>(
				row.AutoApprove.Amount.ToString("C0", CultureInfo.InvariantCulture), row.AutoApprove.Amount != row.Input.Amount
			).Add<Ezbob.Utils.Html.Attributes.Style>("text-align:right;"));

			return tr;
		} // CreateDatumRow

		private T CreateCell<T>(string sText, bool? bMismatch = null) where T : ATag, new() {
			T cell = new T();

			cell.Add<Ezbob.Utils.Html.Attributes.Style>(
				"border:1px solid black;" +
				"padding:3px;"
			);

			if (typeof (T) == typeof (Th)) {
				cell.Add<Ezbob.Utils.Html.Attributes.Style>(
					"background-color:darkmagenta;" +
					"color:white;"
				);
			} // if

			if (bMismatch.HasValue && bMismatch.Value) {
				cell.Add<Ezbob.Utils.Html.Attributes.Style>(
					"color:red;"
				);
			} // if)

			cell.Append(new Text(sText));

			return cell;
		} // CreateCell

		private readonly AConnection m_oDB;
		private readonly ASafeLog m_oLog;

		private readonly int m_nTopCount;
		private readonly int m_nLastCheckedID;
	} // class YesMaam
} // namespace
