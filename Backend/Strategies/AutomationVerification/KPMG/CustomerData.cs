namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using System.Collections.Generic;
	using Ezbob.Logger;
	using TCrLoans = System.Collections.Generic.SortedDictionary<
		long,
		System.Collections.Generic.List<LoanMetaData>
	>;

	internal class CustomerData {
		public CustomerData(SpLoadCashRequestsForAutomationReport.ResultRow sr, string tag, ASafeLog log) {
			this.log = log.Safe();

			Data = new List<Datum>();

			Add(sr);

			this.tag = tag;
		} // constructor

		public void Add(SpLoadCashRequestsForAutomationReport.ResultRow sr) {
			if (Data.Count < 1) {
				Data.Add(new Datum(sr, this.tag, this.log));
				return;
			} // if

			Datum lastDatum = Data[Data.Count - 1];
			ManualDatumItem lastCr = lastDatum.Manual(-1);

			if (sr.IsApproved) {
				if (!lastCr.IsApproved || (lastCr.CrLoanCount > 0))
					Data.Add(new Datum(sr, this.tag, this.log));
				else
					lastDatum.Add(sr);
			} else {
				if (lastCr.IsApproved)
					Data.Add(new Datum(sr, this.tag, this.log));
				else
					lastDatum.Add(sr);
			} // if
		} // Add

		/// <summary>
		/// If customer has ARA-sequence (i.e. approve without loan taken, reject, approve with or without loan)
		/// then drop the first approve and the reject of the ARA-sequence.
		/// </summary>
		public void FindLoansAndFilterAraOut(TCrLoans crLoans, SortedSet<string> allLoanSources) {
			var filtered = new List<Datum>();

			for (int i = 0; i < Data.Count; i++) {
				Datum current = Data[i];

				current.FindLoans(crLoans, allLoanSources);

				// The first is always appended.

				if (filtered.Count < 1) {
					filtered.Add(current);
					continue;
				} // if

				Datum next = i < Data.Count - 1 ? Data[i + 1] : null;

				// The last is always appended.

				if (next == null) {
					filtered.Add(current);
					continue;
				} // if

				if (current.IsApproved) {
					// Approved in the middle is appended: previous is either rejected or approved with loan.
					filtered.Add(current);
					continue;
				} // if

				// At this point current is a middle item that was rejected.
				// Therefore previous is for sure approved.

				Datum previous = filtered[filtered.Count - 1];

				if (previous.ActualLoanCount.Total.Count == 0)
					filtered.RemoveAt(filtered.Count - 1);
				else
					filtered.Add(current);
			} // for each

			Data = filtered;
		} // FindLoansAndFilterAraOut

		public List<Datum> Data { get; private set; }

		private readonly string tag;
		private readonly ASafeLog log;
	} // class CustomerData
} // namespace
