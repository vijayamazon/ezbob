namespace Ezbob.Maam {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using AutomationCalculator.AutoDecision.AutoApproval.ManAgainstAMachine;
	using AutomationCalculator.Common;
	using AutomationCalculator.MedalCalculation;
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
	///     Yet Ezbob Strategy for Man Against A Machine verification.
	/// </summary>
	public class YesMaam {
		public YesMaam(
			int nTopCount,
			int nLastCheckedID,
			AConnection oDB,
			ASafeLog oLog
		) {
			this.topCount = nTopCount;
			this.lastCheckedID = nLastCheckedID;
			this.cashRequestID = 0;

			this.Init(oDB, oLog);
		} // constructor

		public YesMaam(
			int cashRequestID,
			AConnection oDB,
			ASafeLog oLog
		) {
			this.topCount = 0;
			this.lastCheckedID = 0;
			this.cashRequestID = cashRequestID;

			DB = oDB;
			Log = oLog ?? new SafeLog();

			this.medalChooser = new MedalChooser(DB, Log);

			Tag = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss_", CultureInfo.InvariantCulture) + Guid.NewGuid()
				.ToString("N");
		} // constructor

		public void Execute() {
			Log.Debug("Loading relevant cash requests...");

			List<YesMaamInputRow> lst = this.cashRequestID > 0
				? YesMaamInputRow.Load(DB, this.cashRequestID)
				: YesMaamInputRow.Load(DB, this.topCount, this.lastCheckedID);

			Log.Debug("Loading relevant cash requests complete, {0} loaded.", Grammar.Number(lst.Count, "row"));

			var pc = new ProgressCounter("{0} of " + lst.Count + " cash requests processed.", Log, 10);

			var oResult = new List<YesMaamResult>();

			foreach (YesMaamInputRow row in lst) {
				var ymr = new YesMaamResult(row);
				oResult.Add(ymr);

				// DoReject(ymr);
				DoApprove(ymr);

				pc++;
			} // for each

			pc.Log();

			string sEmail = CurrentValues.Instance.MaamEmailReceiver;

			ATag rpt = ToEmail(oResult);

			if (string.IsNullOrWhiteSpace(sEmail))
				Log.Debug("Not sending:\n{0}", rpt);
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

		public string Tag { get; private set; }

		private AConnection DB { get; set; }

		private ASafeLog Log { get; set; }

		private int CapOffer(int customerID, int modelLoanOffer) {
			Log.Info("Finalizing and capping offer");

			int offeredCreditLine = modelLoanOffer;

			bool isHomeOwnerAccordingToLandRegistry = DB.ExecuteScalar<bool>(
				"GetIsCustomerHomeOwnerAccordingToLandRegistry",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerID)
				);

			if (isHomeOwnerAccordingToLandRegistry) {
				Log.Info("Capped for home owner according to land registry");
				offeredCreditLine = Math.Min(offeredCreditLine, CurrentValues.Instance.MaxCapHomeOwner);
			} else {
				Log.Info("Capped for not home owner");
				offeredCreditLine = Math.Min(offeredCreditLine, CurrentValues.Instance.MaxCapNotHomeOwner);
			} // if

			return offeredCreditLine;
		} // CapOffer

		private T CreateCell<T>(string sText, bool? bMismatch = null) where T : ATag, new() {
			T cell = new T();

			cell.Add<Ezbob.Utils.Html.Attributes.Style>(
				"border:1px solid black;" +
				"padding:3px;"
			);

			if (typeof(T) == typeof(Th)) {
				cell.Add<Ezbob.Utils.Html.Attributes.Style>(
					"background-color:darkmagenta;" +
					"color:white;"
				);
			} // if

			if (bMismatch.HasValue && bMismatch.Value)
				cell.Add<Ezbob.Utils.Html.Attributes.Style>("color:red;");

			cell.Append(new Text(sText));

			return cell;
		} // CreateCell

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
				row.Input.ApprovedAmount.ToString("C0", CultureInfo.InvariantCulture)
			).Add<Ezbob.Utils.Html.Attributes.Style>("text-align:right;"));

			tr.Append(CreateCell<Td>(
				row.AutoReject.Data, !row.AutoReject.Data.StartsWith(row.Input.Decision)
			));

			tr.Append(CreateCell<Td>(
				row.AutoApprove.Data, !row.AutoApprove.Data.StartsWith(row.Input.Decision)
			));
			tr.Append(CreateCell<Td>(
				row.AutoApprove.Amount.ToString("C0", CultureInfo.InvariantCulture), row.AutoApprove.Amount != row.Input.ApprovedAmount
			).Add<Ezbob.Utils.Html.Attributes.Style>("text-align:right;"));

			return tr;
		} // CreateDatumRow

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

		private void DoApprove(YesMaamResult ymr) {
			AutomationCalculator.AutoDecision.AutoApproval.ManAgainstAMachine.SameDataAgent agent = null;

			try {
				MedalOutputModel medal = this.medalChooser.GetMedal(ymr.Input.CustomerID, ymr.Input.DecisionTime);

				agent = new SameDataAgent(
					ymr.Input.CustomerID,
					CapOffer(ymr.Input.CustomerID, medal.OfferedLoanAmount),
					medal.Medal,
					medal.MedalType,
					medal.TurnoverType,
					ymr.Input.DecisionTime,
					DB,
					Log
				);

				agent.Init();

				agent.MakeDecision();

				ymr.AutoApprove.Data = (agent.Trail.HasDecided ? "Approved" : "Not approved") + "<br>" + agent.Trail.UniqueID;

				agent.Trail.Save(DB, null, ymr.Input.CashRequestID, Tag);

				ymr.AutoApprove.Amount = agent.Result == null ? 0 : agent.Result.ApprovedAmount;
			} catch (Exception e) {
				ymr.AutoApprove.Data = "Exception";

				if ((agent != null) && (agent.Trail != null))
					ymr.AutoApprove.Data += "<br>" + agent.Trail.UniqueID.ToString();

				Log.Alert(
					e,
					"Exception thrown while executing Approve(customer {0}, time {1}).",
					ymr.Input.CustomerID,
					ymr.Input.DecisionTime.ToString("d/MMM/yyyy H:mm:ss z", CultureInfo.InvariantCulture)
				);
			} // try
		} // DoApprove

		private void DoReject(YesMaamResult ymr) {
			Ezbob.Backend.Strategies.MainStrategy.AutoDecisions.Reject.ManAgainstAMachine.SameDataAgent agent = null;

			try {
				agent = new Ezbob.Backend.Strategies.MainStrategy.AutoDecisions.Reject.ManAgainstAMachine.SameDataAgent(
					ymr.Input.CustomerID,
					ymr.Input.DecisionTime,
					DB,
					Log
				);

				agent.Init();

				ymr.AutoReject.Data = (agent.Decide(ymr.Input.CashRequestID, Tag)
					? "Rejected"
					: "Not rejected") + "<br>" + agent.Trail.UniqueID;
			} catch (Exception e) {
				ymr.AutoReject.Data = "Exception";

				if ((agent != null) && (agent.Trail != null))
					ymr.AutoReject.Data += "<br>" + agent.Trail.UniqueID.ToString();

				Log.Alert(
					e,
					"Exception thrown while executing Reject(customer {0}, time {1}).",
					ymr.Input.CustomerID,
					ymr.Input.DecisionTime.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture)
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

		private void Init(AConnection db, ASafeLog log) {
			DB = db;
			Log = log ?? new SafeLog();

			this.medalChooser = new MedalChooser(DB, Log);

			Tag = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss_", CultureInfo.InvariantCulture) + Guid.NewGuid()
				.ToString("N");
		} // Init

		private MedalChooser medalChooser;

		private readonly int lastCheckedID;
		private readonly int topCount;
		private readonly int cashRequestID;
	} // class YesMaam
} // namespace
