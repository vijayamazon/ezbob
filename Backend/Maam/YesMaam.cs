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
		#region public

		#region constructor

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

		#endregion constructor

		#region method Execute

		public void Execute() {
			m_oLog.Debug("Loading relevant cash requests...");

			List<YesMaamInputRow> lst = YesMaamInputRow.Load(m_oDB, m_nTopCount, m_nLastCheckedID);

			m_oLog.Debug("Loading relevant cash requests complete, {0} loaded.", Grammar.Number(lst.Count, "row"));

			var pc = new ProgressCounter("{0} of " + lst.Count + " cash requests processed.", m_oLog, 10);

			var oResult = new List<YesMaamResult>();

			foreach (YesMaamInputRow row in lst) {
				var ymr = new YesMaamResult(row);
				oResult.Add(ymr);

				DoSameDataReject(ymr);
				DoCurrentDataReject(ymr);
				// TODO: DoSameDataApprove(ymr);
				DoCurrentDataApprove(ymr);

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

		#endregion method Execute

		#endregion public

		#region private

		private readonly AConnection m_oDB;
		private readonly ASafeLog m_oLog;

		private readonly int m_nTopCount;
		private readonly int m_nLastCheckedID;

		#region method DoSameDataReject

		private void DoSameDataReject(YesMaamResult ymr) {
			EzBob.Backend.Strategies.MainStrategy.AutoDecisions.Reject.ManAgainstAMachine.SameDataAgent agent = null;

			try {
				agent = new EzBob.Backend.Strategies.MainStrategy.AutoDecisions.Reject.ManAgainstAMachine.SameDataAgent(
					ymr.Input.CustomerID,
					ymr.Input.DecisionTime,
					m_oDB,
					m_oLog
				);

				agent.Init();

				ymr.AutoReject.SameData = (agent.Decide() ? "Rejected" : "Not rejected") + "<br>" + agent.Trail.UniqueID;
			}
			catch (Exception e) {
				ymr.AutoReject.SameData = "Exception";

				if ((agent != null) && (agent.Trail != null))
					ymr.AutoReject.SameData += "<br>" + agent.Trail.UniqueID.ToString();

				m_oLog.Alert(
					e,
					"Exception thrown while executing SameDataReject(customer {0}, time {1}).",
					ymr.Input.CustomerID,
					ymr.Input.DecisionTime.ToString("d/MMM/yyyy H:mm:ss z", CultureInfo.InvariantCulture)
				);
			} // try
		} // DoSameDataReject

		#endregion method DoSameDataReject

		#region method DoCurrentDataReject

		private void DoCurrentDataReject(YesMaamResult ymr) {
			EzBob.Backend.Strategies.MainStrategy.AutoDecisions.Reject.Agent agent = null;

			try {
				agent = new EzBob.Backend.Strategies.MainStrategy.AutoDecisions.Reject.Agent(
					ymr.Input.CustomerID,
					m_oDB,
					m_oLog
				);

				agent.Init();

				agent.MakeAndVerifyDecision();

				ymr.AutoReject.CurrentData = (agent.Trail.HasDecided ? "Rejected" : "Not rejected") + "<br>" + agent.Trail.UniqueID;
			}
			catch (Exception e) {
				ymr.AutoReject.CurrentData = "Exception";

				if ((agent != null) && (agent.Trail != null))
					ymr.AutoReject.CurrentData += "<br>" + agent.Trail.UniqueID.ToString();

				m_oLog.Alert(
					e,
					"Exception thrown while executing CurrentDataReject(customer {0}, time {1}).",
					ymr.Input.CustomerID,
					ymr.Input.DecisionTime.ToString("d/MMM/yyyy H:mm:ss z", CultureInfo.InvariantCulture)
				);
			} // try
		} // DoCurrentDataReject

		#endregion method DoCurrentDataReject

		#region method DoCurrentDataApprove

		private void DoCurrentDataApprove(YesMaamResult ymr) {
			AutomationCalculator.AutoDecision.AutoApproval.Agent agent = null;

			try {
				agent = new AutomationCalculator.AutoDecision.AutoApproval.Agent(
					ymr.Input.CustomerID,
					ymr.Input.Amount,
					ymr.Input.Medal,
					m_oDB,
					m_oLog
				);

				agent.Init();

				agent.MakeDecision();

				ymr.AutoApprove.CurrentData = (agent.Trail.HasDecided ? "Approved" : "Not approved") + "<br>" + agent.Trail.UniqueID;

				if (agent.Result != null)
					ymr.AutoApprove.CurrentAmount = agent.Result.ApprovedAmount;
			}
			catch (Exception e) {
				ymr.AutoApprove.CurrentData = "Exception";

				if ((agent != null) && (agent.Trail != null))
					ymr.AutoApprove.CurrentData += "<br>" + agent.Trail.UniqueID.ToString();

				m_oLog.Alert(
					e,
					"Exception thrown while executing CurrentDataApprove(customer {0}, time {1}).",
					ymr.Input.CustomerID,
					ymr.Input.DecisionTime.ToString("d/MMM/yyyy H:mm:ss z", CultureInfo.InvariantCulture)
				);
			} // try
		} // DoCurrentDataApprove

		#endregion method DoCurrentDataApprove

		#region method ToEmail

		private ATag ToEmail(IEnumerable<YesMaamResult> lst) {
			ATag tbl = new Table().Add<Ezbob.Utils.Html.Attributes.Style>("border-collapse:collapse;");

			tbl.Append(CreateEmailTableHeader());

			var tbody = new Tbody();
			tbl.Append(tbody);

			foreach (YesMaamResult res in lst)
				tbody.Append(CreateDatumRow(res));

			return new Body().Append(tbl);
		} // ToEmail

		#endregion method ToEmail

		#region method CreateEmailTableHeader

		private ATag CreateEmailTableHeader() {
			var thead = new Thead();

			var tr = new Tr();
			thead.Append(tr);

			tr.Append(CreateCell<Th>("CustomerID").Add<Rowspan>("2"));
			tr.Append(CreateCell<Th>("Underwriter").Add<Colspan>("2"));
			tr.Append(CreateCell<Th>("Manual decision").Add<Colspan>("3"));
			tr.Append(CreateCell<Th>("Auto reject").Add<Colspan>("2"));
			tr.Append(CreateCell<Th>("Auto approve").Add<Colspan>("4"));

			var tr2 = new Tr();
			thead.Append(tr2);

			// Underwriter
			tr2.Append(CreateCell<Th>("Name"));
			tr2.Append(CreateCell<Th>("ID"));

			// Manual decision
			tr2.Append(CreateCell<Th>("Result"));
			tr2.Append(CreateCell<Th>("Time"));
			tr2.Append(CreateCell<Th>("Amount"));

			// Auto reject
			tr2.Append(CreateCell<Th>("Same data"));
			tr2.Append(CreateCell<Th>("Current data"));

			// Auto approve
			tr2.Append(CreateCell<Th>("Same data"));
			tr2.Append(CreateCell<Th>("Amount"));
			tr2.Append(CreateCell<Th>("Current data"));
			tr2.Append(CreateCell<Th>("Amount"));

			return thead;
		} // CreateEmailTableHeader

		#endregion method CreateEmailTableHeader

		#region method CreateDatumRow

		private Tr CreateDatumRow(YesMaamResult row) {
			var tr = new Tr();

			tr.Append(CreateCell<Td>(
				row.Input.CustomerID.ToString(CultureInfo.InvariantCulture)
			).Add<Ezbob.Utils.Html.Attributes.Style>("text-align:right;"));

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
				row.AutoReject.SameData, !row.AutoReject.SameData.StartsWith(row.Input.Decision)
			));
			tr.Append(CreateCell<Td>(
				row.AutoReject.CurrentData, !row.AutoReject.CurrentData.StartsWith(row.Input.Decision)
			));

			tr.Append(CreateCell<Td>(
				row.AutoApprove.SameData, !row.AutoApprove.SameData.StartsWith(row.Input.Decision)
			));
			tr.Append(CreateCell<Td>(
				row.AutoApprove.SameAmount.ToString("C0", CultureInfo.InvariantCulture), row.AutoApprove.SameAmount != row.Input.Amount
			).Add<Ezbob.Utils.Html.Attributes.Style>("text-align:right;"));
			tr.Append(CreateCell<Td>(
				row.AutoApprove.CurrentData, !row.AutoApprove.CurrentData.StartsWith(row.Input.Decision)
			));
			tr.Append(CreateCell<Td>(
				row.AutoApprove.CurrentAmount.ToString("C0", CultureInfo.InvariantCulture), row.AutoApprove.CurrentAmount != row.Input.Amount
			).Add<Ezbob.Utils.Html.Attributes.Style>("text-align:right;"));

			return tr;
		} // CreateDatumRow

		#endregion method CreateDatumRow

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

		#endregion private
	} // class YesMaam
} // namespace
