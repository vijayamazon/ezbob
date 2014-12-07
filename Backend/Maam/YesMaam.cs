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

			var pc = new ProgressCounter("{0} of " + lst.Count + " cash requests processed.", m_oLog, 1);

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

			string sEmail = CurrentValues.Instance.MailSenderEmail;

			if (string.IsNullOrWhiteSpace(sEmail))
				m_oLog.Debug("Not sending:\n{0}", ToEmail(oResult));
			else {
				new Mail().Send(
					sEmail,
					null,
					ToEmail(oResult).ToString(),
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

				ymr.AutoReject.SameData = (agent.Decide() ? "Rejected" : "Not rejected") + " " + agent.Trail.UniqueID;
			}
			catch (Exception e) {
				ymr.AutoReject.SameData = "Exception";

				if ((agent != null) && (agent.Trail != null))
					ymr.AutoReject.SameData += " " + agent.Trail.UniqueID.ToString();

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

				ymr.AutoReject.CurrentData = (agent.Trail.HasDecided ? "Rejected" : "Not rejected") + " " + agent.Trail.UniqueID;
			}
			catch (Exception e) {
				ymr.AutoReject.CurrentData = "Exception";

				if ((agent != null) && (agent.Trail != null))
					ymr.AutoReject.CurrentData += " " + agent.Trail.UniqueID.ToString();

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

				ymr.AutoApprove.CurrentData = (agent.Trail.HasDecided ? "Approved" : "Not approved") + " " + agent.Trail.UniqueID;

				if (agent.Result != null)
					ymr.AutoApprove.CurrentAmount = agent.Result.ApprovedAmount;
			}
			catch (Exception e) {
				ymr.AutoApprove.CurrentData = "Exception";

				if ((agent != null) && (agent.Trail != null))
					ymr.AutoApprove.CurrentData += " " + agent.Trail.UniqueID.ToString();

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
			var tbl = new Table();

			tbl.Append(CreateEmailTableHeader());

			var tbody = new Tbody();
			tbl.Append(tbody);

			foreach (YesMaamResult res in lst)
				tbody.Append(CreateDatumRow(res));

			return tbl;
		} // ToEmail

		#endregion method ToEmail

		#region method CreateEmailTableHeader

		private ATag CreateEmailTableHeader() {
			var thead = new Thead();

			var tr = new Tr();
			thead.Append(tr);

			tr.Append(new Th().Append(new Text("Customer ID").Add<Rowspan>("2")));
			tr.Append(new Th().Append(new Text("Underwriter").Add<Colspan>("2")));
			tr.Append(new Th().Append(new Text("Manual Decision").Add<Colspan>("2")));
			tr.Append(new Th().Append(new Text("Auto reject").Add<Colspan>("2")));
			tr.Append(new Th().Append(new Text("Auto approve").Add<Colspan>("4")));

			var tr2 = new Tr();
			thead.Append(tr2);

			// Underwriter
			tr2.Append(new Th().Append(new Text("Name")));
			tr2.Append(new Th().Append(new Text("ID")));

			// Manual decision
			tr2.Append(new Th().Append(new Text("Result")));
			tr2.Append(new Th().Append(new Text("Time")));

			// Auto reject
			tr2.Append(new Th().Append(new Text("Same data")));
			tr2.Append(new Th().Append(new Text("Current data")));

			// Auto approve
			tr2.Append(new Th().Append(new Text("Same data")));
			tr2.Append(new Th().Append(new Text("Amount")));
			tr2.Append(new Th().Append(new Text("Current data")));
			tr2.Append(new Th().Append(new Text("Amount")));

			return thead;
		} // CreateEmailTableHeader

		#endregion method CreateEmailTableHeader

		#region method CreateDatumRow

		private Tr CreateDatumRow(YesMaamResult row) {
			var tr = new Tr();

			tr.Append(new Td().Append(new Text(
				row.Input.CustomerID.ToString(CultureInfo.InvariantCulture)
			)));

			tr.Append(new Td().Append(new Text(
				row.Input.UnderwriterName
			)));
			tr.Append(new Td().Append(new Text(
				row.Input.UnderwriterID.ToString(CultureInfo.InvariantCulture)
			)));

			tr.Append(new Td().Append(new Text(
				row.Input.Decision
			)));
			tr.Append(new Td().Append(new Text(
				row.Input.DecisionTime.ToString("d/MMM/yyyy H:mm:ss z", CultureInfo.InvariantCulture)
			)));

			tr.Append(new Td().Append(new Text(
				row.AutoReject.SameData
			)));
			tr.Append(new Td().Append(new Text(
				row.AutoReject.CurrentData
			)));

			tr.Append(new Td().Append(new Text(
				row.AutoApprove.SameData
			)));
			tr.Append(new Td().Append(new Text(
				row.AutoApprove.SameAmount.ToString(CultureInfo.InvariantCulture)
			)));
			tr.Append(new Td().Append(new Text(
				row.AutoApprove.CurrentData
			)));
			tr.Append(new Td().Append(new Text(
				row.AutoApprove.CurrentAmount.ToString(CultureInfo.InvariantCulture)
			)));

			return tr;
		} // CreateDatumRow

		#endregion method CreateDatumRow

		#endregion private
	} // class YesMaam
} // namespace
