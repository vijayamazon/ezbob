namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using System.Collections.Generic;
	using Ezbob.Logger;

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

		public List<Datum> Data { get; private set; }

		private readonly string tag;
		private readonly ASafeLog log;
	} // class CustomerData
} // namespace
