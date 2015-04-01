namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using System.Collections.Generic;

	internal class CustomerData {
		public CustomerData(SpLoadCashRequestsForAutomationReport.ResultRow sr, string tag) {
			Data = new List<Datum>();

			Add(sr);

			this.tag = tag;
		} // constructor

		public void Add(SpLoadCashRequestsForAutomationReport.ResultRow sr) {
			if (Data.Count < 1) {
				Data.Add(new Datum(sr, this.tag));
				return;
			} // if

			Datum lastDatum = Data[Data.Count - 1];
			ManualDatumItem lastCr = lastDatum.Manual(-1);

			if (sr.IsApproved) {
				if (!lastCr.IsApproved || (lastCr.CrLoanCount > 0))
					Data.Add(new Datum(sr, this.tag));
				else
					lastDatum.Add(sr);
			} else {
				if (lastCr.IsApproved)
					Data.Add(new Datum(sr, this.tag));
				else
					lastDatum.Add(sr);
			} // if
		} // Add

		public List<Datum> Data { get; private set; }

		private readonly string tag;
	} // class CustomerData
} // namespace
