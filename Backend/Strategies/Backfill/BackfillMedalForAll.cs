namespace Ezbob.Backend.Strategies.Backfill {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Threading.Tasks;
	using Ezbob.Database;
	using Ezbob.Utils;
	using Ezbob.Utils.Lingvo;

	public class BackfillMedalForAll : AStrategy {
		public BackfillMedalForAll() {
			this.tag = "#MedalBackFill_" + DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture);
			this.pc = new ProgressCounter("{0} cash requests processed.", Log, 100);
		} // constructor

		public override string Name {
			get { return "BackfillMedalForAll"; }
		} // Name

		public override void Execute() {
			List<CashRequestInfo> lst = DB.Fill<CashRequestInfo>(Query, CommandSpecies.Text);

			Log.Debug("{0} loaded for tag '{1}'.", Grammar.Number(lst.Count, "cash request"), this.tag);

			if (ThreadCount <= 1)
				DoCashRequestList(lst);
			else {
				var paraList = new List<CashRequestInfo>[ThreadCount];

				for (int i = 0; i < ThreadCount; i++)
					paraList[i] = new List<CashRequestInfo>();

				uint curIdx = 0;

				foreach (CashRequestInfo cri in lst) {
					paraList[curIdx].Add(cri);

					curIdx++;

					if (curIdx >= ThreadCount)
						curIdx = 0;
				} // for each cash request

				Parallel.ForEach(paraList, DoCashRequestList);
			} // if

			this.pc.Log();
		} // Execute

		private void DoCashRequestList(IEnumerable<CashRequestInfo> lst) {
			foreach (var cri in lst)
				DoOneCashRequest(cri);
		} // DoCashRequestList

		private void DoOneCashRequest(CashRequestInfo cri) {
			try {
				Log.Debug(
					"Medal backfill started for customer {0} with cash request id {1}.",
					cri.CustomerID,
					cri.CashRequestID
				);

				var verification = new AutomationCalculator.MedalCalculation.MedalChooser(DB, Log);
				AutomationCalculator.Common.MedalOutputModel medal = verification.GetMedal(cri.CustomerID, cri.DecisionTime);
				medal.SaveToDb(cri.CashRequestID, null, this.tag, DB, Log);

				Log.Debug(
					"Medal backfill complete for customer {0} with cash request id {1}.",
					cri.CustomerID,
					cri.CashRequestID
				);
			} catch (Exception e) {
				Log.Alert(
					e,
					"Medal backfill failed for customer {0} with cash request id {1}.",
					cri.CustomerID,
					cri.CashRequestID
				);
			} // try

			this.pc.Next();
		} // DoOneCashRequest

		private readonly string tag;
		private readonly ProgressCounter pc;

		private const uint ThreadCount = 3;

		private class CashRequestInfo {
			[FieldName("Id")]
			public long CashRequestID { get; set; }

			[FieldName("IdCustomer")]
			public int CustomerID { get; set; }

			[FieldName("CrTime")]
			public DateTime DecisionTime { get; set; }
		} // class CashRequestInfo

		private const string Query = @"
SELECT
	r.Id,
	r.IdCustomer,
	CrTime = ISNULL(r.UnderwriterDecisionDate, r.CreationDate)
FROM
	CashRequests r
	INNER JOIN Customer c ON r.IdCustomer = c.Id AND c.IsTest = 0
ORDER BY
	r.Id
";
	} // class BackfillMedalForAll
} // namespace

