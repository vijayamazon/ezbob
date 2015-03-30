namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using System.Collections.Generic;

	internal class CustomerData {
		public CustomerData(SpLoadCashRequestsForAutomationReport.ResultRow sr) {
			Data = new List<Datum>();

			Add(sr);
		} // constructor

		public void Add(SpLoadCashRequestsForAutomationReport.ResultRow sr) {
			if (Data.Count < 1) {
				Data.Add(new Datum(sr));
				return;
			} // if

			Datum lastDatum = Data[Data.Count - 1];
			ManualDatumItem lastCr = lastDatum.LastManual;

			if (sr.IsApproved) {
				if (!lastCr.IsApproved || (lastCr.CrLoanCount > 0))
					Data.Add(new Datum(sr));
				else
					lastDatum.Add(sr);
			} else {
				if (lastCr.IsApproved)
					Data.Add(new Datum(sr));
				else
					lastDatum.Add(sr);
			} // if
		} // Add

		public List<Datum> Data { get; private set; }
	} // class CustomerData
} // namespace
